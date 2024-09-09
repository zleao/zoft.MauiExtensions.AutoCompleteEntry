using System.Collections;

using Android.Content;
using Android.Runtime;
using Android.Text;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;

using AndroidX.AppCompat.Widget;

using Java.Lang;

using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Platform;

using zoft.MauiExtensions.Core.Extensions;

using AView = Android.Views.View;
using Color = Microsoft.Maui.Graphics.Color;

namespace zoft.MauiExtensions.Controls.Platform;

/// <summary>
///  Extends AppCompatAutoCompleteTextView to have similar APIs and behavior to WinUI's AutoSuggestBox, which greatly simplifies wrapping it
/// </summary>
public sealed class AndroidAutoCompleteEntry : AppCompatAutoCompleteTextView
{
    private bool _suppressTextChangedEvent;
    private Func<object, string> _textMemberPathFunc;
    private readonly AutoCompleteAdapter _adapter;

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

        Adapter = _adapter = new AutoCompleteAdapter(Context);
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

    internal void SetItems(IList items, string? displayMemberPath, Func<object, string> textMemberPathFunc)
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

    /// <inheritdoc />
    protected override void OnTextChanged(ICharSequence text, int start, int lengthBefore, int lengthAfter)
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

    private void OnItemClick(object sender, AdapterView.ItemClickEventArgs e)
    {
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

    class AutoCompleteAdapter : BaseAdapter, IFilterable
    {
        private CustomFilter _filter;
        private List<object> resultList;
        private string? _displayMemberPath;

        private DataTemplate _defaultTemplate;
        private bool _disposed = false;
        internal IMauiContext MauiContext => _listViewContainer.Handler.MauiContext;
        Page _listViewContainer;
        public DataTemplate ItemTemplate { get; internal set; }

        internal DataTemplate DefaultTemplate
        {
            get
            {
                if (_defaultTemplate == null)
                {
                    _defaultTemplate = new DataTemplate(() =>
                    {
                        var label = new Label();
                        label.SetBinding(Label.TextProperty, _displayMemberPath ?? ".");
                        label.HorizontalTextAlignment = Microsoft.Maui.TextAlignment.Center;
                        label.VerticalTextAlignment = Microsoft.Maui.TextAlignment.Center;

                        return label;
                    });
                }
                return _defaultTemplate;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            _disposed = true;

            if (disposing)
            {
                _filter?.Dispose();
            }

            _filter = null;
            _defaultTemplate = null;

            base.Dispose(disposing);
        }

        public AutoCompleteAdapter(Context context) : base()
        {
            _listViewContainer = Application.Current.MainPage;
            resultList = new List<object>();
            NotifyDataSetChanged();
        }

        public void UpdateList(IEnumerable<object> list, string? displayMemberPath)
        {
            _displayMemberPath = displayMemberPath;
            resultList = list.ToList();
            NotifyDataSetChanged();
        }

        public override int Count => resultList.Count;

        public Filter Filter => _filter ??= new CustomFilter(this);

        public override Java.Lang.Object GetItem(int position) => new ObjectWrapper(GetObject(position));

        public object GetObject(int position) => resultList[position];

        public override long GetItemId(int position)
        {
            return position;
        }

        public override AView GetView(int position, AView convertView, ViewGroup parent)
        {
            var item = GetObject(position);

            Microsoft.Maui.Controls.Platform.Compatibility.ContainerView result = null;
            if (convertView != null)
            {
                result = convertView as Microsoft.Maui.Controls.Platform.Compatibility.ContainerView;
                result.View.BindingContext = item;
            }
            else
            {
                var template = ItemTemplate ?? DefaultTemplate;
                var view = (Microsoft.Maui.Controls.View)template.CreateContent(item, _listViewContainer);
                view.BindingContext = item;

                result = new Microsoft.Maui.Controls.Platform.Compatibility.ContainerView(parent.Context, view, MauiContext);
                result.MatchWidth = true;

                //TODO is internal - find better solution to set true value.
                //result.MeasureHeight = true;

                result.SetPropertyValue("MeasureHeight", true);
            }

            return result;
        }

        private class CustomFilter : Filter
        {
            private readonly AutoCompleteAdapter _adapter;

            public CustomFilter(AutoCompleteAdapter adapter)
            {
                _adapter = adapter;
            }

            protected override FilterResults PerformFiltering(ICharSequence constraint)
            {
                var results = new FilterResults();

                results.Count = 100;
                return results;
            }

            protected override void PublishResults(ICharSequence constraint, FilterResults results)
            {
                _adapter.NotifyDataSetChanged();
            }
        }
    }

    public const string DoNotUpdateMarker = "__DO_NOT_UPDATE__";
    public class ObjectWrapper : Java.Lang.Object
    {
        public ObjectWrapper(object obj)
        {
            Object = obj;
        }

        public object Object { get; set; }

        public override string ToString() => DoNotUpdateMarker;
    }

}