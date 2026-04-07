using Foundation;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Platform;
using System.Collections;
using System.Collections.Specialized;
using UIKit;

namespace zoft.MauiExtensions.Controls.Platform;

internal class AutoCompleteEntryTableSource : UITableViewSource
{
    private readonly UITableView _view;
    private readonly IList _items;
    private readonly string _displayMemberPath;
    private readonly DataTemplate _itemTemplate;
    private readonly IMauiContext _mauiContext;

    //private readonly string _cellIdentifier;
    private readonly Page _listViewContainer;

    private DataTemplate _defaultItemTemplate;
    internal DataTemplate DefaultItemTemplate
    {
        get
        {
            if (_defaultItemTemplate == null)
            {
                _defaultItemTemplate = new DataTemplate(() =>
                {
                    var label = new Label();

                    label.SetBinding(Label.TextProperty, _displayMemberPath ?? ".");
                    label.HorizontalTextAlignment = Microsoft.Maui.TextAlignment.Center;
                    label.VerticalTextAlignment = Microsoft.Maui.TextAlignment.Center;
                    label.MinimumHeightRequest = 44;

                    return label;
                });
            }

            return _defaultItemTemplate;
        }
    }

    public AutoCompleteEntryTableSource(UITableView view, IList items, string displayMemberPath, DataTemplate itemTemplate, IMauiContext mauiContext)
    {
        _view = view;
        _items = items;
        _displayMemberPath = displayMemberPath;
        _itemTemplate = itemTemplate;
        _mauiContext = mauiContext;
        //_cellIdentifier = Guid.NewGuid().ToString();
        _listViewContainer = Application.Current.Windows[0].Page;

        _view.EstimatedRowHeight = 60f;
        _view.RowHeight = UITableView.AutomaticDimension;

        CheckIfItemsSourceIsNotifiable();
    }

    private void CheckIfItemsSourceIsNotifiable()
    {
        if (_items is INotifyCollectionChanged notifiableItems)
        {
            notifiableItems.CollectionChanged += NotifiableItems_CollectionChanged;
        }
    }

    private void NotifiableItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (!MainThread.IsMainThread)
        {
            MainThread.BeginInvokeOnMainThread(() => CollectionChanged(e));
        }
        else
        {
            CollectionChanged(e);
        }
    }

    private void CollectionChanged(NotifyCollectionChangedEventArgs args)
    {
        _view.ReloadData();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing && _items is INotifyCollectionChanged notifiableItems)
        {
            notifiableItems.CollectionChanged -= NotifiableItems_CollectionChanged;
        }
    }

    public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
    {
        var item = _items[indexPath.Row];
        var templateToUse = _itemTemplate ?? DefaultItemTemplate;

        var cellId = ((IDataTemplateController)templateToUse.SelectDataTemplate(item, _listViewContainer)).IdString;

        var cell = tableView.DequeueReusableCell(cellId) ?? new UITableViewCell(UITableViewCellStyle.Default, cellId);

        // Create the MAUI view from the DataTemplate
        var templateView = templateToUse.CreateContent() as View;
        templateView.BindingContext = item;

        // Convert MAUI view to native iOS view first (creates the handler)
        var nativeView = templateView.ToPlatform(_mauiContext);

        // Measure AFTER handler creation so MAUI's layout system can resolve sizes.
        // MAUI views don't expose intrinsic content size to UIKit Auto Layout,
        // so we measure explicitly and add a height constraint to inform
        // UITableView.AutomaticDimension of the desired row height.
        var widthConstraint = tableView.Bounds.Width > 0 ? (double)tableView.Bounds.Width : double.PositiveInfinity;
        var measure = ((IView)templateView).Measure(widthConstraint, double.PositiveInfinity);

        // Clear previous content to avoid overlapping
        foreach (var subview in cell.ContentView.Subviews)
        {
            subview.RemoveFromSuperview();
        }

        nativeView.TranslatesAutoresizingMaskIntoConstraints = false;
        cell.ContentView.AddSubview(nativeView);

        // Pin to all 4 edges for auto-sizing cell support
        NSLayoutConstraint.ActivateConstraints(
        [
            nativeView.LeadingAnchor.ConstraintEqualTo(cell.ContentView.LeadingAnchor),
            nativeView.TrailingAnchor.ConstraintEqualTo(cell.ContentView.TrailingAnchor),
            nativeView.TopAnchor.ConstraintEqualTo(cell.ContentView.TopAnchor),
            nativeView.BottomAnchor.ConstraintEqualTo(cell.ContentView.BottomAnchor)
        ]);

        // Height at priority 999 (below required 1000) so it informs auto-sizing
        // without conflicting with the top+bottom edge constraints.
        var heightConstraint = nativeView.HeightAnchor.ConstraintEqualTo((nfloat)measure.Height);
        heightConstraint.Priority = 999;
        heightConstraint.Active = true;

        return cell;
    }

    public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
    {
        OnTableRowSelected(indexPath);
    }

    public override nint RowsInSection(UITableView tableview, nint section)
    {
        return _items.Count;
    }

    public event EventHandler<TableRowSelectedEventArgs<object>> TableRowSelected;

    private void OnTableRowSelected(NSIndexPath itemIndexPath)
    {
        var item = _items[itemIndexPath.Row];
        TableRowSelected?.Invoke(this, new TableRowSelectedEventArgs<object>(item, itemIndexPath));
    }
}
