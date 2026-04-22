# Copilot instructions

This repository is a **.NET MAUI control library** for `zoft.MauiExtensions.Controls.AutoCompleteEntry`, plus a sample app that demonstrates intended usage patterns.

## Repository shape
- `src/AutoCompleteEntry` contains the reusable control library that is shipped as the NuGet package.
- `sample/AutoCompleteEntry.Sample` is the reference app used to exercise the control on real MAUI targets.
- `.github/copilot-instructions.md` is this file; keep repo-wide guidance here and use `.github/instructions/*.instructions.md` for narrower rules.

## Build and run commands
- Restore MAUI workloads before the first build:
  - `dotnet workload restore src/AutoCompleteEntry/AutoCompleteEntry.csproj`
- Build the library the same way CI does:
  - `dotnet build src/AutoCompleteEntry/AutoCompleteEntry.csproj -c Release`
- Build the sample app on Windows:
  - `dotnet build sample/AutoCompleteEntry.Sample/AutoCompleteEntry.Sample.csproj -f net10.0-windows10.0.19041.0`
- Run the sample app on Windows:
  - `dotnet run --project sample/AutoCompleteEntry.Sample/AutoCompleteEntry.Sample.csproj -f net10.0-windows10.0.19041.0`

- Run the unit tests:
  - `dotnet test src/Tests/AutoCompleteEntry.Tests/AutoCompleteEntry.Tests.csproj`

The sample app remains the main integration surface for platform behavior changes.

## Architecture
- `AutoCompleteEntry.cs` is the shared public surface: bindable properties, events, and the control-side state transitions.
- `Initialization.cs` exposes `UseZoftAutoCompleteEntry()`, which is the required MAUI registration hook for consumers and for the sample app.
- `Handlers/AutoCompleteEntryHandler.cs` defines the shared property mapper and command mapper. Platform-specific behavior is implemented in partial handler files under `Platforms/<Platform>/`.
- `Handlers/AutoCompleteEntryHandler.Standard.cs` is the no-op fallback for unsupported targets; do not put real behavior there.
- Each platform wraps a native control:
  - Android uses `AndroidAutoCompleteEntry`
  - Windows uses `AutoSuggestBox`
  - iOS and MacCatalyst use a custom `IOSAutoCompleteEntry` view with a text field plus suggestion table
- The control does **not** own filtering logic. Consumers update `ItemsSource` in response to `TextChangedCommand` or the `TextChanged` event.

## Changelog
- Every user-visible change (new feature, bug fix, behavior change, deprecation) must be recorded in `CHANGELOG.md` before the work is committed.
- Add entries under the `## [Unreleased]` section using [Keep a Changelog](https://keepachangelog.com/en/1.1.0/) subsections: `Added`, `Changed`, `Fixed`, `Removed`, `Deprecated`, `Security`.
- Pure internal changes (refactoring with no observable effect, test additions, CI/tooling updates) do not require a changelog entry.
- `CHANGELOG.md` is the source of truth for NuGet `PackageReleaseNotes`: the publish workflow extracts the matching version section automatically on each tag push, so an accurate changelog is essential.

## Branching workflow
- **Never commit directly to `main`.** All development must happen in a dedicated feature/fix branch.
- Branch naming convention: `feature/<short-description>` for new features, `fix/<short-description>` for bug fixes.
- When work is ready for review, open a Pull Request targeting `main`. CI runs automatically on every PR.
- The `main` branch should always represent the last released (or release-ready) state.

## Repo-specific conventions
- Treat the public API as stable. This is a published NuGet package, so prefer additive or opt-in changes over renaming or changing existing behavior.
- Keep shared behavior in `AutoCompleteEntry.cs`; keep native wiring, event subscriptions, and platform rendering in `Platforms/<Platform>/` partial handlers and native views.
- Avoid adding new scattered `#if` blocks in shared files when a platform partial can express the same change.
- When text handling changes, preserve the `AutoCompleteEntryTextChangeReason` flow:
  - `UserInput` is the trigger for filtering
  - `ProgrammaticChange` is raised when code updates `Text`
  - `SuggestionChosen` is raised when a selection updates the control
- `SelectedSuggestion` is the two-way selection state; `TextMemberPath` and `DisplayMemberPath` are used to map item objects into text/list display.
- If UI behavior changes and there is still no automated test coverage, update the sample app so the change is exercised in either `WithBindingsPage` or `WithEventsPage`.
- Keep documented platform gaps consistent with the README. Windows still has known limitations around `ItemTemplate` and `ShowBottomBorder`.
