namespace zoft.MauiExtensions.Controls;

public sealed class AutoCompleteEntryCursorPositionChangedEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AutoCompleteEntryCursorPositionChangedEventArgs"/> class.
    /// </summary>
    /// <param name="cursorPosition"></param>
    internal AutoCompleteEntryCursorPositionChangedEventArgs(int cursorPosition)
    {
        CursorPosition = cursorPosition;
    }

    /// <summary>
    /// Gets a reference to the selected item.
    /// </summary>
    /// <value>A reference to the selected item.</value>
    public int CursorPosition { get; }
}