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

    #endregion
}
