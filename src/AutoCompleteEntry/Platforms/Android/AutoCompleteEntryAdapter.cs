using Android.Content;
using Android.Views;
using Android.Widget;
using Java.Lang;
using Microsoft.Maui.Platform;
using AView = Android.Views.View;

namespace zoft.MauiExtensions.Controls.Platform;

internal class AutoCompleteEntryAdapter : BaseAdapter, IFilterable
{
    private CustomFilter? _filter;
    private List<object> resultList = new();
    private string? _displayMemberPath;
    private DataTemplate? _defaultTemplate;
    private readonly Dictionary<DataTemplate, int> _templateToIdMap = new();
    private readonly Dictionary<int, DataTemplate> _resolvedTemplateCache = new();
    private readonly Page _listViewContainer;
    private bool _disposed = false;
    private int _templateGeneration;

    internal IMauiContext MauiContext => _listViewContainer.Handler?.MauiContext
        ?? throw new InvalidOperationException(
            $"{nameof(AutoCompleteEntryAdapter)} requires the container page to have a handler with a valid {nameof(IMauiContext)}.");

    private DataTemplate? _itemTemplate;
    public DataTemplate? ItemTemplate
    {
        get => _itemTemplate;
        internal set
        {
            if (_itemTemplate == value)
                return;

            _itemTemplate = value;
            // Clear stale IDs — old template instances are no longer valid keys
            // and a new selector may produce a completely different set of templates.
            _templateToIdMap.Clear();
            _resolvedTemplateCache.Clear();
            _templateGeneration++;

            // Tell the widget to discard all recycled views so stale layouts
            // from the previous template set are never handed to GetView.
            NotifyDataSetInvalidated();
        }
    }

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
        _listViewContainer = Application.Current?.Windows.FirstOrDefault()?.Page
            ?? throw new InvalidOperationException(
                $"{nameof(AutoCompleteEntryAdapter)} cannot be created before the MAUI application has an active window with a root page.");

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
        _resolvedTemplateCache.Clear();

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
    // One pool for a plain DataTemplate; up to MaxViewTypes for a DataTemplateSelector.
    public override int ViewTypeCount
        => ItemTemplate is DataTemplateSelector ? TemplateIdMapper.MaxViewTypes : 1;

    /// <summary>
    /// Resolves the concrete DataTemplate for the given position, using the cached
    /// result from a previous call if available, otherwise resolving fresh.
    /// </summary>
    private DataTemplate ResolveTemplate(int position, object item)
    {
        if (_resolvedTemplateCache.TryGetValue(position, out var cached))
        {
            _resolvedTemplateCache.Remove(position);
            return cached;
        }

        var template = ItemTemplate ?? DefaultTemplate;
        return template is DataTemplateSelector selector
            ? selector.SelectTemplate(item, _listViewContainer)
                ?? throw new InvalidOperationException(
                    $"DataTemplateSelector '{template.GetType().FullName}' returned null for item at position {position}.")
            : template;
    }

    // Maps each resolved DataTemplate to a stable integer pool ID so Android
    // never hands GetView a recycled view of the wrong template type.
    // Also caches the resolved template so GetView uses the exact same one.
    public override int GetItemViewType(int position)
    {
        var item = GetObject(position);
        var template = ItemTemplate ?? DefaultTemplate;

        // Resolve and cache the template for this position
        var resolvedTemplate = ResolveTemplate(position, item);
        _resolvedTemplateCache[position] = resolvedTemplate;

        return template is DataTemplateSelector
            ? TemplateIdMapper.GetViewType(resolvedTemplate, _templateToIdMap)
            : 0;
    }

    public override AView GetView(int position, AView convertView, ViewGroup parent)
    {
        var item = GetObject(position);

        // Use the cached resolved template from GetItemViewType to guarantee consistency
        var resolvedTemplate = ResolveTemplate(position, item);

        Microsoft.Maui.Controls.View templateView;
        AView nativeView;

        // Recycle if possible — only update the binding context, skip handler + native view creation.
        // Refuse views from an older template generation to avoid layout mismatches.
        if (convertView != null && convertView.Tag is ViewWrapperTag tag
            && tag.TemplateGeneration == _templateGeneration)
        {
            nativeView = convertView;
            templateView = tag.MauiView;
            templateView.BindingContext = item;
        }
        else
        {
            // First few visible rows: create MAUI view + native handler from scratch
            var createdContent = resolvedTemplate.CreateContent();
            if (createdContent is not Microsoft.Maui.Controls.View createdView)
                throw new InvalidOperationException(
                    $"The resolved item template '{resolvedTemplate.GetType().FullName}' must create a " +
                    $"{typeof(Microsoft.Maui.Controls.View).FullName}, but created " +
                    $"'{createdContent?.GetType().FullName ?? "null"}'.");

            templateView = createdView;
            templateView.BindingContext = item;

            nativeView = templateView.ToPlatform(MauiContext);

            // Stash the MAUI view in the native view's Tag so it can be retrieved on recycle
            nativeView.Tag = new ViewWrapperTag(templateView, _templateGeneration);
        }

        // Measure after handler creation so MAUI's layout system can resolve sizes.
        // Re-measure on every GetView call because recycled rows may bind to data of a different height.
        var density = (double)parent.Context.Resources.DisplayMetrics.Density;
        var widthConstraint = DensityHelper.WidthPixelsToDipConstraint(parent.Width, density);
        var measure = ((IView)templateView).Measure(widthConstraint, double.PositiveInfinity);
        var heightDip = System.Math.Max(measure.Height, 44);

        var heightPx = DensityHelper.HeightDipToPixels(heightDip, density);
        nativeView.LayoutParameters = new ViewGroup.LayoutParams(
            ViewGroup.LayoutParams.MatchParent,
            heightPx);

        return nativeView;
    }

    internal sealed class ViewWrapperTag : Java.Lang.Object
    {
        internal ViewWrapperTag(Microsoft.Maui.Controls.View mauiView, int templateGeneration)
        {
            MauiView = mauiView;
            TemplateGeneration = templateGeneration;
        }

        internal Microsoft.Maui.Controls.View MauiView { get; }
        internal int TemplateGeneration { get; }
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
