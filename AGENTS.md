# Repository guidance

This file is the shared entry point for coding assistants, regardless of provider or editor. Keep repository instructions here and task-specific detail in ordinary documentation; do not introduce parallel tool-specific instruction files. If a tool does not discover this file automatically, include it explicitly in the task context.

## Start here

- Read [CONTRIBUTING.md](CONTRIBUTING.md) for branch conventions, setup, commands, and validation requirements.
- Read the relevant sections of [README.md](README.md) for public API usage, platform limitations, and release procedures.
- Inspect the working tree before editing and preserve existing user changes. Continue on the current work branch unless a new branch is requested; create a work branch before editing on `main`.
- Keep changes focused on the requested task. Report what changed, the validation performed, and any checks that could not run.

## Repository map

| Path | Purpose |
| --- | --- |
| `src/AutoCompleteEntry/` | Published .NET MAUI control library |
| `src/AutoCompleteEntry/AutoCompleteEntry.cs` | Public bindable properties, events, and shared state transitions |
| `src/AutoCompleteEntry/Initialization.cs` | `UseZoftAutoCompleteEntry()` registration |
| `src/AutoCompleteEntry/Handlers/` | Shared handler contract, property mapper, and command mapper |
| `src/AutoCompleteEntry/Platforms/` | Native views, platform partial handlers, and extensions |
| `src/Tests/AutoCompleteEntry.Tests/` | Shared control tests and linked platform-independent Android helpers |
| `sample/AutoCompleteEntry.Sample/` | Reference app for binding, event, and native UI scenarios |
| `src/Directory.build.props` | Shared library/test build and package metadata; does not apply to the sample |
| `global.json` | SDK selection and roll-forward policy |
| `.github/workflows/` | CI and publishing behavior |

## Control contracts

- Preserve public member names, property defaults, and binding behavior. Prefer additive or opt-in API changes.
- Preserve `UseZoftAutoCompleteEntry()` and the XAML namespace `http://zoft.MauiExtensions/Controls`.
- Consumers own filtering: they react to text changes and update `ItemsSource`. Do not move filtering into the control unless explicitly requested.
- Preserve text-change reasons: `UserInput`, `ProgrammaticChange`, and `SuggestionChosen`. `TextChangedCommand` executes for user input; programmatic changes and selection must not accidentally trigger filtering loops.
- Preserve the base `Entry.TextChanged` event as well as the control's own event.
- `SelectedSuggestion` is two-way selection state. `TextMemberPath` determines selected text; `DisplayMemberPath` determines suggestion display.
- Keep shared behavior in the control and native wiring/rendering in platform implementations. Prefer platform partials over scattered conditional compilation in shared files.
- Unsubscribe native events in `DisconnectHandler` and release owned resources through the platform view's cleanup mechanism.

## Platform boundaries

- Android wraps `AndroidAutoCompleteEntry`; Windows wraps `AutoSuggestBox`.
- iOS and MacCatalyst each have their own `IOSAutoCompleteEntry`, handler, extensions, and table source files. These are separate, nearly identical implementations, not one shared source. Inspect both when changing Apple behavior and explain any intentional divergence.
- `Handlers/AutoCompleteEntryHandler.Standard.cs` supports the plain .NET target with no-op mappings; native view creation throws. Do not implement native UI behavior there or treat plain .NET tests as native UI coverage.
- Check README platform limitations before changing behavior. Windows support for `ItemTemplate` and `ShowBottomBorder` is incomplete.
- For observable platform/API changes, extend the appropriate sample scenario when existing coverage does not exercise the change: `Views/WithBindingsPage` for bindings and `Views/WithEventsPage` for events.

## Validation

Use the change-specific validation table and manual checklist in [CONTRIBUTING.md](CONTRIBUTING.md#validation). Run relevant checks on supported hosts. State unavailable workloads, platform tooling, or device checks explicitly; do not present a successful build or plain .NET test run as proof of native interaction behavior.

## Versions and releases

- MinVer derives the NuGet package version from Git tags/history. Do not add or bump `Version` or `PackageVersion` properties to release this package.
- Dependency versions, MAUI versions, the SDK version, and sample application versions are separate from the library's package version. Read their project/configuration files rather than copying version numbers into these instructions.
- Release tags use `X.Y.Z` or `X.Y.Z-preview.N`, without a `v` prefix. Full Git history and tags are required for reliable version calculation.
- Record user-visible changes under `[Unreleased]` in [CHANGELOG.md](CHANGELOG.md). Documentation, tooling, and internal-only changes do not require release notes.
- Release notes come from the matching changelog section through CI-generated `src/ReleaseNotes.g.props`; do not commit that generated file.
- Follow [README release instructions](README.md#-releasing) when a release is requested. Tag pushes publish automatically; ordinary development tasks do not imply creating release tags or publishing packages.
