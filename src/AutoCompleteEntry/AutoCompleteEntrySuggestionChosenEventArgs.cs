namespace zoft.MauiExtensions.Controls;

/// <summary>
/// Provides data for the <see cref="AutoCompleteEntry.SuggestionChosen"/> event.
/// </summary>
public sealed class AutoCompleteEntrySuggestionChosenEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AutoCompleteEntrySuggestionChosenEventArgs"/> class.
    /// </summary>
    /// <param name="selectedItem"></param>
    internal AutoCompleteEntrySuggestionChosenEventArgs(object? selectedItem)
        : this(selectedItem, true) { }

    internal AutoCompleteEntrySuggestionChosenEventArgs(object? selectedItem, bool isSelected)
    {
        SelectedItem = selectedItem;
        IsSelected = isSelected;
    }

    /// <summary>
    /// Gets a reference to the selected item.
    /// </summary>
    /// <value>A reference to the selected item.</value>
    public object? SelectedItem
    {
        get;
    }

    /// <summary>Whether the activated suggestion is selected after this interaction.</summary>
    public bool IsSelected
    {
        get;
    }
}
