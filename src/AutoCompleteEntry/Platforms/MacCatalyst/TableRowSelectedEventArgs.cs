using Foundation;

namespace zoft.MauiExtensions.Controls.Platform;

internal class TableRowSelectedEventArgs<T> : EventArgs
{
    public TableRowSelectedEventArgs(T selectedItem, NSIndexPath selectedItemIndexPath)
    {
        SelectedItem = selectedItem;
        SelectedItemIndexPath = selectedItemIndexPath;
    }

    public T SelectedItem { get; }
    public NSIndexPath SelectedItemIndexPath { get; }
}