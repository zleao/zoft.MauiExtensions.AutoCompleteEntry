using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using zoft.MauiExtensions.Controls.Platform;

namespace zoft.MauiExtensions.Controls.Handlers
{
    public partial class AutoCompleteEntryHandler : ViewHandler<IAutoCompleteEntry, AndroidAutoCompleteEntry>
    {
        protected override AndroidAutoCompleteEntry CreatePlatformView() => new(Context);

        protected override void ConnectHandler(AndroidAutoCompleteEntry platformView)
        {
            base.ConnectHandler(platformView);

            PlatformView.ViewAttachedToWindow += OnLoaded;
            PlatformView.QuerySubmitted += AutoCompleteEntry_QuerySubmitted;
            PlatformView.TextChanged += AutoCompleteEntry_TextChanged;
            PlatformView.SuggestionChosen += AutoCompleteEntry_SuggestionChosen;
        }

        protected override void DisconnectHandler(AndroidAutoCompleteEntry platformView)
        {
            PlatformView.ViewAttachedToWindow -= OnLoaded;
            PlatformView.QuerySubmitted -= AutoCompleteEntry_QuerySubmitted;
            PlatformView.TextChanged -= AutoCompleteEntry_TextChanged;
            PlatformView.SuggestionChosen -= AutoCompleteEntry_SuggestionChosen;

            base.DisconnectHandler(platformView);
        }

        private void AutoCompleteEntry_QuerySubmitted(object sender, AutoCompleteEntryQuerySubmittedEventArgs e)
        {
            VirtualView?.OnQuerySubmitted(e.QueryText, e.ChosenSuggestion);
        }

        private void AutoCompleteEntry_TextChanged(object sender, AutoCompleteEntryTextChangedEventArgs e)
        {
            VirtualView?.OnTextChanged(PlatformView.Text, (AutoCompleteEntryTextChangeReason)e.Reason);
        }

        private void AutoCompleteEntry_SuggestionChosen(object sender, AutoCompleteEntrySuggestionChosenEventArgs e)
        {
            VirtualView?.OnSuggestionSelected(e.SelectedItem);
        }

        private void OnLoaded(object sender, Android.Views.View.ViewAttachedToWindowEventArgs e)
        {
            if (VirtualView != null)
            {
                PlatformView?.UpdateTextColor(VirtualView);
                PlatformView?.UpdatePlaceholder(VirtualView);
                PlatformView?.UpdatePlaceholderColor(VirtualView);
                PlatformView?.UpdateHorizontalTextAlignment(VirtualView);
                PlatformView?.UpdateMaxLength(VirtualView.MaxLength);
                PlatformView?.UpdateIsReadOnly(VirtualView);
                PlatformView?.UpdateCancelButtonColor(VirtualView);
                PlatformView?.UpdateDisplayMemberPath(VirtualView);
                PlatformView?.UpdateIsEnabled(VirtualView);
                PlatformView?.UpdateUpdateTextOnSelect(VirtualView);
                PlatformView?.UpdateIsSuggestionListOpen(VirtualView);
                PlatformView?.UpdateItemsSource(VirtualView);
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

        public static void MapText(IAutoCompleteEntryHandler handler, IAutoCompleteEntry autoCompleteEntry)
        {
            handler.PlatformView?.UpdateText(autoCompleteEntry);
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

        public static void MapIsTextPredictionEnabled(IAutoCompleteEntryHandler handler, IAutoCompleteEntry autoCompleteEntry)
        {
            handler.PlatformView?.UpdateIsTextPredictionEnabled(autoCompleteEntry);
        }

        public static void MapMaxLength(IAutoCompleteEntryHandler handler, ISearchBar searchBar)
        {
            handler.PlatformView?.UpdateMaxLength(searchBar.MaxLength);
        }

        public static void MapIsReadOnly(IAutoCompleteEntryHandler handler, ISearchBar searchBar)
        {
            handler.PlatformView?.UpdateIsReadOnly(searchBar);
        }

        public static void MapCancelButtonColor(IAutoCompleteEntryHandler handler, IAutoCompleteEntry autoCompleteEntry)
        {
            handler.PlatformView?.UpdateCancelButtonColor(autoCompleteEntry);
        }

        public static void MapTextMemberPath(IAutoCompleteEntryHandler handler, IAutoCompleteEntry autoCompleteEntry)
        {
            // AndroidAutoCompleteEntry does not support this property
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
