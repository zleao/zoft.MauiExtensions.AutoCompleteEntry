﻿using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Microsoft.UI.Xaml.Controls;
using zoft.MauiExtensions.Controls.Platform;

namespace zoft.MauiExtensions.Controls.Handlers
{
    public partial class AutoCompleteEntryHandler : ViewHandler<AutoCompleteEntry, AutoSuggestBox>
    {
        /// <inheritdoc/>
        protected override AutoSuggestBox CreatePlatformView() => new()
        {
            AutoMaximizeSuggestionArea = false,
            //QueryIcon = new SymbolIcon(Symbol.Find),
        };

        /// <inheritdoc/>
        protected override void ConnectHandler(AutoSuggestBox platformView)
        {
            platformView.Loaded += OnLoaded;
            platformView.TextChanged += AutoSuggestBox_TextChanged;
            platformView.SuggestionChosen += AutoSuggestBox_SuggestionChosen;
            platformView.GotFocus += Control_GotFocus;

        }

        /// <inheritdoc/>
        protected override void DisconnectHandler(AutoSuggestBox platformView)
        {
            platformView.Loaded -= OnLoaded;
            platformView.TextChanged -= AutoSuggestBox_TextChanged;
            platformView.SuggestionChosen -= AutoSuggestBox_SuggestionChosen;
            platformView.GotFocus -= Control_GotFocus;

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

        /// <summary>
        /// Map the Background value
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="entry"></param>
        public static void MapBackground(IAutoCompleteEntryHandler handler, IEntry entry)
        {
            handler.PlatformView?.UpdateBackground(entry);
        }

        /// <summary>
        /// Map the IsEnabled value
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="entry"></param>
        public static void MapIsEnabled(IAutoCompleteEntryHandler handler, IEntry entry)
        {
            handler.PlatformView?.UpdateIsEnabled(entry);
        }

        /// <summary>
        /// Map the Text value
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="autoCompleteEntry"></param>
        public static void MapText(IAutoCompleteEntryHandler handler, AutoCompleteEntry autoCompleteEntry)
        {
            handler.PlatformView?.UpdateText(autoCompleteEntry);
        }

        /// <summary>
        /// Map the Placeholder value
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="autoCompleteEntry"></param>
        public static void MapPlaceholder(IAutoCompleteEntryHandler handler, AutoCompleteEntry autoCompleteEntry)
        {
            handler.PlatformView?.UpdatePlaceholder(autoCompleteEntry);
        }

        /// <summary>
        /// Map the VerticalTextAlignment value
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="autoCompleteEntry"></param>
        public static void MapVerticalTextAlignment(IAutoCompleteEntryHandler handler, AutoCompleteEntry autoCompleteEntry)
        {
            handler.PlatformView?.UpdateVerticalTextAlignment(autoCompleteEntry);
        }

        /// <summary>
        /// Map the PlaceholderColor value
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="autoCompleteEntry"></param>
        public static void MapPlaceholderColor(IAutoCompleteEntryHandler handler, AutoCompleteEntry autoCompleteEntry)
        {
            handler.PlatformView?.UpdatePlaceholderColor(autoCompleteEntry);
        }

        /// <summary>
        /// Map the HorizontalTextAlignment value
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="autoCompleteEntry"></param>
        public static void MapHorizontalTextAlignment(IAutoCompleteEntryHandler handler, AutoCompleteEntry autoCompleteEntry)
        {
            handler.PlatformView?.UpdateHorizontalTextAlignment(autoCompleteEntry);
        }

        /// <summary>
        /// Map the Font value
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="entry"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public static void MapFont(IAutoCompleteEntryHandler handler, IEntry entry)
        {
            var context = handler.MauiContext ??
                throw new InvalidOperationException($"Unable to find the context. The {nameof(MauiContext)} property should have been set by the host.");

            var services = context?.Services ??
                throw new InvalidOperationException($"Unable to find the service provider. The {nameof(MauiContext)} property should have been set by the host.");

            var fontManager = services.GetRequiredService<IFontManager>();

            handler.PlatformView?.UpdateFont(entry, fontManager);
        }

        /// <summary>
        /// Map the CharacterSpacing value
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="entry"></param>
        public static void MapCharacterSpacing(IAutoCompleteEntryHandler handler, IEntry entry)
        {
            handler.PlatformView?.UpdateCharacterSpacing(entry);
        }

        /// <summary>
        /// Map the TextColor value
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="entry"></param>
        public static void MapTextColor(IAutoCompleteEntryHandler handler, IEntry entry)
        {
            handler?.PlatformView?.UpdateTextColor(entry);
        }

        /// <summary>
        /// Map the IsTextPredictionEnabled value
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="autoCompleteEntry"></param>
        public static void MapIsTextPredictionEnabled(IAutoCompleteEntryHandler handler, AutoCompleteEntry autoCompleteEntry)
        {
            // AutoSuggestBox does not support this property
        }

        /// <summary>
        /// Map the MaxLength value
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="autoCompleteEntry"></param>
        public static void MapMaxLength(IAutoCompleteEntryHandler handler, AutoCompleteEntry autoCompleteEntry)
        {
            handler.PlatformView?.UpdateMaxLength(autoCompleteEntry);
        }

        /// <summary>
        /// Map the Background value
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="autoCompleteEntry"></param>
        public static void MapIsReadOnly(IAutoCompleteEntryHandler handler, AutoCompleteEntry autoCompleteEntry)
        {
            handler.PlatformView?.UpdateIsReadOnly(autoCompleteEntry);
        }

        /// <summary>
        /// Map the TextMemberPath value
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="autoCompleteEntry"></param>
        public static void MapTextMemberPath(IAutoCompleteEntryHandler handler, AutoCompleteEntry autoCompleteEntry)
        {
            handler?.PlatformView?.UpdateTextMemberPath(autoCompleteEntry);
        }

        /// <summary>
        /// Map the DysplayMemberPath value
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="autoCompleteEntry"></param>
        public static void MapDisplayMemberPath(IAutoCompleteEntryHandler handler, AutoCompleteEntry autoCompleteEntry)
        {
            handler?.PlatformView?.UpdateDisplayMemberPath(autoCompleteEntry);
        }

        /// <summary>
        /// Map the IsSuggestionListOpen value
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="autoCompleteEntry"></param>
        public static void MapIsSuggestionListOpen(IAutoCompleteEntryHandler handler, AutoCompleteEntry autoCompleteEntry)
        {
            handler?.PlatformView?.UpdateIsSuggestionListOpen(autoCompleteEntry);
        }

        /// <summary>
        /// Map the UpdateTextOnSelect value
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="autoCompleteEntry"></param>
        public static void MapUpdateTextOnSelect(IAutoCompleteEntryHandler handler, AutoCompleteEntry autoCompleteEntry)
        {
            handler?.PlatformView?.UpdateUpdateTextOnSelect(autoCompleteEntry);
        }

        /// <summary>
        /// Map the ItemsSource value
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="autoCompleteEntry"></param>
        public static void MapItemsSource(IAutoCompleteEntryHandler handler, AutoCompleteEntry autoCompleteEntry)
        {
            handler?.PlatformView?.UpdateItemsSource(autoCompleteEntry);
        }

        /// <summary>
        /// Map the SelectedSuggestion value
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="autoCompleteEntry"></param>
        public static void MapSelectedSuggestion(IAutoCompleteEntryHandler handler, AutoCompleteEntry autoCompleteEntry)
        {
            handler?.PlatformView.UpdateSelectedSuggestion(autoCompleteEntry);
        }
    }
}