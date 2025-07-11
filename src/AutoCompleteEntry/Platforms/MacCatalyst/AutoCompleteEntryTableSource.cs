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
                    label.MinimumHeightRequest = 35;

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

        // Convert MAUI view to native iOS view with proper MauiContext
        var nativeView = templateView.ToPlatform(_mauiContext);

        // Clear previous content to avoid overlapping
        foreach (var subview in cell.ContentView.Subviews)
        {
            subview.RemoveFromSuperview();
        }

        nativeView.Frame = cell.ContentView.Bounds;
        nativeView.AutoresizingMask = UIViewAutoresizing.FlexibleDimensions;

        cell.ContentView.AddSubview(nativeView);

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

    public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath)
    {
        return 35f;
    }

    public event EventHandler<TableRowSelectedEventArgs<object>> TableRowSelected;

    private void OnTableRowSelected(NSIndexPath itemIndexPath)
    {
        var item = _items[itemIndexPath.Row];
        TableRowSelected?.Invoke(this, new TableRowSelectedEventArgs<object>(item, itemIndexPath));
    }
}