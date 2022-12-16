using Android.Content;
using Android.Runtime;
using Android.Text;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using Java.Lang;
using Microsoft.Maui.Controls.Compatibility.Platform.Android;
using System.Collections;
using System.Collections.Specialized;
using zoft.MauiExtensions.Core.Extensions;
using Color = Microsoft.Maui.Graphics.Color;

namespace zoft.MauiExtensions.Controls.Platform
{
    /// <summary>
    ///  Extends AutoCompleteTextView to have similar APIs and behavior to UWP's AutoSuggestBox, which greatly simplifies wrapping it
    /// </summary>
    public class AndroidAutoCompleteEntry : AutoCompleteTextView
    {
        private bool _suppressTextChangedEvent;
        private Func<object, string> _textMemberPathFunc;
        private AutoCompleteAdapter MyAdapter => Adapter as AutoCompleteAdapter;

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

            //Adapter = new SuggestCompleteAdapter(Context, global::Android.Resource.Layout.SimpleDropDownItem1Line);
        }

        /// <inheritdoc />
        public override bool EnoughToFilter()
        {
            // Setting Threshold = 0 in the constructor does not allow the control to display suggestions when the Text property is null or empty.
            // This is by design by Android.
            // See https://stackoverflow.com/questions/2126717/android-autocompletetextview-show-suggestions-when-no-text-entered for details
            // Overriding this method to always returns true changes this behaviour.
            return true;
        }

        /// <inheritdoc />
        protected override void OnFocusChanged(bool gainFocus, [GeneratedEnum] FocusSearchDirection direction, global::Android.Graphics.Rect previouslyFocusedRect)
        {
            IsSuggestionListOpen = gainFocus;

            base.OnFocusChanged(gainFocus, direction, previouslyFocusedRect);
        }

        internal void SetItems(IList items, Func<object, string> displayMemberPathFunc, Func<object, string> textMemberPathFunc)
        {
            _textMemberPathFunc = textMemberPathFunc;

            //Make sure to dispose any previously created adapter
            Adapter?.Dispose();
            Adapter= null;

            //Create new adpter with current items list
            Adapter = new AutoCompleteAdapter(Context, 
                                              Android.Resource.Layout.SimpleDropDownItem1Line, 
                                              items ?? new List<object>(),
                                              displayMemberPathFunc);
        }

        /// <summary>
        /// Gets or sets the text displayed in the entry field
        /// </summary>
        public virtual new string Text
        {
            get => base.Text;
            set
            {
                _suppressTextChangedEvent = true;
                base.Text = value;
                _suppressTextChangedEvent = false;
                this.TextChanged?.Invoke(this, new AutoCompleteEntryTextChangedEventArgs(AutoCompleteEntryTextChangeReason.ProgrammaticChange));
            }
        }

        /// <summary>
        /// Sets the text color on the entry field
        /// </summary>
        /// <param name="color"></param>
        public virtual void SetTextColor(Color color)
        {
            this.SetTextColor(color.ToAndroid(color));
        }

        /// <summary>
        /// Gets or sets the placeholder text to be displayed in the <see cref="AutoCompleteTextView"/>
        /// </summary>
        public virtual string PlaceholderText
        {
            set => HintFormatted = new Java.Lang.String(value as string ?? "");
        }

        /// <summary>
        /// Gets or sets the color of the <see cref="PlaceholderText"/>.
        /// </summary>
        /// <param name="color">color</param>
        public virtual void SetPlaceholderTextColor(Color color)
        {
            this.SetHintTextColor(color.ToAndroid(color));
        }

        /// <summary>
        /// Sets a Boolean value indicating whether the drop-down portion of the <see cref="AutoCompleteEntry"/> is open.
        /// </summary>
        public virtual bool IsSuggestionListOpen
        {
            set
            {
                if (value)
                    ShowDropDown();
                else
                    DismissDropDown();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether items in the view will trigger an update of the editable text part of the <see cref="AutoCompleteEntry"/> when clicked.
        /// </summary>
        public virtual bool UpdateTextOnSelect { get; set; } = true;

        /// <inheritdoc />
        protected override void OnTextChanged(ICharSequence text, int start, int lengthBefore, int lengthAfter)
        {
            if (!_suppressTextChangedEvent)
            {
                TextChanged?.Invoke(this, new AutoCompleteEntryTextChangedEventArgs(AutoCompleteEntryTextChangeReason.UserInput));
            }

            base.OnTextChanged(text, start, lengthBefore, lengthAfter);
        }

        private void DismissKeyboard()
        {
            var imm = (global::Android.Views.InputMethods.InputMethodManager)Context.GetSystemService(Context.InputMethodService);
            imm.HideSoftInputFromWindow(WindowToken, 0);
        }

        private void OnItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            DismissKeyboard();
            //var obj = _adapter.GetObject(e.Position);
            var obj = MyAdapter?.GetItem(e.Position);
            if (UpdateTextOnSelect)
            {
                _suppressTextChangedEvent = true;
                base.Text = _textMemberPathFunc(obj);
                _suppressTextChangedEvent = false;
                TextChanged?.Invoke(this, new AutoCompleteEntryTextChangedEventArgs(AutoCompleteEntryTextChangeReason.SuggestionChosen));
            }
            SuggestionChosen?.Invoke(this, new AutoCompleteEntrySuggestionChosenEventArgs(obj));
        }

        /// <inheritdoc />
        public override void OnEditorAction([GeneratedEnum] ImeAction actionCode)
        {
            if (actionCode == ImeAction.Done || actionCode == ImeAction.Next)
            {
                DismissDropDown();
                DismissKeyboard();
            }
            else
                base.OnEditorAction(actionCode);
        }

        /// <inheritdoc />
        protected override void ReplaceText(ICharSequence text)
        {
            //Override to avoid updating textbox on itemclick. We'll do this later using TextMemberPath and raise the proper TextChanged event then
        }

        /// <summary>
        /// Raised after the text content of the editable control component is updated.
        /// </summary>
        public new event EventHandler<AutoCompleteEntryTextChangedEventArgs> TextChanged;

        /// <summary>
        /// Raised before the text content of the editable control component is updated.
        /// </summary>
        public event EventHandler<AutoCompleteEntrySuggestionChosenEventArgs> SuggestionChosen;

        private class AutoCompleteAdapter : ArrayAdapter
        {
            //private readonly SuggestFilter _filter = new();
            protected IList Items;
            protected Func<object, string> DisplayMemberPathFunc { get; private set; }

            public AutoCompleteAdapter(Context context, int textViewResourceId, IList items, Func<object, string> displayMemberPathFunc) 
                : base(context, textViewResourceId, items)
            {
                Items = items;
                DisplayMemberPathFunc = displayMemberPathFunc;

                SetNotifyOnChange(false);

                //CheckIfItemsSourceIsNotifiable();
            }

            //public void UpdateList(IList list, Func<object, string> labelFunc)
            //{
            //    _labelFunc = labelFunc;
            //    _items = list;

            //    CollectionChanged();

                
            //}

            private void CheckIfItemsSourceIsNotifiable()
            {
                if (Items is INotifyCollectionChanged notifiableItems)
                {
                    notifiableItems.CollectionChanged += NotifiableItems_CollectionChanged;
                }
            }

            private void NotifiableItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
            {
                if (!MainThread.IsMainThread)
                {
                    MainThread.BeginInvokeOnMainThread(CollectionChanged);
                }
                else
                {
                    CollectionChanged();
                }
            }

            private void CollectionChanged()
            {
                //_filter.SetFilter(_items.OfType<object>().Select(s => _labelFunc(s)));

                NotifyDataSetChanged();
            }

            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);

                if (disposing && Items is INotifyCollectionChanged notifiableItems)
                {
                    notifiableItems.CollectionChanged -= NotifiableItems_CollectionChanged;
                }
            }

            public override Android.Views.View GetView(int position, Android.Views.View convertView, ViewGroup parent)
            {
                var view = base.GetView(position, convertView, parent);


                return view;
            }

            //public override int Count
            //{
            //    get
            //    {
            //        return _items.Count;
            //    }
            //}

            //public override Filter Filter => _filter;

            //public override Java.Lang.Object GetItem(int position)
            //{
            //    return _labelFunc(GetObject(position));
            //}

            //public object GetObject(int position)
            //{
            //    return _items[position];
            //}

            //public override long GetItemId(int position)
            //{
            //    return base.GetItemId(position);
            //}

            //private class AutoCompleteFilter : Filter
            //{
            //    private readonly AutoCompleteAdapter _autoCompleteAdapter;

            //    public AutoCompleteFilter(AutoCompleteAdapter autoCompleteAdapter)
            //    {
            //        _autoCompleteAdapter = autoCompleteAdapter;
            //    }
               
            //    protected override FilterResults PerformFiltering(ICharSequence constraint)
            //    {
            //        if(_autoCompleteAdapter.Items == null)
            //        {
            //            return new FilterResults() { Count = 0, Values = null };
            //        }

            //        return new FilterResults()
            //        {
            //            Count = _autoCompleteAdapter.Items.Count,
            //            Values = _autoCompleteAdapter.Items.OfType<object>().Select(_autoCompleteAdapter.DisplayMemberPathFunc).ToArray()
            //        };
            //    }
                
            //    protected override void PublishResults(ICharSequence constraint, FilterResults results)
            //    {
            //    }
            //}
        }
    }
}
