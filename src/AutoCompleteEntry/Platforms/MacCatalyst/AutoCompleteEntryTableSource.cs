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
    private readonly DataTemplate? _itemTemplate;
    private readonly IMauiContext _mauiContext;
    private readonly Page _listViewContainer;

    private DataTemplate? _defaultItemTemplate;
    internal DataTemplate DefaultItemTemplate
    {
        get
        {
            _defaultItemTemplate ??= new DataTemplate(() =>
                {
                    var label = new Label();

                    label.SetBinding(Label.TextProperty, _displayMemberPath ?? ".");
                    label.HorizontalTextAlignment = Microsoft.Maui.TextAlignment.Center;
                    label.VerticalTextAlignment = Microsoft.Maui.TextAlignment.Center;
                    label.MinimumHeightRequest = 44;

                    return label;
                });

            return _defaultItemTemplate;
        }
    }

    public AutoCompleteEntryTableSource(UITableView view, IList items, string displayMemberPath, DataTemplate? itemTemplate, IMauiContext mauiContext)
    {
        _view = view;
        _items = items;
        _displayMemberPath = displayMemberPath;
        _itemTemplate = itemTemplate;
        _mauiContext = mauiContext;
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

    private void NotifiableItems_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
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

        // Resolve the concrete DataTemplate (returns self for a plain DataTemplate,
        // selects one when templateToUse is a DataTemplateSelector).
        var resolvedTemplate = templateToUse.SelectDataTemplate(item, _listViewContainer)
            ?? throw new InvalidOperationException(
                $"DataTemplateSelector '{templateToUse.GetType().FullName}' returned null for item '{item}'.");
        var cellId = ((IDataTemplateController)resolvedTemplate).IdString;

        if (tableView.DequeueReusableCell(cellId) is not AutoCompleteCell cell)
        {
            // First time for this template type — create the MAUI view and its handler
            cell = new AutoCompleteCell(cellId);

            var templateView = resolvedTemplate.CreateContent() as View
                ?? throw new InvalidOperationException(
                    $"DataTemplate did not produce a View for item '{item}'.");
            templateView.BindingContext = item;
            cell.MauiView = templateView;

            var nativeView = templateView.ToPlatform(_mauiContext);
            nativeView.TranslatesAutoresizingMaskIntoConstraints = false;
            cell.ContentView.AddSubview(nativeView);

            // Pin to all 4 edges — done once per cell, never repeated on recycle
            NSLayoutConstraint.ActivateConstraints(new[]
            {
                nativeView.LeadingAnchor.ConstraintEqualTo(cell.ContentView.LeadingAnchor),
                nativeView.TrailingAnchor.ConstraintEqualTo(cell.ContentView.TrailingAnchor),
                nativeView.TopAnchor.ConstraintEqualTo(cell.ContentView.TopAnchor),
                nativeView.BottomAnchor.ConstraintEqualTo(cell.ContentView.BottomAnchor)
            });

            // Stash a reusable height constraint at priority 999 so it informs
            // AutomaticDimension without conflicting with the edge constraints.
            // We update only its Constant on recycle — no new constraint objects.
            cell.HeightConstraint = nativeView.HeightAnchor.ConstraintEqualTo(44f);
            cell.HeightConstraint.Priority = 999;
            cell.HeightConstraint.Active = true;
        }
        else
        {
            // Recycled cell — skip handler creation, just swap the data
            cell.MauiView!.BindingContext = item;
        }

        // Measure on every GetCell call because recycled rows may bind to data of a different height
        var widthConstraint = tableView.Bounds.Width > 0 ? (double)tableView.Bounds.Width : double.PositiveInfinity;
        var measure = ((IView)cell.MauiView!).Measure(widthConstraint, double.PositiveInfinity);
        cell.HeightConstraint!.Constant = (nfloat)System.Math.Max(measure.Height, 44);

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

/// <summary>
/// Custom cell that holds MAUI view and height constraint references across recycles,
/// avoiding repeated handler creation and constraint leaks.
/// </summary>
internal sealed class AutoCompleteCell : UITableViewCell
{
    internal View? MauiView { get; set; }
    internal NSLayoutConstraint? HeightConstraint { get; set; }

    internal AutoCompleteCell(string cellId) : base(UITableViewCellStyle.Default, cellId) { }
}
