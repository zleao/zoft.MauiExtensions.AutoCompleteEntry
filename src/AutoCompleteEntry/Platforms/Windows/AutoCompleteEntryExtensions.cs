using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Platform;
using Microsoft.UI.Xaml.Controls;
using zoft.MauiExtensions.Core.Extensions;

namespace zoft.MauiExtensions.Controls.Platform
{
    public static class AutoCompleteEntryExtensions
    {
        private static readonly string[] _placeholderForegroundColorKeys =
        {
            "TextControlPlaceholderForeground",
            "TextControlPlaceholderForegroundPointerOver",
            "TextControlPlaceholderForegroundFocused",
            "TextControlPlaceholderForegroundDisabled"
        };

        public static void UpdateText(this AutoSuggestBox platformControl, AutoCompleteEntry autoCompleteEntry)
        {
            platformControl.Text = TextTransformUtilites.GetTransformedText(autoCompleteEntry.Text, autoCompleteEntry.TextTransform);
        }

        public static void UpdatePlaceholder(this AutoSuggestBox platformControl, AutoCompleteEntry autoCompleteEntry)
        {
            platformControl.PlaceholderText = autoCompleteEntry.Placeholder ?? string.Empty;
        }

        public static void UpdateVerticalTextAlignment(this AutoSuggestBox platformControl, AutoCompleteEntry autoCompleteEntry)
        {
            platformControl.VerticalContentAlignment = autoCompleteEntry.VerticalTextAlignment.ToPlatformVerticalAlignment();
        }

        public static void UpdatePlaceholderColor(this AutoSuggestBox platformControl, AutoCompleteEntry autoCompleteEntry)
        {
            UpdateColors(platformControl.Resources, 
                        _placeholderForegroundColorKeys,
                        autoCompleteEntry.PlaceholderColor?.ToPlatform());
        }

        public static void UpdateHorizontalTextAlignment(this AutoSuggestBox platformControl, AutoCompleteEntry autoCompleteEntry)
        {
            platformControl.HorizontalContentAlignment = autoCompleteEntry.HorizontalTextAlignment.ToPlatformHorizontalAlignment();
        }

        public static void UpdateMaxLength(this AutoSuggestBox platformControl, AutoCompleteEntry autoCompleteEntry)
        {
            var maxLength = autoCompleteEntry.MaxLength;

            if (maxLength == -1)
                maxLength = int.MaxValue;

            if (maxLength == 0)
                MauiAutoSuggestBox.SetIsReadOnly(platformControl, true);
            else
                MauiAutoSuggestBox.SetIsReadOnly(platformControl, autoCompleteEntry.IsReadOnly);

            var currentControlText = platformControl.Text;

            if (currentControlText.Length > maxLength)
                platformControl.Text = currentControlText.Substring(0, maxLength);
        }

        public static void UpdateIsReadOnly(this AutoSuggestBox platformControl, AutoCompleteEntry autoCompleteEntry)
        {
            MauiAutoSuggestBox.SetIsReadOnly(platformControl, autoCompleteEntry.IsReadOnly);
        }

        public static void UpdateTextMemberPath(this AutoSuggestBox platformControl, AutoCompleteEntry autoCompleteEntry)
        {
            platformControl.TextMemberPath = autoCompleteEntry.TextMemberPath;
        }

        public static void UpdateDisplayMemberPath(this AutoSuggestBox platformControl, AutoCompleteEntry autoCompleteEntry)
        {
            platformControl.DisplayMemberPath = autoCompleteEntry.DisplayMemberPath;
        }

        public static void UpdateIsSuggestionListOpen(this AutoSuggestBox platformControl, AutoCompleteEntry autoCompleteEntry)
        {
            platformControl.IsSuggestionListOpen = autoCompleteEntry.IsSuggestionListOpen;
        }

        public static void UpdateUpdateTextOnSelect(this AutoSuggestBox platformControl, AutoCompleteEntry autoCompleteEntry)
        {
            platformControl.UpdateTextOnSelect = autoCompleteEntry.UpdateTextOnSelect;
        }

        public static void UpdateItemsSource(this AutoSuggestBox platformControl, AutoCompleteEntry autoCompleteEntry)
        {
            platformControl.ItemsSource = autoCompleteEntry.ItemsSource;
        }

        public static void UpdateSelectedSuggestion(this AutoSuggestBox platformControl, AutoCompleteEntry autoCompleteEntry)
        {
            platformControl.Text = autoCompleteEntry.SelectedSuggestion.GetPropertyValueAsString(autoCompleteEntry.TextMemberPath);
        }

        private static void UpdateColors(Microsoft.UI.Xaml.ResourceDictionary resource, string[] keys, Microsoft.UI.Xaml.Media.Brush brush)
        {
            if (brush is null)
            {
                resource.RemoveKeys(keys);
            }
            else
            {
                resource.SetValueForAllKey(keys, brush);
            }
        }

        internal static void RemoveKeys(this Microsoft.UI.Xaml.ResourceDictionary resources, IEnumerable<string> keys)
        {
            foreach (string key in keys)
            {
                resources.Remove(key);
            }
        }

        internal static void SetValueForAllKey(this Microsoft.UI.Xaml.ResourceDictionary resources, IEnumerable<string> keys, object? value)
        {
            foreach (string key in keys)
            {
                resources[key] = value;
            }
        }
    }
}
