using Android.Text;
using zoft.MauiExtensions.Core.Extensions;

namespace zoft.MauiExtensions.Controls.Platform
{
    public static class AutoCompleteEntryExtensions
    {
        public static void UpdateText(this AndroidAutoCompleteEntry androidAutoCompleteEntry, AutoCompleteEntry autoCompleteEntry)
        {
            androidAutoCompleteEntry.Text = autoCompleteEntry.Text;
            androidAutoCompleteEntry.SetSelection(androidAutoCompleteEntry.Text?.Length ?? 0);
        }

        public static void UpdateIsTextPredictionEnabled(this AndroidAutoCompleteEntry androidAutoCompleteEntry, AutoCompleteEntry autoCompleteEntry)
        {
            if (autoCompleteEntry.IsTextPredictionEnabled)
                androidAutoCompleteEntry.InputType &= ~InputTypes.TextFlagNoSuggestions;
            else
                androidAutoCompleteEntry.InputType |= InputTypes.TextFlagNoSuggestions;
        }
        
        public static void UpdateDisplayMemberPath(this AndroidAutoCompleteEntry androidAutoCompleteEntry, AutoCompleteEntry autoCompleteEntry)
        {
            androidAutoCompleteEntry.SetItems(autoCompleteEntry.ItemsSource,
                                              (o) => o.GetPropertyValueAsString(autoCompleteEntry?.DisplayMemberPath),
                                              (o) => o.GetPropertyValueAsString(autoCompleteEntry?.TextMemberPath));
        }

        public static void UpdateIsSuggestionListOpen(this AndroidAutoCompleteEntry androidAutoCompleteEntry, AutoCompleteEntry autoCompleteEntry)
        {
            androidAutoCompleteEntry.IsSuggestionListOpen = autoCompleteEntry.IsSuggestionListOpen;
        }

        public static void UpdateUpdateTextOnSelect(this AndroidAutoCompleteEntry androidAutoCompleteEntry, AutoCompleteEntry autoCompleteEntry)
        {
            androidAutoCompleteEntry.UpdateTextOnSelect = autoCompleteEntry.UpdateTextOnSelect;
        }

        public static void UpdateItemsSource(this AndroidAutoCompleteEntry androidAutoCompleteEntry, AutoCompleteEntry autoCompleteEntry)
        {
            androidAutoCompleteEntry.SetItems(autoCompleteEntry?.ItemsSource, 
                                              (o) => o.GetPropertyValueAsString(autoCompleteEntry?.DisplayMemberPath), 
                                              (o) => o.GetPropertyValueAsString(autoCompleteEntry?.TextMemberPath));
        }

        public static void UpdateSelectedSuggestion(this AndroidAutoCompleteEntry androidAutoCompleteEntry, AutoCompleteEntry autoCompleteEntry)
        {
            androidAutoCompleteEntry.Text = autoCompleteEntry.SelectedSuggestion.GetPropertyValueAsString(autoCompleteEntry.TextMemberPath);
            androidAutoCompleteEntry.SetSelection(androidAutoCompleteEntry.Text?.Length ?? 0);
        }
    }
}
