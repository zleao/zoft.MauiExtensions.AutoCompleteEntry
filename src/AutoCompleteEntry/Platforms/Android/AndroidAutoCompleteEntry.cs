using Android.Content;
using Android.Graphics.Drawables;
using Android.Runtime;
using Android.Text;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using Java.Lang;
using Microsoft.Maui.Platform;
using System.Collections;
using Color = Microsoft.Maui.Graphics.Color;

namespace zoft.MauiExtensions.Controls.Platform;

/// <summary>
///  Extends AppCompatAutoCompleteTextView to have similar APIs and behavior to WinUI's AutoSuggestBox, which greatly simplifies wrapping it
/// </summary>
public sealed partial class AndroidAutoCompleteEntry : AppCompatAutoCompleteTextView
{
    private bool _suppressTextChangedEvent;
    private bool _showBottomBorder = true;
    private Func<object, string> _textMemberPathFunc;
    private readonly AutoCompleteEntryAdapter _adapter;
    private Drawable _originalBackground;

    /// <summary>
    /// Initializes a new instance of the <see cref="AndroidAutoCompleteEntry"/>.
    /// </summary>
    public AndroidAutoCompleteEntry(Context context) : base(context)
    {
        SetMaxLines(1);

        //Search should be triggered even with empty text field
        Threshold = 0;

        //Disables text suggestions
        InputType = InputTypes.TextFlagNoSuggestions | InputTypes.TextVariationVisiblePassword;

        //Listen to when a suggestion is selected
        ItemClick += OnItemClick;

        Adapter = _adapter = new AutoCompleteEntryAdapter(Context);

        _originalBackground = Background;

        UpdateBottomBorderVisibility();
    }

    public void FreeResources()
    {
        ItemClick -= OnItemClick;
        _adapter?.Dispose();
    }

    // Setting Threshold = 0 in the constructor does not allow the control to display suggestions when the Text property is null or empty.
    // This is by design by Android.
    // See https://stackoverflow.com/questions/2126717/android-autocompletetextview-show-suggestions-when-no-text-entered for details
    // Overriding this method to always returns true changes this behaviour.
    public override bool EnoughToFilter() => true;

    /// <inheritdoc />
    protected override void OnFocusChanged(bool gainFocus, [GeneratedEnum] FocusSearchDirection direction, global::Android.Graphics.Rect previouslyFocusedRect)
    {
        IsSuggestionListOpen = gainFocus;

        base.OnFocusChanged(gainFocus, direction, previouslyFocusedRect);
    }

    internal void SetItems(IList items, string displayMemberPath, Func<object, string> textMemberPathFunc)
    {
        _textMemberPathFunc = textMemberPathFunc;
        _adapter.UpdateList(items is null ? Enumerable.Empty<string>() : items.OfType<object>(), displayMemberPath);
    }

    /// <summary>
    /// Gets or sets the text displayed in the entry field
    /// </summary>
    public new string Text
    {
        get => base.Text;
        set
        {
            _suppressTextChangedEvent = true;
            var oldText = base.Text;
            base.Text = value;
            _suppressTextChangedEvent = false;
            TextChanged?.Invoke(this, new AutoCompleteEntryTextChangedEventArgs(
                oldText, base.Text, AutoCompleteEntryTextChangeReason.ProgrammaticChange));
        }
    }

    /// <summary>
    /// Sets the text color on the entry field
    /// </summary>
    /// <param name="color"></param>
    public void SetTextColor(Color color)
    {
        SetTextColor(color.ToPlatform());
    }

    /// <summary>
    /// Gets or sets the placeholder text to be displayed in the <see cref="AppCompatAutoCompleteTextView"/>
    /// </summary>
    public string PlaceholderText
    {
        set => HintFormatted = new Java.Lang.String(value as string ?? "");
    }

    /// <summary>
    /// Gets or sets the color of the <see cref="PlaceholderText"/>.
    /// </summary>
    /// <param name="color">color</param>
    public void SetPlaceholderTextColor(Color color)
    {
        SetHintTextColor(color.ToPlatform());
    }

    /// <summary>
    /// Sets a Boolean value indicating whether the drop-down portion of the <see cref="AutoCompleteEntry"/> is open.
    /// </summary>
    public bool IsSuggestionListOpen
    {
        set
        {
            if (value)
            {
                ShowDropDown();
            }
            else
            {
                DismissDropDown();
            }
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether items in the view will trigger an update of the editable text part of the <see cref="AutoCompleteEntry"/> when clicked.
    /// </summary>
    public bool UpdateTextOnSelect { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to render a border line under the text field
    /// </summary>
    public bool ShowBottomBorder
    {
        get => _showBottomBorder;
        set
        {
            _showBottomBorder = value;

            UpdateBottomBorderVisibility();
        }
    }

    private void UpdateBottomBorderVisibility()
    {
        if (ShowBottomBorder)
            Background = _originalBackground;
        else
            Background = null;
    }

    /// <inheritdoc />
    protected override void OnTextChanged(ICharSequence text, int start, int lengthBefore, int lengthAfter)
    {
        if (!_suppressTextChangedEvent)
        {
            TextChanged?.Invoke(this, new AutoCompleteEntryTextChangedEventArgs(null, base.Text, AutoCompleteEntryTextChangeReason.UserInput));
        }

        base.OnTextChanged(text, start, lengthBefore, lengthAfter);
    }

    protected override void OnSelectionChanged(int selStart, int selEnd)
    {
        base.OnSelectionChanged(selStart, selEnd);

        CursorPositionChanged?.Invoke(this, new AutoCompleteEntryCursorPositionChangedEventArgs(selStart));
    }

    private void DismissKeyboard()
    {
        var imm = Context?.GetSystemService(Context.InputMethodService) as InputMethodManager;
        imm?.HideSoftInputFromWindow(WindowToken, 0);
    }

    private void OnItemClick(object sender, AdapterView.ItemClickEventArgs e)
    {
        DismissKeyboard();
        var obj = _adapter.GetObject(e.Position);
        if (UpdateTextOnSelect)
        {
            _suppressTextChangedEvent = true;
            var oldText = base.Text;
            base.Text = _textMemberPathFunc(obj);
            _suppressTextChangedEvent = false;
            TextChanged?.Invoke(this, new AutoCompleteEntryTextChangedEventArgs(
                oldText, base.Text, AutoCompleteEntryTextChangeReason.SuggestionChosen));
        }
        SuggestionChosen?.Invoke(this, new AutoCompleteEntrySuggestionChosenEventArgs(obj));
    }

    private void OnClick(object sender, EventArgs e)
    {
        Text = string.Empty;
    }

    /// <inheritdoc />
    public override void OnEditorAction([GeneratedEnum] ImeAction actionCode)
    {
        if (actionCode == ImeAction.Done || actionCode == ImeAction.Next)
        {
            DismissDropDown();
            DismissKeyboard();
        }

        base.OnEditorAction(actionCode);
    }

    /// <inheritdoc />
    protected override void ReplaceText(ICharSequence text)
    {
        //Override to avoid updating textbox on itemclick. We'll do this later using TextMemberPath and raise the proper TextChanged event then
    }

    internal void SetItemTemplate(DataTemplate itemTemplate)
    {
        _adapter.ItemTemplate = itemTemplate;
    }

    /// <summary>
    /// Raised after the text content of the editable control component is updated.
    /// </summary>
    public new event EventHandler<AutoCompleteEntryTextChangedEventArgs> TextChanged;

    public event EventHandler<AutoCompleteEntryCursorPositionChangedEventArgs> CursorPositionChanged;

    /// <summary>
    /// Raised before the text content of the editable control component is updated.
    /// </summary>
    public event EventHandler<AutoCompleteEntrySuggestionChosenEventArgs> SuggestionChosen;
}