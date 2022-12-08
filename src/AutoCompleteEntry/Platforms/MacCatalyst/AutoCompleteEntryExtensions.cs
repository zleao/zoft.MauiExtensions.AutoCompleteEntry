using Foundation;
using Microsoft.Maui.Platform;
using UIKit;
using zoft.MauiExtensions.Controls.Platforms.iOS;
using zoft.MauiExtensions.Core.Extensions;

namespace zoft.MauiExtensions.Controls.Platforms.Extensions
{
    public static class AutoCompleteEntryExtensions
    {
        public static void UpdateText(this IOSAutoCompleteEntry iosAutoCompleteEntry, IAutoCompleteEntry autoCompleteEntry)
        {
            iosAutoCompleteEntry.Text = autoCompleteEntry.Text;
        }

        public static void UpdatePlaceholder(this IOSAutoCompleteEntry iosAutoCompleteEntry, IAutoCompleteEntry autoCompleteEntry, Color defaultPlaceholderColor = null)
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

        public static void UpdateIsTextPredictionEnabled(this IOSAutoCompleteEntry iosAutoCompleteEntry, IAutoCompleteEntry autoCompleteEntry)
        {
            if (autoCompleteEntry.IsTextPredictionEnabled)
            {
                iosAutoCompleteEntry.InputTextField.AutocorrectionType = UITextAutocorrectionType.Yes;
            }
            else
            {
                iosAutoCompleteEntry.InputTextField.AutocorrectionType = UITextAutocorrectionType.No;
            }
        }

        public static void UpdateMaxLength(this IOSAutoCompleteEntry iosAutoCompleteEntry, IAutoCompleteEntry autoCompleteEntry)
        {
            var newText = iosAutoCompleteEntry.InputTextField.AttributedText.TrimToMaxLength(autoCompleteEntry.MaxLength);
            if (newText != null && iosAutoCompleteEntry.InputTextField.AttributedText != newText)
            {
                iosAutoCompleteEntry.InputTextField.AttributedText = newText;
            }
        }

        public static void UpdateIsReadOnly(this IOSAutoCompleteEntry iosAutoCompleteEntry, IAutoCompleteEntry autoCompleteEntry)
        {
            iosAutoCompleteEntry.InputTextField.UserInteractionEnabled = !(autoCompleteEntry.IsReadOnly || autoCompleteEntry.InputTransparent);
        }

        public static void UpdateDisplayMemberPath(this IOSAutoCompleteEntry iosAutoCompleteEntry, IAutoCompleteEntry autoCompleteEntry)
        {
            iosAutoCompleteEntry.SetItems(autoCompleteEntry.ItemsSource,
                                          (o) => o.GetPropertyValueAsString(autoCompleteEntry?.DisplayMemberPath),
                                          (o) => o.GetPropertyValueAsString(autoCompleteEntry?.TextMemberPath));
        }

        public static void UpdateIsSuggestionListOpen(this IOSAutoCompleteEntry iosAutoCompleteEntry, IAutoCompleteEntry autoCompleteEntry)
        {
            iosAutoCompleteEntry.IsSuggestionListOpen = autoCompleteEntry.IsSuggestionListOpen;
        }

        public static void UpdateUpdateTextOnSelect(this IOSAutoCompleteEntry iosAutoCompleteEntry, IAutoCompleteEntry autoCompleteEntry)
        {
            iosAutoCompleteEntry.UpdateTextOnSelect = autoCompleteEntry.UpdateTextOnSelect;
        }

        public static void UpdateItemsSource(this IOSAutoCompleteEntry iosAutoCompleteEntry, IAutoCompleteEntry autoCompleteEntry)
        {
            iosAutoCompleteEntry.SetItems(autoCompleteEntry?.ItemsSource, 
                                          (o) => o.GetPropertyValueAsString(autoCompleteEntry?.DisplayMemberPath), 
                                          (o) => o.GetPropertyValueAsString(autoCompleteEntry?.TextMemberPath));
        }

        public static void UpdateSelectedSuggestion(this IOSAutoCompleteEntry iosAutoCompleteEntry, IAutoCompleteEntry autoCompleteEntry)
        {
            iosAutoCompleteEntry.Text = autoCompleteEntry.SelectedSuggestion.GetPropertyValueAsString(autoCompleteEntry.TextMemberPath);
        }
    }
}
