using zoft.MauiExtensions.Core.Extensions;

namespace zoft.MauiExtensions.Controls.Platforms.Extensions
{
    public static class AutoCompleteEntryExtensions
    {
        public static void UpdateTextMemberPath(this Microsoft.UI.Xaml.Controls.AutoSuggestBox platformControl, IAutoCompleteEntry autoCompleteEntry )
        {
            platformControl.TextMemberPath = autoCompleteEntry.TextMemberPath;
        }

        public static void UpdateDisplayMemberPath(this Microsoft.UI.Xaml.Controls.AutoSuggestBox platformControl, IAutoCompleteEntry autoCompleteEntry)
        {
            platformControl.DisplayMemberPath = autoCompleteEntry.DisplayMemberPath;
        }

        public static void UpdateIsSuggestionListOpen(this Microsoft.UI.Xaml.Controls.AutoSuggestBox platformControl, IAutoCompleteEntry autoCompleteEntry)
        {
            platformControl.IsSuggestionListOpen = autoCompleteEntry.IsSuggestionListOpen;
        }

        public static void UpdateUpdateTextOnSelect(this Microsoft.UI.Xaml.Controls.AutoSuggestBox platformControl, IAutoCompleteEntry autoCompleteEntry)
        {
            platformControl.UpdateTextOnSelect = autoCompleteEntry.UpdateTextOnSelect;
        }

        public static void UpdateItemsSource(this Microsoft.UI.Xaml.Controls.AutoSuggestBox platformControl, IAutoCompleteEntry autoCompleteEntry)
        {
            platformControl.ItemsSource = autoCompleteEntry.ItemsSource;
        }

        public static void UpdateSelectedSuggestion(this Microsoft.UI.Xaml.Controls.AutoSuggestBox platformControl, IAutoCompleteEntry autoCompleteEntry)
        {
            platformControl.Text = autoCompleteEntry.SelectedSuggestion.GetPropertyValueAsString(autoCompleteEntry.TextMemberPath);
        }
    }
}
