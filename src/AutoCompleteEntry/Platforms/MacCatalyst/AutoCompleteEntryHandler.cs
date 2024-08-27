using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using zoft.MauiExtensions.Controls.Platform;

namespace zoft.MauiExtensions.Controls.Handlers;

public partial class AutoCompleteEntryHandler : ViewHandler<AutoCompleteEntry, IOSAutoCompleteEntry>
{
    /// <inheritdoc/>
    protected override IOSAutoCompleteEntry CreatePlatformView() => new();

    /// <inheritdoc/>
    public override Size GetDesiredSize(double widthConstraint, double heightConstraint)
    {
        if (double.IsInfinity(widthConstraint) || double.IsInfinity(heightConstraint))
        {
            PlatformView.InputTextField.SizeToFit();
            return new Size(PlatformView.InputTextField.Frame.Width, PlatformView.InputTextField.Frame.Height);
        }

        return base.GetDesiredSize(widthConstraint, heightConstraint);
    }

    /// <inheritdoc/>
    protected override void ConnectHandler(IOSAutoCompleteEntry platformView)
    {
        base.ConnectHandler(platformView);

        platformView.OnLoaded += AutoCompleteEntry_OnLoaded;
        platformView.TextChanged += AutoCompleteEntry_TextChanged;
        platformView.CursorPositionChanged += InputTextFieldOnCursorPositionChanged;
        platformView.SuggestionChosen += AutoCompleteEntry_SuggestionChosen;
        platformView.EditingDidBegin += AutoCompleteEntry_EditingDidBegin;
        platformView.EditingDidEnd += AutoCompleteEntry_EditingDidEnd;
    }

    private void InputTextFieldOnCursorPositionChanged(object sender, AutoCompleteEntryCursorPositionChangedEventArgs e)
    {
        VirtualView?.OnCursorPositionChanged(e.CursorPosition);
    }

    /// <inheritdoc/>
    protected override void DisconnectHandler(IOSAutoCompleteEntry platformView)
    {
        platformView.OnLoaded -= AutoCompleteEntry_OnLoaded;
        platformView.TextChanged -= AutoCompleteEntry_TextChanged;
        platformView.CursorPositionChanged -= InputTextFieldOnCursorPositionChanged;
        platformView.SuggestionChosen -= AutoCompleteEntry_SuggestionChosen;
        platformView.EditingDidBegin -= AutoCompleteEntry_EditingDidBegin;
        platformView.EditingDidEnd -= AutoCompleteEntry_EditingDidEnd;

        base.DisconnectHandler(platformView);
    }

    private void AutoCompleteEntry_OnLoaded(object sender, EventArgs e)
    {
        PlatformView.UpdateText(VirtualView);
        PlatformView.UpdatePlaceholder(VirtualView);
        PlatformView.UpdatePlaceholder(VirtualView);
        PlatformView.InputTextField.UpdateHorizontalTextAlignment(VirtualView);
        PlatformView.UpdateMaxLength(VirtualView);
        PlatformView.UpdateIsReadOnly(VirtualView);
        PlatformView.UpdateDisplayMemberPath(VirtualView);
        PlatformView.UpdateIsEnabled(VirtualView);
        PlatformView.UpdateUpdateTextOnSelect(VirtualView);
        PlatformView.UpdateIsSuggestionListOpen(VirtualView);
        PlatformView.UpdateItemsSource(VirtualView);
    }

    private void AutoCompleteEntry_TextChanged(object sender, AutoCompleteEntryTextChangedEventArgs e)
    {
        VirtualView.OnTextChanged(PlatformView.Text, e.Reason);
    }

    private void AutoCompleteEntry_SuggestionChosen(object sender, AutoCompleteEntrySuggestionChosenEventArgs e)
    {
        VirtualView.OnSuggestionSelected(e.SelectedItem);
    }

    private void AutoCompleteEntry_EditingDidBegin(object sender, EventArgs e)
    {
        VirtualView.Focus();
    }

    private void AutoCompleteEntry_EditingDidEnd(object sender, EventArgs e)
    {
        VirtualView.Unfocus();
    }

    /// <summary>
    /// Map the background value
    /// </summary>
    /// <param name="handler"></param>
    /// <param name="entry"></param>
    public static void MapBackground(IAutoCompleteEntryHandler handler, IEntry entry)
    {
        handler.PlatformView?.InputTextField.UpdateBackground(entry);
    }

    /// <summary>
    /// Map the CursorPosition value
    /// </summary>
    /// <param name="handler"></param>
    /// <param name="entry"></param>
    public static void MapCursorPosition(IAutoCompleteEntryHandler handler, IEntry entry)
    {
        handler.PlatformView?.InputTextField.UpdateCursorPosition(entry);
    }

    /// <summary>
    /// Map the IsEnabled value
    /// </summary>
    /// <param name="handler"></param>
    /// <param name="entry"></param>
    public static void MapIsEnabled(IAutoCompleteEntryHandler handler, IEntry entry)
    {
        handler.PlatformView?.InputTextField.UpdateIsEnabled(entry);
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
        handler.PlatformView?.InputTextField.UpdateVerticalTextAlignment(autoCompleteEntry);
    }

    /// <summary>
    /// Map the PlaceholderColor value
    /// </summary>
    /// <param name="handler"></param>
    /// <param name="autoCompleteEntry"></param>
    public static void MapPlaceholderColor(IAutoCompleteEntryHandler handler, AutoCompleteEntry autoCompleteEntry)
    {
        handler.PlatformView?.UpdatePlaceholder(autoCompleteEntry);
    }

    /// <summary>
    /// Map the HorizontalTextAlignment value
    /// </summary>
    /// <param name="handler"></param>
    /// <param name="autoCompleteEntry"></param>
    public static void MapHorizontalTextAlignment(IAutoCompleteEntryHandler handler, AutoCompleteEntry autoCompleteEntry)
    {
        handler.PlatformView?.InputTextField.UpdateHorizontalTextAlignment(autoCompleteEntry);
    }

    /// <summary>
    /// Map the Font value
    /// </summary>
    /// <param name="handler"></param>
    /// <param name="autoCompleteEntry"></param>
    public static void MapFont(IAutoCompleteEntryHandler handler, AutoCompleteEntry autoCompleteEntry)
    {
        var context = handler.MauiContext ??
                      throw new InvalidOperationException($"Unable to find the context. The {nameof(MauiContext)} property should have been set by the host.");

        var services = context?.Services ??
                       throw new InvalidOperationException($"Unable to find the service provider. The {nameof(MauiContext)} property should have been set by the host.");

        var fontManager = services.GetRequiredService<IFontManager>();

        handler.PlatformView?.InputTextField.UpdateFont(autoCompleteEntry, fontManager);
    }

    /// <summary>
    /// Map the CharacterSpacing value
    /// </summary>
    /// <param name="handler"></param>
    /// <param name="autoCompleteEntry"></param>
    public static void MapCharacterSpacing(IAutoCompleteEntryHandler handler, AutoCompleteEntry autoCompleteEntry)
    {
        handler.PlatformView?.InputTextField.UpdateCharacterSpacing(autoCompleteEntry);
    }

    /// <summary>
    /// Map the TextColor value
    /// </summary>
    /// <param name="handler"></param>
    /// <param name="autoCompleteEntry"></param>
    public static void MapTextColor(IAutoCompleteEntryHandler handler, AutoCompleteEntry autoCompleteEntry)
    {
        handler?.PlatformView?.InputTextField.UpdateTextColor(autoCompleteEntry);
    }

    /// <summary>
    /// Map the TextPredictionEnabled value
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
    /// <param name="autoCompleteEntry"></param>
    public static void MapMaxLength(IAutoCompleteEntryHandler handler, AutoCompleteEntry autoCompleteEntry)
    {
        handler.PlatformView?.UpdateMaxLength(autoCompleteEntry);
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
    /// Map the TextMemberPath value
    /// </summary>
    /// <param name="handler"></param>
    /// <param name="autoCompleteEntry"></param>
    public static void MapTextMemberPath(IAutoCompleteEntryHandler handler, AutoCompleteEntry autoCompleteEntry)
    {
        // IOSAutoCompleteEntry does not support this property
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
        handler?.PlatformView?.UpdateSelectedSuggestion(autoCompleteEntry);
    }
}