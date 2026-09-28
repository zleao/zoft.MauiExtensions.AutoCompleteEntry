# Multiple selection implementation and validation

Specification: [issue #74](https://github.com/zleao/zoft.MauiExtensions.AutoCompleteEntry/issues/74).
The public collection, event, query, and mode-transition contracts are documented in
[README](../README.md#multiple-selection).
This work targets the planned 6.0.0 major release; see the [migration guide](migration-6.0.md).

## Design decisions

- Shared `AutoCompleteEntry.Selection.cs` owns membership, selection order, collection
  observation, mode conversion, deltas, summary text, and search-session transitions.
  Platform code never owns a second selected collection. `ItemsSource` is independent.
- `SelectedSuggestions` is a mutable `IList`; null creates an empty per-instance
  `ObservableCollection<object>`. Fixed-size/read-only lists are rejected. Membership
  uses the default object equality comparer. Nulls are ignored, and equal duplicates
  normalize to the first occurrence in effective state. Caller-supplied duplicate
  occurrences are left in the source list until deselection removes all of them.
  Mutating an `ObservableCollection` during its own notification would throw when
  other subscribers exist, so normalization deliberately does not do that.
- Observable mutations are synchronous on the UI thread. Plain lists must be replaced
  with a new instance to publish external edits. Collection replacement detaches the
  old observer. A weak owner reference prevents a long-lived collection from retaining
  a discarded control. Subscription lifetime is independent of native handler lifetime,
  so state remains correct while a control is temporarily disconnected.
- Mode conversion discards inactive state. It carries the single item into multiple
  mode, or the first effective multiple item into single mode. It closes the previous
  search, clears the inactive property, and never raises a user-activation event.
  Assign preselection after setting the mode. No public property defaults changed.
- `SelectionChanged` uses immutable snapshots and fires only for nonempty membership
  differences. Reordering updates summary presentation without a false add/remove.
  Checked presentation updates before selection events; bound collection writes precede
  both. Consumer event handlers may perform subsequent selection edits.
- In multiple mode, `IsSuggestionListOpen` represents the search session, including an empty result set.
  This lets consumers populate results during `SuggestionListOpening` and prevents
  asynchronous/transient result replacement from ending a query. Explicit/native
  dismissal ends the session. Summary presentation never enters the native text value.
- `SelectionRow` wraps content in a leading, passive checkbox. Existing selectors still
  receive their original container (root page on Android/Apple, owner control on Windows)
  and original item. Native row activation owns input, preventing a double toggle.

## Native implementation

| Platform | Multiple-mode approach | Lifecycle/accessibility |
| --- | --- | --- |
| Android | Separate nonmodal `PopupWindow`/`ListView` uses the existing adapter, bypassing `AutoCompleteTextView` completion and keyboard dismissal. Height uses the available space above or below the editor, accounting for the keyboard. Up/Down highlights; Enter activates once. Closed summary is drawn separately with `TextUtils.Ellipsize`. | Editor taps are excluded from outside dismissal. Back/dismissal synchronizes session state. Adapter observes result changes, refreshes recycled checks, exposes a checkable accessibility node/action, and disconnects owned row handlers. Reconnection recreates disposed adapter resources. |
| Windows | `AutoSuggestBox` remains the editor; a separate `Popup`/`ListView` owns multiple suggestions. `ItemClick` and Enter activate; native `SuggestionChosen` highlight changes are ignored in multiple mode. A `TextBlock` overlay provides ellipsis without changing query text. | Outside pointer routing excludes the editor, preserving caret edits; dismissal/focus departure closes the session. Row peers expose Toggle state and activation. Converter/template resources, events, popup, and summary are released at disconnect or mode change. |
| iOS | Existing table stays open on row activation and retains the text field's first responder. A passive checkbox wraps default/custom content; a separate `UILabel` truncates the closed summary. Hardware arrows highlight and Return activates. | Native edit/focus changes synchronize session state. Cells expose labels and selection traits. Table sources, row handlers, constraints, keyboard observers, and input subscriptions are cleaned up. |
| MacCatalyst | Same implementation as iOS, retained in its separate platform files. | No intentional behavioral divergence. Hardware keyboard, focus, VoiceOver, and layout still require checks on a Mac. |

## Acceptance review

| Issue requirement | Implementation / automated evidence | Native checklist (verified subset below) |
| --- | --- | --- |
| Single-mode compatibility | Existing shared tests retained; default remains Single; native single activation paths preserved. User-requested Windows outside dismissal and deferred result application are intentional behavioral changes documented for 6.0 | Existing single-mode text, selection, dropdown, keyboard scenarios on every platform |
| Default/custom/selector checkbox rows | Internal wrapper and existing template resolution/container contracts | Row sizing, selector changes, RTL, pointer versus checkbox activation |
| Binding-before-events, added/removed, IsSelected | `MultipleSelectionTests` verifies collection/presentation/SelectionChanged/SuggestionChosen order | Native activation raises the shared path exactly once |
| Replacement, observable edits, no loops | Tests cover detached old sources, additions, replacement, moves, clear, duplicates, reentrant event edits | Visible/recycled checks and summary after edits |
| A filtered out, then select B; deselect independently | Regression tests retain selection across result replacement/null | Focus and open list retained while refining a query |
| Close/reset without filtering; reopen stale/empty | Tests check base and reason-aware events, command suppression, opening callback timing and no repeats | Native outside dismissal, Escape/Back, focus loss, programmatic close |
| Query clear does not clear selections | Shared clear-query regression; native clear paths preserve selection | Clear button on all four targets |
| TextMemberPath/order/semicolon/ellipsis | Tests cover text resolution, path changes, equality independent of labels, semicolons and order | Resize/narrow field, long labels, complete accessibility text |
| Repeated/empty mode conversion | Tests cover inactive state, discarded extra selections, open query, empty transitions, no activation | Runtime row/popup/template switching |
| Accessibility, keyboard, focus, reuse | Native accessibility state/actions, explicit keyboard activation, authoritative row refresh | TalkBack, Narrator, VoiceOver; hardware keyboard; scroll/recycle |
| Navigation/reconnection cleanup | Explicit native event/resource cleanup and reconnect initialization; weak collection observer | Navigate repeatedly, reconnect handler, verify no duplicate callbacks or leaks |
| Samples/docs/changelog | Both sample pages expose mode switching, opening, query refinement, programmatic edits; README API and Unreleased entry updated | Run both sample pages on devices |
| Shared tests and builds | See validation record below | Builds do not prove native interaction correctness |

## Native manual script

Run both sample pages; repeat with default, custom, wrapped, and selector rows where
available. Start in Single and verify existing interactions before enabling Multiple.

1. Open with empty/stale results and verify one opening callback populates countries.
2. Select Ecuador by its checkbox. Refine to Japan (Ecuador disappears), select Japan
   by its row, and clear the query. Both must remain checked, with no keyboard dismissal.
3. Deselect Ecuador. Verify one removal, one activation with `IsSelected=false`, no
   query change, and an open list. Check the event log and bound collection count.
4. Use arrows to move highlight without changing selection, then Enter to toggle once.
   Type again immediately. Check pointer, accessibility activation, and hardware keys.
5. Close with Escape/Back, outside interaction/focus loss, and the sample open/close
   button. Verify empty query, no filtering command, and an ellipsized summary. Reopen
   and verify initial population and retained checks.
6. Add/remove/replace/clear selection programmatically, including while closed; scroll
   and reopen to verify recycled rows. Try an item whose text includes a semicolon and
   preselected objects absent from current results. Resize and test RTL.
7. Switch modes while searching, with several selections, and with none. Only the first
   item survives Multiple → Single; discarded items must not return on switching back.
8. Navigate away/back repeatedly and reconnect the handler. Check duplicate events,
   retained query/selection, disposed row resources, and accessibility checked state.

## Platform details

### Windows

Both selection modes dismiss on outside clicks and reopen from a still-focused
editor. Handled pointer routing excludes the editor and popup surface, preserving
caret placement, row activation and scrollbar interaction. Native close callbacks
cannot close a multiple-mode popup that has already reopened. Listeners are removed
on unload/disconnect or presenter disposal.

The multiple summary occupies the native TextBox content cell, using its padding,
alignment and typography. Only text content/placeholder are hidden; the border,
background and buttons stay visible. Default row labels are vertically centered.
The popup copies the installed AutoSuggestBox template's surface/list styling and
refreshes on theme changes, with an opaque fallback before the template is ready.

Both Windows lists receive deferred snapshots of consumer results. Observable
mutations and replacements coalesce to the latest state after collection callbacks
finish. Item identities are preserved; old subscriptions and queued work are
invalidated on replacement/disconnect. This prevents reentrant WinUI collection
updates when filtering during focus or clear-button callbacks. The binding sample
publishes one complete filtered collection per query.

### Android

Multiple mode detaches the native completion adapter and uses a separate popup,
so an empty result set cannot erase the query. Retired native rows are removed
from selection-refresh tracking and native peer lifetime is checked before use.
Popup placement uses visible screen bounds and updates during layout, scrolling
and pre-draw. Single-mode native dropdown measurements also refresh when anchor
or viewport bounds change.

The sample uses Android keyboard resize mode: its ScrollView reveals the editor
inside the smaller viewport instead of panning the whole window. Native single-mode
popup positioning can overlap the editor under window panning; host applications
should use the [documented resize setup](../README.md#android-keyboard-layout).
The control does not change global keyboard settings. Native wrap-content sizing
is retained in single mode.

## Validation record

PR preparation on 2026-09-28, Windows host with .NET SDK 10.0.401:

After updating the stable dependencies (MAUI 10.0.110), the full library and
Windows sample Release builds passed with zero warnings/errors. All 127 tests
passed with xUnit 4.0.1 using the VSTest-compatible `xunit.v3.mtp-off` package;
TRX output and Coverlet Cobertura collection were verified. The Android Release
APK was also rebuilt successfully with zero warnings/errors, using
`RunAOTCompilation=false` and the ASCII TEMP/TMP setup described below. Native
interaction and Android AOT were not rerun for this dependency update.

- Full Release library build: all five targets (plain .NET, Android, Windows,
  iOS, MacCatalyst), zero warnings/errors. Local packages were not published.
- Shared/helper tests: **127 passed**. Coverage includes collection state,
  synchronization, mode transitions, query lifecycle, event ordering, Windows
  snapshot isolation and Android popup geometry. Test compilation can report the
  repository's existing XML-comment warnings.
- Windows Release sample build: zero warnings/errors.
- Android Release sample build on 2026-09-24: zero warnings/errors. The final
  keyboard-layout APK used `RunAOTCompilation=false` for native iteration; AOT
  had passed earlier but was not rerun after that layout change. ASCII process-local
  TEMP/TMP paths under ignored `obj` avoided the host's Unicode-path AOT issue.

### Native checks performed

The native checks below predate the dependency update to MAUI 10.0.110. That
update was validated with builds and shared tests; native interaction has not
been rerun with the updated packages.

- Android API 36 Pixel 7 emulator: both sample pages, single-mode typing/selection,
  multiple row/checkbox toggles, filtering while retaining selections, empty-result
  queries, clearing, keyboard navigation/Enter, Back dismissal, programmatic edits,
  repeated mode conversion and custom/selector rows. Event logs showed selection
  deltas before activation. Ten consecutive type/clear cycles completed without a
  crash after the disposed-row fix.
- On a 1080x1920 emulator viewport, the initial With Events focus flow reproduced
  the overlap: keyboard panning moved the editor from Y=1307 to Y=930. With the
  resize configuration, both modes kept suggestions above the editor and keyboard;
  `jap` filtered to Japan, single selection committed Japan, and multiple mode
  retained Ecuador through filtering and dismissal. Crash logs were empty.
- Windows fixes were build/shared-test validated. The user subsequently reported
  that the result looked good. This is user feedback, not an automated native
  regression suite or a claim of complete Windows checklist coverage.

Temporary Android emulators were read-only and shut down after testing. No physical
phone was modified. Screenshots, hierarchy dumps and logs remain under ignored
sample `obj`; they are not part of the package or PR.

### Remaining native checks

No iOS/MacCatalyst device or Mac test host was available. Apple compilation does not
validate native interaction. TalkBack, Narrator, VoiceOver, exhaustive row recycling,
RTL/resizing and repeated handler reconnection remain manual checks on all targets.
On Windows, rerun the clear-after-selection, outside-dismiss/reopen and light/dark
appearance cases on both pages. Other Android device/keyboard combinations remain
unverified. Use the full checklist above before releasing; successful builds and
plain .NET tests do not establish native interaction correctness.
