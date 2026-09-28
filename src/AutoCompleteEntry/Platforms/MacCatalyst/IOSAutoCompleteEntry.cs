using CoreGraphics;
using Foundation;
using ObjCRuntime;
using System.Collections;
using UIKit;

namespace zoft.MauiExtensions.Controls.Platform;

/// <summary>
///  Creates a UIView with dropdown with a similar API and behavior to UWP's AutoSuggestBox
/// </summary>
public sealed class IOSAutoCompleteEntry : UIView
{
    /// <summary>
    /// Raised after the text content of the editable control component is updated.
    /// </summary>
    public event EventHandler<AutoCompleteEntryTextChangedEventArgs>? TextChanged;

    /// <summary>
    /// Raised after the cursor position of the editable control component is updated.
    /// </summary>
    public event EventHandler<AutoCompleteEntryCursorPositionChangedEventArgs>? CursorPositionChanged;

    /// <summary>
    /// Raised before the text content of the editable control component is updated.
    /// </summary>
    public event EventHandler<AutoCompleteEntrySuggestionChosenEventArgs>? SuggestionChosen;

    internal EventHandler? Loaded;

    internal EventHandler? EditingDidBegin;

    internal EventHandler? EditingDidEnd;

    internal EventHandler? ShouldReturn;

    private nfloat _keyboardHeight;
    private NSLayoutConstraint? _bottomConstraint;
    private Func<object, string> _textFunc = static item => item?.ToString() ?? string.Empty;
    private CoreAnimation.CALayer? _border;
    private bool _showBottomBorder = true;
    private NSObject? _keyboardShownObserverToken;
    private NSObject? _keyboardHiddenObserverToken;
    internal AutoCompleteEntry? Owner { get; set; }
    private readonly UILabel _summary = new() { Lines = 1, LineBreakMode = UILineBreakMode.TailTruncation, UserInteractionEnabled = false, Hidden = true };
    private int _highlight = -1;
    private NSLayoutConstraint[]? _listConstraints;
    private bool _resourcesFreed;

    internal void ConnectOwner(AutoCompleteEntry owner)
    {
        Owner = owner;
        InputTextField.SearchKeyHandler = HandleSearchKeys;
        if (!_resourcesFreed) return;
        _resourcesFreed = false;
        InputTextField.EditingDidBegin += InputText_OnEditingDidBegin;
        InputTextField.EditingDidEnd += InputText_OnEditingDidEnd;
        InputTextField.EditingChanged += InputText_OnEditingChanged;
        InputTextField.SelectedTextRangeChanged += InputText_OnTextRangeChanged;
        InputTextField.ShouldReturn += InputText_OnShouldReturn;
        InputTextField.TouchDown += InputText_OnTouchDown;
        _keyboardShownObserverToken = UIKeyboard.Notifications.ObserveDidShow(OnKeyboardShow);
        _keyboardHiddenObserverToken = UIKeyboard.Notifications.ObserveWillHide(OnKeyboardHide);
    }

    /// <summary>
    /// Gets or sets the template used to render suggestion rows.
    /// </summary>
    public DataTemplate? ItemTemplate { get; set; }

    /// <summary>
    /// Gets a reference to the text field in the view
    /// </summary>
    public MyUITextField InputTextField { get; } = CreateInputTextField();

    /// <summary>
    /// Gets a reference to the drop down selection list in the view
    /// </summary>
    public UITableView SelectionList { get; } = new() { TranslatesAutoresizingMaskIntoConstraints = false };

    /// <summary>
    /// Gets or sets the text displayed in the <see cref="InputTextField"/>
    /// </summary>
    public string? Text
    {
        get => InputTextField.Text;
        set
        {
            InputTextField.Text = value;
            TextChanged?.Invoke(this, new AutoCompleteEntryTextChangedEventArgs(AutoCompleteEntryTextChangeReason.ProgrammaticChange));
            InputText_OnTextRangeChanged(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Gets or sets a Boolean value indicating whether the drop-down portion of the AutoSuggestBox is open.
    /// </summary>
    public bool IsSuggestionListOpen
    {
        get => _isSuggestionListOpen;
        set
        {
            if (_isSuggestionListOpen == value) return;
            _isSuggestionListOpen = value;
            if (!value) _highlight = -1;
            if (Owner is not null) Owner.IsSuggestionListOpen = value;
            RefreshSelection();
            UpdateSuggestionListOpenState();
        }
    }
    private bool _isSuggestionListOpen;

    /// <summary>
    /// Gets or sets a value indicating whether to render a border line under the text field
    /// </summary>
    public bool ShowBottomBorder
    {
        get => _showBottomBorder;
        set
        {
            _showBottomBorder = value;
            if (_border != null)
            {
                _border.Hidden = !value;
            }
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether items in the view will trigger an update of the editable text part of the AutoSuggestBox when clicked.
    /// </summary>
    public bool UpdateTextOnSelect { get; set; } = true;

    /// <summary>
    /// Initializes a new instance of the <see cref="IOSAutoCompleteEntry"/>.
    /// </summary>
    public IOSAutoCompleteEntry() : this(CGRect.Empty) { }

    /// <summary>
    /// Releases event subscriptions and native resources held by this view.
    /// </summary>
    public void FreeResources()
    {
        if (_resourcesFreed) return;
        _resourcesFreed = true;
        _isSuggestionListOpen = false;
        if (SelectionList.Source is AutoCompleteEntryTableSource source)
        {
            source.TableRowSelected -= SuggestionTableSource_TableRowSelected;
            SelectionList.Source = null;
            source.Dispose();
        }
        SelectionList.RemoveFromSuperview();
        ReleaseListConstraints();
        Owner = null;
        InputTextField.SearchKeyHandler = null;
        InputTextField.EditingDidBegin -= InputText_OnEditingDidBegin;
        InputTextField.EditingDidEnd -= InputText_OnEditingDidEnd;
        InputTextField.EditingChanged -= InputText_OnEditingChanged;
        InputTextField.SelectedTextRangeChanged -= InputText_OnTextRangeChanged;
        InputTextField.ShouldReturn -= InputText_OnShouldReturn;
        InputTextField.TouchDown -= InputText_OnTouchDown;

        _keyboardShownObserverToken?.Dispose();
        _keyboardHiddenObserverToken?.Dispose();

        if (_border != null && _border.SuperLayer != null)
        {
            _border.RemoveFromSuperLayer();
            _border.Dispose();
            _border = null;
        }
    }

    /// <summary>
    /// Create instance of <see cref="IOSAutoCompleteEntry"/>
    /// </summary>
    /// <param name="coder"></param>
    public IOSAutoCompleteEntry(NSCoder coder) : base(coder)
    {
        InitializeView();
    }

    /// <summary>
    /// Create instance of <see cref="IOSAutoCompleteEntry"/>
    /// </summary>
    /// <param name="t"></param>
    private IOSAutoCompleteEntry(NSObjectFlag t) : base(t)
    {
        InitializeView();
    }

    /// <summary>
    /// Create instance of <see cref="IOSAutoCompleteEntry"/>
    /// </summary>
    /// <param name="handle"></param>
    internal IOSAutoCompleteEntry(NativeHandle handle) : base(handle)
    {
        InitializeView();
    }

    /// <summary>
    /// Create instance of <see cref="IOSAutoCompleteEntry"/>
    /// </summary>
    /// <param name="frame"></param>
    public IOSAutoCompleteEntry(CGRect frame) : base(frame)
    {
        InitializeView();
    }

    private static MyUITextField CreateInputTextField() =>
        new()
        {
            TranslatesAutoresizingMaskIntoConstraints = false,
            BorderStyle = UITextBorderStyle.None,
            ReturnKeyType = UIReturnKeyType.Done,
            AutocorrectionType = UITextAutocorrectionType.No,
            ShouldReturn = field =>
            {
                field.ResignFirstResponder();
                return false;
            }
        };

    private void InitializeView()
    {
        InputTextField.EditingDidBegin += InputText_OnEditingDidBegin;
        InputTextField.EditingDidEnd += InputText_OnEditingDidEnd;
        InputTextField.EditingChanged += InputText_OnEditingChanged;
        InputTextField.SelectedTextRangeChanged += InputText_OnTextRangeChanged;
        InputTextField.ShouldReturn += InputText_OnShouldReturn;
        InputTextField.TouchDown += InputText_OnTouchDown;

        AddSubview(InputTextField);
        AddSubview(_summary);

        NSLayoutConstraint.ActivateConstraints(new[]
        {
            InputTextField.TopAnchor.ConstraintEqualTo(TopAnchor),
            InputTextField.LeftAnchor.ConstraintEqualTo(LeftAnchor),
            InputTextField.RightAnchor.ConstraintEqualTo(RightAnchor),
            InputTextField.BottomAnchor.ConstraintEqualTo(BottomAnchor),
        });

        _keyboardShownObserverToken = UIKeyboard.Notifications.ObserveDidShow(OnKeyboardShow);
        _keyboardHiddenObserverToken = UIKeyboard.Notifications.ObserveWillHide(OnKeyboardHide);
    }

    /// <inheritdoc />
    public override void MovedToWindow()
    {
        base.MovedToWindow();

        Loaded?.Invoke(this, EventArgs.Empty);

        UpdateSuggestionListOpenState();
    }

    private void InputText_OnEditingDidBegin(object? sender, EventArgs e)
    {
        IsSuggestionListOpen = true;
        EditingDidBegin?.Invoke(this, e);
    }

    private void InputText_OnEditingDidEnd(object? sender, EventArgs e)
    {
        IsSuggestionListOpen = false;
        EditingDidEnd?.Invoke(this, e);
    }

    private void InputText_OnEditingChanged(object? sender, EventArgs e)
    {
        TextChanged?.Invoke(this, new AutoCompleteEntryTextChangedEventArgs(AutoCompleteEntryTextChangeReason.UserInput));

        InputText_OnTextRangeChanged(sender, e);

        IsSuggestionListOpen = true;
    }

    private void InputText_OnTextRangeChanged(object? sender, EventArgs e)
    {
        var cp = InputTextField.GetOffsetFromPosition(InputTextField.BeginningOfDocument, InputTextField.SelectedTextRange?.Start ?? InputTextField.EndOfDocument).ToInt32();

        CursorPositionChanged?.Invoke(this, new AutoCompleteEntryCursorPositionChangedEventArgs(cp));
    }

    private bool InputText_OnShouldReturn(UITextField view)
    {
        ShouldReturn?.Invoke(this, EventArgs.Empty);
        return false;
    }

    /// <inheritdoc />
    public override void LayoutSubviews()
    {
        base.LayoutSubviews();
        _summary.Frame = InputTextField.Frame;

        AddBottomBorder();
    }

    private void AddBottomBorder()
    {
        if (_border != null) return;

        const float width = 1f;
        _border = new CoreAnimation.CALayer();
        _border.BorderColor = UIColor.LightGray.CGColor;
        _border.Frame = new CGRect(0, Frame.Size.Height - width, Frame.Size.Width, Frame.Size.Height);
        _border.BorderWidth = width;
        _border.Hidden = !ShowBottomBorder;
        Layer.AddSublayer(_border);
        Layer.MasksToBounds = true;
    }

    internal void SetItems(IList? items, string? displayMemberPath, Func<object, string> textFunc, IMauiContext mauiContext)
    {
        _highlight = -1;
        _textFunc = textFunc;

        if (SelectionList.Source is AutoCompleteEntryTableSource oldSource)
        {
            oldSource.TableRowSelected -= SuggestionTableSource_TableRowSelected;
            oldSource.Dispose();
        }

        SelectionList.Source = null;

        if (items != null)
        {
            var suggestionTableSource = new AutoCompleteEntryTableSource(SelectionList, items, displayMemberPath ?? string.Empty, ItemTemplate, mauiContext, Owner);
            suggestionTableSource.TableRowSelected += SuggestionTableSource_TableRowSelected;
            SelectionList.Source = suggestionTableSource;
            SelectionList.ReloadData();
        }
        else if (Owner?.IsMultiple != true)
        {
            IsSuggestionListOpen = false;
        }
        UpdateSuggestionListOpenState();
    }

    private void UpdateSuggestionListOpenState()
    {
        if (_isSuggestionListOpen && SelectionList.Source != null && SelectionList.Source.RowsInSection(SelectionList, 0) > 0)
        {
            var viewController = InputTextField.Window?.RootViewController;
            if (viewController == null)
            {
                return;
            }

            if (viewController.PresentedViewController != null)
            {
                viewController = viewController.PresentedViewController;
            }

            if (SelectionList.Superview == null)
            {
                viewController.Add(SelectionList);
                var selectionListSuperview = SelectionList.Superview!;
                _bottomConstraint = Owner?.IsMultiple == true
                    ? SelectionList.BottomAnchor.ConstraintEqualTo(selectionListSuperview.BottomAnchor, -_keyboardHeight)
                    : SelectionList.BottomAnchor.ConstraintGreaterThanOrEqualTo(selectionListSuperview.BottomAnchor, -_keyboardHeight);
                _listConstraints =
                [
                    SelectionList.TopAnchor.ConstraintEqualTo(InputTextField.BottomAnchor),
                    SelectionList.LeadingAnchor.ConstraintEqualTo(InputTextField.LeadingAnchor),
                    SelectionList.WidthAnchor.ConstraintEqualTo(InputTextField.WidthAnchor),
                    _bottomConstraint
                ];
                NSLayoutConstraint.ActivateConstraints(_listConstraints);
            }
            SelectionList.UpdateConstraints();
        }
        else
        {
            if (SelectionList.Superview != null)
            {
                ReleaseListConstraints();
                SelectionList.RemoveFromSuperview();
            }
        }
    }

    private void OnKeyboardHide(object? sender, UIKeyboardEventArgs e)
    {
        _keyboardHeight = 0;
        if (_bottomConstraint != null)
        {
            _bottomConstraint.Constant = _keyboardHeight;
            SelectionList.UpdateConstraints();
        }
    }

    private void OnKeyboardShow(object? sender, UIKeyboardEventArgs e)
    {
        _keyboardHeight = e.FrameEnd.Height;
        if (_bottomConstraint != null)
        {
            _bottomConstraint.Constant = -_keyboardHeight;
            SelectionList.UpdateConstraints();
        }
    }

    /// <inheritdoc />
    public override bool BecomeFirstResponder()
    {
        return InputTextField.BecomeFirstResponder();
    }

    /// <inheritdoc />
    public override bool ResignFirstResponder()
    {
        return InputTextField.ResignFirstResponder();
    }

    /// <inheritdoc />
    public override bool IsFirstResponder => InputTextField.IsFirstResponder;

    private void SuggestionTableSource_TableRowSelected(object? sender, TableRowSelectedEventArgs<object> e)
    {
        SelectionList.DeselectRow(e.SelectedItemIndexPath, false);
        var selection = e.SelectedItem;
        if (Owner?.IsMultiple == true)
        {
            Owner.OnSuggestionSelected(selection);
            return;
        }
        if (UpdateTextOnSelect)
        {
            InputTextField.Text = _textFunc(selection);
            TextChanged?.Invoke(this, new AutoCompleteEntryTextChangedEventArgs(AutoCompleteEntryTextChangeReason.SuggestionChosen));
            InputText_OnTextRangeChanged(sender, e);
        }
        SuggestionChosen?.Invoke(this, new AutoCompleteEntrySuggestionChosenEventArgs(selection));
        IsSuggestionListOpen = false;
        ResignFirstResponder();
    }

    private void ReleaseListConstraints()
    {
        if (_listConstraints is null) return;
        NSLayoutConstraint.DeactivateConstraints(_listConstraints);
        foreach (var constraint in _listConstraints) constraint.Dispose();
        _listConstraints = null;
        _bottomConstraint = null;
    }

    private void InputText_OnTouchDown(object? sender, EventArgs e)
    {
        if (Owner?.IsMultiple == true) IsSuggestionListOpen = true;
    }

    internal void RefreshSelection()
    {
        if (SelectionList.Source is AutoCompleteEntryTableSource source) source.RefreshSelection();
        var show = Owner?.ShowsSelectionSummary == true;
        _summary.Text = Owner?.SelectionSummary ?? string.Empty;
        _summary.Font = InputTextField.Font ?? UIFont.SystemFontOfSize(UIFont.SystemFontSize);
        _summary.TextColor = InputTextField.TextColor;
        _summary.TextAlignment = InputTextField.TextAlignment;
        _summary.Hidden = !show;
        _summary.BackgroundColor = InputTextField.BackgroundColor ?? BackgroundColor ?? UIColor.SystemBackground;
        _summary.IsAccessibilityElement = false;
        InputTextField.AccessibilityValue = show ? Owner?.SelectionSummary : InputTextField.Text;
    }

    private bool HandleSearchKeys(NSSet<UIPress> presses)
    {
        var key = presses.ToArray<UIPress>().FirstOrDefault()?.Key?.CharactersIgnoringModifiers;
        if (Owner?.IsMultiple == true && key == UIKeyCommand.Escape)
        {
            IsSuggestionListOpen = false;
            return true;
        }
        if (Owner?.IsMultiple == true && (key == UIKeyCommand.DownArrow || key == UIKeyCommand.UpArrow))
            IsSuggestionListOpen = true;
        if (Owner?.IsMultiple == true && IsSuggestionListOpen && Owner.ItemsSource is { Count: > 0 } items)
        {
            if (key == UIKeyCommand.DownArrow || key == UIKeyCommand.UpArrow)
            {
                if (_highlight >= 0 && _highlight < items.Count)
                    SelectionList.CellAt(NSIndexPath.FromRowSection(_highlight, 0))?.SetHighlighted(false, false);
                _highlight = Math.Clamp(_highlight + (key == UIKeyCommand.DownArrow ? 1 : -1), 0, items.Count - 1);
                var index = NSIndexPath.FromRowSection(_highlight, 0);
                SelectionList.ScrollToRow(index, UITableViewScrollPosition.Middle, false);
                SelectionList.CellAt(index)?.SetHighlighted(true, false);
                return true;
            }
            if (key == "\r" && _highlight >= 0 && _highlight < items.Count)
            {
                Owner.OnSuggestionSelected(items[_highlight]);
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Text field implementation that raises an event when the selected text range changes.
    /// </summary>
    public class MyUITextField : UITextField
    {
        internal Func<NSSet<UIPress>, bool>? SearchKeyHandler { get; set; }
        private bool _handledSearchPress;

        /// <inheritdoc />
        public override void PressesBegan(NSSet<UIPress> presses, UIPressesEvent evt)
        {
            _handledSearchPress = SearchKeyHandler?.Invoke(presses) == true;
            if (!_handledSearchPress) base.PressesBegan(presses, evt);
        }

        /// <inheritdoc />
        public override void PressesEnded(NSSet<UIPress> presses, UIPressesEvent evt)
        {
            if (!_handledSearchPress) base.PressesEnded(presses, evt);
            _handledSearchPress = false;
        }
        /// <summary>
        /// Raised when the selected text range changes.
        /// </summary>
        public event EventHandler<EventArgs>? SelectedTextRangeChanged;

        /// <summary>
        /// Gets or sets the currently selected text range.
        /// </summary>
        public override UITextRange? SelectedTextRange
        {
            get => base.SelectedTextRange;
            set
            {
                if (base.SelectedTextRange == null || !base.SelectedTextRange.Equals(value))
                {
                    base.SelectedTextRange = value;

                    SelectedTextRangeChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }
    }
}
