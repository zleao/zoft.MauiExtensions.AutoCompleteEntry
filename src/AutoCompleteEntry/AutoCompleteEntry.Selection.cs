using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using zoft.MauiExtensions.Core.Extensions;

namespace zoft.MauiExtensions.Controls;

/// <summary>Controls whether suggestion activation selects one item or toggles several.</summary>
public enum AutoCompleteEntrySelectionMode
{
    /// <summary>Existing single-selection behavior.</summary>
    Single,
    /// <summary>Keep an ordered collection of selections independently of the query.</summary>
    Multiple
}

/// <summary>Immutable snapshots of the effective selection difference.</summary>
public sealed class AutoCompleteEntrySelectionChangedEventArgs : EventArgs
{
    internal AutoCompleteEntrySelectionChangedEventArgs(IEnumerable<object> added, IEnumerable<object> removed)
    {
        AddedItems = Array.AsReadOnly(added.ToArray());
        RemovedItems = Array.AsReadOnly(removed.ToArray());
    }

    /// <summary>Items added, in new selection order.</summary>
    public IReadOnlyList<object> AddedItems
    {
        get;
    }
    /// <summary>Items removed, in previous selection order.</summary>
    public IReadOnlyList<object> RemovedItems
    {
        get;
    }
}

public partial class AutoCompleteEntry
{
    private List<object> _effectiveSelection = [];
    private bool _changingSelection;
    private INotifyCollectionChanged? _observedSelection;
    private NotifyCollectionChangedEventHandler? _selectionObserver;

    /// <summary>Identifies <see cref="SelectionMode"/>.</summary>
    public static readonly BindableProperty SelectionModeProperty = BindableProperty.Create(
        nameof(SelectionMode), typeof(AutoCompleteEntrySelectionMode), typeof(AutoCompleteEntry),
        AutoCompleteEntrySelectionMode.Single,
        validateValue: (b, v) => v is AutoCompleteEntrySelectionMode.Single or AutoCompleteEntrySelectionMode.Multiple,
        propertyChanged: (b, o, n) => ((AutoCompleteEntry)b).ConvertSelectionMode());

    /// <summary>Gets or sets the selection mode. Defaults to Single.</summary>
    public AutoCompleteEntrySelectionMode SelectionMode
    {
        get => (AutoCompleteEntrySelectionMode)GetValue(SelectionModeProperty);
        set => SetValue(SelectionModeProperty, value);
    }

    /// <summary>Identifies <see cref="SelectedSuggestions"/>.</summary>
    public static readonly BindableProperty SelectedSuggestionsProperty = BindableProperty.Create(
        nameof(SelectedSuggestions), typeof(IList), typeof(AutoCompleteEntry), defaultBindingMode: BindingMode.TwoWay,
        defaultValueCreator: _ => new ObservableCollection<object>(),
        coerceValue: (b, v) => ValidateSelection(v),
        propertyChanged: (b, o, n) => ((AutoCompleteEntry)b).ReplaceSelection((IList)n));

    /// <summary>
    /// Ordered multiple selections. Null becomes a fresh empty collection. Use a mutable
    /// observable IList for external mutation notifications; replace ordinary lists after editing.
    /// Membership uses default object equality, not display text. Null entries are ignored;
    /// equal duplicates in consumer lists count once in first-occurrence order. External
    /// duplicate entries are not rewritten during observable notifications. User deselection
    /// removes every equal entry. Read-only/fixed-size lists throw ArgumentException.
    /// </summary>
    public IList SelectedSuggestions
    {
        get => (IList)GetValue(SelectedSuggestionsProperty);
        set => SetValue(SelectedSuggestionsProperty, value);
    }

    /// <summary>Raised after effective selection and presentation have been updated.</summary>
    public event EventHandler<AutoCompleteEntrySelectionChangedEventArgs>? SelectionChanged;

    /// <summary>
    /// Raised once when a search session opens, before native rows are displayed, even with
    /// empty ItemsSource. Multiple mode exposes an empty query. Populate ItemsSource here.
    /// </summary>
    public event EventHandler? SuggestionListOpening;

    /// <summary>Full, untruncated informational summary; never written to Text.</summary>
    public string SelectionSummary => string.Join("; ", _effectiveSelection.Select(GetSelectionText));

    internal event EventHandler? SelectionPresentationChanged;
    internal bool IsMultiple => SelectionMode == AutoCompleteEntrySelectionMode.Multiple;
    internal bool ShowsSelectionSummary => IsMultiple && !IsSuggestionListOpen && _effectiveSelection.Count > 0;
    internal bool IsSuggestionSelected(object? item) => item is not null && _effectiveSelection.Contains(item);
    internal string GetSelectionText(object item) => string.IsNullOrEmpty(TextMemberPath)
        ? item.ToString() ?? string.Empty : item.GetPropertyValueAsString(TextMemberPath);

    private static IList ValidateSelection(object? value)
    {
        IList list = value as IList ?? new ObservableCollection<object>();
        if (list.IsReadOnly || list.IsFixedSize)
        {
            throw new ArgumentException("SelectedSuggestions requires a mutable, variable-size IList.");
        }

        return list;
    }

    private void ObserveSelection(IList list)
    {
        if (_observedSelection is not null && _selectionObserver is not null)
        {
            _observedSelection.CollectionChanged -= _selectionObserver;
        }

        _observedSelection = list as INotifyCollectionChanged;
        if (_observedSelection is null)
        {
            return;
        }
        // A long-lived view-model collection must not keep a discarded control alive.
        var weakOwner = new WeakReference<AutoCompleteEntry>(this);
        NotifyCollectionChangedEventHandler? observer = null;
        observer = (sender, args) =>
        {
            if (weakOwner.TryGetTarget(out AutoCompleteEntry? owner))
            {
                owner.SynchronizeSelection();
            }
            else if (sender is INotifyCollectionChanged source)
            {
                source.CollectionChanged -= observer;
            }
        };
        _selectionObserver = observer;
        _observedSelection.CollectionChanged += observer;
    }

    private void ReplaceSelection(IList list)
    {
        ObserveSelection(list);
        SynchronizeSelection();
    }

    private void SynchronizeSelection()
    {
        if (_changingSelection)
        {
            return;
        }
        // Normalize the effective selection, without mutating a consumer's observable list
        // inside its CollectionChanged notification (ObservableCollection forbids reentrancy).
        List<object> next = IsMultiple ? SelectedSuggestions.Cast<object?>().OfType<object>().Distinct().ToList()
            : SelectedSuggestion is { } item ? new List<object> { item } : [];
        List<object> previous = _effectiveSelection;
        _effectiveSelection = next;
        RefreshSelectionPresentation();
        object[] added = next.Except(previous).ToArray();
        object[] removed = previous.Except(next).ToArray();
        if (added.Length != 0 || removed.Length != 0)
        {
            SelectionChanged?.Invoke(this, new(added, removed));
        }
    }

    private void SingleSelectionChanged()
    {
        if (!IsMultiple)
        {
            SynchronizeSelection();
        }
    }

    private void ConvertSelectionMode()
    {
        _changingSelection = true;
        try
        {
            // Always end the previous search. Inactive state is discarded at conversion;
            // preselection is assigned after setting SelectionMode to Multiple.
            IsSuggestionListOpen = false;
            object? first = IsMultiple ? SelectedSuggestion : _effectiveSelection.FirstOrDefault();
            SelectedSuggestions.Clear();
            if (IsMultiple)
            {
                if (first is not null)
                {
                    SelectedSuggestions.Add(first);
                }

                SelectedSuggestion = null;
                Text = string.Empty;
            }
            else
            {
                SelectedSuggestion = first;
                Text = first is null ? string.Empty : GetSelectionText(first);
            }
        }
        finally { _changingSelection = false; }
        SynchronizeSelection();
    }

    private void ToggleSuggestion(object? item)
    {
        if (item is null)
        {
            return;
        }

        bool selected = !SelectedSuggestions.Cast<object?>().Any(value => Equals(value, item));
        _changingSelection = true;
        try
        {
            if (selected)
            {
                SelectedSuggestions.Add(item);
            }
            else
            {
                for (int i = SelectedSuggestions.Count - 1; i >= 0; i--)
                {
                    if (Equals(SelectedSuggestions[i], item))
                    {
                        SelectedSuggestions.RemoveAt(i);
                    }
                }
            }
        }
        finally { _changingSelection = false; }
        SynchronizeSelection();
        SuggestionChosen?.Invoke(this, new(item, selected));
    }

    private void SuggestionListStateChanged(bool open)
    {
        if (IsMultiple)
        {
            Text = string.Empty;
        }

        RefreshSelectionPresentation();
        if (open)
        {
            SuggestionListOpening?.Invoke(this, EventArgs.Empty);
        }
    }

    private void RefreshSelectionPresentation()
    {
        OnPropertyChanged(nameof(SelectionSummary));
        SelectionPresentationChanged?.Invoke(this, EventArgs.Empty);
    }
}
