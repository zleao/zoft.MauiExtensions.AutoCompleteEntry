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
    private Func<object, string> _textMemberPathFunc = static item => item?.ToString() ?? string.Empty;
    private AutoCompleteEntryAdapter _adapter;
    private Drawable? _originalBackground;
    internal AutoCompleteEntry? Owner { get; set; }
    private PopupWindow? _multiplePopup;
    private Android.Widget.ListView? _multipleList;
    private bool _updatingPopup;
    private bool _updatingItems;
    private int _highlight = -1;
    private IList? _items;
    private string? _displayPath;
    private bool _consumeActivationKeyUp;
    private bool _resourcesFreed;
    private SuggestionPopupPlacement? _popupPlacement;
    private (int X, int Y, int Width, int Height, int Left, int Top, int Right, int Bottom)? _singleViewport;

    internal void ConnectOwner(AutoCompleteEntry owner)
    {
        Owner = owner;
        if (_resourcesFreed)
        {
            Adapter = _adapter = new AutoCompleteEntryAdapter(Context!);
            ItemClick += OnItemClick;
            Dismiss += OnNativeDismiss;
            _resourcesFreed = false;
        }
        _adapter.Owner = owner;
        if (IsAttachedToWindow) ObserveViewport();
    }

    protected override void OnAttachedToWindow()
    {
        base.OnAttachedToWindow();
        ObserveViewport();
    }

    private void ObserveViewport()
    {
        ViewTreeObserver!.GlobalLayout -= OnViewportChanged;
        ViewTreeObserver.GlobalLayout += OnViewportChanged;
        ViewTreeObserver.ScrollChanged -= OnViewportChanged;
        ViewTreeObserver.ScrollChanged += OnViewportChanged;
        ViewTreeObserver.PreDraw -= OnViewportPreDraw;
        ViewTreeObserver.PreDraw += OnViewportPreDraw;
    }

    protected override void OnDetachedFromWindow()
    {
        ViewTreeObserver!.GlobalLayout -= OnViewportChanged;
        ViewTreeObserver.ScrollChanged -= OnViewportChanged;
        ViewTreeObserver.PreDraw -= OnViewportPreDraw;
        if (Owner?.IsMultiple == true) CloseSuggestionList();
        base.OnDetachedFromWindow();
    }

    private void OnViewportChanged(object? sender, EventArgs e)
    {
        // The IME can appear after the first popup layout. Refit without ending
        // the search session or covering the user's keyboard.
        if (Owner is { IsMultiple: true, IsSuggestionListOpen: true } && _multiplePopup?.IsShowing == true)
            ShowDropDown();
        else if (Owner?.IsMultiple != true && IsPopupShowing)
        {
            // The native popup tracks its anchor but can retain the height from
            // before IME-driven parent scrolling. Re-measure it at the new anchor.
            var viewport = GetSingleViewport();
            if (_singleViewport != viewport) ShowDropDown();
        }
    }

    private void OnViewportPreDraw(object? sender, ViewTreeObserver.PreDrawEventArgs e)
    {
        // Window adjust-pan can move the editor after GlobalLayout/ScrollChanged.
        // Reconcile at draw time too; cached bounds prevent redundant updates.
        OnViewportChanged(sender, EventArgs.Empty);
        e.Handled = true; // OnPreDraw's return value: allow this frame to draw.
    }

    private (int X, int Y, int Width, int Height, int Left, int Top, int Right, int Bottom) GetSingleViewport()
    {
        using var visible = new Android.Graphics.Rect();
        GetWindowVisibleDisplayFrame(visible);
        var position = new int[2];
        GetLocationOnScreen(position);
        return (position[0], position[1], Width, Height, visible.Left, visible.Top, visible.Right, visible.Bottom);
    }

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
        Dismiss += OnNativeDismiss;

        Adapter = _adapter = new AutoCompleteEntryAdapter(Context ?? context);

        _originalBackground = Background;

        UpdateBottomBorderVisibility();
    }

    public void FreeResources()
    {
        if (_resourcesFreed) return;
        _resourcesFreed = true;
        ViewTreeObserver!.GlobalLayout -= OnViewportChanged;
        ViewTreeObserver.ScrollChanged -= OnViewportChanged;
        ViewTreeObserver.PreDraw -= OnViewportPreDraw;
        if (_items is System.Collections.Specialized.INotifyCollectionChanged observable)
            observable.CollectionChanged -= OnItemsChanged;
        _items = null;
        ItemClick -= OnItemClick;
        Dismiss -= OnNativeDismiss;
        base.DismissDropDown();
        if (_multiplePopup is not null)
        {
            _multipleList!.ItemClick -= OnMultipleItemClick;
            _multipleList.Adapter = null;
            _multiplePopup.DismissEvent -= OnMultipleDismiss;
            _multiplePopup.TouchIntercepted -= OnPopupTouch;
            _multiplePopup.Dismiss();
            _multiplePopup.Dispose();
            _multiplePopup = null;
            _multipleList.Dispose();
            _multipleList = null;
        }
        Owner = null;
        Adapter = null;
        _adapter?.Dispose();
    }

    // Setting Threshold = 0 in the constructor does not allow the control to display suggestions when the Text property is null or empty.
    // This is by design by Android.
    // See https://stackoverflow.com/questions/2126717/android-autocompletetextview-show-suggestions-when-no-text-entered for details
    // Overriding this method to always returns true changes this behaviour.
    public override bool EnoughToFilter() => true;

    protected override void PerformFiltering(ICharSequence? text, int keyCode)
    {
        if (Owner?.IsMultiple != true) base.PerformFiltering(text, keyCode);
    }

    /// <inheritdoc />
    protected override void OnFocusChanged(bool gainFocus, [GeneratedEnum] FocusSearchDirection direction, global::Android.Graphics.Rect? previouslyFocusedRect)
    {
        IsSuggestionListOpen = gainFocus;

        base.OnFocusChanged(gainFocus, direction, previouslyFocusedRect);
    }

    internal void SetItems(IList? items, string? displayMemberPath, Func<object, string> textMemberPathFunc)
    {
        if (!ReferenceEquals(_items, items))
        {
            if (_items is System.Collections.Specialized.INotifyCollectionChanged previous) previous.CollectionChanged -= OnItemsChanged;
            _items = items;
            if (_items is System.Collections.Specialized.INotifyCollectionChanged current) current.CollectionChanged += OnItemsChanged;
            _highlight = -1;
        }
        _displayPath = displayMemberPath;
        _adapter.Owner = Owner;
        _textMemberPathFunc = textMemberPathFunc;
        var updatingItems = _updatingItems;
        _updatingItems = true;
        try { _adapter.UpdateList(items is null ? Enumerable.Empty<string>() : items.OfType<object>(), displayMemberPath); }
        finally { _updatingItems = updatingItems; }
        if (Owner?.IsMultiple == true && Owner.IsSuggestionListOpen) ShowDropDown();
    }

    private void OnItemsChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        if (Owner?.IsMultiple != true) return;
        _highlight = -1;
        SetItems(_items, _displayPath, _textMemberPathFunc);
    }

    /// <summary>
    /// Gets or sets the text displayed in the entry field
    /// </summary>
    public new string? Text
    {
        get => base.Text;
        set
        {
            _suppressTextChangedEvent = true;
            base.Text = value;
            _suppressTextChangedEvent = false;
            TextChanged?.Invoke(this, new AutoCompleteEntryTextChangedEventArgs(AutoCompleteEntryTextChangeReason.ProgrammaticChange));
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
        set => HintFormatted = new Java.Lang.String(value ?? string.Empty);
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
                CloseSuggestionList();
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
    protected override void OnTextChanged(ICharSequence? text, int start, int lengthBefore, int lengthAfter)
    {
        if (!_suppressTextChangedEvent)
        {
            TextChanged?.Invoke(this, new AutoCompleteEntryTextChangedEventArgs(AutoCompleteEntryTextChangeReason.UserInput));
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

    private void OnItemClick(object? sender, AdapterView.ItemClickEventArgs e)
    {
        if (Owner?.IsMultiple == true) { OnMultipleItemClick(sender, e); return; }
        DismissKeyboard();
        var obj = _adapter.GetObject(e.Position);
        if (UpdateTextOnSelect)
        {
            _suppressTextChangedEvent = true;
            base.Text = _textMemberPathFunc(obj);
            _suppressTextChangedEvent = false;
            TextChanged?.Invoke(this, new AutoCompleteEntryTextChangedEventArgs(AutoCompleteEntryTextChangeReason.SuggestionChosen));
        }
        SuggestionChosen?.Invoke(this, new AutoCompleteEntrySuggestionChosenEventArgs(obj));
    }

    private void OnClick(object? sender, EventArgs e)
    {
        Text = string.Empty;
    }

    /// <inheritdoc />
    public override void OnEditorAction([GeneratedEnum] ImeAction actionCode)
    {
        if (actionCode == ImeAction.Done || actionCode == ImeAction.Next)
        {
            CloseSuggestionList();
            DismissKeyboard();
        }

        base.OnEditorAction(actionCode);
    }

    /// <inheritdoc />
    protected override void ReplaceText(ICharSequence? text)
    {
        //Override to avoid updating textbox on itemclick. We'll do this later using TextMemberPath and raise the proper TextChanged event then
    }

    internal void SetItemTemplate(DataTemplate? itemTemplate)
    {
        _adapter.ItemTemplate = itemTemplate;
        // ListView captures ViewTypeCount when its adapter is assigned. Reset its
        // recycling pools when switching between a template and a selector.
        if (_multipleList is not null) _multipleList.Adapter = _adapter;
        _highlight = -1;
    }

    internal void RefreshSelection(bool modeChanged = false)
    {
        if (Owner?.IsMultiple != true && !modeChanged) return;
        _adapter.Owner = Owner;
        if (modeChanged)
        {
            _updatingPopup = true;
            try { base.DismissDropDown(); _multiplePopup?.Dismiss(); }
            finally { _updatingPopup = false; }
            // AutoCompleteTextView observes its adapter and asynchronously dismisses
            // on zero results. Multiple mode owns a separate list and must not let
            // that observer terminate a still-active query session.
            Adapter = Owner?.IsMultiple == true ? null : _adapter;
            _adapter.ResetPresentation();
        }
        else _adapter.RefreshSelection();
        ContentDescription = Owner?.ShowsSelectionSummary == true ? Owner.SelectionSummary : null;
        Invalidate();
    }

    public override void ShowDropDown()
    {
        if (_updatingPopup) return;
        _updatingPopup = true;
        try
        {
            if (Owner is not null) Owner.IsSuggestionListOpen = true;
            if (Owner?.IsMultiple != true)
            {
                _singleViewport = GetSingleViewport();
                base.ShowDropDown();
                return;
            }
            if (WindowToken is null) return;
            if (_multiplePopup is null)
            {
                _multipleList = new Android.Widget.ListView(Context!) { Adapter = _adapter, ItemsCanFocus = false };
                _multipleList.ItemClick += OnMultipleItemClick;
                _multiplePopup = new PopupWindow(_multipleList, Width, 1, false)
                {
                    OutsideTouchable = true,
                    InputMethodMode = Android.Widget.InputMethod.Needed
                };
                using var background = new Android.Util.TypedValue();
                Context!.Theme!.ResolveAttribute(Android.Resource.Attribute.ColorBackground, background, true);
                _multiplePopup.SetBackgroundDrawable(new ColorDrawable(new Android.Graphics.Color(background.Data)));
                _multiplePopup.DismissEvent += OnMultipleDismiss;
                _multiplePopup.TouchIntercepted += OnPopupTouch;
            }
            var density = Resources?.DisplayMetrics?.Density ?? 1;
            using var visible = new Android.Graphics.Rect();
            GetWindowVisibleDisplayFrame(visible);
            var position = new int[2];
            GetLocationOnScreen(position);
            // Keep an empty-result session alive without an opaque empty panel.
            var screenPlacement = SuggestionPopupPlacement.Calculate(position[0], position[1], Width, Height,
                visible.Left, visible.Top, visible.Right, visible.Bottom, _adapter.Count == 0 ? 1 : (int)(320 * density));
            var windowPosition = new int[2];
            GetLocationInWindow(windowPosition);
            var placement = screenPlacement with
            {
                X = screenPlacement.X - position[0] + windowPosition[0],
                Y = screenPlacement.Y - position[1] + windowPosition[1]
            };
            if (_multiplePopup.IsShowing)
            {
                if (_popupPlacement != placement)
                {
                    _popupPlacement = placement;
                    _multiplePopup.Update(placement.X, placement.Y, placement.Width, placement.Height);
                }
            }
            else
            {
                _popupPlacement = placement;
                _multiplePopup.Width = placement.Width;
                _multiplePopup.Height = placement.Height;
                _multiplePopup.ShowAtLocation(this, GravityFlags.Top | GravityFlags.Left, placement.X, placement.Y);
            }
        }
        finally { _updatingPopup = false; }
    }

    public override void DismissDropDown()
    {
        // Calls from AutoCompleteTextView's internal text/filter observers concern
        // only its own dropdown. Explicit closes use CloseSuggestionList instead.
        if (Owner?.IsMultiple == true) { base.DismissDropDown(); return; }
        CloseSuggestionList();
    }

    private void CloseSuggestionList()
    {
        if (_updatingPopup) return;
        base.DismissDropDown();
        _multiplePopup?.Dismiss();
        if (!_updatingItems && Owner is not null) Owner.IsSuggestionListOpen = false;
        _highlight = -1;
    }

    private void OnNativeDismiss(object? sender, EventArgs e)
    {
        if (!_updatingPopup && !_updatingItems && Owner is { IsMultiple: false }) Owner.IsSuggestionListOpen = false;
    }

    private void OnMultipleDismiss(object? sender, EventArgs e)
    {
        if (!_updatingPopup && Owner is not null) Owner.IsSuggestionListOpen = false;
        _highlight = -1;
    }

    private void OnMultipleItemClick(object? sender, AdapterView.ItemClickEventArgs e)
        => Owner?.OnSuggestionSelected(_adapter.GetObject(e.Position));

    private void OnPopupTouch(object? sender, Android.Views.View.TouchEventArgs e)
    {
        e.Handled = false;
        if (e.Event?.Action != MotionEventActions.Outside) return;
        var position = new int[2];
        GetLocationOnScreen(position);
        // Clicking the editor is part of the current query session, not dismissal.
        e.Handled = e.Event.RawX >= position[0] && e.Event.RawX < position[0] + Width &&
            e.Event.RawY >= position[1] && e.Event.RawY < position[1] + Height;
    }

    public override bool OnKeyDown([GeneratedEnum] Keycode keyCode, KeyEvent? e)
    {
        if (Owner?.IsMultiple == true && Owner.IsSuggestionListOpen)
        {
            if (keyCode is Keycode.DpadDown or Keycode.DpadUp && _adapter.Count > 0)
            {
                _highlight = System.Math.Clamp(_highlight + (keyCode == Keycode.DpadDown ? 1 : -1), 0, _adapter.Count - 1);
                _multipleList?.SetSelection(_highlight);
                return true;
            }
            if (keyCode is Keycode.Enter or Keycode.DpadCenter && _highlight >= 0 && _highlight < _adapter.Count)
            {
                if (e?.RepeatCount == 0) Owner.OnSuggestionSelected(_adapter.GetObject(_highlight));
                _consumeActivationKeyUp = true;
                return true;
            }
            if (keyCode == Keycode.Escape) { CloseSuggestionList(); return true; }
        }
        return base.OnKeyDown(keyCode, e);
    }

    /// <inheritdoc />
    public override bool OnKeyUp([GeneratedEnum] Keycode keyCode, KeyEvent? e)
    {
        if (_consumeActivationKeyUp && keyCode is Keycode.Enter or Keycode.DpadCenter)
        {
            _consumeActivationKeyUp = false;
            return true;
        }
        return base.OnKeyUp(keyCode, e);
    }

    /// <inheritdoc />
    public override bool OnKeyPreIme([GeneratedEnum] Keycode keyCode, KeyEvent? e)
    {
        if (Owner?.IsMultiple == true && Owner.IsSuggestionListOpen && keyCode == Keycode.Back)
        {
            if (e?.Action == KeyEventActions.Up)
            {
                CloseSuggestionList();
                DismissKeyboard();
            }
            return true;
        }
        return base.OnKeyPreIme(keyCode, e);
    }

    protected override void OnDraw(Android.Graphics.Canvas canvas)
    {
        if (Owner?.ShowsSelectionSummary != true) { base.OnDraw(canvas); return; }
        var paint = Paint!;
        paint.Color = new Android.Graphics.Color(CurrentTextColor);
        var clipped = TextUtils.Ellipsize(Owner.SelectionSummary, paint, System.Math.Max(0, Width - PaddingLeft - PaddingRight), TextUtils.TruncateAt.End);
        var x = LayoutDirection == Android.Views.LayoutDirection.Rtl
            ? Width - PaddingRight - paint.MeasureText(clipped?.ToString()) : PaddingLeft;
        canvas.DrawText(clipped?.ToString() ?? string.Empty, x, Baseline, paint);
    }

    internal void ClearQueryFromUser()
    {
        // Bypass the programmatic Text setter so the normal user-input event path runs.
        base.Text = string.Empty;
    }

    /// <summary>
    /// Raised after the text content of the editable control component is updated.
    /// </summary>
    public new event EventHandler<AutoCompleteEntryTextChangedEventArgs>? TextChanged;

    public event EventHandler<AutoCompleteEntryCursorPositionChangedEventArgs>? CursorPositionChanged;

    /// <summary>
    /// Raised before the text content of the editable control component is updated.
    /// </summary>
    public event EventHandler<AutoCompleteEntrySuggestionChosenEventArgs>? SuggestionChosen;
}
