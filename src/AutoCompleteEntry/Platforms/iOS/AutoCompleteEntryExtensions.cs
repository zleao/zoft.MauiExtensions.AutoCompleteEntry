using Foundation;
using Microsoft.Maui.Platform;
using UIKit;
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
    /// <param name="iosAutoCompleteEntry"></param>
    /// <param name="autoCompleteEntry"></param>
    public static void UpdateText(this IOSAutoCompleteEntry iosAutoCompleteEntry, AutoCompleteEntry autoCompleteEntry)
    {
        if (iosAutoCompleteEntry.Text != autoCompleteEntry.Text)
        {
            iosAutoCompleteEntry.Text = autoCompleteEntry.Text;
        }
    }

    /// <summary>
    /// Update the Placeholder
    /// </summary>
    /// <param name="iosAutoCompleteEntry"></param>
    /// <param name="autoCompleteEntry"></param>
    /// <param name="defaultPlaceholderColor"></param>
    public static void UpdatePlaceholder(this IOSAutoCompleteEntry iosAutoCompleteEntry, AutoCompleteEntry autoCompleteEntry, Color defaultPlaceholderColor = null)
    {
        var placeholder = autoCompleteEntry.Placeholder;
        if (placeholder == null)
        {
            iosAutoCompleteEntry.InputTextField.AttributedPlaceholder = null;
            return;
        }

        var placeholderColor = autoCompleteEntry.PlaceholderColor;
        var foregroundColor = placeholderColor ?? defaultPlaceholderColor;

        iosAutoCompleteEntry.InputTextField.AttributedPlaceholder = foregroundColor == null
            ? new NSAttributedString(placeholder)
            : new NSAttributedString(str: placeholder, foregroundColor: foregroundColor.ToPlatform());

        iosAutoCompleteEntry.InputTextField.AttributedPlaceholder.WithCharacterSpacing(autoCompleteEntry.CharacterSpacing);
    }

    /// <summary>
    /// Update the IsTextPredictionEnabled
    /// </summary>
    /// <param name="iosAutoCompleteEntry"></param>
    /// <param name="autoCompleteEntry"></param>
    public static void UpdateIsTextPredictionEnabled(this IOSAutoCompleteEntry iosAutoCompleteEntry, AutoCompleteEntry autoCompleteEntry)
    {
        iosAutoCompleteEntry.InputTextField.AutocorrectionType = autoCompleteEntry.IsTextPredictionEnabled ? UITextAutocorrectionType.Yes : UITextAutocorrectionType.No;
    }

    /// <summary>
    /// Update the MaxLength
    /// </summary>
    /// <param name="iosAutoCompleteEntry"></param>
    /// <param name="autoCompleteEntry"></param>
    public static void UpdateMaxLength(this IOSAutoCompleteEntry iosAutoCompleteEntry, AutoCompleteEntry autoCompleteEntry)
    {
        var newText = iosAutoCompleteEntry.InputTextField.AttributedText.TrimToMaxLength(autoCompleteEntry.MaxLength);
        if (newText != null && iosAutoCompleteEntry.InputTextField.AttributedText != null && !iosAutoCompleteEntry.InputTextField.AttributedText.Equals(newText))
        {
            iosAutoCompleteEntry.InputTextField.AttributedText = newText;
        }
    }

    /// <summary>
    /// Update the IsReadOnly
    /// </summary>
    /// <param name="iosAutoCompleteEntry"></param>
    /// <param name="autoCompleteEntry"></param>
    public static void UpdateIsReadOnly(this IOSAutoCompleteEntry iosAutoCompleteEntry, AutoCompleteEntry autoCompleteEntry)
    {
        iosAutoCompleteEntry.InputTextField.UserInteractionEnabled = !(autoCompleteEntry.IsReadOnly || autoCompleteEntry.InputTransparent);
    }

        /// <summary>
        /// Update the DisplayCompleteEntry
        /// </summary>
        /// <param name="iosAutoCompleteEntry"></param>
        /// <param name="autoCompleteEntry"></param>
        public static void UpdateDisplayMemberPath(this IOSAutoCompleteEntry iosAutoCompleteEntry, AutoCompleteEntry autoCompleteEntry)
        {
            iosAutoCompleteEntry.SetItems(autoCompleteEntry.ItemsSource,
                                          (o) => !string.IsNullOrEmpty(autoCompleteEntry?.DisplayMemberPath) ? o.GetPropertyValueAsString(autoCompleteEntry?.DisplayMemberPath) : o?.ToString(),
                                          (o) => !string.IsNullOrEmpty(autoCompleteEntry?.TextMemberPath) ? o.GetPropertyValueAsString(autoCompleteEntry?.TextMemberPath) : o?.ToString());
        }

    /// <summary>
    /// Update the IsSuggestionListOpen
    /// </summary>
    /// <param name="iosAutoCompleteEntry"></param>
    /// <param name="autoCompleteEntry"></param>
    public static void UpdateIsSuggestionListOpen(this IOSAutoCompleteEntry iosAutoCompleteEntry, AutoCompleteEntry autoCompleteEntry)
    {
        iosAutoCompleteEntry.IsSuggestionListOpen = autoCompleteEntry.IsSuggestionListOpen;
    }

    /// <summary>
    /// Update the UpdateTextOnSelect
    /// </summary>
    /// <param name="iosAutoCompleteEntry"></param>
    /// <param name="autoCompleteEntry"></param>
    public static void UpdateUpdateTextOnSelect(this IOSAutoCompleteEntry iosAutoCompleteEntry, AutoCompleteEntry autoCompleteEntry)
    {
        iosAutoCompleteEntry.UpdateTextOnSelect = autoCompleteEntry.UpdateTextOnSelect;
    }

        /// <summary>
        /// Update the ItemsSource
        /// </summary>
        /// <param name="iosAutoCompleteEntry"></param>
        /// <param name="autoCompleteEntry"></param>
        public static void UpdateItemsSource(this IOSAutoCompleteEntry iosAutoCompleteEntry, AutoCompleteEntry autoCompleteEntry)
        {
            iosAutoCompleteEntry.SetItems(autoCompleteEntry?.ItemsSource,
                                          (o) => !string.IsNullOrEmpty(autoCompleteEntry?.DisplayMemberPath) ? o.GetPropertyValueAsString(autoCompleteEntry?.DisplayMemberPath) : o?.ToString(),
                                          (o) => !string.IsNullOrEmpty(autoCompleteEntry?.TextMemberPath) ? o.GetPropertyValueAsString(autoCompleteEntry?.TextMemberPath) : o?.ToString());
        }

        /// <summary>
        /// Update the SelectedSuggestion
        /// </summary>
        /// <param name="iosAutoCompleteEntry"></param>
        /// <param name="autoCompleteEntry"></param>
        public static void UpdateSelectedSuggestion(this IOSAutoCompleteEntry iosAutoCompleteEntry, AutoCompleteEntry autoCompleteEntry)
        {
            object o = autoCompleteEntry.SelectedSuggestion;
            iosAutoCompleteEntry.Text = !string.IsNullOrEmpty(autoCompleteEntry.TextMemberPath) ? o.GetPropertyValueAsString(autoCompleteEntry.TextMemberPath) : o?.ToString();
        }
    }
}