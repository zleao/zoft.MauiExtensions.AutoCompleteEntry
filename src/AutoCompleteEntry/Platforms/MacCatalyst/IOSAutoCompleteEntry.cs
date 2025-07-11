using CoreGraphics;
using Foundation;
using ObjCRuntime;
using System.Collections;
using System.Collections.Specialized;
using System.Drawing;
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
    public event EventHandler<AutoCompleteEntryTextChangedEventArgs> TextChanged;
        
    /// <summary>
    /// Raised after the cursor position of the editable control component is updated.
    /// </summary>
    public event EventHandler<AutoCompleteEntryCursorPositionChangedEventArgs> CursorPositionChanged;

    /// <summary>
    /// Raised before the text content of the editable control component is updated.
    /// </summary>
    public event EventHandler<AutoCompleteEntrySuggestionChosenEventArgs> SuggestionChosen;

    internal EventHandler Loaded;

    internal EventHandler EditingDidBegin;

    internal EventHandler EditingDidEnd;

    internal EventHandler ShouldReturn;

    private nfloat _keyboardHeight;
    private NSLayoutConstraint _bottomConstraint;
    private Func<object, string> _textFunc;
    private CoreAnimation.CALayer _border;
    private bool _showBottomBorder = true;
    private readonly NSObject _keyboardShownObserverToken;
    private readonly NSObject _keyboardHiddenObserverToken;

    public DataTemplate ItemTemplate { get; set; }

    /// <summary>
    /// Gets a reference to the text field in the view
    /// </summary>
    public MyUITextField InputTextField { get; }

    /// <summary>
    /// Gets a reference to the drop down selection list in the view
    /// </summary>
    public UITableView SelectionList { get; }

    /// <summary>
    /// Gets or sets the text displayed in the <see cref="InputTextField"/>
    /// </summary>
    public string Text
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
            _isSuggestionListOpen = value;
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
    public IOSAutoCompleteEntry() : this(RectangleF.Empty)
    {
        InputTextField = new MyUITextField
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
        InputTextField.EditingDidBegin += InputText_OnEditingDidBegin;
        InputTextField.EditingDidEnd += InputText_OnEditingDidEnd;
        InputTextField.EditingChanged += InputText_OnEditingChanged;
        InputTextField.SelectedTextRangeChanged += InputText_OnTextRangeChanged;
        InputTextField.ShouldReturn += InputText_OnShouldReturn;

        AddSubview(InputTextField);    

        InputTextField.TopAnchor.ConstraintEqualTo(TopAnchor).Active = true;
        InputTextField.LeftAnchor.ConstraintEqualTo(LeftAnchor).Active = true;
        InputTextField.WidthAnchor.ConstraintEqualTo(WidthAnchor).Active = true;
        InputTextField.HeightAnchor.ConstraintEqualTo(HeightAnchor).Active = true;

        SelectionList = new UITableView { TranslatesAutoresizingMaskIntoConstraints = false };

        _keyboardShownObserverToken = UIKeyboard.Notifications.ObserveDidShow(OnKeyboardShow);
        _keyboardHiddenObserverToken = UIKeyboard.Notifications.ObserveWillHide(OnKeyboardHide);
    }

    public void FreeResources()
    {
        InputTextField.EditingDidBegin -= InputText_OnEditingDidBegin;
        InputTextField.EditingDidEnd -= InputText_OnEditingDidEnd;
        InputTextField.EditingChanged -= InputText_OnEditingChanged;
        InputTextField.SelectedTextRangeChanged -= InputText_OnTextRangeChanged;
        InputTextField.ShouldReturn -= InputText_OnShouldReturn;

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
    }

    /// <summary>
    /// Create instance of <see cref="IOSAutoCompleteEntry"/>
    /// </summary>
    /// <param name="t"></param>
    private IOSAutoCompleteEntry(NSObjectFlag t) : base(t)
    {
    }

    /// <summary>
    /// Create instance of <see cref="IOSAutoCompleteEntry"/>
    /// </summary>
    /// <param name="handle"></param>
    internal IOSAutoCompleteEntry(NativeHandle handle) : base(handle)
    {
    }

    /// <summary>
    /// Create instance of <see cref="IOSAutoCompleteEntry"/>
    /// </summary>
    /// <param name="frame"></param>
    public IOSAutoCompleteEntry(CGRect frame) : base(frame)
    {
    }

    /// <inheritdoc />
    public override void MovedToWindow()
    {
        base.MovedToWindow();

        Loaded?.Invoke(this, EventArgs.Empty);

        UpdateSuggestionListOpenState();
    }

    private void InputText_OnEditingDidBegin(object sender, EventArgs e)
    {
        IsSuggestionListOpen = true;
        EditingDidBegin?.Invoke(this, e);
    }

    private void InputText_OnEditingDidEnd(object sender, EventArgs e)
    {
        IsSuggestionListOpen = false;
        EditingDidEnd?.Invoke(this, e);
    }

    private void InputText_OnEditingChanged(object sender, EventArgs e)
    {
        TextChanged?.Invoke(this, new AutoCompleteEntryTextChangedEventArgs(AutoCompleteEntryTextChangeReason.UserInput));

        InputText_OnTextRangeChanged(sender, e);
            
        IsSuggestionListOpen = true;
    }

    private void InputText_OnTextRangeChanged(object sender, EventArgs e)
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

    internal void SetItems(IList items, string displayMemberPath, Func<object, string> textFunc, IMauiContext mauiContext)
    {
        _textFunc = textFunc;

        if (SelectionList.Source is AutoCompleteEntryTableSource oldSource)
        {
            oldSource.TableRowSelected -= SuggestionTableSource_TableRowSelected;
            oldSource.Dispose();
        }

        SelectionList.Source = null;

        if (items != null)
        {
            var suggestionTableSource = new AutoCompleteEntryTableSource(SelectionList, items, displayMemberPath, ItemTemplate, mauiContext);
            suggestionTableSource.TableRowSelected += SuggestionTableSource_TableRowSelected;
            SelectionList.Source = suggestionTableSource;
            SelectionList.ReloadData();
        }
        else
        {
            IsSuggestionListOpen = false;
        }
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

#if MACCATALYST14_2_OR_GREATER
            if (viewController.PresentedViewController != null)
            {
                viewController = viewController.PresentedViewController;
            }
#else
            if (viewController.ModalViewController != null)
            {
                viewController = viewController.ModalViewController;
            }
#endif

            if (SelectionList.Superview == null)
            {
                viewController.Add(SelectionList);
            }

            SelectionList.TopAnchor.ConstraintEqualTo(InputTextField.BottomAnchor).Active = true;
            SelectionList.LeftAnchor.ConstraintEqualTo(InputTextField.LeftAnchor).Active = true;
            SelectionList.WidthAnchor.ConstraintEqualTo(InputTextField.WidthAnchor).Active = true;
            _bottomConstraint = SelectionList.BottomAnchor.ConstraintGreaterThanOrEqualTo(SelectionList.Superview.BottomAnchor, -_keyboardHeight);
            _bottomConstraint.Active = true;
            SelectionList.UpdateConstraints();
        }
        else
        {
            if (SelectionList.Superview != null)
                SelectionList.RemoveFromSuperview();
        }
    }

    private void OnKeyboardHide(object sender, UIKeyboardEventArgs e)
    {
        _keyboardHeight = 0;
        if (_bottomConstraint != null)
        {
            _bottomConstraint.Constant = _keyboardHeight;
            SelectionList.UpdateConstraints();
        }
    }

    private void OnKeyboardShow(object sender, UIKeyboardEventArgs e)
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

    private void SuggestionTableSource_TableRowSelected(object sender, TableRowSelectedEventArgs<object> e)
    {
        SelectionList.DeselectRow(e.SelectedItemIndexPath, false);
        var selection = e.SelectedItem;
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

    public class MyUITextField : UITextField
    {
        public event EventHandler<EventArgs> SelectedTextRangeChanged;
            
        public override UITextRange SelectedTextRange
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