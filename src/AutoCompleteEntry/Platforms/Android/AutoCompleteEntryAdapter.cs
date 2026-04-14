using Android.Content;
using Android.Views;
using Android.Widget;
using Java.Lang;
using Microsoft.Maui.Platform;
using AView = Android.Views.View;

namespace zoft.MauiExtensions.Controls.Platform;

internal class AutoCompleteEntryAdapter : BaseAdapter, IFilterable
{
    private CustomFilter _filter;
    private List<object> resultList;
    private string _displayMemberPath;
    private DataTemplate _defaultTemplate;
    private readonly Dictionary<DataTemplate, int> _templateToIdMap = new();
    Page _listViewContainer;
    private bool _disposed = false;

    internal IMauiContext MauiContext => _listViewContainer.Handler.MauiContext;

    public DataTemplate ItemTemplate { get; internal set; }

    internal DataTemplate DefaultTemplate
    {
        get
        {
            _defaultTemplate ??= new DataTemplate(() =>
                {
                    var label = new Label();
                    label.SetBinding(Label.TextProperty, _displayMemberPath ?? ".");
                    label.HorizontalTextAlignment = Microsoft.Maui.TextAlignment.Center;
                    label.VerticalTextAlignment = Microsoft.Maui.TextAlignment.Center;
                    label.MinimumHeightRequest = 44;

                    return label;
                });
            return _defaultTemplate;
        }
    }

    public AutoCompleteEntryAdapter(Context context) : base()
    {
        _listViewContainer = Application.Current.Windows[0].Page;

        resultList = new List<object>();

        NotifyDataSetChanged();
    }

    protected override void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        _disposed = true;

        if (disposing)
        {
            _filter?.Dispose();
        }

        _filter = null;
        _defaultTemplate = null;

        base.Dispose(disposing);
    }

    public void UpdateList(IEnumerable<object> list, string displayMemberPath)
    {
        _displayMemberPath = displayMemberPath;

        resultList = list.ToList();

        NotifyDataSetChanged();
    }

    public override int Count => resultList.Count;

    public Filter Filter => _filter ??= new CustomFilter(this);

    public override Java.Lang.Object GetItem(int position) => new ObjectWrapper(GetObject(position));

    public object GetObject(int position) => resultList[position];

    public override long GetItemId(int position)
    {
        return position;
    }

    // Returns the number of distinct recycling pools Android should maintain.
    // One pool for a plain DataTemplate; up to 10 for a DataTemplateSelector.
    public override int ViewTypeCount
        => ItemTemplate is DataTemplateSelector ? 10 : 1;

    // Maps each resolved DataTemplate to a stable integer pool ID so Android
    // never hands GetView a recycled view of the wrong template type.
    public override int GetItemViewType(int position)
    {
        var item = GetObject(position);
        var template = ItemTemplate ?? DefaultTemplate;
        return TemplateIdMapper.GetViewType(template, item, _listViewContainer, _templateToIdMap);
    }

    public override AView GetView(int position, AView convertView, ViewGroup parent)
    {
        var item = GetObject(position);
        var template = ItemTemplate ?? DefaultTemplate;

        // Resolve DataTemplateSelector if needed
        var resolvedTemplate = template is DataTemplateSelector selector
            ? selector.SelectTemplate(item, _listViewContainer)
            : template;

        Microsoft.Maui.Controls.View templateView;
        AView nativeView;

        // Recycle if possible — only update the binding context, skip handler + native view creation
        if (convertView != null && convertView.Tag is ViewWrapperTag tag)
        {
            nativeView = convertView;
            templateView = tag.MauiView;
            templateView.BindingContext = item;
        }
        else
        {
            // First few visible rows: create MAUI view + native handler from scratch
            templateView = (Microsoft.Maui.Controls.View)resolvedTemplate.CreateContent();
            templateView.BindingContext = item;

            nativeView = templateView.ToPlatform(MauiContext);

            // Stash the MAUI view in the native view's Tag so it can be retrieved on recycle
            nativeView.Tag = new ViewWrapperTag { MauiView = templateView };
        }

        // Measure after handler creation so MAUI's layout system can resolve sizes.
        // Re-measure on every GetView call because recycled rows may bind to data of a different height.
        var density = (double)parent.Context.Resources.DisplayMetrics.Density;
        var widthConstraint = DensityHelper.WidthPixelsToDipConstraint(parent.Width, density);
        var measure = ((IView)templateView).Measure(widthConstraint, double.PositiveInfinity);

        var heightPx = DensityHelper.HeightDipToPixels(measure.Height, density);
        nativeView.LayoutParameters = new ViewGroup.LayoutParams(
            ViewGroup.LayoutParams.MatchParent,
            heightPx);

        return nativeView;
    }

    internal sealed class ViewWrapperTag : Java.Lang.Object
    {
        internal Microsoft.Maui.Controls.View MauiView { get; set; }
    }

    private class CustomFilter : Filter
    {
        private readonly AutoCompleteEntryAdapter _adapter;

        public CustomFilter(AutoCompleteEntryAdapter adapter)
        {
            _adapter = adapter;
        }

        protected override FilterResults PerformFiltering(ICharSequence constraint)
        {
            var results = new FilterResults();

            results.Count = 100;
            return results;
        }

        protected override void PublishResults(ICharSequence constraint, FilterResults results)
        {
            _adapter.NotifyDataSetChanged();
        }
    }

    public const string DoNotUpdateMarker = "__DO_NOT_UPDATE__";
    internal class ObjectWrapper : Java.Lang.Object
    {
        public ObjectWrapper(object obj)
        {
            Object = obj;
        }

        public object Object { get; set; }

        public override string ToString() => DoNotUpdateMarker;
    }
}
