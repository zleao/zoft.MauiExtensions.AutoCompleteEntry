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
    /// Update the DisplayCompleteEntry
    /// </summary>
    /// <param name="iosAutoCompleteEntry"></param>
    /// <param name="autoCompleteEntry"></param>
    public static void UpdateDisplayMemberPath(this IOSAutoCompleteEntry iosAutoCompleteEntry, AutoCompleteEntry autoCompleteEntry, IMauiContext mauiContext)
    {
        iosAutoCompleteEntry.SetItems(autoCompleteEntry.ItemsSource,
                                      autoCompleteEntry?.DisplayMemberPath,
                                      (o) => !string.IsNullOrEmpty(autoCompleteEntry?.TextMemberPath) ? o.GetPropertyValueAsString(autoCompleteEntry?.TextMemberPath) : o?.ToString(),
                                      mauiContext);
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
    /// Update the IsSuggestionListOpen
    /// </summary>
    /// <param name="iosAutoCompleteEntry"></param>
    /// <param name="autoCompleteEntry"></param>
    public static void UpdateIsSuggestionListOpen(this IOSAutoCompleteEntry iosAutoCompleteEntry, AutoCompleteEntry autoCompleteEntry)
    {
        iosAutoCompleteEntry.IsSuggestionListOpen = autoCompleteEntry.IsSuggestionListOpen;
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
    /// Update the ItemsSource
    /// </summary>
    /// <param name="iosAutoCompleteEntry"></param>
    /// <param name="autoCompleteEntry"></param>
    public static void UpdateItemsSource(this IOSAutoCompleteEntry iosAutoCompleteEntry, AutoCompleteEntry autoCompleteEntry, IMauiContext mauiContext)
    {
        iosAutoCompleteEntry.SetItems(autoCompleteEntry?.ItemsSource,
                                      autoCompleteEntry?.DisplayMemberPath,
                                      (o) => !string.IsNullOrEmpty(autoCompleteEntry?.TextMemberPath) ? o.GetPropertyValueAsString(autoCompleteEntry?.TextMemberPath) : o?.ToString(),
                                      mauiContext);
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
    /// Update the ReturnType
    /// </summary>
    /// <param name="iosAutoCompleteEntry"></param>
    /// <param name="entry"></param>
    public static void UpdateReturnType(this IOSAutoCompleteEntry iosAutoCompleteEntry, IEntry entry)
    {
        iosAutoCompleteEntry.InputTextField.UpdateReturnType(entry);
    }

    /// <summary>
    /// Update the SelectedSuggestion
    /// </summary>
    /// <param name="iosAutoCompleteEntry"></param>
    /// <param name="autoCompleteEntry"></param>
    public static void UpdateSelectedSuggestion(this IOSAutoCompleteEntry iosAutoCompleteEntry, AutoCompleteEntry autoCompleteEntry)
    {
        if (autoCompleteEntry.SelectedSuggestion is null)
        {
            return;
        }

        iosAutoCompleteEntry.Text = 
            !string.IsNullOrEmpty(autoCompleteEntry.TextMemberPath) ?
            autoCompleteEntry.SelectedSuggestion.GetPropertyValueAsString(autoCompleteEntry.TextMemberPath) 
            :
            autoCompleteEntry.SelectedSuggestion.ToString();
    }

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
    /// Update the UpdateTextOnSelect
    /// </summary>
    /// <param name="iosAutoCompleteEntry"></param>
    /// <param name="autoCompleteEntry"></param>
    public static void UpdateUpdateTextOnSelect(this IOSAutoCompleteEntry iosAutoCompleteEntry, AutoCompleteEntry autoCompleteEntry)
    {
        iosAutoCompleteEntry.UpdateTextOnSelect = autoCompleteEntry.UpdateTextOnSelect;
    }

    /// <summary>
    /// Update the ShowBottomBorder
    /// </summary>
    /// <param name="iosAutoCompleteEntry"></param>
    /// <param name="autoCompleteEntry"></param>
    public static void UpdateShowBottomBorder(this IOSAutoCompleteEntry iosAutoCompleteEntry, AutoCompleteEntry autoCompleteEntry)
    {
        iosAutoCompleteEntry.ShowBottomBorder = autoCompleteEntry.ShowBottomBorder;
    }

    /// <summary>
    /// Update the ItemTemplate
    /// </summary>
    /// <param name="platformView"></param>
    /// <param name="virtualView"></param>
    public static void UpdateItemTemplate(this IOSAutoCompleteEntry iosAutoCompleteEntry, AutoCompleteEntry autoCompleteEntry)
    {
        iosAutoCompleteEntry.ItemTemplate = autoCompleteEntry.ItemTemplate;
    }
}
