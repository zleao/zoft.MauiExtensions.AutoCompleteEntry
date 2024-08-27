using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using zoft.MauiExtensions.Controls.Platform;

namespace zoft.MauiExtensions.Controls.Handlers;

public partial class AutoCompleteEntryHandler : ViewHandler<AutoCompleteEntry, AndroidAutoCompleteEntry>
{
    /// <inheritdoc/>
    protected override AndroidAutoCompleteEntry CreatePlatformView() => new(Context);

    /// <inheritdoc/>
    protected override void ConnectHandler(AndroidAutoCompleteEntry platformView)
    {
        base.ConnectHandler(platformView);

        platformView.ViewAttachedToWindow += OnLoaded;
        platformView.TextChanged += AutoCompleteEntry_TextChanged;
        platformView.CursorPositionChanged += AutoCompleteEntry_CursorPositionChanged;
        platformView.SuggestionChosen += AutoCompleteEntry_SuggestionChosen;
    }

    /// <inheritdoc/>
    protected override void DisconnectHandler(AndroidAutoCompleteEntry platformView)
    {
        platformView.ViewAttachedToWindow -= OnLoaded;
        platformView.TextChanged -= AutoCompleteEntry_TextChanged;
        platformView.CursorPositionChanged -= AutoCompleteEntry_CursorPositionChanged;
        platformView.SuggestionChosen -= AutoCompleteEntry_SuggestionChosen;

        base.DisconnectHandler(platformView);
    }

    private void AutoCompleteEntry_TextChanged(object sender, AutoCompleteEntryTextChangedEventArgs e)
    {
        VirtualView?.OnTextChanged(PlatformView.Text, e.Reason);
    }

    private void AutoCompleteEntry_CursorPositionChanged(object sender, AutoCompleteEntryCursorPositionChangedEventArgs e)
    {
        VirtualView?.OnCursorPositionChanged(e.CursorPosition);
    }

    private void AutoCompleteEntry_SuggestionChosen(object sender, AutoCompleteEntrySuggestionChosenEventArgs e)
    {
        VirtualView?.OnSuggestionSelected(e.SelectedItem);
    }

    private void OnLoaded(object sender, Android.Views.View.ViewAttachedToWindowEventArgs e)
    {
        PlatformView.UpdateTextColor(VirtualView);
        PlatformView.UpdatePlaceholder(VirtualView);
        PlatformView.UpdatePlaceholderColor(VirtualView);
        PlatformView.UpdateHorizontalTextAlignment(VirtualView);
        PlatformView.UpdateMaxLength(VirtualView.MaxLength);
        PlatformView.UpdateIsReadOnly(VirtualView);
        PlatformView.UpdateDisplayMemberPath(VirtualView);
        PlatformView.UpdateIsEnabled(VirtualView);
        PlatformView.UpdateUpdateTextOnSelect(VirtualView);
        PlatformView.UpdateIsSuggestionListOpen(VirtualView);
        PlatformView.UpdateItemsSource(VirtualView);
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
    /// Map the CursorPosition value
    /// </summary>
    /// <param name="handler"></param>
    /// <param name="entry"></param>
    public static void MapCursorPosition(IAutoCompleteEntryHandler handler, IEntry entry)
    {
        handler.PlatformView?.UpdateCursorPosition(entry);
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
    /// <param name="entry"></param>
    public static void MapPlaceholder(IAutoCompleteEntryHandler handler, IEntry entry)
    {
        handler.PlatformView?.UpdatePlaceholder(entry);
    }

    /// <summary>
    /// Map the VerticalTextAlignment value
    /// </summary>
    /// <param name="handler"></param>
    /// <param name="entry"></param>
    public static void MapVerticalTextAlignment(IAutoCompleteEntryHandler handler, IEntry entry)
    {
        handler.PlatformView?.UpdateVerticalTextAlignment(entry);
    }

    /// <summary>
    /// Map the PlaceholderColor value
    /// </summary>
    /// <param name="handler"></param>
    /// <param name="entry"></param>
    public static void MapPlaceholderColor(IAutoCompleteEntryHandler handler, IEntry entry)
    {
        handler.PlatformView?.UpdatePlaceholderColor(entry);
    }

    /// <summary>
    /// Map the HorizontalTextAlignment value
    /// </summary>
    /// <param name="handler"></param>
    /// <param name="entry"></param>
    public static void MapHorizontalTextAlignment(IAutoCompleteEntryHandler handler, IEntry entry)
    {
        handler.PlatformView?.UpdateHorizontalTextAlignment(entry);
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
        handler.PlatformView?.UpdateIsTextPredictionEnabled(autoCompleteEntry);
    }

    /// <summary>
    /// Map the MaxLength value
    /// </summary>
    /// <param name="handler"></param>
    /// <param name="entry"></param>
    public static void MapMaxLength(IAutoCompleteEntryHandler handler, IEntry entry)
    {
        handler.PlatformView?.UpdateMaxLength(entry.MaxLength);
    }

    /// <summary>
    /// Map the IsReadOnly value
    /// </summary>
    /// <param name="handler"></param>
    /// <param name="entry"></param>
    public static void MapIsReadOnly(IAutoCompleteEntryHandler handler, IEntry entry)
    {
        handler.PlatformView?.UpdateIsReadOnly(entry);
    }

    /// <summary>
    /// Map the TextMemberPath value
    /// </summary>
    /// <param name="handler"></param>
    /// <param name="autoCompleteEntry"></param>
    public static void MapTextMemberPath(IAutoCompleteEntryHandler handler, AutoCompleteEntry autoCompleteEntry)
    {
        // AndroidAutoCompleteEntry does not support this property
    }

    /// <summary>
    /// Map the DisplayMemberPath value
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