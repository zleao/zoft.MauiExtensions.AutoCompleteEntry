namespace zoft.MauiExtensions.Controls;

/// <summary>
/// Provides data for the TextChanged event.
/// </summary>
public sealed class AutoCompleteEntryTextChangedEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AutoCompleteEntryTextChangedEventArgs"/> class.
    /// </summary>
    /// <param name="reason"></param>
    internal AutoCompleteEntryTextChangedEventArgs(AutoCompleteEntryTextChangeReason reason)
    {
        Reason = reason;
    }

    /// <summary>
    /// Gets or sets a value that indicates the reason for the text changing in the <see cref="AutoCompleteEntry"/>.
    /// </summary>
    /// <value>The reason for the text changing in the <see cref="AutoCompleteEntry"/>.</value>
    public AutoCompleteEntryTextChangeReason Reason { get; }
}