namespace zoft.MauiExtensions.Controls;

/// <summary>
/// Provides data for the <see cref="AutoCompleteEntry.AutoCompleteTextChanged"/> event.
/// </summary>
public enum AutoCompleteEntryTextChangeReason
{
    /// <summary>The user edited the text.</summary>
    UserInput = 0,

    /// <summary>The text was changed via code.</summary>
    ProgrammaticChange = 1,

    /// <summary>The user selected one of the items in the auto-suggestion box.</summary>
    SuggestionChosen = 2
}