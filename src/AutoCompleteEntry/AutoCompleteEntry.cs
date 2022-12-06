namespace zoft.MauiExtensions.Controls
{
    /// <summary>
    /// Represents a text control that makes suggestions to users as they type. The app is notified when text 
    /// has been changed by the user and is responsible for providing relevant suggestions for this control to display.
    /// </summary>
    public partial class AutoCompleteEntry : SearchBar, IAutoCompleteEntry
    {
        private bool _suppressTextChangedEvent;

        /// <summary>
        /// Initializes a new instance of the <see cref="AutoCompleteEntry"/> class
        /// </summary>
        public AutoCompleteEntry()
        {
        }

        /// <inheritdoc/>
        public string TextMemberPath
        {
            get { return (string)GetValue(TextMemberPathProperty); }
            set { SetValue(TextMemberPathProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="TextMemberPath"/> bindable property.
        /// </summary>
        public static readonly BindableProperty TextMemberPathProperty =
            BindableProperty.Create(nameof(TextMemberPath), typeof(string), typeof(AutoCompleteEntry), string.Empty, BindingMode.OneWay, null, null);


        /// <inheritdoc/>
        public string DisplayMemberPath
        {
            get { return (string)GetValue(DisplayMemberPathProperty); }
            set { SetValue(DisplayMemberPathProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="DisplayMemberPath"/> bindable property.
        /// </summary>
        public static readonly BindableProperty DisplayMemberPathProperty =
            BindableProperty.Create(nameof(DisplayMemberPath), typeof(string), typeof(AutoCompleteEntry), string.Empty, BindingMode.OneWay, null, null);


        /// <inheritdoc/>
        public bool IsSuggestionListOpen
        {
            get { return (bool)GetValue(IsSuggestionListOpenProperty); }
            set { SetValue(IsSuggestionListOpenProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="IsSuggestionListOpen"/> bindable property.
        /// </summary>
        public static readonly BindableProperty IsSuggestionListOpenProperty =
            BindableProperty.Create(nameof(IsSuggestionListOpen), typeof(bool), typeof(AutoCompleteEntry), false, BindingMode.OneWay, null, null);


        /// <inheritdoc/>
        public bool UpdateTextOnSelect
        {
            get { return (bool)GetValue(UpdateTextOnSelectProperty); }
            set { SetValue(UpdateTextOnSelectProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="UpdateTextOnSelect"/> bindable property.
        /// </summary>
        public static readonly BindableProperty UpdateTextOnSelectProperty =
            BindableProperty.Create(nameof(UpdateTextOnSelect), typeof(bool), typeof(AutoCompleteEntry), true, BindingMode.OneWay, null, null);


        /// <inheritdoc/>
        public System.Collections.IList ItemsSource
        {
            get { return GetValue(ItemsSourceProperty) as System.Collections.IList; }
            set { SetValue(ItemsSourceProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="ItemsSource"/> bindable property.
        /// </summary>
        public static readonly BindableProperty ItemsSourceProperty =
            BindableProperty.Create(nameof(ItemsSource), typeof(System.Collections.IList), typeof(AutoCompleteEntry), null, BindingMode.OneWay, null, null);


        /// <inheritdoc/>
        public Microsoft.Maui.Controls.Command<string> TextChangedCommand
        {
            get { return (Microsoft.Maui.Controls.Command<string>)GetValue(TextChangedCommandProperty); }
            set { SetValue(TextChangedCommandProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="TextChangedCommand"/> bindable property.
        /// </summary>
        public static readonly BindableProperty TextChangedCommandProperty = 
            BindableProperty.Create(nameof(TextChangedCommand), 
                                    typeof(Command<string>), 
                                    typeof(AutoCompleteEntry), null, BindingMode.OneWay, null, null);
        
        /// <inheritdoc/>
        public void OnTextChanged(string text, AutoCompleteEntryTextChangeReason reason)
        {
            // Called by the native control when users enter text

            _suppressTextChangedEvent = true; //prevent loop of events raising, as setting this property will make it back into the native control
            Text = text;
            _suppressTextChangedEvent = false;
            
            TextChanged?.Invoke(this, new AutoCompleteEntryTextChangedEventArgs(reason));

            if (reason == AutoCompleteEntryTextChangeReason.UserInput && 
                TextChangedCommand?.CanExecute(Text) == true)
            {
                TextChangedCommand?.Execute(Text);
            }
        }

        /// <inheritdoc/>
        public new event EventHandler<AutoCompleteEntryTextChangedEventArgs> TextChanged;

        /// <inheritdoc/>
        protected override void OnTextChanged(string oldValue, string newValue)
        {
            if (!_suppressTextChangedEvent) //Ensure this property changed didn't get call because we were updating it from the native text property
            {
                TextChanged?.Invoke(this, new AutoCompleteEntryTextChangedEventArgs(AutoCompleteEntryTextChangeReason.ProgrammaticChange));
            }
        }


        /// <inheritdoc/>
        public object SelectedSuggestion
        {
            get { return (object)GetValue(SelectedSuggestionProperty); }
            set { SetValue(SelectedSuggestionProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="SelectedSuggestion"/> bindable property.
        /// </summary>
        public static readonly BindableProperty SelectedSuggestionProperty =
            BindableProperty.Create(nameof(SelectedSuggestion),
                                    typeof(object),
                                    typeof(AutoCompleteEntry), null, BindingMode.TwoWay, null, null);

        /// <inheritdoc/>
        public void OnSuggestionSelected(object selectedItem)
        {
            SelectedSuggestion = selectedItem;

            SuggestionChosen?.Invoke(this, new AutoCompleteEntrySuggestionChosenEventArgs(selectedItem));
        }

        /// <inheritdoc/>
        public event EventHandler<AutoCompleteEntrySuggestionChosenEventArgs> SuggestionChosen;


        /// <inheritdoc/>
        public void OnQuerySubmitted(string queryText, object chosenSuggestion)
        {
            QuerySubmitted?.Invoke(this, new AutoCompleteEntryQuerySubmittedEventArgs(queryText, chosenSuggestion));
        }

        /// <inheritdoc/>
        public event EventHandler<AutoCompleteEntryQuerySubmittedEventArgs> QuerySubmitted;
    }
}
