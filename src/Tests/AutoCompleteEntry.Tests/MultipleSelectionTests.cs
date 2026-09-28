using System.Collections;
using System.Collections.ObjectModel;
using Microsoft.Maui.Controls;
using Control = zoft.MauiExtensions.Controls.AutoCompleteEntry;

namespace AutoCompleteEntry.Tests;

public class MultipleSelectionTests
{
    private static Control Multiple() => new() { SelectionMode = AutoCompleteEntrySelectionMode.Multiple };

    [Fact]
    public void SingleModeDismissAndReopenRetainsTextSelectionAndFocusWithoutActivation()
    {
        var entry = new Control { SelectedSuggestion = "A", Text = "query" };
        ((Microsoft.Maui.IEntry)entry).IsFocused = true;
        var openings = 0;
        entry.SuggestionListOpening += (_, _) => openings++;
        entry.TextChanged += (_, _) => Assert.Fail("Dismiss/reopen must not edit single-mode text");
        entry.SuggestionChosen += (_, _) => Assert.Fail("Dismiss/reopen must not choose an item");
        entry.SelectionChanged += (_, _) => Assert.Fail("Dismiss/reopen must not alter selection");
        for (var cycle = 1; cycle <= 3; cycle++)
        {
            entry.IsSuggestionListOpen = true;
            entry.IsSuggestionListOpen = true;
            Assert.Equal(cycle, openings);
            entry.IsSuggestionListOpen = false;
            Assert.Equal("query", entry.Text);
            Assert.Equal("A", entry.SelectedSuggestion);
            Assert.True(entry.IsFocused);
        }
    }

    [Fact]
    public void DismissAndReopenWhileStillFocusedPreservesSelectionAndStartsOneSession()
    {
        var entry = Multiple();
        ((Microsoft.Maui.IEntry)entry).IsFocused = true;
        entry.SelectedSuggestions = new ObservableCollection<string> { "A", "B" };
        var openings = 0;
        entry.SuggestionListOpening += (_, _) =>
        {
            openings++;
            Assert.False(entry.ShowsSelectionSummary);
            Assert.Empty(entry.Text);
            entry.ItemsSource = new[] { "A", "B", "C" };
        };
        entry.SelectionChanged += (_, _) => Assert.Fail("Dismissal/reopening must retain selection");

        for (var cycle = 1; cycle <= 3; cycle++)
        {
            entry.IsSuggestionListOpen = true;
            entry.Text = "query";
            // An editor click while already open must not restart the query.
            entry.IsSuggestionListOpen = true;
            Assert.Equal("query", entry.Text);
            Assert.Equal(cycle, openings);
            entry.IsSuggestionListOpen = false;
            Assert.True(entry.IsFocused);
            Assert.True(entry.ShowsSelectionSummary);
            Assert.Empty(entry.Text);
            Assert.Equal("A; B", entry.SelectionSummary);
        }
    }

    [Fact]
    public void DefaultsAreCompatibleAndCollectionsArePerInstance()
    {
        var first = new Control();
        var second = new Control();
        Assert.Equal(AutoCompleteEntrySelectionMode.Single, first.SelectionMode);
        Assert.Empty(first.SelectedSuggestions);
        Assert.NotSame(first.SelectedSuggestions, second.SelectedSuggestions);
        Assert.Equal(BindingMode.TwoWay, Control.SelectedSuggestionsProperty.DefaultBindingMode);
        first.SelectedSuggestions = null!;
        Assert.NotNull(first.SelectedSuggestions);
    }

    [Fact]
    public void ToggleUpdatesBoundStateAndPresentationBeforeEventsWithoutChangingQuery()
    {
        var entry = Multiple();
        var bound = new ObservableCollection<string>();
        entry.SelectedSuggestions = bound;
        entry.IsSuggestionListOpen = true;
        entry.Text = "query";
        var events = new List<string>();
        entry.SelectionPresentationChanged += (_, _) => events.Add("presentation");
        entry.SelectionChanged += (_, e) =>
        {
            events.Add("selection");
            Assert.Equal(bound.Count == 1 ? "A" : "", entry.SelectionSummary);
            Assert.Equal(bound.Count == 1 ? new[] { "A" } : [], e.AddedItems);
            Assert.Equal(bound.Count == 0 ? new[] { "A" } : [], e.RemovedItems);
        };
        entry.SuggestionChosen += (_, e) =>
        {
            events.Add("chosen");
            Assert.Equal(bound.Contains("A"), e.IsSelected);
            Assert.Equal("A", e.SelectedItem);
        };

        entry.OnSuggestionSelected("A");
        Assert.Equal(new[] { "presentation", "selection", "chosen" }, events);
        events.Clear();
        entry.OnSuggestionSelected("A");
        Assert.Equal(new[] { "presentation", "selection", "chosen" }, events);
        Assert.Equal("query", entry.Text);
        Assert.True(entry.IsSuggestionListOpen);
        Assert.Null(entry.SelectedSuggestion);
    }

    [Fact]
    public void FilteringNeverRemovesSelectionAndClearOnlyChangesQuery()
    {
        var entry = Multiple();
        entry.IsSuggestionListOpen = true;
        entry.ItemsSource = new[] { "A" };
        entry.OnSuggestionSelected("A");
        entry.OnTextChanged("B", AutoCompleteEntryTextChangeReason.UserInput);
        entry.ItemsSource = new[] { "B" };
        entry.OnSuggestionSelected("B");
        entry.ItemsSource = null;
        entry.OnTextChanged("", AutoCompleteEntryTextChangeReason.UserInput);
        Assert.Equal(new[] { "A", "B" }, entry.SelectedSuggestions.Cast<string>());
        entry.OnSuggestionSelected("A");
        Assert.Equal(new[] { "B" }, entry.SelectedSuggestions.Cast<string>());
        Assert.True(entry.IsSuggestionListOpen);
    }

    [Fact]
    public void ObservableReplacementMutationMoveAndResetProduceEffectiveDeltas()
    {
        var entry = Multiple();
        var old = new ObservableCollection<string> { "A" };
        var current = new ObservableCollection<string> { "A", "B" };
        var changes = new List<AutoCompleteEntrySelectionChangedEventArgs>();
        entry.SelectedSuggestions = old;
        entry.SelectionChanged += (_, e) => changes.Add(e);
        entry.SuggestionChosen += (_, _) => Assert.Fail("Programmatic edits are not activations");
        entry.SelectedSuggestions = current;
        Assert.Equal(new[] { "B" }, changes.Single().AddedItems);
        old.Add("obsolete");
        Assert.Single(changes);
        current.Move(1, 0);
        Assert.Single(changes);
        Assert.Equal("B; A", entry.SelectionSummary);
        current[1] = "C";
        Assert.Equal(new[] { "C" }, changes[1].AddedItems);
        Assert.Equal(new[] { "A" }, changes[1].RemovedItems);
        current.Clear();
        Assert.Equal(new[] { "B", "C" }, changes[2].RemovedItems);
        Assert.Empty(entry.SelectionSummary);
    }

    [Fact]
    public void DuplicateAndNullEntriesAreOneEffectiveSelectionEvenWithOtherObservers()
    {
        var entry = Multiple();
        var items = new ObservableCollection<object> { "A", "A", null! };
        items.CollectionChanged += (_, _) => { }; // ObservableCollection disallows nested edits with two observers.
        entry.SelectedSuggestions = items;
        var changes = 0;
        entry.SelectionChanged += (_, _) => changes++;
        items.Add("A");
        Assert.Equal(0, changes);
        Assert.Equal("A", entry.SelectionSummary);
        entry.OnSuggestionSelected("A");
        Assert.False(entry.IsSuggestionSelected("A"));
        Assert.DoesNotContain("A", items);
        Assert.Equal(1, changes);
    }

    [Fact]
    public void EqualityDoesNotUseDisplayText()
    {
        var entry = Multiple();
        entry.TextMemberPath = nameof(Item.Name);
        var first = new Item("same; text");
        var second = new Item("same; text");
        entry.OnSuggestionSelected(first);
        entry.OnSuggestionSelected(second);
        Assert.Equal(2, entry.SelectedSuggestions.Count);
        Assert.Equal("same; text; same; text", entry.SelectionSummary);
        entry.OnSuggestionSelected(first);
        Assert.Same(second, entry.SelectedSuggestions[0]);
    }

    [Fact]
    public void ValueEqualityAndSelectionOrderAreUsed()
    {
        var entry = Multiple();
        entry.OnSuggestionSelected(new ValueItem("A"));
        entry.OnSuggestionSelected(new ValueItem("B"));
        entry.OnSuggestionSelected(new ValueItem("A"));
        Assert.Equal(new ValueItem("B"), entry.SelectedSuggestions[0]);
    }

    [Fact]
    public void UnsupportedCollectionsFailBeforeReplacingState()
    {
        var entry = Multiple();
        var original = entry.SelectedSuggestions;
        Assert.Throws<ArgumentException>(() => entry.SelectedSuggestions = new[] { "A" });
        Assert.Throws<ArgumentException>(() => entry.SelectedSuggestions = ArrayList.ReadOnly(new ArrayList()));
        Assert.Same(original, entry.SelectedSuggestions);
    }

    [Fact]
    public void NonObservableListRequiresReplacementForExternalEdits()
    {
        var entry = Multiple();
        var items = new List<string> { "A" };
        entry.SelectedSuggestions = items;
        items.Add("B");
        Assert.Equal("A", entry.SelectionSummary);
        entry.SelectedSuggestions = new List<string>(items);
        Assert.Equal("A; B", entry.SelectionSummary);
    }

    [Fact]
    public void CloseResetsQueryOnceWithBothTextEventsButNoFiltering()
    {
        var entry = Multiple();
        entry.IsSuggestionListOpen = true;
        entry.OnSuggestionSelected("A");
        entry.Text = "refine";
        var command = Substitute.For<ICommand>();
        command.CanExecute(Arg.Any<object>()).Returns(true);
        entry.TextChangedCommand = command;
        var baseEvents = 0;
        var reasons = new List<AutoCompleteEntryTextChangeReason>();
        ((Entry)entry).TextChanged += (_, _) => baseEvents++;
        entry.TextChanged += (_, e) => reasons.Add(e.Reason);
        entry.IsSuggestionListOpen = false;
        entry.IsSuggestionListOpen = false;
        Assert.Equal("", entry.Text);
        Assert.Equal("A", entry.SelectionSummary);
        Assert.True(entry.ShowsSelectionSummary);
        Assert.Equal(1, baseEvents);
        Assert.Equal(new[] { AutoCompleteEntryTextChangeReason.ProgrammaticChange }, reasons);
        command.DidNotReceive().Execute(Arg.Any<object>());
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void OpeningNotificationCanPopulateEmptyOrStaleItemsAndDoesNotRepeat(bool multiple)
    {
        var entry = multiple ? Multiple() : new Control();
        entry.Text = "stale";
        var openings = 0;
        entry.SuggestionListOpening += (_, _) =>
        {
            openings++;
            Assert.Equal(multiple ? "" : "stale", entry.Text);
            entry.ItemsSource = new ObservableCollection<string> { "initial" };
        };
        entry.IsSuggestionListOpen = true;
        entry.ItemsSource = new[] { "replacement" };
        entry.IsSuggestionListOpen = true;
        Assert.Equal(1, openings);
        entry.IsSuggestionListOpen = false;
        entry.ItemsSource = null;
        entry.IsSuggestionListOpen = true;
        Assert.Equal(2, openings);
    }

    [Fact]
    public void ModeConversionsDiscardInactiveStateAndCloseQueryWithoutActivation()
    {
        var entry = new Control { SelectedSuggestion = "A" };
        entry.SelectedSuggestions.Add("stale");
        entry.SuggestionChosen += (_, _) => Assert.Fail("Mode changes are not activations");
        entry.SelectionMode = AutoCompleteEntrySelectionMode.Multiple;
        Assert.Equal(new[] { "A" }, entry.SelectedSuggestions.Cast<string>());
        Assert.Null(entry.SelectedSuggestion);
        entry.SelectedSuggestions.Add("B");
        entry.SelectedSuggestion = "ignored";
        entry.IsSuggestionListOpen = true;
        entry.Text = "query";
        var removed = new List<object>();
        entry.SelectionChanged += (_, e) => removed.AddRange(e.RemovedItems);
        entry.SelectionMode = AutoCompleteEntrySelectionMode.Single;
        Assert.Equal("A", entry.SelectedSuggestion);
        Assert.Equal("A", entry.Text);
        Assert.False(entry.IsSuggestionListOpen);
        Assert.Empty(entry.SelectedSuggestions);
        Assert.Equal(new[] { "B" }, removed);
        entry.SelectedSuggestions.Add("inactive");
        entry.SelectionMode = AutoCompleteEntrySelectionMode.Multiple;
        Assert.Equal(new[] { "A" }, entry.SelectedSuggestions.Cast<string>());
        entry.SelectedSuggestions.Clear();
        entry.SelectionMode = AutoCompleteEntrySelectionMode.Single;
        Assert.Null(entry.SelectedSuggestion);
        Assert.Equal("", entry.Text);
        entry.SelectionMode = AutoCompleteEntrySelectionMode.Multiple;
        Assert.Empty(entry.SelectedSuggestions);
    }

    [Fact]
    public void SelectionEventsSupportReentrantBindingEditsWithoutDuplicates()
    {
        var entry = Multiple();
        var events = new List<string>();
        entry.SelectionChanged += (_, e) =>
        {
            events.Add(string.Join(",", e.AddedItems));
            if (e.AddedItems.Contains("A")) entry.SelectedSuggestions.Add("B");
        };
        entry.OnSuggestionSelected("A");
        Assert.Equal(new[] { "A", "B" }, events);
        Assert.Equal("A; B", entry.SelectionSummary);
    }

    [Fact]
    public void TextPathChangesRefreshSummaryWithoutQueryEvents()
    {
        var entry = Multiple();
        entry.OnSuggestionSelected(new Item("Name"));
        entry.TextChanged += (_, _) => Assert.Fail("Summary is not query text");
        entry.TextMemberPath = nameof(Item.Name);
        Assert.Equal("Name", entry.SelectionSummary);
    }

    [Fact]
    public void SingleActivationRemainsSelectedAndProgrammaticSelectionIsNotActivation()
    {
        var entry = new Control();
        var chosen = 0;
        entry.SuggestionChosen += (_, e) => { chosen++; Assert.True(e.IsSelected); Assert.Equal(e.SelectedItem, entry.SelectedSuggestion); };
        entry.SelectedSuggestion = "A";
        Assert.Equal(0, chosen);
        entry.OnSuggestionSelected("B");
        Assert.Equal(1, chosen);
    }

    private sealed class Item(string name) { public string Name { get; } = name; }

    [Fact]
    public void RecycledWrapperKeepsOriginalItemAndRefreshesCheckedState()
    {
        var entry = Multiple();
        var label = new Label();
        label.SetBinding(Label.TextProperty, nameof(Item.Name));
        var row = new SelectionRow(label);
        var first = new Item("A");
        var second = new Item("B");
        entry.OnSuggestionSelected(first);
        row.Update(entry, first);
        Assert.Same(first, label.BindingContext);
        Assert.Equal("A", label.Text);
        Assert.True(((CheckBox)row.Children[0]).IsChecked);
        row.Update(entry, second);
        Assert.Same(second, label.BindingContext);
        Assert.Equal("B", label.Text);
        Assert.False(((CheckBox)row.Children[0]).IsChecked);
        entry.OnSuggestionSelected(second);
        row.Update(entry, second);
        Assert.True(((CheckBox)row.Children[0]).IsChecked);
    }

    [Fact]
    public void NullReplacementReportsRemovalAndDetachesCollection()
    {
        var entry = Multiple();
        var old = new ObservableCollection<string> { "A" };
        entry.SelectedSuggestions = old;
        var changes = new List<AutoCompleteEntrySelectionChangedEventArgs>();
        entry.SelectionChanged += (_, e) => changes.Add(e);
        entry.SelectedSuggestions = null!;
        old.Add("B");
        Assert.Equal(new[] { "A" }, changes.Single().RemovedItems);
        Assert.Empty(entry.SelectedSuggestions);
        Assert.False(entry.ShowsSelectionSummary);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void UpdateTextOnSelectDoesNotChangeMultipleQuery(bool updateText)
    {
        var entry = Multiple();
        entry.UpdateTextOnSelect = updateText;
        entry.IsSuggestionListOpen = true;
        entry.Text = "search";
        entry.TextChanged += (_, _) => Assert.Fail("Activation must not change the query");
        entry.OnSuggestionSelected("A");
        entry.OnSuggestionSelected("A");
        Assert.Equal("search", entry.Text);
    }

    [Fact]
    public void OpeningAndSelectionNeverRunFilteringCommand()
    {
        var entry = Multiple();
        var command = Substitute.For<ICommand>();
        command.CanExecute(Arg.Any<object>()).Returns(true);
        entry.TextChangedCommand = command;
        entry.IsSuggestionListOpen = true;
        entry.OnSuggestionSelected("A");
        entry.IsSuggestionListOpen = false;
        entry.IsSuggestionListOpen = true;
        command.DidNotReceive().Execute(Arg.Any<object>());
        entry.OnTextChanged("", AutoCompleteEntryTextChangeReason.UserInput);
        command.Received(1).Execute("");
    }

    private sealed record ValueItem(string Name);

    [Fact]
    public void TypingAfterDismissalOpensEmptySessionWithoutErasingFirstKeystroke()
    {
        var entry = Multiple();
        entry.OnSuggestionSelected("A");
        entry.SuggestionListOpening += (_, _) => Assert.Equal("", entry.Text);
        var command = Substitute.For<ICommand>();
        command.CanExecute(Arg.Any<object>()).Returns(true);
        entry.TextChangedCommand = command;
        entry.OnTextChanged("j", AutoCompleteEntryTextChangeReason.UserInput);
        Assert.True(entry.IsSuggestionListOpen);
        Assert.Equal("j", entry.Text);
        Assert.Equal("A", entry.SelectionSummary);
        command.Received(1).Execute("j");
    }
}
