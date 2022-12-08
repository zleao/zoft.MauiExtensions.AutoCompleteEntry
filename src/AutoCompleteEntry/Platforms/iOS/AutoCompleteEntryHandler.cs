using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using zoft.MauiExtensions.Controls.Platforms.Extensions;
using zoft.MauiExtensions.Controls.Platforms.iOS;

namespace zoft.MauiExtensions.Controls.Handlers
{
    public partial class AutoCompleteEntryHandler : ViewHandler<IAutoCompleteEntry, IOSAutoCompleteEntry>
	{
        protected override IOSAutoCompleteEntry CreatePlatformView() => new();

        public override Size GetDesiredSize(double widthConstraint, double heightConstraint)
        {
            if (double.IsInfinity(widthConstraint) || double.IsInfinity(heightConstraint))
            {
                PlatformView.InputTextField.SizeToFit();
                return new Size(PlatformView.InputTextField.Frame.Width, PlatformView.InputTextField.Frame.Height);
            }

            return base.GetDesiredSize(widthConstraint, heightConstraint);
        }

        protected override void ConnectHandler(IOSAutoCompleteEntry platformView)
        {
            base.ConnectHandler(platformView);

            PlatformView.OnLoaded += AutoCompleteEntry_OnLoaded;
            PlatformView.QuerySubmitted += AutoCompleteEntry_QuerySubmitted;
            PlatformView.TextChanged += AutoCompleteEntry_TextChanged;
            PlatformView.SuggestionChosen += AutoCompleteEntry_SuggestionChosen;
            PlatformView.EditingDidBegin += AutoCompleteEntry_EditingDidBegin;
            PlatformView.EditingDidEnd += AutoCompleteEntry_EditingDidEnd;
        }

        protected override void DisconnectHandler(IOSAutoCompleteEntry platformView)
        {
            PlatformView.OnLoaded -= AutoCompleteEntry_OnLoaded;
            PlatformView.QuerySubmitted -= AutoCompleteEntry_QuerySubmitted;
            PlatformView.TextChanged -= AutoCompleteEntry_TextChanged;
            PlatformView.SuggestionChosen -= AutoCompleteEntry_SuggestionChosen;
            PlatformView.EditingDidBegin -= AutoCompleteEntry_EditingDidBegin;
            PlatformView.EditingDidEnd -= AutoCompleteEntry_EditingDidEnd;

            base.DisconnectHandler(platformView);
        }

        private void AutoCompleteEntry_OnLoaded(object sender, EventArgs e)
        {
            if (VirtualView != null)
            {
                PlatformView?.UpdateText(VirtualView);
                PlatformView?.UpdatePlaceholder(VirtualView);
                PlatformView?.UpdatePlaceholder(VirtualView);
                PlatformView?.InputTextField.UpdateHorizontalTextAlignment(VirtualView);
                PlatformView?.UpdateMaxLength(VirtualView);
                PlatformView?.UpdateIsReadOnly(VirtualView);
                PlatformView?.UpdateDisplayMemberPath(VirtualView);
                PlatformView?.UpdateIsEnabled(VirtualView);
                PlatformView?.UpdateUpdateTextOnSelect(VirtualView);
                PlatformView?.UpdateIsSuggestionListOpen(VirtualView);
                PlatformView?.UpdateItemsSource(VirtualView);
            }
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

        private void AutoCompleteEntry_EditingDidBegin(object sender, EventArgs e)
        {
            if (VirtualView != null)
            {
                VirtualView.IsFocused = true;
            }
        }
       
        private void AutoCompleteEntry_EditingDidEnd(object sender, EventArgs e)
        {
            if (VirtualView != null)
            {
                VirtualView.IsFocused = false;
            }
        }

        public static void MapBackground(IAutoCompleteEntryHandler handler, ISearchBar searchBar)
        {
            handler.PlatformView?.InputTextField.UpdateBackground(searchBar);
        }

        public static void MapIsEnabled(IAutoCompleteEntryHandler handler, ISearchBar searchBar)
        {
            handler.PlatformView?.InputTextField.UpdateIsEnabled(searchBar);
        }

        public static void MapText(IAutoCompleteEntryHandler handler, IAutoCompleteEntry autoCompleteEntry)
        {
            handler.PlatformView?.UpdateText(autoCompleteEntry);
        }

        public static void MapPlaceholder(IAutoCompleteEntryHandler handler, IAutoCompleteEntry autoCompleteEntry)
        {
            handler.PlatformView?.UpdatePlaceholder(autoCompleteEntry);
        }

        public static void MapVerticalTextAlignment(IAutoCompleteEntryHandler handler, IAutoCompleteEntry autoCompleteEntry)
        {
            handler.PlatformView?.InputTextField.UpdateVerticalTextAlignment(autoCompleteEntry);
        }

        public static void MapPlaceholderColor(IAutoCompleteEntryHandler handler, IAutoCompleteEntry autoCompleteEntry)
        {
            handler.PlatformView?.UpdatePlaceholder(autoCompleteEntry);
        }

        public static void MapHorizontalTextAlignment(IAutoCompleteEntryHandler handler, IAutoCompleteEntry autoCompleteEntry)
        {
            handler.PlatformView?.InputTextField.UpdateHorizontalTextAlignment(autoCompleteEntry);
        }

        public static void MapFont(IAutoCompleteEntryHandler handler, IAutoCompleteEntry autoCompleteEntry)
        {
            var context = handler.MauiContext ??
               throw new InvalidOperationException($"Unable to find the context. The {nameof(MauiContext)} property should have been set by the host.");

            var services = context?.Services ??
                throw new InvalidOperationException($"Unable to find the service provider. The {nameof(MauiContext)} property should have been set by the host.");

            var fontManager = services.GetRequiredService<IFontManager>();

            handler.PlatformView?.InputTextField.UpdateFont(autoCompleteEntry, fontManager);
        }

        public static void MapCharacterSpacing(IAutoCompleteEntryHandler handler, IAutoCompleteEntry autoCompleteEntry)
        {
            handler.PlatformView?.InputTextField.UpdateCharacterSpacing(autoCompleteEntry);
        }

        public static void MapTextColor(IAutoCompleteEntryHandler handler, IAutoCompleteEntry autoCompleteEntry)
        {
            handler?.PlatformView?.InputTextField.UpdateTextColor(autoCompleteEntry);
        }

        public static void MapIsTextPredictionEnabled(IAutoCompleteEntryHandler handler, IAutoCompleteEntry autoCompleteEntry)
        {
            handler.PlatformView?.UpdateIsTextPredictionEnabled(autoCompleteEntry);
        }

        public static void MapMaxLength(IAutoCompleteEntryHandler handler, IAutoCompleteEntry autoCompleteEntry)
        {
            handler.PlatformView?.UpdateMaxLength(autoCompleteEntry);
        }

        public static void MapIsReadOnly(IAutoCompleteEntryHandler handler, IAutoCompleteEntry autoCompleteEntry)
        {
            handler.PlatformView?.UpdateIsReadOnly(autoCompleteEntry);
        }

        public static void MapCancelButtonColor(IAutoCompleteEntryHandler handler, IAutoCompleteEntry autoCompleteEntry)
        {
            // IOSAutoCompleteEntry does not support this property
        }

        public static void MapTextMemberPath(IAutoCompleteEntryHandler handler, IAutoCompleteEntry autoCompleteEntry)
        {
            // IOSAutoCompleteEntry does not support this property
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
            handler?.PlatformView?.UpdateSelectedSuggestion(autoCompleteEntry);
        }
    }
}
