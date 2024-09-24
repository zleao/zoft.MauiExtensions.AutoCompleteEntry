using Android.Graphics.Drawables;
using Android.Views;
using Android.Views.InputMethods;
using AndroidX.Core.Content;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using zoft.MauiExtensions.Controls.Platform;

namespace zoft.MauiExtensions.Controls.Handlers;

public partial class AutoCompleteEntryHandler : ViewHandler<AutoCompleteEntry, AndroidAutoCompleteEntry>
{
    Drawable _clearButtonDrawable;
    bool _clearButtonVisible;

   

    /// <inheritdoc/>
    protected override AndroidAutoCompleteEntry CreatePlatformView() => new(Context);

    /// <inheritdoc/>
    protected override void ConnectHandler(AndroidAutoCompleteEntry platformView)
    {
        base.ConnectHandler(platformView);

        platformView.CursorPositionChanged += PlatformView_OnCursorPositionChanged;
        platformView.EditorAction += PlatformView_OnEditorAction;
        platformView.SuggestionChosen += PlatformView_OnSuggestionChosen;
        platformView.TextChanged += PlatformView_OnTextChanged;
        platformView.Touch += PlatformView_OnTouch;
        platformView.ViewAttachedToWindow += PlatformView_OnViewAttachedToWindow;
    }

    /// <inheritdoc/>
    protected override void DisconnectHandler(AndroidAutoCompleteEntry platformView)
    {
        platformView.CursorPositionChanged -= PlatformView_OnCursorPositionChanged;
        platformView.EditorAction -= PlatformView_OnEditorAction;
        platformView.SuggestionChosen -= PlatformView_OnSuggestionChosen;
        platformView.TextChanged -= PlatformView_OnTextChanged;
        platformView.Touch -= PlatformView_OnTouch;
        platformView.ViewAttachedToWindow -= PlatformView_OnViewAttachedToWindow;

        platformView.FreeResources();

        base.DisconnectHandler(platformView);
    }


    private void PlatformView_OnCursorPositionChanged(object sender, AutoCompleteEntryCursorPositionChangedEventArgs e)
    {
        VirtualView?.OnCursorPositionChanged(e.CursorPosition);
    }

    // Note: this is copied from MAUI's EntryHandler.Android.cs > OnEditorAction
    private void PlatformView_OnEditorAction(object sender, Android.Widget.TextView.EditorActionEventArgs e)
    {
        var returnType = VirtualView?.ReturnType;

        // Inside of the android implementations that map events to listeners, the default return value for "Handled" is always true
        // This means, just by subscribing to EditorAction/KeyPressed/etc.. you change the behavior of the control
        // So, we are setting handled to false here in order to maintain default behavior
        bool handled = false;
        if (returnType != null)
        {
            var actionId = e.ActionId;
            var evt = e.Event;
            ImeAction currentInputImeFlag = PlatformView.ImeOptions;

            // On API 34 it looks like they fixed the issue where the actionId is ImeAction.ImeNull when using a keyboard
            // so I'm just setting the actionId here to the current ImeOptions so the logic can all be simplified
            if (actionId == ImeAction.ImeNull && evt?.KeyCode == Keycode.Enter)
            {
                actionId = currentInputImeFlag;
            }

            // keyboard path
            if (evt?.KeyCode == Keycode.Enter && evt?.Action == KeyEventActions.Down)
            {
                handled = true;
            }
            else if (evt?.KeyCode == Keycode.Enter && evt?.Action == KeyEventActions.Up)
            {
                VirtualView?.SendCompleted();
            }
            // InputPaneView Path
            else if (evt?.KeyCode is null && (actionId == ImeAction.Done || actionId == currentInputImeFlag))
            {
                VirtualView?.SendCompleted();
            }
        }

        e.Handled = handled;
    }

    private void PlatformView_OnSuggestionChosen(object sender, AutoCompleteEntrySuggestionChosenEventArgs e)
    {
        VirtualView?.OnSuggestionSelected(e.SelectedItem);
    }

    private void PlatformView_OnTextChanged(object sender, AutoCompleteEntryTextChangedEventArgs e)
    {
        VirtualView?.OnTextChanged(PlatformView.Text, e.Reason);

        PlatformView.UpdateClearButtonVisibility(VirtualView);
    }

    private void PlatformView_OnTouch(object sender, Android.Views.View.TouchEventArgs e)
    {
        e.Handled = _clearButtonVisible
                    &&
                    VirtualView != null
                    &&
                    PlatformView.HandleClearButtonTouched(e, GetClearButtonDrawable);
    }

    private void PlatformView_OnViewAttachedToWindow(object sender, Android.Views.View.ViewAttachedToWindowEventArgs e)
    {
        PlatformView.UpdateClearButtonVisibility(VirtualView);
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
    /// Map the ReturnType value
    /// </summary>
    /// <param name="handler"></param>
    /// <param name="entry"></param>
    public static void MapReturnType(IAutoCompleteEntryHandler handler, IEntry entry)
    {
        handler.PlatformView?.UpdateReturnType(entry);
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

    /// <summary>
    /// Map the ItemTemplate value
    /// </summary>
    /// <param name="handler"></param>
    /// <param name="autoCompleteEntry"></param>
    public static void MapItemTemplate(IAutoCompleteEntryHandler handler, AutoCompleteEntry autoCompleteEntry)
    {
        handler?.PlatformView.UpdateItemTemplate(autoCompleteEntry);
    }

    // Returns the default 'X' char drawable in the AppCompatEditText.
    protected virtual Drawable GetClearButtonDrawable() =>
        _clearButtonDrawable ??= ContextCompat.GetDrawable(Context, Resource.Drawable.abc_ic_clear_material);

    internal void ShowClearButton()
    {
        if (_clearButtonVisible)
        {
            return;
        }

        var drawable = GetClearButtonDrawable();

        if (VirtualView?.TextColor is not null)
            drawable?.SetColorFilter(VirtualView.TextColor.ToPlatform(), FilterMode.SrcIn);
        else
            drawable?.ClearColorFilter();

        if (PlatformView.LayoutDirection == Android.Views.LayoutDirection.Rtl)
            PlatformView.SetCompoundDrawablesWithIntrinsicBounds(drawable, null, null, null);
        else
            PlatformView.SetCompoundDrawablesWithIntrinsicBounds(null, null, drawable, null);

        _clearButtonVisible = true;
    }

    internal void HideClearButton()
    {
        if (!_clearButtonVisible)
        {
            return;
        }

        PlatformView.SetCompoundDrawablesWithIntrinsicBounds(null, null, null, null);
        _clearButtonVisible = false;
    }
}