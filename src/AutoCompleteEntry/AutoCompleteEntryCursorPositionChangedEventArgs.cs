namespace zoft.MauiExtensions.Controls;

/// <summary>
/// Provides data for cursor position changes in <see cref="AutoCompleteEntry"/>.
/// </summary>
public sealed class AutoCompleteEntryCursorPositionChangedEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AutoCompleteEntryCursorPositionChangedEventArgs"/> class.
    /// </summary>
    /// <param name="cursorPosition">The current cursor position.</param>
    internal AutoCompleteEntryCursorPositionChangedEventArgs(int cursorPosition)
    {
        CursorPosition = cursorPosition;
    }

    /// <summary>
    /// Gets the current cursor position.
    /// </summary>
    /// <value>The current cursor position.</value>
    public int CursorPosition { get; }
}