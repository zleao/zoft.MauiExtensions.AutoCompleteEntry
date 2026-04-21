namespace AutoCompleteEntry.Tests;

/// <summary>
/// Tests for the shared <see cref="zoft.MauiExtensions.Controls.AutoCompleteEntry"/> control behavior.
/// These validate the core state management, event flow, and property defaults that the
/// platform-specific implementations depend on.
///
/// Platform-specific behavior (e.g., iOS UITableView row sizing) requires
/// device/emulator-based integration tests and cannot be exercised in a net9.0 unit test.
/// </summary>
public class AutoCompleteEntryControlTests
{
    private static zoft.MauiExtensions.Controls.AutoCompleteEntry CreateEntry() => new();

    #region Default Property Values

    [Fact]
    public void IsSuggestionListOpen_DefaultValue_IsFalse()
    {
        var entry = CreateEntry();

        Assert.False(entry.IsSuggestionListOpen);
    }

    [Fact]
    public void UpdateTextOnSelect_DefaultValue_IsTrue()
    {
        var entry = CreateEntry();

        Assert.True(entry.UpdateTextOnSelect);
    }

    [Fact]
    public void ShowBottomBorder_DefaultValue_IsTrue()
    {
        var entry = CreateEntry();

        Assert.True(entry.ShowBottomBorder);
    }

    [Fact]
    public void TextMemberPath_DefaultValue_IsEmpty()
    {
        var entry = CreateEntry();

        Assert.Equal(string.Empty, entry.TextMemberPath);
    }

    [Fact]
    public void DisplayMemberPath_DefaultValue_IsEmpty()
    {
        var entry = CreateEntry();

        Assert.Equal(string.Empty, entry.DisplayMemberPath);
    }

    [Fact]
    public void ItemsSource_DefaultValue_IsNull()
    {
        var entry = CreateEntry();

        Assert.Null(entry.ItemsSource);
    }

    [Fact]
    public void SelectedSuggestion_DefaultValue_IsNull()
    {
        var entry = CreateEntry();

        Assert.Null(entry.SelectedSuggestion);
    }

    [Fact]
    public void ItemTemplate_DefaultValue_IsNull()
    {
        var entry = CreateEntry();

        Assert.Null(entry.ItemTemplate);
    }

    [Fact]
    public void TextChangedCommand_DefaultValue_IsNull()
    {
        var entry = CreateEntry();

        Assert.Null(entry.TextChangedCommand);
    }

    #endregion

    #region OnTextChanged

    [Fact]
    public void OnTextChanged_WithUserInput_UpdatesTextProperty()
    {
        var entry = CreateEntry();

        entry.OnTextChanged("hello", AutoCompleteEntryTextChangeReason.UserInput);

        Assert.Equal("hello", entry.Text);
    }

    [Fact]
    public void OnTextChanged_WithUserInput_FiresTextChangedEventWithCorrectReason()
    {
        var entry = CreateEntry();
        AutoCompleteEntryTextChangedEventArgs? receivedArgs = null;
        entry.TextChanged += (_, e) => receivedArgs = e;

        entry.OnTextChanged("hello", AutoCompleteEntryTextChangeReason.UserInput);

        Assert.NotNull(receivedArgs);
        Assert.Equal(AutoCompleteEntryTextChangeReason.UserInput, receivedArgs.Reason);
    }

    [Fact]
    public void OnTextChanged_WithProgrammaticChange_FiresTextChangedEventWithCorrectReason()
    {
        var entry = CreateEntry();
        AutoCompleteEntryTextChangedEventArgs? receivedArgs = null;
        entry.TextChanged += (_, e) => receivedArgs = e;

        entry.OnTextChanged("hello", AutoCompleteEntryTextChangeReason.ProgrammaticChange);

        Assert.NotNull(receivedArgs);
        Assert.Equal(AutoCompleteEntryTextChangeReason.ProgrammaticChange, receivedArgs.Reason);
    }

    [Fact]
    public void OnTextChanged_WithSuggestionChosen_FiresTextChangedEventWithCorrectReason()
    {
        var entry = CreateEntry();
        AutoCompleteEntryTextChangedEventArgs? receivedArgs = null;
        entry.TextChanged += (_, e) => receivedArgs = e;

        entry.OnTextChanged("hello", AutoCompleteEntryTextChangeReason.SuggestionChosen);

        Assert.NotNull(receivedArgs);
        Assert.Equal(AutoCompleteEntryTextChangeReason.SuggestionChosen, receivedArgs.Reason);
    }

    [Fact]
    public void OnTextChanged_WithUserInput_ExecutesTextChangedCommand()
    {
        var entry = CreateEntry();
        var command = Substitute.For<ICommand>();
        command.CanExecute(Arg.Any<object?>()).Returns(true);
        entry.TextChangedCommand = command;

        entry.OnTextChanged("test", AutoCompleteEntryTextChangeReason.UserInput);

        command.Received(1).Execute("test");
    }

    [Fact]
    public void OnTextChanged_WithProgrammaticChange_DoesNotExecuteCommand()
    {
        var entry = CreateEntry();
        var command = Substitute.For<ICommand>();
        command.CanExecute(Arg.Any<object?>()).Returns(true);
        entry.TextChangedCommand = command;

        entry.OnTextChanged("test", AutoCompleteEntryTextChangeReason.ProgrammaticChange);

        command.DidNotReceive().Execute(Arg.Any<object?>());
    }

    [Fact]
    public void OnTextChanged_WithSuggestionChosen_DoesNotExecuteCommand()
    {
        var entry = CreateEntry();
        var command = Substitute.For<ICommand>();
        command.CanExecute(Arg.Any<object?>()).Returns(true);
        entry.TextChangedCommand = command;

        entry.OnTextChanged("test", AutoCompleteEntryTextChangeReason.SuggestionChosen);

        command.DidNotReceive().Execute(Arg.Any<object?>());
    }

    [Fact]
    public void OnTextChanged_WithUserInput_WhenCommandCannotExecute_DoesNotExecuteCommand()
    {
        var entry = CreateEntry();
        var command = Substitute.For<ICommand>();
        command.CanExecute(Arg.Any<object?>()).Returns(false);
        entry.TextChangedCommand = command;

        entry.OnTextChanged("test", AutoCompleteEntryTextChangeReason.UserInput);

        command.DidNotReceive().Execute(Arg.Any<object?>());
    }

    [Fact]
    public void OnTextChanged_WithNoCommandSet_DoesNotThrow()
    {
        var entry = CreateEntry();

        var exception = Record.Exception(() =>
            entry.OnTextChanged("test", AutoCompleteEntryTextChangeReason.UserInput));

        Assert.Null(exception);
    }

    [Fact]
    public void OnTextChanged_WithUserInput_ExactlyOneTextChangedEventFires()
    {
        // Guards the _suppressTextChangedEvent flag: without it, setting Text internally
        // would trigger a second TextChanged via the protected OnTextChanged override.
        var entry = CreateEntry();
        int eventCount = 0;
        entry.TextChanged += (_, _) => eventCount++;

        entry.OnTextChanged("hello", AutoCompleteEntryTextChangeReason.UserInput);

        Assert.Equal(1, eventCount);
    }

    [Fact]
    public void OnTextChanged_TextPropertyIsUpdatedBeforeEventFires()
    {
        var entry = CreateEntry();
        string? textDuringEvent = "sentinel";
        entry.TextChanged += (_, _) => textDuringEvent = entry.Text;

        entry.OnTextChanged("hello", AutoCompleteEntryTextChangeReason.UserInput);

        Assert.Equal("hello", textDuringEvent);
    }

    [Fact]
    public void OnTextChanged_WithNullText_SetsTextToNull()
    {
        var entry = CreateEntry();
        entry.OnTextChanged("initial", AutoCompleteEntryTextChangeReason.UserInput);

        entry.OnTextChanged(null, AutoCompleteEntryTextChangeReason.UserInput);

        Assert.Null(entry.Text);
    }

    [Fact]
    public void OnTextChanged_WithNullText_UserInput_FiresEventAndExecutesCommandWithNull()
    {
        var entry = CreateEntry();
        AutoCompleteEntryTextChangedEventArgs? receivedArgs = null;
        entry.TextChanged += (_, e) => receivedArgs = e;
        var command = Substitute.For<ICommand>();
        command.CanExecute(Arg.Any<object?>()).Returns(true);
        entry.TextChangedCommand = command;

        entry.OnTextChanged(null, AutoCompleteEntryTextChangeReason.UserInput);

        Assert.NotNull(receivedArgs);
        command.Received(1).Execute(null);
    }

    [Fact]
    public void OnTextChanged_WithUserInput_CanExecuteIsCalledWithCurrentText()
    {
        var entry = CreateEntry();
        var command = Substitute.For<ICommand>();
        command.CanExecute(Arg.Any<object?>()).Returns(true);
        entry.TextChangedCommand = command;

        entry.OnTextChanged("test", AutoCompleteEntryTextChangeReason.UserInput);

        command.Received(1).CanExecute("test");
    }

    [Fact]
    public void OnTextChanged_WithProgrammaticChange_CanExecuteIsNotCalled()
    {
        var entry = CreateEntry();
        var command = Substitute.For<ICommand>();
        command.CanExecute(Arg.Any<object?>()).Returns(true);
        entry.TextChangedCommand = command;

        entry.OnTextChanged("test", AutoCompleteEntryTextChangeReason.ProgrammaticChange);

        command.DidNotReceive().CanExecute(Arg.Any<object?>());
    }

    [Fact]
    public void OnTextChanged_WithSuggestionChosen_CanExecuteIsNotCalled()
    {
        var entry = CreateEntry();
        var command = Substitute.For<ICommand>();
        command.CanExecute(Arg.Any<object?>()).Returns(true);
        entry.TextChangedCommand = command;

        entry.OnTextChanged("test", AutoCompleteEntryTextChangeReason.SuggestionChosen);

        command.DidNotReceive().CanExecute(Arg.Any<object?>());
    }

    #endregion

    #region OnSuggestionSelected

    [Fact]
    public void OnSuggestionSelected_SetsSelectedSuggestionProperty()
    {
        var entry = CreateEntry();
        var item = new { Name = "Test Item" };

        entry.OnSuggestionSelected(item);

        Assert.Same(item, entry.SelectedSuggestion);
    }

    [Fact]
    public void OnSuggestionSelected_FiresSuggestionChosenEvent()
    {
        var entry = CreateEntry();
        AutoCompleteEntrySuggestionChosenEventArgs? receivedArgs = null;
        entry.SuggestionChosen += (_, e) => receivedArgs = e;
        var item = new { Name = "Test Item" };

        entry.OnSuggestionSelected(item);

        Assert.NotNull(receivedArgs);
        Assert.Same(item, receivedArgs.SelectedItem);
    }

    [Fact]
    public void OnSuggestionSelected_WithNull_SetsSelectedSuggestionToNull()
    {
        var entry = CreateEntry();
        entry.OnSuggestionSelected(new object());

        entry.OnSuggestionSelected(null!);

        Assert.Null(entry.SelectedSuggestion);
    }

    [Fact]
    public void OnSuggestionSelected_WithNull_FiresEventArgsWithNullSelectedItem()
    {
        var entry = CreateEntry();
        AutoCompleteEntrySuggestionChosenEventArgs? receivedArgs = null;
        entry.SuggestionChosen += (_, e) => receivedArgs = e;

        entry.OnSuggestionSelected(null);

        Assert.NotNull(receivedArgs);
        Assert.Null(receivedArgs.SelectedItem);
    }

    [Fact]
    public void OnSuggestionSelected_SelectedSuggestionIsSetBeforeEventFires()
    {
        var entry = CreateEntry();
        object? suggestionDuringEvent = null;
        entry.SuggestionChosen += (_, _) => suggestionDuringEvent = entry.SelectedSuggestion;
        var item = new { Name = "Test" };

        entry.OnSuggestionSelected(item);

        Assert.Same(item, suggestionDuringEvent);
    }

    #endregion

    #region OnCursorPositionChanged

    [Fact]
    public void OnCursorPositionChanged_UpdatesCursorPositionProperty()
    {
        var entry = CreateEntry();

        entry.OnCursorPositionChanged(5);

        Assert.Equal(5, entry.CursorPosition);
    }

    [Fact]
    public void OnCursorPositionChanged_FiresCursorPositionChangedEvent()
    {
        var entry = CreateEntry();
        AutoCompleteEntryCursorPositionChangedEventArgs? receivedArgs = null;
        entry.CursorPositionChanged += (_, e) => receivedArgs = e;

        entry.OnCursorPositionChanged(5);

        Assert.NotNull(receivedArgs);
        Assert.Equal(5, receivedArgs.CursorPosition);
    }

    [Fact]
    public void OnCursorPositionChanged_SamePosition_DoesNotFireEvent()
    {
        var entry = CreateEntry();
        entry.OnCursorPositionChanged(5);

        int eventCount = 0;
        entry.CursorPositionChanged += (_, _) => eventCount++;

        entry.OnCursorPositionChanged(5);

        Assert.Equal(0, eventCount);
    }

    [Fact]
    public void OnCursorPositionChanged_DifferentPosition_FiresEvent()
    {
        var entry = CreateEntry();
        entry.OnCursorPositionChanged(5);

        AutoCompleteEntryCursorPositionChangedEventArgs? receivedArgs = null;
        entry.CursorPositionChanged += (_, e) => receivedArgs = e;

        entry.OnCursorPositionChanged(10);

        Assert.NotNull(receivedArgs);
        Assert.Equal(10, receivedArgs.CursorPosition);
    }

    [Fact]
    public void OnCursorPositionChanged_FromDefault_WithZero_DoesNotFireEvent()
    {
        // The default CursorPosition on Entry is 0. Calling with 0 is a no-op.
        var entry = CreateEntry();
        int eventCount = 0;
        entry.CursorPositionChanged += (_, _) => eventCount++;

        entry.OnCursorPositionChanged(0);

        Assert.Equal(0, eventCount);
    }

    [Fact]
    public void OnCursorPositionChanged_UpdatesPropertyBeforeEventFires()
    {
        var entry = CreateEntry();
        int positionDuringEvent = -1;
        entry.CursorPositionChanged += (_, _) => positionDuringEvent = entry.CursorPosition;

        entry.OnCursorPositionChanged(7);

        Assert.Equal(7, positionDuringEvent);
    }

    #endregion

    #region Property Setting

    [Fact]
    public void IsSuggestionListOpen_CanBeSetToTrue()
    {
        var entry = CreateEntry();

        entry.IsSuggestionListOpen = true;

        Assert.True(entry.IsSuggestionListOpen);
    }

    [Fact]
    public void UpdateTextOnSelect_CanBeSetToFalse()
    {
        var entry = CreateEntry();

        entry.UpdateTextOnSelect = false;

        Assert.False(entry.UpdateTextOnSelect);
    }

    [Fact]
    public void ShowBottomBorder_CanBeSetToFalse()
    {
        var entry = CreateEntry();

        entry.ShowBottomBorder = false;

        Assert.False(entry.ShowBottomBorder);
    }

    [Fact]
    public void ItemsSource_CanBeSet()
    {
        var entry = CreateEntry();
        var items = new List<string> { "one", "two", "three" };

        entry.ItemsSource = items;

        Assert.Same(items, entry.ItemsSource);
    }

    [Fact]
    public void TextMemberPath_CanBeSetToNonEmptyString()
    {
        var entry = CreateEntry();

        entry.TextMemberPath = "Name";

        Assert.Equal("Name", entry.TextMemberPath);
    }

    [Fact]
    public void DisplayMemberPath_CanBeSetToNonEmptyString()
    {
        var entry = CreateEntry();

        entry.DisplayMemberPath = "Label";

        Assert.Equal("Label", entry.DisplayMemberPath);
    }

    [Fact]
    public void ItemTemplate_CanBeSetToNonNullTemplate()
    {
        var entry = CreateEntry();
        var template = new DataTemplate();

        entry.ItemTemplate = template;

        Assert.Same(template, entry.ItemTemplate);
    }

    #endregion

    #region Text Property (programmatic assignment)

    [Fact]
    public void Text_SetDirectly_FiresTextChangedWithProgrammaticChangeReason()
    {
        var entry = CreateEntry();
        AutoCompleteEntryTextChangedEventArgs? receivedArgs = null;
        entry.TextChanged += (_, e) => receivedArgs = e;

        entry.Text = "hello";

        Assert.NotNull(receivedArgs);
        Assert.Equal(AutoCompleteEntryTextChangeReason.ProgrammaticChange, receivedArgs.Reason);
    }

    [Fact]
    public void Text_SetDirectly_DoesNotExecuteTextChangedCommand()
    {
        // TextChangedCommand is only executed when OnTextChanged is called with UserInput.
        // Direct property assignment must not trigger the command.
        var entry = CreateEntry();
        var command = Substitute.For<ICommand>();
        command.CanExecute(Arg.Any<object?>()).Returns(true);
        entry.TextChangedCommand = command;

        entry.Text = "hello";

        command.DidNotReceive().Execute(Arg.Any<object?>());
    }

    [Fact]
    public void Text_SetDirectlyTwice_WithDifferentValues_FiresEventTwice()
    {
        var entry = CreateEntry();
        int eventCount = 0;
        entry.TextChanged += (_, _) => eventCount++;

        entry.Text = "hello";
        entry.Text = "world";

        Assert.Equal(2, eventCount);
    }

    #endregion

    #region SelectedSuggestion Property

    [Fact]
    public void SelectedSuggestion_SetDirectly_DoesNotFireSuggestionChosenEvent()
    {
        // SuggestionChosen fires only via OnSuggestionSelected, not via direct property assignment.
        var entry = CreateEntry();
        int eventCount = 0;
        entry.SuggestionChosen += (_, _) => eventCount++;

        entry.SelectedSuggestion = new object();

        Assert.Equal(0, eventCount);
    }

    #endregion
}
