# Contributing

Thank you for your interest in contributing to `zoft.MauiExtensions.Controls.AutoCompleteEntry`!

Read [AGENTS.md](AGENTS.md) for the repository map, architecture contracts, and guidance shared by all coding assistants. Tools that do not load it automatically should be pointed to that file explicitly. Keep these instructions provider-neutral rather than maintaining separate editor-specific copies.

## Branching workflow

- **Never commit directly to `main`.** The `main` branch represents the last released (or release-ready) state.
- Create a dedicated branch for every change:
  - Features: `feature/<short-description>` (e.g. `feature/upgrade-to-net10`)
  - Bug fixes: `fix/<short-description>` (e.g. `fix/ios-suggestion-layout`)
- Continue on an existing work branch unless a new branch is requested. Preserve unrelated local changes.
- When your work is ready for review, open a **Pull Request targeting `main`**.
- CI runs on PRs to `main` when paths matched by `.github/workflows/ci.yml` change (source/project files, the sample, workflows, or `global.json`). Documentation-only changes do not trigger it. CI builds the library, runs unit tests, and builds the Windows sample; it does not run native UI scenarios.

## Development setup

1. Install the [.NET SDK](https://dotnet.microsoft.com/download) selected by `global.json`, respecting its roll-forward policy. Use a host with the workloads and native tooling required by the target; Apple target validation requires an appropriately configured Mac/build host.
2. Restore MAUI workloads:
   ```
   dotnet workload restore src\AutoCompleteEntry\AutoCompleteEntry.csproj
   ```
3. Build the library:
   ```
   dotnet build src\AutoCompleteEntry\AutoCompleteEntry.csproj -c Release
   ```
4. Run unit tests:
   ```
   dotnet test src\Tests\AutoCompleteEntry.Tests\AutoCompleteEntry.Tests.csproj -c Release
   ```
5. Build the sample app on Windows:
   ```
   dotnet workload restore sample/AutoCompleteEntry.Sample/AutoCompleteEntry.Sample.csproj
   dotnet build sample/AutoCompleteEntry.Sample/AutoCompleteEntry.Sample.csproj -f net10.0-windows10.0.19041.0 -c Release
   ```

## Validation

Run commands from the repository root. The library Release build above matches CI and generates NuGet packages locally; it does not publish them. For a targeted platform build, pass `-f <target-framework>` using a framework declared in the project. A targeted build is useful feedback but does not replace the full CI build.

| Change | Validation |
| --- | --- |
| Documentation/instructions only | Check relative links, referenced paths, commands against project/workflow configuration, and `git diff --check`; no application build required. |
| Shared control behavior or helpers | Add or update meaningful regression tests, run the unit-test command above, and build the library. |
| Native handler/view behavior | Build the affected platform target and sample on a supported host; exercise the relevant scenarios below. Run shared tests if shared state/event behavior is affected. |
| Public API or binding usage | Build the library and sample, run relevant tests, and update the README/sample usage and changelog. |
| Project/dependency/build configuration | Build the library, run unit tests, and build the affected sample targets; inspect package output when packaging changes. |

The test project targets plain `net10.0`. It exercises shared control state, defaults, and event flow, plus linked Android `DensityHelper` and `TemplateIdMapper` source. It does not instantiate native controls or validate dropdown rendering, keyboard behavior, or platform event subscriptions. A project reference to the multi-target library may still require MAUI workload restoration.

### Manual platform checks

Use `WithBindingsPage` and/or `WithEventsPage` for affected scenarios. On Windows, launch the sample with:

```sh
dotnet run --project sample/AutoCompleteEntry.Sample/AutoCompleteEntry.Sample.csproj -f net10.0-windows10.0.19041.0
```

- Type, edit, and clear text: verify suggestions update through consumer filtering and text changes report the expected reason.
- Update `Text` programmatically: verify display/binding updates without a user-input filtering loop.
- Choose a suggestion: verify `SelectedSuggestion`, `SuggestionChosen`, selected text, and `UpdateTextOnSelect` behavior.
- Verify `TextMemberPath`, `DisplayMemberPath`, and supported item templates with the affected item type.
- Open/dismiss the dropdown, change focus, and exercise the keyboard, cursor, and layout behavior affected by the change.
- Navigate away and back or reconnect the handler: check for duplicate callbacks and stale native subscriptions.
- For Apple changes, check both iOS and MacCatalyst implementations and validate on each affected platform when available.

Report the commands/results and the platforms actually exercised. Document any unavailable device, workload, or host checks as unverified.

## Changelog

Every user-visible change (new feature, bug fix, behavior change, deprecation) must be recorded in `CHANGELOG.md` under the `## [Unreleased]` section before the work is committed. See [Keep a Changelog](https://keepachangelog.com/en/1.1.0/) for the format.

Pure internal changes (refactoring, test additions, CI/tooling updates) do not require a changelog entry.

## Public API

This library is published as a NuGet package. Prefer additive or opt-in changes over renaming or changing existing behavior. When in doubt, open an issue to discuss the change before implementing it.
