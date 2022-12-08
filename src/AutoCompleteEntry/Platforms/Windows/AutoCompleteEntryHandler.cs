using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Microsoft.UI.Xaml.Controls;
using zoft.MauiExtensions.Controls.Platforms.Extensions;

namespace zoft.MauiExtensions.Controls.Handlers
{
    public partial class AutoCompleteEntryHandler : ViewHandler<IAutoCompleteEntry, AutoSuggestBox>
    {
        protected override AutoSuggestBox CreatePlatformView() => new()
        {
            AutoMaximizeSuggestionArea = false,
            //QueryIcon = new SymbolIcon(Symbol.Find),
        };

        protected override void ConnectHandler(AutoSuggestBox platformView)
        {
            PlatformView.Loaded += OnLoaded;
            PlatformView.QuerySubmitted += AutoSuggestBox_QuerySubmitted;
            PlatformView.TextChanged += AutoSuggestBox_TextChanged;
            PlatformView.SuggestionChosen += AutoSuggestBox_SuggestionChosen;
            PlatformView.GotFocus += Control_GotFocus;

        }

        protected override void DisconnectHandler(AutoSuggestBox platformView)
        {
            PlatformView.Loaded -= OnLoaded;
            PlatformView.QuerySubmitted -= AutoSuggestBox_QuerySubmitted;
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

        private void AutoSuggestBox_QuerySubmitted(object sender, AutoSuggestBoxQuerySubmittedEventArgs e)
        {
            VirtualView?.OnQuerySubmitted(e.QueryText, e.ChosenSuggestion);
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

        public static void MapBackground(IAutoCompleteEntryHandler handler, ISearchBar searchBar)
        {
            handler.PlatformView?.UpdateBackground(searchBar);
        }

        public static void MapIsEnabled(IAutoCompleteEntryHandler handler, ISearchBar searchBar)
        {
            handler.PlatformView?.UpdateIsEnabled(searchBar);
        }

        public static void MapText(IAutoCompleteEntryHandler handler, ISearchBar searchBar)
        {
            handler.PlatformView?.UpdateText(searchBar);
        }

        public static void MapPlaceholder(IAutoCompleteEntryHandler handler, ISearchBar searchBar)
        {
            handler.PlatformView?.UpdatePlaceholder(searchBar);
        }

        public static void MapVerticalTextAlignment(IAutoCompleteEntryHandler handler, ISearchBar searchBar)
        {
            handler.PlatformView?.UpdateVerticalTextAlignment(searchBar);
        }

        public static void MapPlaceholderColor(IAutoCompleteEntryHandler handler, ISearchBar searchBar)
        {
            handler.PlatformView?.UpdatePlaceholderColor(searchBar);
        }

        public static void MapHorizontalTextAlignment(IAutoCompleteEntryHandler handler, ISearchBar searchBar)
        {
            handler.PlatformView?.UpdateHorizontalTextAlignment(searchBar);
        }

        public static void MapFont(IAutoCompleteEntryHandler handler, ISearchBar searchBar)
        {
            var context = handler.MauiContext ??
                throw new InvalidOperationException($"Unable to find the context. The {nameof(MauiContext)} property should have been set by the host.");

            var services = context?.Services ??
                throw new InvalidOperationException($"Unable to find the service provider. The {nameof(MauiContext)} property should have been set by the host.");

            var fontManager = services.GetRequiredService<IFontManager>();

            handler.PlatformView?.UpdateFont(searchBar, fontManager);
        }

        public static void MapCharacterSpacing(IAutoCompleteEntryHandler handler, ISearchBar searchBar)
        {
            handler.PlatformView?.UpdateCharacterSpacing(searchBar);
        }

        public static void MapTextColor(IAutoCompleteEntryHandler handler, ISearchBar searchBar)
        {
            handler?.PlatformView?.UpdateTextColor(searchBar);
        }

        public static void MapIsTextPredictionEnabled(IAutoCompleteEntryHandler handler, ISearchBar searchBar)
        {
            handler.PlatformView?.UpdateIsTextPredictionEnabled(searchBar);
        }

        public static void MapMaxLength(IAutoCompleteEntryHandler handler, ISearchBar searchBar)
        {
            handler.PlatformView?.UpdateMaxLength(searchBar);
        }

        public static void MapIsReadOnly(IAutoCompleteEntryHandler handler, ISearchBar searchBar)
        {
            handler.PlatformView?.UpdateIsReadOnly(searchBar);
        }

        public static void MapCancelButtonColor(IAutoCompleteEntryHandler handler, ISearchBar searchBar)
        {
            // AutoSuggestBox does not support this property
        }

        public static void MapTextMemberPath(IAutoCompleteEntryHandler handler, IAutoCompleteEntry autoCompleteEntry)
        {
            handler?.PlatformView?.UpdateTextMemberPath(autoCompleteEntry);
        }

        public static void MapDisplayMemberPath(IAutoCompleteEntryHandler handler, IAutoCompleteEntry autoCompleteEntry)
        {
            handler?.PlatformView?.UpdateDisplayMemberPath(autoCompleteEntry);
        }

        public static void MapIsSuggestionListOpen(IAutoCompleteEntryHandler handler, IAutoCompleteEntry autoCompleteEntry)
        {
            handler?.PlatformView?.UpdateIsSuggestionListOpen(autoCompleteEntry);
        }

        public static void MapUpdateTextOnSelect(IAutoCompleteEntryHandler handler, IAutoCompleteEntry autoCompleteEntry)
        {
            handler?.PlatformView?.UpdateUpdateTextOnSelect(autoCompleteEntry);
        }

        public static void MapItemsSource(IAutoCompleteEntryHandler handler, IAutoCompleteEntry autoCompleteEntry)
        {
            handler?.PlatformView?.UpdateItemsSource(autoCompleteEntry);
        }

        public static void MapSelectedSuggestion(IAutoCompleteEntryHandler handler, IAutoCompleteEntry autoCompleteEntry)
        {
            handler?.PlatformView.UpdateSelectedSuggestion(autoCompleteEntry);
        }
    }
}