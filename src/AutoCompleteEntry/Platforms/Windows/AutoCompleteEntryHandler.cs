using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Windows.System;
using zoft.MauiExtensions.Controls.Platform;

namespace zoft.MauiExtensions.Controls.Handlers;

public partial class AutoCompleteEntryHandler : ViewHandler<AutoCompleteEntry, AutoSuggestBox>
{
    private MultipleSelectionPresentation? _multiple;
    private long _openStateToken;
    private SuggestionItemsSnapshot? _suggestionItems;

    /// <inheritdoc/>
    protected override AutoSuggestBox CreatePlatformView() => new()
    {
        AutoMaximizeSuggestionArea = false
    };

    /// <inheritdoc/>
    protected override void ConnectHandler(AutoSuggestBox platformView)
    {
        _suggestionItems = new(action => platformView.DispatcherQueue.TryEnqueue(() => action()), items =>
        {
            if (VirtualView.IsMultiple) _multiple?.SetItems(items);
            else platformView.ItemsSource = items;
        });
        _openStateToken = platformView.RegisterPropertyChangedCallback(AutoSuggestBox.IsSuggestionListOpenProperty,
            (sender, property) =>
            {
                if (VirtualView?.IsMultiple != true && VirtualView is not null)
                    VirtualView.IsSuggestionListOpen = platformView.IsSuggestionListOpen;
            });
        platformView.GotFocus += PlatformView_OnGotFocus;
        platformView.LostFocus += PlatformView_LostFocus;
        platformView.KeyUp += PlatformView_OnKeyUp;
        platformView.Loaded += PlatformView_OnLoaded;
        platformView.Unloaded += PlatformView_OnUnloaded;
        ConnectSingleSelectionPointerHandlers(platformView);
        platformView.SuggestionChosen += PlatformView_OnSuggestionChosen;
        platformView.TextChanged += PlatformView_OnTextChanged;
    }

    /// <inheritdoc/>
    protected override void DisconnectHandler(AutoSuggestBox platformView)
    {
        _suggestionItems?.Dispose();
        _suggestionItems = null;
        DisconnectSingleSelectionPointerHandlers(platformView);
        VirtualView.IsSuggestionListOpen = false;
        platformView.UnregisterPropertyChangedCallback(AutoSuggestBox.IsSuggestionListOpenProperty, _openStateToken);
        _multiple?.Dispose();
        _multiple = null;
        platformView.GotFocus -= PlatformView_OnGotFocus;
        platformView.LostFocus -= PlatformView_LostFocus;
        platformView.KeyUp -= PlatformView_OnKeyUp;
        platformView.Loaded -= PlatformView_OnLoaded;
        platformView.Unloaded -= PlatformView_OnUnloaded;
        platformView.SuggestionChosen -= PlatformView_OnSuggestionChosen;
        platformView.TextChanged -= PlatformView_OnTextChanged;

        platformView.ClearItemTemplate();

        base.DisconnectHandler(platformView);
    }


    private void PlatformView_OnGotFocus(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (VirtualView is not null) VirtualView.IsSuggestionListOpen = true;
        if (VirtualView?.IsMultiple == true) _multiple?.Update();
        else if (VirtualView?.ItemsSource?.Count > 0)
        {
            PlatformView.IsSuggestionListOpen = true;
        }

        if (VirtualView is IEntry virtualView)
        {
            virtualView.IsFocused = true;
        }
    }

    private void PlatformView_LostFocus(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (_multiple is not null)
            PlatformView.DispatcherQueue.TryEnqueue(() =>
            {
                if (_multiple is not null && !_multiple.ContainsFocus()) VirtualView.IsSuggestionListOpen = false;
            });
        if (VirtualView is IEntry virtualView)
        {
            virtualView.IsFocused = false;
        }
    }

    // Note: this is copied from MAUI's EntryHandler.Windows.cs > OnPlatformKeyUp
    private void PlatformView_OnKeyUp(object sender, KeyRoutedEventArgs args)
    {
        if (VirtualView?.IsMultiple == true && VirtualView.IsSuggestionListOpen) return;
        if (args?.Key != VirtualKey.Enter)
            return;

        if (VirtualView?.ReturnType == ReturnType.Next)
        {
            PlatformView?.TryMoveFocus(FocusNavigationDirection.Next);
        }

        VirtualView?.SendCompleted();
    }

    private void PlatformView_OnLoaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        AttachSingleSelectionRootHandler();
        if (VirtualView != null)
        {
            PlatformView?.UpdateClearButtonVisibility(VirtualView);
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
            MapIsSuggestionListOpen(this, VirtualView);
            MapItemsSource(this, VirtualView);
        }
    }

    private void PlatformView_OnUnloaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        => DetachSingleSelectionRootHandler();

    private void PlatformView_OnSuggestionChosen(object sender, AutoSuggestBoxSuggestionChosenEventArgs e)
    {
        if (VirtualView?.IsMultiple == true) return;
        VirtualView?.OnSuggestionSelected(e.SelectedItem);
    }

    private void PlatformView_OnTextChanged(object sender, AutoSuggestBoxTextChangedEventArgs e)
    {
        if (VirtualView?.IsMultiple == true &&
            (e.Reason == AutoSuggestionBoxTextChangeReason.SuggestionChosen || PlatformView.Text == VirtualView.Text)) return;
        VirtualView?.OnTextChanged(PlatformView.Text, (AutoCompleteEntryTextChangeReason)e.Reason);
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
    /// Map the CharacterSpacing value
    /// </summary>
    /// <param name="handler"></param>
    /// <param name="entry"></param>
    public static void MapCharacterSpacing(IAutoCompleteEntryHandler handler, IEntry entry)
    {
        handler.PlatformView?.UpdateCharacterSpacing(entry);
    }

    /// <summary>
    /// Map the ClearButtonVisibility value
    /// </summary>
    /// <param name="handler"></param>
    /// <param name="autoCompleteEntry"></param>
    public static void MapClearButtonVisibility(IAutoCompleteEntryHandler handler, AutoCompleteEntry autoCompleteEntry)
    {
        handler.PlatformView?.UpdateClearButtonVisibility(autoCompleteEntry);
    }

    /// <summary>
    /// Map the CursorPosition value
    /// </summary>
    /// <param name="handler"></param>
    /// <param name="entry"></param>
    public static void MapCursorPosition(IAutoCompleteEntryHandler handler, IEntry entry)
    {
        // AutoSuggestBox does not support this property
    }

    /// <summary>
    /// Map the DisplayMemberPath value
    /// </summary>
    /// <param name="handler"></param>
    /// <param name="autoCompleteEntry"></param>
    public static void MapDisplayMemberPath(IAutoCompleteEntryHandler handler, AutoCompleteEntry autoCompleteEntry)
    {
        if (handler is AutoCompleteEntryHandler { _multiple: { } multiple }) multiple.SetTemplate();
        handler?.PlatformView?.UpdateDisplayMemberPath(autoCompleteEntry);
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
    /// Map the HorizontalTextAlignment value
    /// </summary>
    /// <param name="handler"></param>
    /// <param name="autoCompleteEntry"></param>
    public static void MapHorizontalTextAlignment(IAutoCompleteEntryHandler handler, AutoCompleteEntry autoCompleteEntry)
    {
        handler.PlatformView?.UpdateHorizontalTextAlignment(autoCompleteEntry);
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
    /// Map the IsReadOnly value
    /// </summary>
    /// <param name="handler"></param>
    /// <param name="autoCompleteEntry"></param>
    public static void MapIsReadOnly(IAutoCompleteEntryHandler handler, AutoCompleteEntry autoCompleteEntry)
    {
        handler.PlatformView?.UpdateIsReadOnly(autoCompleteEntry);
    }

    /// <summary>
    /// Map the IsSuggestionListOpen value
    /// </summary>
    /// <param name="handler"></param>
    /// <param name="autoCompleteEntry"></param>
    public static void MapIsSuggestionListOpen(IAutoCompleteEntryHandler handler, AutoCompleteEntry autoCompleteEntry)
    {
        if (autoCompleteEntry.IsMultiple) { MapSelectionPresentation(handler, autoCompleteEntry); return; }
        handler?.PlatformView?.UpdateIsSuggestionListOpen(autoCompleteEntry);
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
    /// Map the ItemsSource value
    /// </summary>
    /// <param name="handler"></param>
    /// <param name="autoCompleteEntry"></param>
    public static void MapItemsSource(IAutoCompleteEntryHandler handler, AutoCompleteEntry autoCompleteEntry)
    {
        if (handler is AutoCompleteEntryHandler actual)
            actual._suggestionItems?.SetSource(autoCompleteEntry.ItemsSource);
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
    /// Map the Placeholder value
    /// </summary>
    /// <param name="handler"></param>
    /// <param name="autoCompleteEntry"></param>
    public static void MapPlaceholder(IAutoCompleteEntryHandler handler, AutoCompleteEntry autoCompleteEntry)
    {
        handler.PlatformView?.UpdatePlaceholder(autoCompleteEntry);
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
    /// Map the ReturnType value
    /// </summary>
    /// <param name="handler"></param>
    /// <param name="entry"></param>
    public static void MapReturnType(IAutoCompleteEntryHandler handler, IEntry entry)
    {
        handler.PlatformView?.UpdateReturnType(entry);
    }

    /// <summary>
    /// Map the SelectedSuggestion value
    /// </summary>
    /// <param name="handler"></param>
    /// <param name="autoCompleteEntry"></param>
    public static void MapSelectedSuggestion(IAutoCompleteEntryHandler handler, AutoCompleteEntry autoCompleteEntry)
    {
        if (autoCompleteEntry.IsMultiple) return;
        handler?.PlatformView.UpdateSelectedSuggestion(autoCompleteEntry);
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
    /// Map the TextColor value
    /// </summary>
    /// <param name="handler"></param>
    /// <param name="entry"></param>
    public static void MapTextColor(IAutoCompleteEntryHandler handler, IEntry entry)
    {
        handler?.PlatformView?.UpdateTextColor(entry);
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
    /// Map the UpdateTextOnSelect value
    /// </summary>
    /// <param name="handler"></param>
    /// <param name="autoCompleteEntry"></param>
    public static void MapUpdateTextOnSelect(IAutoCompleteEntryHandler handler, AutoCompleteEntry autoCompleteEntry)
    {
        if (autoCompleteEntry.IsMultiple) { handler.PlatformView.UpdateTextOnSelect = false; return; }
        handler?.PlatformView?.UpdateUpdateTextOnSelect(autoCompleteEntry);
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
    /// Map the ShowBottomBorder value
    /// </summary>
    /// <param name="handler"></param>
    /// <param name="autoCompleteEntry"></param>
    public static void MapShowBottomBorder(IAutoCompleteEntryHandler handler, AutoCompleteEntry autoCompleteEntry)
    {
        handler?.PlatformView.UpdateShowBottomBorder(autoCompleteEntry);
    }

    /// <summary>
    /// Map the ItemTemplate value
    /// </summary>
    /// <param name="handler"></param>
    /// <param name="autoCompleteEntry"></param>
    public static void MapItemTemplate(IAutoCompleteEntryHandler handler, AutoCompleteEntry autoCompleteEntry)
    {
        if (handler is AutoCompleteEntryHandler { _multiple: { } multiple })
        {
            multiple.SetTemplate();
            return;
        }
        handler?.PlatformView.UpdateItemTemplate(autoCompleteEntry, handler.MauiContext);
    }

    /// <summary>Updates checked rows and the separate summary overlay.</summary>
    public static void MapSelectionPresentation(IAutoCompleteEntryHandler handler, AutoCompleteEntry entry)
    {
        if (handler is AutoCompleteEntryHandler actual) actual._multiple?.Update();
    }

    /// <summary>Switches between native single and persistent multiple suggestion lists.</summary>
    public static void MapSelectionMode(IAutoCompleteEntryHandler handler, AutoCompleteEntry entry)
    {
        if (handler is not AutoCompleteEntryHandler actual) return;
        actual._multiple?.Dispose();
        actual._multiple = null;
        if (entry.IsMultiple && handler.MauiContext is { } context)
        {
            handler.PlatformView.ClearItemTemplate();
            actual._multiple = new(handler.PlatformView, entry, context);
            actual._multiple.Update();
            MapItemsSource(handler, entry);
        }
        else
        {
            MapUpdateTextOnSelect(handler, entry);
            MapItemTemplate(handler, entry);
            MapItemsSource(handler, entry);
        }
    }
}
