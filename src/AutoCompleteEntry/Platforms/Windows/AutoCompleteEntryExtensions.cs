using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Platform;
using Microsoft.UI.Xaml.Controls;
using Uno.UI.Extensions;
using zoft.MauiExtensions.Core.Extensions;

namespace zoft.MauiExtensions.Controls.Platform;

/// <summary>
/// Extensions for <see cref="AutoCompleteEntry"/>
/// </summary>
public static class AutoCompleteEntryExtensions
{
    private static readonly string[] _placeholderForegroundColorKeys =
    {
        "TextControlPlaceholderForeground",
        "TextControlPlaceholderForegroundPointerOver",
        "TextControlPlaceholderForegroundFocused",
        "TextControlPlaceholderForegroundDisabled"
    };

    public static void UpdateClearButtonVisibility(this AutoSuggestBox platformControl, AutoCompleteEntry autoCompleteEntry)
    {
        platformControl.FindFirstDescendant<TextBox>()?.UpdateClearButtonVisibility(autoCompleteEntry);
    }

    /// <summary>
    /// Update the DisplayMemberPath
    /// </summary>
    /// <param name="platformControl"></param>
    /// <param name="autoCompleteEntry"></param>
    public static void UpdateDisplayMemberPath(this AutoSuggestBox platformControl, AutoCompleteEntry autoCompleteEntry)
    {
        platformControl.DisplayMemberPath = autoCompleteEntry.DisplayMemberPath;
    }

    /// <summary>
    /// Update the HorizontalTextAlignment
    /// </summary>
    /// <param name="platformControl"></param>
    /// <param name="autoCompleteEntry"></param>
    public static void UpdateHorizontalTextAlignment(this AutoSuggestBox platformControl, AutoCompleteEntry autoCompleteEntry)
    {
        platformControl.HorizontalContentAlignment = autoCompleteEntry.HorizontalTextAlignment.ToPlatformHorizontalAlignment();
    }

    /// <summary>
    /// Update the IsReadOnly
    /// </summary>
    /// <param name="platformControl"></param>
    /// <param name="autoCompleteEntry"></param>
    public static void UpdateIsReadOnly(this AutoSuggestBox platformControl, AutoCompleteEntry autoCompleteEntry)
    {
        MauiAutoSuggestBox.SetIsReadOnly(platformControl, autoCompleteEntry.IsReadOnly);
    }

    /// <summary>
    /// Update the IsSuggestionListOpen
    /// </summary>
    /// <param name="platformControl"></param>
    /// <param name="autoCompleteEntry"></param>
    public static void UpdateIsSuggestionListOpen(this AutoSuggestBox platformControl, AutoCompleteEntry autoCompleteEntry)
    {
        platformControl.IsSuggestionListOpen = autoCompleteEntry.IsSuggestionListOpen;
    }

    /// <summary>
    /// Update the ItemsSource
    /// </summary>
    /// <param name="platformControl"></param>
    /// <param name="autoCompleteEntry"></param>
    public static void UpdateItemsSource(this AutoSuggestBox platformControl, AutoCompleteEntry autoCompleteEntry)
    {
        platformControl.ItemsSource = autoCompleteEntry.ItemsSource;
    }

    /// <summary>
    /// Update the MaxLength
    /// </summary>
    /// <param name="platformControl"></param>
    /// <param name="autoCompleteEntry"></param>
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

    /// <summary>
    /// Update the Placeholder
    /// </summary>
    /// <param name="platformControl"></param>
    /// <param name="autoCompleteEntry"></param>
    public static void UpdatePlaceholder(this AutoSuggestBox platformControl, AutoCompleteEntry autoCompleteEntry)
    {
        platformControl.PlaceholderText = autoCompleteEntry.Placeholder ?? string.Empty;
    }

    /// <summary>
    /// Update the PlaceholderColor
    /// </summary>
    /// <param name="platformControl"></param>
    /// <param name="autoCompleteEntry"></param>
    public static void UpdatePlaceholderColor(this AutoSuggestBox platformControl, AutoCompleteEntry autoCompleteEntry)
    {
        UpdateColors(platformControl.Resources,
                    _placeholderForegroundColorKeys,
                    autoCompleteEntry.PlaceholderColor?.ToPlatform());
    }

    /// <summary>
    /// Update the ReturnType
    /// </summary>
    /// <param name="platformControl"></param>
    /// <param name="entry"></param>
    public static void UpdateReturnType(this AutoSuggestBox platformControl, IEntry entry)
    {
        platformControl.FindFirstDescendant<TextBox>()?.UpdateReturnType(entry);
    }

    /// <summary>
    /// Update the SelectedSuggestion
    /// </summary>
    /// <param name="platformControl"></param>
    /// <param name="autoCompleteEntry"></param>
    public static void UpdateSelectedSuggestion(this AutoSuggestBox platformControl, AutoCompleteEntry autoCompleteEntry)
    {
        object o = autoCompleteEntry.SelectedSuggestion;
        platformControl.Text = !string.IsNullOrEmpty(autoCompleteEntry.TextMemberPath) ? o.GetPropertyValueAsString(autoCompleteEntry.TextMemberPath) : o?.ToString();
    }

    /// <summary>
    /// Update the Text
    /// </summary>
    /// <param name="platformControl"></param>
    /// <param name="autoCompleteEntry"></param>
    public static void UpdateText(this AutoSuggestBox platformControl, AutoCompleteEntry autoCompleteEntry)
    {
        var text = TextTransformUtilites.GetTransformedText(autoCompleteEntry.Text, autoCompleteEntry.TextTransform);
        if (platformControl.Text != text)
        {
            platformControl.Text = text;
        }
    }

    /// <summary>
    /// Update the TextMemberPath
    /// </summary>
    /// <param name="platformControl"></param>
    /// <param name="autoCompleteEntry"></param>
    public static void UpdateTextMemberPath(this AutoSuggestBox platformControl, AutoCompleteEntry autoCompleteEntry)
    {
        platformControl.TextMemberPath = autoCompleteEntry.TextMemberPath;
    }

    /// <summary>
    /// Update the UpdateTextOnSelect
    /// </summary>
    /// <param name="platformControl"></param>
    /// <param name="autoCompleteEntry"></param>
    public static void UpdateUpdateTextOnSelect(this AutoSuggestBox platformControl, AutoCompleteEntry autoCompleteEntry)
    {
        platformControl.UpdateTextOnSelect = autoCompleteEntry.UpdateTextOnSelect;
    }

    /// <summary>
    /// Update the VerticalTextAlignment
    /// </summary>
    /// <param name="platformControl"></param>
    /// <param name="autoCompleteEntry"></param>
    public static void UpdateVerticalTextAlignment(this AutoSuggestBox platformControl, AutoCompleteEntry autoCompleteEntry)
    {
        platformControl.VerticalContentAlignment = autoCompleteEntry.VerticalTextAlignment.ToPlatformVerticalAlignment();
    }

    /// <summary>
    /// Update the ShowBottomBorder
    /// </summary>
    /// <param name="iosAutoCompleteEntry"></param>
    /// <param name="autoCompleteEntry"></param>
    public static void UpdateShowBottomBorder(this AutoSuggestBox platformView, AutoCompleteEntry virtualView)
    {
        //TODO: Implement for Windows
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

    internal static void SetValueForAllKey(this Microsoft.UI.Xaml.ResourceDictionary resources, IEnumerable<string> keys, object value)
    {
        foreach (string key in keys)
        {
            resources[key] = value;
        }
    }
}
