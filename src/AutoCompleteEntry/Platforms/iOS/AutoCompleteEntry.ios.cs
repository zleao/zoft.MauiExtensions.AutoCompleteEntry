using CoreGraphics;
using Foundation;
using ObjCRuntime;
using System.Collections;
using System.Collections.Specialized;
using System.Drawing;
using System.Runtime.InteropServices;
using UIKit;

namespace zoft.MauiExtensions.Controls.Platforms.iOS
{
    /// <summary>
    ///  Creates a UIView with dropdown with a similar API and behavior to UWP's AutoSuggestBox
    /// </summary>
    public class IOSAutoCompleteEntry : UIKit.UIView
    {
        /// <summary>
        /// Raised after the text content of the editable control component is updated.
        /// </summary>
        public event EventHandler<AutoCompleteEntryTextChangedEventArgs> TextChanged;

        /// <summary>
        /// Occurs when the user submits a search query.
        /// </summary>
        public event EventHandler<AutoCompleteEntryQuerySubmittedEventArgs> QuerySubmitted;

        /// <summary>
        /// Raised before the text content of the editable control component is updated.
        /// </summary>
        public event EventHandler<AutoCompleteEntrySuggestionChosenEventArgs> SuggestionChosen;

        internal EventHandler OnLoaded;

        internal EventHandler EditingDidBegin;

        internal EventHandler EditingDidEnd;

        private NFloat keyboardHeight;
        private NSLayoutConstraint bottomConstraint;
        private Func<object, string> textFunc;
        private CoreAnimation.CALayer border;
        private bool showBottomBorder = true;

        /// <summary>
        /// Gets a reference to the text field in the view
        /// </summary>
        public UITextField InputTextField { get; }

        /// <summary>
        /// Gets a reference to the drop down selection list in the view
        /// </summary>
        public UIKit.UITableView SelectionList { get; }

        /// <summary>
        /// Gets or sets the text displayed in the <see cref="InputTextField"/>
        /// </summary>
        public virtual string Text
        {
            get => InputTextField.Text;
            set
            {
                InputTextField.Text = value;
                this.TextChanged?.Invoke(this, new AutoCompleteEntryTextChangedEventArgs(AutoCompleteEntryTextChangeReason.ProgrammaticChange));
            }
        }

        /// <summary>
        /// Gets or sets a Boolean value indicating whether the drop-down portion of the AutoSuggestBox is open.
        /// </summary>
        public virtual bool IsSuggestionListOpen
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
            get => showBottomBorder;
            set
            {
                showBottomBorder = value;
                if (border != null)
                {
                    border.Hidden = !value;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether items in the view will trigger an update of the editable text part of the AutoSuggestBox when clicked.
        /// </summary>
        public virtual bool UpdateTextOnSelect { get; set; } = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="IOSAutoCompleteEntry"/>.
        /// </summary>
        public IOSAutoCompleteEntry() : this(RectangleF.Empty)
        {            
            InputTextField = new UITextField
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                BorderStyle = UITextBorderStyle.None,
                ReturnKeyType = UIReturnKeyType.Search,
                AutocorrectionType = UITextAutocorrectionType.No,
                ShouldReturn = InputText_OnShouldReturn,
            };
            InputTextField.EditingDidBegin += InputText_OnEditingDidBegin;
            InputTextField.EditingDidEnd += InputText_OnEditingDidEnd;
            InputTextField.EditingChanged += InputText_OnEditingChanged;

            AddSubview(InputTextField);

            InputTextField.TopAnchor.ConstraintEqualTo(TopAnchor).Active = true;
            InputTextField.LeftAnchor.ConstraintEqualTo(LeftAnchor).Active = true;
            InputTextField.WidthAnchor.ConstraintEqualTo(WidthAnchor).Active = true;
            InputTextField.HeightAnchor.ConstraintEqualTo(HeightAnchor).Active = true;
            
            SelectionList = new UITableView() { TranslatesAutoresizingMaskIntoConstraints = false };

            UIKeyboard.Notifications.ObserveWillShow(OnKeyboardShow);
            UIKeyboard.Notifications.ObserveWillHide(OnKeyboardHide);
        }

        public IOSAutoCompleteEntry(NSCoder coder) : base(coder)
        {
        }

        protected IOSAutoCompleteEntry(NSObjectFlag t) : base(t)
        {
        }

        protected internal IOSAutoCompleteEntry(NativeHandle handle) : base(handle)
        {
        }

        public IOSAutoCompleteEntry(CGRect frame) : base(frame)
        {
        }

        /// <inheritdoc />
        public override void MovedToWindow()
        {
            base.MovedToWindow();

            OnLoaded?.Invoke(this, EventArgs.Empty);

            UpdateSuggestionListOpenState();
        }

        private bool InputText_OnShouldReturn(UITextField field)
        {
            if (string.IsNullOrWhiteSpace(field.Text)) 
            { 
                return false; 
            }

            field.ResignFirstResponder();
            
            QuerySubmitted?.Invoke(this, new AutoCompleteEntryQuerySubmittedEventArgs(InputTextField.Text, null));
            
            return true;
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
            IsSuggestionListOpen = true;
        }

        /// <inheritdoc />
        public override void LayoutSubviews()
        {
            base.LayoutSubviews();
            AddBottomBorder();
        }

        private void AddBottomBorder()
        {
            border = new CoreAnimation.CALayer();
            var width = 1f;
            border.BorderColor = UIColor.LightGray.CGColor;
            border.Frame = new CGRect(0, Frame.Size.Height - width, Frame.Size.Width, Frame.Size.Height);
            border.BorderWidth = width;
            border.Hidden = !showBottomBorder;
            Layer.AddSublayer(border);
            Layer.MasksToBounds = true;
        }
       
        internal void SetItems(IList items, Func<object, string> labelFunc, Func<object, string> textFunc)
        {
            this.textFunc = textFunc;
            
            if (SelectionList.Source is TableSource oldSource)
            {
                oldSource.TableRowSelected -= SuggestionTableSource_TableRowSelected;
                oldSource.Dispose();
            }

            SelectionList.Source = null;

            if (items != null)
            {
                var suggestionTableSource = new TableSource(SelectionList, items, labelFunc);
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
                    return;
                if (viewController.ModalViewController != null)
                    viewController = viewController.ModalViewController;
                if (SelectionList.Superview == null)
                {
                    viewController.Add(SelectionList);
                }
                SelectionList.TopAnchor.ConstraintEqualTo(InputTextField.BottomAnchor).Active = true;
                SelectionList.LeftAnchor.ConstraintEqualTo(InputTextField.LeftAnchor).Active = true;
                SelectionList.WidthAnchor.ConstraintEqualTo(InputTextField.WidthAnchor).Active = true;
                bottomConstraint = SelectionList.BottomAnchor.ConstraintGreaterThanOrEqualTo(SelectionList.Superview.BottomAnchor, -keyboardHeight);
                bottomConstraint.Active = true;
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
            keyboardHeight = 0;
            if (bottomConstraint != null)
            {
                bottomConstraint.Constant = keyboardHeight;
                SelectionList.UpdateConstraints();
            }
        }

        private void OnKeyboardShow(object sender, UIKeyboardEventArgs e)
        {
            NSValue nsKeyboardBounds = (NSValue)e.Notification.UserInfo.ObjectForKey(UIKeyboard.FrameBeginUserInfoKey);
            var keyboardBounds = nsKeyboardBounds.RectangleFValue;
            keyboardHeight = keyboardBounds.Height;
            if (bottomConstraint != null)
            {
                bottomConstraint.Constant = -keyboardHeight;
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
                InputTextField.Text = textFunc(selection);
                TextChanged?.Invoke(this, new AutoCompleteEntryTextChangedEventArgs(AutoCompleteEntryTextChangeReason.SuggestionChosen));
            }
            SuggestionChosen?.Invoke(this, new AutoCompleteEntrySuggestionChosenEventArgs(selection));
            QuerySubmitted?.Invoke(this, new AutoCompleteEntryQuerySubmittedEventArgs(Text, selection));
            IsSuggestionListOpen = false;
        }

        private class TableSource : UITableViewSource
        {
            private readonly UITableView _view;
            private readonly IList _items;
            private readonly Func<object, string> _labelFunc;
            private readonly string _cellIdentifier;

            public TableSource(UITableView view, IList items, Func<object, string> labelFunc)
            {
                _view = view;
                _items = items;
                _labelFunc = labelFunc;
                _cellIdentifier = Guid.NewGuid().ToString();

                CheckIfItemsSourceIsNotifiable();
            }

            private void CheckIfItemsSourceIsNotifiable()
            {
                if(_items is INotifyCollectionChanged notifiableItems)
                {
                    notifiableItems.CollectionChanged += NotifiableItems_CollectionChanged;
                }
            }

            private void NotifiableItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
            {
                if (!MainThread.IsMainThread)
                {
                    MainThread.BeginInvokeOnMainThread(() => CollectionChanged(e));
                }
                else
                {
                    CollectionChanged(e);
                }
            }

            private void CollectionChanged(NotifyCollectionChangedEventArgs args)
            {
                _view.ReloadData();
            }

            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);
                
                if(disposing && _items is INotifyCollectionChanged notifiableItems)
                {
                    notifiableItems.CollectionChanged -= NotifiableItems_CollectionChanged;
                }
            }

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                var cell = tableView.DequeueReusableCell(_cellIdentifier);
                if (cell == null)
                    cell = new UITableViewCell(UITableViewCellStyle.Default, _cellIdentifier);

                var item = _items[indexPath.Row];

                cell.TextLabel.Text = _labelFunc(item);

                return cell;
            }

            public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
            {
                OnTableRowSelected(indexPath);
            }

            public override nint RowsInSection(UITableView tableview, nint section)
            {
                return _items.Count;
            }

            public override NFloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath)
            {
                return 30f;
            }

            public event EventHandler<TableRowSelectedEventArgs<object>> TableRowSelected;

            private void OnTableRowSelected(NSIndexPath itemIndexPath)
            {
                var item = _items[itemIndexPath.Row];
                var label = _labelFunc(item);
                TableRowSelected?.Invoke(this, new TableRowSelectedEventArgs<object>(item, label, itemIndexPath));
            }
        }

        private class TableRowSelectedEventArgs<T> : EventArgs
        {
            public TableRowSelectedEventArgs(T selectedItem, string selectedItemLabel, NSIndexPath selectedItemIndexPath)
            {
                SelectedItem = selectedItem;
                SelectedItemLabel = selectedItemLabel;
                SelectedItemIndexPath = selectedItemIndexPath;
            }

            public T SelectedItem { get; }
            public string SelectedItemLabel { get; }
            public NSIndexPath SelectedItemIndexPath { get; }
        }
    }
}