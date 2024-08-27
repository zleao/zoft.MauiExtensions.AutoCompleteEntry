namespace zoft.MauiExtensions.Controls;

/// <summary>
/// Represents a text control that makes suggestions to users as they type. The app is notified when text 
/// has been changed by the user and is responsible for providing relevant suggestions for this control to display.
/// </summary>
public class AutoCompleteEntry : Entry
{
    private bool _suppressTextChangedEvent;

    /// <summary>
    /// Initializes a new instance of the <see cref="AutoCompleteEntry"/> class
    /// </summary>
    public AutoCompleteEntry()
    {
    }
    
    /// <summary>
    /// Gets or sets the property path that is used to get the value for display in the
    /// text box portion of the <see cref="AutoCompleteEntry"/> control, when an item is selected.
    /// </summary>
    /// <value>
    /// The property path that is used to get the value for display in the text box portion
    /// of the <see cref="AutoCompleteEntry"/> control, when an item is selected.
    /// The default is an empty string (""), which will use the item's ToString() method.
    /// </value>
    public string TextMemberPath
    {
        get { return (string)GetValue(TextMemberPathProperty); }
        set { SetValue(TextMemberPathProperty, value); }
    }

    /// <summary>
    /// Identifies the <see cref="TextMemberPath"/> bindable property.
    /// </summary>
    public static readonly BindableProperty TextMemberPathProperty =
        BindableProperty.Create(nameof(TextMemberPath), typeof(string), typeof(AutoCompleteEntry), string.Empty);


    /// <summary>
    /// Gets or sets the name or path of the property that is displayed for each data item.
    /// </summary>
    /// <value>
    /// The name or path of the property that is displayed for each the data item in the control.
    /// The default is an empty string (""), which will use the item's ToString() method.
    /// </value>
    public string DisplayMemberPath
    {
        get { return (string)GetValue(DisplayMemberPathProperty); }
        set { SetValue(DisplayMemberPathProperty, value); }
    }

    /// <summary>
    /// Identifies the <see cref="DisplayMemberPath"/> bindable property.
    /// </summary>
    public static readonly BindableProperty DisplayMemberPathProperty =
        BindableProperty.Create(nameof(DisplayMemberPath), typeof(string), typeof(AutoCompleteEntry), string.Empty);


    /// <summary>
    /// Gets or sets a Boolean value indicating whether the drop-down portion of the <see cref="AutoCompleteEntry"/> is open.
    /// </summary>
    /// <value>A Boolean value indicating whether the drop-down portion of the <see cref="AutoCompleteEntry"/> is open.</value>
    public bool IsSuggestionListOpen
    {
        get => (bool)GetValue(IsSuggestionListOpenProperty);
        set => SetValue(IsSuggestionListOpenProperty, value);
    }

    /// <summary>
    /// Identifies the <see cref="IsSuggestionListOpen"/> bindable property.
    /// </summary>
    public static readonly BindableProperty IsSuggestionListOpenProperty =
        BindableProperty.Create(nameof(IsSuggestionListOpen), typeof(bool), typeof(AutoCompleteEntry), false);

    /// <summary>
    /// Used in conjunction with <see cref="TextMemberPath"/>, gets or sets a value indicating whether items in the view will trigger an update 
    /// of the editable text part of the <see cref="AutoCompleteEntry"/> when clicked.
    /// </summary>
    /// <value>A value indicating whether items in the view will trigger an update of the editable text part of the <see cref="AutoCompleteEntry"/> when clicked.</value>
    public bool UpdateTextOnSelect
    {
        get => (bool)GetValue(UpdateTextOnSelectProperty);
        set => SetValue(UpdateTextOnSelectProperty, value);
    }

    /// <summary>
    /// Identifies the <see cref="UpdateTextOnSelect"/> bindable property.
    /// </summary>
    public static readonly BindableProperty UpdateTextOnSelectProperty =
        BindableProperty.Create(nameof(UpdateTextOnSelect), typeof(bool), typeof(AutoCompleteEntry), true);


    /// <summary>
    /// Gets or sets the ItemsSource list with the suggestions to diplay.
    /// </summary>
    /// <value>The header object for the text box portion of this control.</value>
    public System.Collections.IList ItemsSource
    {
        get => GetValue(ItemsSourceProperty) as System.Collections.IList;
        set => SetValue(ItemsSourceProperty, value);
    }

    /// <summary>
    /// Identifies the <see cref="ItemsSource"/> bindable property.
    /// </summary>
    public static readonly BindableProperty ItemsSourceProperty =
        BindableProperty.Create(nameof(ItemsSource), typeof(System.Collections.IList), typeof(AutoCompleteEntry));


    /// <summary>
    /// Gets or Sets the TextChangedCommand, that is trigered everytime the text changes.
    /// The command receives as parameter the changed text.
    /// </summary>
    public Command<string> TextChangedCommand
    {
        get => (Command<string>)GetValue(TextChangedCommandProperty);
        set => SetValue(TextChangedCommandProperty, value);
    }

    /// <summary>
    /// Identifies the <see cref="TextChangedCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty TextChangedCommandProperty =
        BindableProperty.Create(nameof(TextChangedCommand),
            typeof(Command<string>),
            typeof(AutoCompleteEntry));

    /// <summary>
    /// Method used to signal the platform control, that the text changed
    /// </summary>
    /// <param name="text"></param>
    /// <param name="reason"></param>
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

    /// <summary>
    /// Raised after the text content of the editable control component is updated.
    /// </summary>
    public new event EventHandler<AutoCompleteEntryTextChangedEventArgs> TextChanged;

    /// <inheritdoc/>
    protected override void OnTextChanged(string oldValue, string newValue)
    {
        if (!_suppressTextChangedEvent) //Ensure this property changed didn't get call because we were updating it from the native text property
        {
            TextChanged?.Invoke(this, new AutoCompleteEntryTextChangedEventArgs(AutoCompleteEntryTextChangeReason.ProgrammaticChange));
        }
    }


    /// <summary>
    /// Raised after the cursor position changes
    /// </summary>
    public event EventHandler<AutoCompleteEntryCursorPositionChangedEventArgs> CursorPositionChanged;

    /// <summary>
    /// Method used to signal the platform control, that the cursor position changed
    /// </summary>
    /// <param name="cursorPosition"></param>
    public void OnCursorPositionChanged(int cursorPosition)
    {
        if (CursorPosition == cursorPosition) return;
            
        CursorPosition = cursorPosition;
        CursorPositionChanged?.Invoke(this, new AutoCompleteEntryCursorPositionChangedEventArgs(cursorPosition));
    }


    /// <summary>
    /// Get or Set the currently selected suggestion, from the items source list
    /// </summary>
    public object SelectedSuggestion
    {
        get => GetValue(SelectedSuggestionProperty);
        set => SetValue(SelectedSuggestionProperty, value);
    }

    /// <summary>
    /// Identifies the <see cref="SelectedSuggestion"/> bindable property.
    /// </summary>
    public static readonly BindableProperty SelectedSuggestionProperty =
        BindableProperty.Create(nameof(SelectedSuggestion),
            typeof(object),
            typeof(AutoCompleteEntry), null, BindingMode.TwoWay);

    /// <summary>
    /// Method used to signal the platform control, that a suggestion was selected.
    /// </summary>
    /// <param name="selectedItem">Item selected</param>
    public void OnSuggestionSelected(object selectedItem)
    {
        SelectedSuggestion = selectedItem;

        SuggestionChosen?.Invoke(this, new AutoCompleteEntrySuggestionChosenEventArgs(selectedItem));
    }

    /// <summary>
    /// Raised before the text content of the editable control component is updated.
    /// </summary>
    public event EventHandler<AutoCompleteEntrySuggestionChosenEventArgs> SuggestionChosen;
}