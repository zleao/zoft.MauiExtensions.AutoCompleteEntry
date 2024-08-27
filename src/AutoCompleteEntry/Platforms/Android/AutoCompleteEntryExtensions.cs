using Android.Text;
using zoft.MauiExtensions.Core.Extensions;

namespace zoft.MauiExtensions.Controls.Platform;

/// <summary>
/// Extensions for <see cref="AutoCompleteEntry"/>
/// </summary>
public static class AutoCompleteEntryExtensions
{
    /// <summary>
    /// Update the Text
    /// </summary>
    /// <param name="androidAutoCompleteEntry"></param>
    /// <param name="autoCompleteEntry"></param>
    public static void UpdateText(this AndroidAutoCompleteEntry androidAutoCompleteEntry, AutoCompleteEntry autoCompleteEntry)
    {
        if (androidAutoCompleteEntry.Text != autoCompleteEntry.Text)
        {
            androidAutoCompleteEntry.Text = autoCompleteEntry.Text;
            androidAutoCompleteEntry.SetSelection(androidAutoCompleteEntry.Text?.Length ?? 0);
        }
    }

    /// <summary>
    /// Update the IsTextPredictionEnabled
    /// </summary>
    /// <param name="androidAutoCompleteEntry"></param>
    /// <param name="autoCompleteEntry"></param>
    public static void UpdateIsTextPredictionEnabled(this AndroidAutoCompleteEntry androidAutoCompleteEntry, AutoCompleteEntry autoCompleteEntry)
    {
        if (autoCompleteEntry.IsTextPredictionEnabled)
            androidAutoCompleteEntry.InputType &= ~InputTypes.TextFlagNoSuggestions;
        else
            androidAutoCompleteEntry.InputType |= InputTypes.TextFlagNoSuggestions;
    }

        /// <summary>
        /// Update the DisplayMemberPath
        /// </summary>
        /// <param name="androidAutoCompleteEntry"></param>
        /// <param name="autoCompleteEntry"></param>
        public static void UpdateDisplayMemberPath(this AndroidAutoCompleteEntry androidAutoCompleteEntry, AutoCompleteEntry autoCompleteEntry)
        {
            androidAutoCompleteEntry.SetItems(autoCompleteEntry.ItemsSource,
                                              (o) => !string.IsNullOrEmpty(autoCompleteEntry?.DisplayMemberPath) ? o.GetPropertyValueAsString(autoCompleteEntry?.DisplayMemberPath) : o?.ToString(),
                                              (o) => !string.IsNullOrEmpty(autoCompleteEntry?.TextMemberPath) ? o.GetPropertyValueAsString(autoCompleteEntry?.TextMemberPath) : o?.ToString());
        }

    /// <summary>
    /// Update the IsSuggestionListOpen
    /// </summary>
    /// <param name="androidAutoCompleteEntry"></param>
    /// <param name="autoCompleteEntry"></param>
    public static void UpdateIsSuggestionListOpen(this AndroidAutoCompleteEntry androidAutoCompleteEntry, AutoCompleteEntry autoCompleteEntry)
    {
        androidAutoCompleteEntry.IsSuggestionListOpen = autoCompleteEntry.IsSuggestionListOpen;
    }

    /// <summary>
    /// Update the UpdateTextOnSelect
    /// </summary>
    /// <param name="androidAutoCompleteEntry"></param>
    /// <param name="autoCompleteEntry"></param>
    public static void UpdateUpdateTextOnSelect(this AndroidAutoCompleteEntry androidAutoCompleteEntry, AutoCompleteEntry autoCompleteEntry)
    {
        androidAutoCompleteEntry.UpdateTextOnSelect = autoCompleteEntry.UpdateTextOnSelect;
    }

        /// <summary>
        /// Update the ItemsSource
        /// </summary>
        /// <param name="androidAutoCompleteEntry"></param>
        /// <param name="autoCompleteEntry"></param>
        public static void UpdateItemsSource(this AndroidAutoCompleteEntry androidAutoCompleteEntry, AutoCompleteEntry autoCompleteEntry)
        {
            androidAutoCompleteEntry.SetItems(autoCompleteEntry?.ItemsSource, 
                                              (o) => !string.IsNullOrEmpty(autoCompleteEntry?.DisplayMemberPath) ? o.GetPropertyValueAsString(autoCompleteEntry?.DisplayMemberPath) : o?.ToString(),
                                              (o) => !string.IsNullOrEmpty(autoCompleteEntry?.TextMemberPath) ? o.GetPropertyValueAsString(autoCompleteEntry?.TextMemberPath) : o?.ToString());
        }

        /// <summary>
        /// Update the SelectedSuggestion
        /// </summary>
        /// <param name="androidAutoCompleteEntry"></param>
        /// <param name="autoCompleteEntry"></param>
        public static void UpdateSelectedSuggestion(this AndroidAutoCompleteEntry androidAutoCompleteEntry, AutoCompleteEntry autoCompleteEntry)
        {
            object o = autoCompleteEntry.SelectedSuggestion;
            androidAutoCompleteEntry.Text = !string.IsNullOrEmpty(autoCompleteEntry.TextMemberPath) ? o.GetPropertyValueAsString(autoCompleteEntry.TextMemberPath) : o?.ToString();
            androidAutoCompleteEntry.SetSelection(androidAutoCompleteEntry.Text?.Length ?? 0);
        }
    }
}