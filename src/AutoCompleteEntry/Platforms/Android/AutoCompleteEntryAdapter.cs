using Android.Content;
using Android.Content.Res;
using Android.Util;
using Android.Views;
using Android.Widget;
using Java.Lang;
using Microsoft.Maui.Platform;
using AView = Android.Views.View;

namespace zoft.MauiExtensions.Controls.Platform;

internal class AutoCompleteEntryAdapter : BaseAdapter, IFilterable
{
    private CustomFilter? _filter;
    private List<object> _resultList = new();
    private string? _displayMemberPath;
    private DataTemplate? _defaultTemplate;
    private readonly Dictionary<DataTemplate, int> _templateToIdMap = new();
    private readonly Dictionary<int, DataTemplate> _resolvedTemplateCache = new();
    private readonly Page _listViewContainer;
    private bool _disposed = false;
    private int _templateGeneration;
    internal AutoCompleteEntry? Owner
    {
        get; set;
    }
    private readonly List<WeakReference<ViewWrapperTag>> _rows = [];
    private readonly SelectionAccessibilityDelegate _selectionAccessibility;

    internal void RefreshSelection()
    {
        // Weak references can still resolve after MAUI has disposed the Java peer.
        // Forget retired/native-disposed rows before touching presentation or JNI.
        _rows.RemoveAll(reference => !reference.TryGetTarget(out ViewWrapperTag? tag) ||
            tag.MauiView.Handler is null || tag.NativeView?.TryGetTarget(out AView? native) != true ||
            native is null || native.Handle == IntPtr.Zero);
        foreach (WeakReference<ViewWrapperTag> reference in _rows)
        {
            if (reference.TryGetTarget(out ViewWrapperTag? tag) && tag.MauiView is SelectionRow row && row.BindingContext is { } item && Owner is not null)
            {
                row.Update(Owner, item);
                if (tag.NativeView?.TryGetTarget(out AView? native) == true)
                {
                    native.SendAccessibilityEvent(Android.Views.Accessibility.EventTypes.WindowContentChanged);
                }
            }
        }
    }

    internal void ResetPresentation()
    {
        _templateGeneration++;
        NotifyDataSetChanged();
    }

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
            {
                return;
            }

            _itemTemplate = value;
            // Clear stale IDs — old template instances are no longer valid keys
            // and a new selector may produce a completely different set of templates.
            _templateToIdMap.Clear();
            _resolvedTemplateCache.Clear();
            _templateGeneration++;

            // Tell the widget to discard all recycled views so stale layouts
            // from the previous template set are never handed to GetView.
            if (Owner?.IsMultiple == true)
            {
                NotifyDataSetChanged();
            }
            else
            {
                NotifyDataSetInvalidated();
            }
        }
    }

    internal DataTemplate DefaultTemplate
    {
        get
        {
            _defaultTemplate ??= new DataTemplate(() =>
                {
                    var label = new Label();
                    label.SetBinding(Label.TextProperty, string.IsNullOrEmpty(_displayMemberPath) ? "." : _displayMemberPath);
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
        _ = context;
        _selectionAccessibility = new(this);

        _listViewContainer = Application.Current?.Windows.FirstOrDefault()?.Page
            ?? throw new InvalidOperationException(
                $"{nameof(AutoCompleteEntryAdapter)} cannot be created before the MAUI application has an active window with a root page.");

        NotifyDataSetChanged();
    }

    protected override void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        if (disposing)
        {
            _filter?.Dispose();
            foreach (WeakReference<ViewWrapperTag> reference in _rows)
            {
                if (reference.TryGetTarget(out ViewWrapperTag? tag))
                {
                    if (tag.NativeView?.TryGetTarget(out AView? native) == true && native.Handle != IntPtr.Zero)
                    {
                        native.SetAccessibilityDelegate(null);
                    }

                    tag.MauiView.DisconnectHandlers();
                }
            }

            _rows.Clear();
            _selectionAccessibility.Dispose();
            Owner = null;
        }

        _filter = null;
        _defaultTemplate = null;

        base.Dispose(disposing);
    }

    public void UpdateList(IEnumerable<object> list, string? displayMemberPath)
    {
        if (_displayMemberPath != displayMemberPath)
        {
            _defaultTemplate = null;
            _templateGeneration++;
        }
        _displayMemberPath = displayMemberPath;

        _resultList = list.ToList();
        _resolvedTemplateCache.Clear();

        // When using a DataTemplateSelector the map can accumulate entries from
        // previous suggestion sets.  Reset it on every list replacement so
        // view-type IDs stay in sync with the *current* items and the adapter
        // never hits MaxViewTypes due to stale entries.
        if (ItemTemplate is DataTemplateSelector)
        {
            _templateToIdMap.Clear();
            _templateGeneration++;
            if (Owner?.IsMultiple == true)
            {
                NotifyDataSetChanged();
            }
            else
            {
                NotifyDataSetInvalidated();
            }
        }
        else
        {
            NotifyDataSetChanged();
        }
    }

    public override int Count => _resultList.Count;

    public Filter Filter => _filter ??= new CustomFilter(this);

    public override Java.Lang.Object GetItem(int position) => new ObjectWrapper(GetObject(position));

    public object GetObject(int position) => _resultList[position];

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
        if (_resolvedTemplateCache.TryGetValue(position, out DataTemplate? cached))
        {
            _resolvedTemplateCache.Remove(position);
            return cached;
        }

        DataTemplate template = ItemTemplate ?? DefaultTemplate;
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
        object item = GetObject(position);
        DataTemplate template = ItemTemplate ?? DefaultTemplate;

        // Resolve and cache the template for this position
        DataTemplate resolvedTemplate = ResolveTemplate(position, item);
        _resolvedTemplateCache[position] = resolvedTemplate;

        return template is DataTemplateSelector
            ? TemplateIdMapper.GetViewType(resolvedTemplate, _templateToIdMap)
            : 0;
    }

    public override AView GetView(int position, AView? convertView, ViewGroup? parent)
    {
        object item = GetObject(position);

        // Use the cached resolved template from GetItemViewType to guarantee consistency
        DataTemplate resolvedTemplate = ResolveTemplate(position, item);

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
            if (convertView?.Tag is ViewWrapperTag retired)
            {
                _rows.RemoveAll(reference => !reference.TryGetTarget(out ViewWrapperTag? row) || ReferenceEquals(row, retired));
                convertView.SetAccessibilityDelegate(null);
                retired.MauiView.DisconnectHandlers();
            }
            object createdContent = resolvedTemplate.CreateContent();
            if (createdContent is not Microsoft.Maui.Controls.View createdView)
            {
                throw new InvalidOperationException(
                    $"The resolved item template '{resolvedTemplate.GetType().FullName}' must create a " +
                    $"{typeof(Microsoft.Maui.Controls.View).FullName}, but created " +
                    $"'{createdContent?.GetType().FullName ?? "null"}'.");
            }

            templateView = createdView;
            if (Owner?.IsMultiple == true)
            {
                templateView = new SelectionRow(templateView);
            }

            templateView.BindingContext = item;

            nativeView = templateView.ToPlatform(MauiContext);

            // Stash the MAUI view in the native view's Tag so it can be retrieved on recycle
            var rowTag = new ViewWrapperTag(templateView, _templateGeneration);
            rowTag.NativeView = new(nativeView);
            nativeView.Tag = rowTag;
            _rows.RemoveAll(reference => !reference.TryGetTarget(out _));
            _rows.Add(new(rowTag));
        }

        if (templateView is SelectionRow selectionRow && Owner is not null)
        {
            selectionRow.Update(Owner, item);
        }

        if (Owner?.IsMultiple == true)
        {
            nativeView.ContentDescription = Owner.GetSelectionText(item);
            nativeView.Focusable = false;
            nativeView.Clickable = false;
            nativeView.ImportantForAccessibility = ImportantForAccessibility.Yes;
            nativeView.SetAccessibilityDelegate(_selectionAccessibility);
            if (nativeView is ViewGroup group)
            {
                group.DescendantFocusability = DescendantFocusability.BlockDescendants;
                for (int i = 0; i < group.ChildCount; i++)
                {
                    group.GetChildAt(i)!.ImportantForAccessibility = ImportantForAccessibility.NoHideDescendants;
                }
            }
        }

        // Measure after handler creation so MAUI's layout system can resolve sizes.
        // Re-measure on every GetView call because recycled rows may bind to data of a different height.
        ViewGroup parentView = parent
            ?? throw new InvalidOperationException($"{nameof(AutoCompleteEntryAdapter)} requires a non-null parent view.");
        Resources resources = parentView.Context?.Resources
            ?? throw new InvalidOperationException($"{nameof(AutoCompleteEntryAdapter)} requires the parent view to expose Resources.");
        DisplayMetrics displayMetrics = resources.DisplayMetrics
            ?? throw new InvalidOperationException($"{nameof(AutoCompleteEntryAdapter)} requires Android display metrics.");
        double density = (double)displayMetrics.Density;
        double widthConstraint = DensityHelper.WidthPixelsToDipConstraint(parentView.Width, density);
        Microsoft.Maui.Graphics.Size measure = ((IView)templateView).Measure(widthConstraint, double.PositiveInfinity);
        double heightDip = System.Math.Max(measure.Height, 44);

        int heightPx = DensityHelper.HeightDipToPixels(heightDip, density);
        if (nativeView.LayoutParameters is { } lp)
        {
            lp.Width = ViewGroup.LayoutParams.MatchParent;
            lp.Height = heightPx;
        }
        else
        {
            nativeView.LayoutParameters = new AbsListView.LayoutParams(
                ViewGroup.LayoutParams.MatchParent,
                heightPx);
        }

        return nativeView;
    }

    internal sealed class ViewWrapperTag : Java.Lang.Object
    {
        internal ViewWrapperTag(Microsoft.Maui.Controls.View mauiView, int templateGeneration)
        {
            MauiView = mauiView;
            TemplateGeneration = templateGeneration;
        }

        internal Microsoft.Maui.Controls.View MauiView
        {
            get;
        }
        internal int TemplateGeneration
        {
            get;
        }
        internal WeakReference<AView>? NativeView
        {
            get; set;
        }
    }

    private sealed class SelectionAccessibilityDelegate(AutoCompleteEntryAdapter adapter) : AView.AccessibilityDelegate
    {
        public override void OnInitializeAccessibilityNodeInfo(AView host, Android.Views.Accessibility.AccessibilityNodeInfo info)
        {
            base.OnInitializeAccessibilityNodeInfo(host, info);
            if (info is null || host?.Tag is not ViewWrapperTag tag || adapter.Owner is not { } owner)
            {
                return;
            }

            info.ClassName = "android.widget.CheckBox";
            info.Checkable = true;
            bool selected = owner.IsSuggestionSelected(tag.MauiView.BindingContext);
            if (OperatingSystem.IsAndroidVersionAtLeast(36))
            {
                info.CheckedState = selected ? Android.Views.Accessibility.CheckedState.True : Android.Views.Accessibility.CheckedState.False;
            }
            else
            {
                info.Checked = selected;
            }

            info.Clickable = true;
            info.AddAction(Android.Views.Accessibility.AccessibilityNodeInfo.AccessibilityAction.ActionClick);
        }

        public override bool PerformAccessibilityAction(AView host, Android.Views.Accessibility.Action action, Android.OS.Bundle? args)
        {
            if (action == Android.Views.Accessibility.Action.Click && host.Tag is ViewWrapperTag tag && adapter.Owner is { } owner)
            {
                owner.OnSuggestionSelected(tag.MauiView.BindingContext);
                host.SendAccessibilityEvent(Android.Views.Accessibility.EventTypes.ViewClicked);
                return true;
            }
            return base.PerformAccessibilityAction(host, action, args);
        }
    }

    private class CustomFilter : Filter
    {
        private readonly AutoCompleteEntryAdapter _adapter;

        public CustomFilter(AutoCompleteEntryAdapter adapter)
        {
            _adapter = adapter;
        }

        protected override FilterResults PerformFiltering(ICharSequence? constraint)
        {
            var results = new FilterResults();

            results.Count = 100;
            return results;
        }

        protected override void PublishResults(ICharSequence? constraint, FilterResults? results)
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

        public object Object
        {
            get; set;
        }

        public override string ToString() => DoNotUpdateMarker;
    }
}
