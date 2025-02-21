using Android.Content;
using Android.Views;
using Android.Widget;
using Java.Lang;
using Microsoft.Maui.Controls.Internals;
using zoft.MauiExtensions.Core.Extensions;
using AView = Android.Views.View;

namespace zoft.MauiExtensions.Controls.Platform;

internal class AutoCompleteEntryAdapter : BaseAdapter, IFilterable
{
    private CustomFilter _filter;
    private List<object> resultList;
    private string _displayMemberPath;
    private DataTemplate _defaultTemplate;
    Page _listViewContainer;
    private bool _disposed = false;
        
    internal IMauiContext MauiContext => _listViewContainer.Handler.MauiContext;
        
    public DataTemplate ItemTemplate { get; internal set; }

    internal DataTemplate DefaultTemplate
    {
        get
        {
            if (_defaultTemplate == null)
            {
                _defaultTemplate = new DataTemplate(() =>
                {
                    var label = new Label();
                    label.SetBinding(Label.TextProperty, _displayMemberPath ?? ".");
                    label.HorizontalTextAlignment = Microsoft.Maui.TextAlignment.Center;
                    label.VerticalTextAlignment = Microsoft.Maui.TextAlignment.Center;
                    label.MinimumHeightRequest = 35;

                    return label;
                });
            }
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

    public override AView GetView(int position, AView convertView, ViewGroup parent)
    {
        var item = GetObject(position);

        Microsoft.Maui.Controls.Platform.Compatibility.ContainerView result = null;
        if (convertView != null)
        {
            result = convertView as Microsoft.Maui.Controls.Platform.Compatibility.ContainerView;
            result.View.BindingContext = item;
        }
        else
        {
            var template = ItemTemplate ?? DefaultTemplate;
            var view = (Microsoft.Maui.Controls.View)template.CreateContent(item, _listViewContainer);
            view.BindingContext = item;

            result = new Microsoft.Maui.Controls.Platform.Compatibility.ContainerView(parent.Context, view, MauiContext);
            result.MatchWidth = true;

            //TODO is internal - find better solution to set true value.
            //result.MeasureHeight = true;
            result.SetPropertyValue("MeasureHeight", true);
        }

        return result;
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
