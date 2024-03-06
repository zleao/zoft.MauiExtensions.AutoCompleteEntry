using System.Collections;
using Android.Content;
using Android.Runtime;
using Android.Text;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using Java.Lang;
using Microsoft.Maui.Controls.Compatibility.Platform.Android;
using Microsoft.Maui.Platform;
using Color = Microsoft.Maui.Graphics.Color;

namespace zoft.MauiExtensions.Controls.Platform
{
    /// <summary>
    ///  Extends AppCompatAutoCompleteTextView to have similar APIs and behavior to WinUI's AutoSuggestBox, which greatly simplifies wrapping it
    /// </summary>
    public class AndroidAutoCompleteEntry : AppCompatAutoCompleteTextView
    {
        private bool _suppressTextChangedEvent;
        private Func<object, string> _textMemberPathFunc;
        private AutoCompleteAdapter adapter;

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

            Adapter = adapter = new AutoCompleteAdapter(Context, global::Android.Resource.Layout.SimpleDropDownItem1Line);
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

        internal void SetItems(IList items, Func<object, string> displayMemberPathFunc, Func<object, string> textMemberPathFunc)
        {
            _textMemberPathFunc = textMemberPathFunc;
            if (items is null)
            {
                adapter.UpdateList(Enumerable.Empty<string>(), displayMemberPathFunc);
            }
            else
            {
                adapter.UpdateList(items.OfType<object>(), displayMemberPathFunc);
            }
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
            this.SetTextColor(color.ToPlatform());
        }

        /// <summary>
        /// Gets or sets the placeholder text to be displayed in the <see cref="AppCompatAutoCompleteTextView"/>
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
            this.SetHintTextColor(color.ToPlatform());
        }

        /// <summary>
        /// Sets a Boolean value indicating whether the drop-down portion of the <see cref="AutoCompleteEntry"/> is open.
        /// </summary>
        public virtual bool IsSuggestionListOpen
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
            var imm = (InputMethodManager)Context.GetSystemService(Context.InputMethodService);
            imm.HideSoftInputFromWindow(WindowToken, 0);
        }

        private void OnItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            DismissKeyboard();
            var obj = adapter.GetObject(e.Position);
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

        private class AutoCompleteAdapter : ArrayAdapter, IFilterable
        {
            private AutoCompleteFilter filter = new();
            private List<object> resultList;
            private Func<object, string> displayMemberPathFunc;

            public AutoCompleteAdapter(Context context, int textViewResourceId) : base(context, textViewResourceId)
            {
                resultList = new List<object>();
                SetNotifyOnChange(true);
            }

            public void UpdateList(IEnumerable<object> list, Func<object, string> displayMemberPathFunc)
            {
                this.displayMemberPathFunc = displayMemberPathFunc;
                resultList = list.ToList();
                filter.SetFilter(resultList.Select(s => displayMemberPathFunc(s)));
                NotifyDataSetChanged();
            }

            public override int Count
            {
                get => resultList.Count;
            }

            public override Filter Filter => filter;

            public override Java.Lang.Object GetItem(int position) => displayMemberPathFunc(GetObject(position));
            
            public object GetObject(int position) => resultList[position];

            public override long GetItemId(int position) => base.GetItemId(position);
            
            private class AutoCompleteFilter : Filter
            {
                private IEnumerable<string> resultList;

                public AutoCompleteFilter()
                {
                }

                public void SetFilter(IEnumerable<string> list)
                {
                    resultList = list;
                }

                protected override FilterResults PerformFiltering(ICharSequence constraint)
                {
                    if (resultList is null)
                    {
                        return new FilterResults() { Count = 0, Values = null };
                    }
                    var arr = resultList.ToArray();
                    return new FilterResults() { Count = arr.Length, Values = arr };
                }

                protected override void PublishResults(ICharSequence constraint, FilterResults results)
                {
                }
            }
        }
    }
}