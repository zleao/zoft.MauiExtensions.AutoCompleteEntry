# Migrating to 6.0

The multiple-selection work targets the planned **6.0.0 major release**. It is not
published by the implementation PR. Package versions come from MinVer tags; the
release tag is `6.0.0`, without a `v` prefix.

## Dependencies

The library and sample reference `Microsoft.Maui.Controls` **10.0.110**; the
MacCatalyst graphics reference is also **10.0.110**. If your app pins MAUI packages,
update those references consistently to avoid a dependency downgrade. Target
frameworks remain .NET 10. The test SDK, xUnit framework/runner, NSubstitute and
coverage collector were also updated; these test dependencies are not part of the
published control's consumer API.

## Existing single-selection applications

`SelectionMode` still defaults to `Single`. Existing property names, defaults,
`SelectedSuggestion` bindings, `UseZoftAutoCompleteEntry()` registration and the
`http://zoft.MauiExtensions/Controls` XAML namespace remain available. No model
conversion or multiple-selection collection is required for single mode.

Review these behavior changes when upgrading:

- **Windows dismissal:** clicking outside the editor and suggestions now closes
  the list even when the clicked page area cannot take focus. Clicking the still-
  focused editor reopens it. Dismissal preserves single-mode text and selection.
  Applications relying on the dropdown staying open after an outside click need
  to revise that interaction.
- **Windows result timing:** native presenters receive deferred snapshots of
  `ItemsSource`, coalescing changes within the current dispatcher turn. The
  consumer's collection and item identities are preserved, but native list
  realization is not synchronous with a collection mutation. This avoids WinUI
  collection reentrancy during filtering/clear-button callbacks.
- **Android keyboard layout:** the sample now uses resize instead of window
  panning. Scrollable consumer pages should follow the [keyboard setup](../README.md#android-keyboard-layout).
  The control does not change an application's keyboard configuration globally;
  native single-mode dropdown overlap can still occur with window panning.

These are behavioral migration considerations. The new public selection members
are additive; the major version does not mean single selection was removed.

## Opting into multiple selection

1. Set `SelectionMode="Multiple"` and bind `SelectedSuggestions` to a mutable
   `ObservableCollection<T>`. Set the mode before assigning initial selections.
   Arrays/read-only collections are unsupported. Selection uses object equality,
   not display labels. Models do not need an `IsSelected` property.
2. Treat `Text` as the search query. Read `SelectedSuggestions` for selected
   objects and `SelectionSummary` for informational text. Never parse the summary.
3. Populate initial results in `SuggestionListOpening`. Closing clears the query
   programmatically and does not invoke `TextChangedCommand`; reopening may
   otherwise show stale results. Consumers still own filtering and asynchronous
   request cancellation/stale-result protection.
4. Handle `SelectionChanged` for effective membership changes, including bound
   collection edits. `SuggestionChosen` reports user activation and now also
   reports deselection in multiple mode: inspect `IsSelected`. Single-mode
   activation reports `true`.
5. Expect the clear button to clear only the query. Use `SelectedSuggestions.Clear()`
   to remove selections. `UpdateTextOnSelect` has no effect in multiple mode.

Switching modes closes the search and discards the query. Single → multiple carries
the current item; multiple → single retains only the first effective selection.
Inactive state is cleared rather than revived on subsequent switches.

See the [complete collection/event contract and examples](../README.md#multiple-selection)
and [platform validation checklist](multiple-selection.md). Android emulator checks
do not establish Apple or Windows accessibility/device coverage.
