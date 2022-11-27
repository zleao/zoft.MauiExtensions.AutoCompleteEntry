using Android.Text;
using Android.Widget;
using Microsoft.Maui.Platform;
using zoft.MauiExtensions.Controls.Platforms.Android;
using zoft.MauiExtensions.Core.Extensions;

namespace zoft.MauiExtensions.Controls.Platforms.Extensions
{
    public static class AutoCompleteEntryExtensions
    {
        public static void UpdateText(this AndroidAutoCompleteEntry androidAutoCompleteEntry, IAutoCompleteEntry autoCompleteEntry)
        {
            androidAutoCompleteEntry.Text = autoCompleteEntry.Text;
            androidAutoCompleteEntry.SetSelection(androidAutoCompleteEntry.Text?.Length ?? 0);
        }

        public static void UpdateIsTextPredictionEnabled(this AndroidAutoCompleteEntry androidAutoCompleteEntry, IAutoCompleteEntry autoCompleteEntry)
        {
            if (autoCompleteEntry.IsTextPredictionEnabled)
                androidAutoCompleteEntry.InputType &= ~InputTypes.TextFlagNoSuggestions;
            else
                androidAutoCompleteEntry.InputType |= InputTypes.TextFlagNoSuggestions;
        }

        public static void UpdateCancelButtonColor(this AndroidAutoCompleteEntry androidAutoCompleteEntry, IAutoCompleteEntry autoCompleteEntry)
        {
            if (androidAutoCompleteEntry.Resources == null)
                return;

            var searchCloseButtonIdentifier = Resource.Id.search_close_btn;

            if (searchCloseButtonIdentifier > 0)
            {
                var image = androidAutoCompleteEntry.FindViewById<ImageView>(searchCloseButtonIdentifier);

                if (image != null && image.Drawable != null)
                {
                    if (autoCompleteEntry.CancelButtonColor != null)
                        image.Drawable.SetColorFilter(autoCompleteEntry.CancelButtonColor, FilterMode.SrcIn);
                    else
                        image.Drawable.ClearColorFilter();
                }
            }
        }

        public static void UpdateDisplayMemberPath(this AndroidAutoCompleteEntry androidAutoCompleteEntry, IAutoCompleteEntry autoCompleteEntry)
        {
            androidAutoCompleteEntry.SetItems(autoCompleteEntry.ItemsSource?.OfType<object>(),
                                              (o) => o.GetPropertyValueAsString(autoCompleteEntry?.DisplayMemberPath),
                                              (o) => o.GetPropertyValueAsString(autoCompleteEntry?.TextMemberPath));
        }

        public static void UpdateIsSuggestionListOpen(this AndroidAutoCompleteEntry androidAutoCompleteEntry, IAutoCompleteEntry autoCompleteEntry)
        {
            androidAutoCompleteEntry.IsSuggestionListOpen = autoCompleteEntry.IsSuggestionListOpen;
        }

        public static void UpdateUpdateTextOnSelect(this AndroidAutoCompleteEntry androidAutoCompleteEntry, IAutoCompleteEntry autoCompleteEntry)
        {
            androidAutoCompleteEntry.UpdateTextOnSelect = autoCompleteEntry.UpdateTextOnSelect;
        }

        public static void UpdateItemsSource(this AndroidAutoCompleteEntry androidAutoCompleteEntry, IAutoCompleteEntry autoCompleteEntry)
        {
            androidAutoCompleteEntry.SetItems(autoCompleteEntry?.ItemsSource?.OfType<object>(), 
                                              (o) => o.GetPropertyValueAsString(autoCompleteEntry?.DisplayMemberPath), 
                                              (o) => o.GetPropertyValueAsString(autoCompleteEntry?.TextMemberPath));
        }

        public static void UpdateSelectedSuggestion(this AndroidAutoCompleteEntry androidAutoCompleteEntry, IAutoCompleteEntry autoCompleteEntry)
        {
            androidAutoCompleteEntry.Text = autoCompleteEntry.SelectedSuggestion.GetPropertyValueAsString(autoCompleteEntry.TextMemberPath);
            androidAutoCompleteEntry.SetSelection(androidAutoCompleteEntry.Text?.Length ?? 0);
        }
    }
}
