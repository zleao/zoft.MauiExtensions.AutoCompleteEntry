using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using zoft.MauiExtensions.Controls.Platform;

namespace zoft.MauiExtensions.Controls.Handlers
{
    public partial class AutoCompleteEntryHandler : ViewHandler<AutoCompleteEntry, AndroidAutoCompleteEntry>
    {
        protected override AndroidAutoCompleteEntry CreatePlatformView() => new(Context);

        protected override void ConnectHandler(AndroidAutoCompleteEntry platformView)
        {
            base.ConnectHandler(platformView);

            PlatformView.ViewAttachedToWindow += OnLoaded;
            PlatformView.TextChanged += AutoCompleteEntry_TextChanged;
            PlatformView.SuggestionChosen += AutoCompleteEntry_SuggestionChosen;
        }

        protected override void DisconnectHandler(AndroidAutoCompleteEntry platformView)
        {
            PlatformView.ViewAttachedToWindow -= OnLoaded;
            PlatformView.TextChanged -= AutoCompleteEntry_TextChanged;
            PlatformView.SuggestionChosen -= AutoCompleteEntry_SuggestionChosen;

            base.DisconnectHandler(platformView);
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
                PlatformView?.UpdateDisplayMemberPath(VirtualView);
                PlatformView?.UpdateIsEnabled(VirtualView);
                PlatformView?.UpdateUpdateTextOnSelect(VirtualView);
                PlatformView?.UpdateIsSuggestionListOpen(VirtualView);
                PlatformView?.UpdateItemsSource(VirtualView);
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

        public static void MapPlaceholder(IAutoCompleteEntryHandler handler, IEntry entry)
        {
            handler.PlatformView?.UpdatePlaceholder(entry);
        }

        public static void MapVerticalTextAlignment(IAutoCompleteEntryHandler handler, IEntry entry)
        {
            handler.PlatformView?.UpdateVerticalTextAlignment(entry);
        }

        public static void MapPlaceholderColor(IAutoCompleteEntryHandler handler, IEntry entry)
        {
            handler.PlatformView?.UpdatePlaceholderColor(entry);
        }

        public static void MapHorizontalTextAlignment(IAutoCompleteEntryHandler handler, IEntry entry)
        {
            handler.PlatformView?.UpdateHorizontalTextAlignment(entry);
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
            handler.PlatformView?.UpdateIsTextPredictionEnabled(autoCompleteEntry);
        }

        public static void MapMaxLength(IAutoCompleteEntryHandler handler, IEntry entry)
        {
            handler.PlatformView?.UpdateMaxLength(entry.MaxLength);
        }

        public static void MapIsReadOnly(IAutoCompleteEntryHandler handler, IEntry entry)
        {
            handler.PlatformView?.UpdateIsReadOnly(entry);
        }
       
        public static void MapTextMemberPath(IAutoCompleteEntryHandler handler, AutoCompleteEntry autoCompleteEntry)
        {
            // AndroidAutoCompleteEntry does not support this property
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
