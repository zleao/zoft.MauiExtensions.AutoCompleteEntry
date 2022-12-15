using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Microsoft.UI.Xaml.Controls;
using zoft.MauiExtensions.Controls.Platform;

namespace zoft.MauiExtensions.Controls.Handlers
{
    public partial class AutoCompleteEntryHandler : ViewHandler<AutoCompleteEntry, AutoSuggestBox>
    {
        protected override AutoSuggestBox CreatePlatformView() => new()
        {
            AutoMaximizeSuggestionArea = false,
            //QueryIcon = new SymbolIcon(Symbol.Find),
        };

        protected override void ConnectHandler(AutoSuggestBox platformView)
        {
            PlatformView.Loaded += OnLoaded;
            PlatformView.TextChanged += AutoSuggestBox_TextChanged;
            PlatformView.SuggestionChosen += AutoSuggestBox_SuggestionChosen;
            PlatformView.GotFocus += Control_GotFocus;

        }

        protected override void DisconnectHandler(AutoSuggestBox platformView)
        {
            PlatformView.Loaded -= OnLoaded;
            PlatformView.TextChanged -= AutoSuggestBox_TextChanged;
            PlatformView.SuggestionChosen -= AutoSuggestBox_SuggestionChosen;
            PlatformView.GotFocus -= Control_GotFocus;

            base.DisconnectHandler(platformView);
        }

        private void OnLoaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            if (VirtualView != null)
            {
                PlatformView?.UpdateTextColor(VirtualView);
                PlatformView?.UpdatePlaceholder(VirtualView);
                PlatformView?.UpdatePlaceholderColor(VirtualView);
                PlatformView?.UpdateHorizontalTextAlignment(VirtualView);
                PlatformView?.UpdateMaxLength(VirtualView);
                PlatformView?.UpdateIsReadOnly(VirtualView);
                PlatformView?.UpdateTextMemberPath(VirtualView);
                PlatformView?.UpdateDisplayMemberPath(VirtualView);
                PlatformView?.UpdateIsEnabled(VirtualView);
                PlatformView?.UpdateUpdateTextOnSelect(VirtualView);
                PlatformView?.UpdateIsSuggestionListOpen(VirtualView);
                PlatformView?.UpdateItemsSource(VirtualView);
            }
        }

        private void AutoSuggestBox_TextChanged(object sender, AutoSuggestBoxTextChangedEventArgs e)
        {
            VirtualView?.OnTextChanged(PlatformView.Text, (AutoCompleteEntryTextChangeReason)e.Reason);
        }

        private void AutoSuggestBox_SuggestionChosen(object sender, AutoSuggestBoxSuggestionChosenEventArgs e)
        {
            VirtualView?.OnSuggestionSelected(e.SelectedItem);
        }

        private void Control_GotFocus(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            if (VirtualView?.ItemsSource?.Count > 0)
            {
                PlatformView.IsSuggestionListOpen = true;
            }
        }

        public static void MapBackground(IAutoCompleteEntryHandler handler, IEntry entry)
        {
            handler.PlatformView?.UpdateBackground(entry);
        }

        public static void MapIsEnabled(IAutoCompleteEntryHandler handler, IEntry entry)
        {
            handler.PlatformView?.UpdateIsEnabled(entry);
        }

        public static void MapText(IAutoCompleteEntryHandler handler, AutoCompleteEntry autoCompleteEntry)
        {
            handler.PlatformView?.UpdateText(autoCompleteEntry);
        }

        public static void MapPlaceholder(IAutoCompleteEntryHandler handler, AutoCompleteEntry autoCompleteEntry)
        {
            handler.PlatformView?.UpdatePlaceholder(autoCompleteEntry);
        }

        public static void MapVerticalTextAlignment(IAutoCompleteEntryHandler handler, AutoCompleteEntry autoCompleteEntry)
        {
            handler.PlatformView?.UpdateVerticalTextAlignment(autoCompleteEntry);
        }

        public static void MapPlaceholderColor(IAutoCompleteEntryHandler handler, AutoCompleteEntry autoCompleteEntry)
        {
            handler.PlatformView?.UpdatePlaceholderColor(autoCompleteEntry);
        }

        public static void MapHorizontalTextAlignment(IAutoCompleteEntryHandler handler, AutoCompleteEntry autoCompleteEntry)
        {
            handler.PlatformView?.UpdateHorizontalTextAlignment(autoCompleteEntry);
        }

        public static void MapFont(IAutoCompleteEntryHandler handler, IEntry entry)
        {
            var context = handler.MauiContext ??
                throw new InvalidOperationException($"Unable to find the context. The {nameof(MauiContext)} property should have been set by the host.");

            var services = context?.Services ??
                throw new InvalidOperationException($"Unable to find the service provider. The {nameof(MauiContext)} property should have been set by the host.");

            var fontManager = services.GetRequiredService<IFontManager>();

            handler.PlatformView?.UpdateFont(entry, fontManager);
        }

        public static void MapCharacterSpacing(IAutoCompleteEntryHandler handler, IEntry entry)
        {
            handler.PlatformView?.UpdateCharacterSpacing(entry);
        }

        public static void MapTextColor(IAutoCompleteEntryHandler handler, IEntry entry)
        {
            handler?.PlatformView?.UpdateTextColor(entry);
        }

        public static void MapIsTextPredictionEnabled(IAutoCompleteEntryHandler handler, AutoCompleteEntry autoCompleteEntry)
        {
            // AutoSuggestBox does not support this property
        }

        public static void MapMaxLength(IAutoCompleteEntryHandler handler, AutoCompleteEntry autoCompleteEntry)
        {
            handler.PlatformView?.UpdateMaxLength(autoCompleteEntry);
        }

        public static void MapIsReadOnly(IAutoCompleteEntryHandler handler, AutoCompleteEntry autoCompleteEntry)
        {
            handler.PlatformView?.UpdateIsReadOnly(autoCompleteEntry);
        }

        public static void MapTextMemberPath(IAutoCompleteEntryHandler handler, AutoCompleteEntry autoCompleteEntry)
        {
            handler?.PlatformView?.UpdateTextMemberPath(autoCompleteEntry);
        }

        public static void MapDisplayMemberPath(IAutoCompleteEntryHandler handler, AutoCompleteEntry autoCompleteEntry)
        {
            handler?.PlatformView?.UpdateDisplayMemberPath(autoCompleteEntry);
        }

        public static void MapIsSuggestionListOpen(IAutoCompleteEntryHandler handler, AutoCompleteEntry autoCompleteEntry)
        {
            handler?.PlatformView?.UpdateIsSuggestionListOpen(autoCompleteEntry);
        }

        public static void MapUpdateTextOnSelect(IAutoCompleteEntryHandler handler, AutoCompleteEntry autoCompleteEntry)
        {
            handler?.PlatformView?.UpdateUpdateTextOnSelect(autoCompleteEntry);
        }

        public static void MapItemsSource(IAutoCompleteEntryHandler handler, AutoCompleteEntry autoCompleteEntry)
        {
            handler?.PlatformView?.UpdateItemsSource(autoCompleteEntry);
        }

        public static void MapSelectedSuggestion(IAutoCompleteEntryHandler handler, AutoCompleteEntry autoCompleteEntry)
        {
            handler?.PlatformView.UpdateSelectedSuggestion(autoCompleteEntry);
        }
    }
}