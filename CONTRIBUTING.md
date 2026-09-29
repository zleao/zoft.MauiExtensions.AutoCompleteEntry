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

## Coding style

Use [Microsoft's common C# conventions](https://learn.microsoft.com/dotnet/csharp/fundamentals/coding-style/coding-conventions), with the root [.editorconfig](.editorconfig) defining this repository's concrete preferences. Microsoft offers several related conventions rather than one mandatory .NET style; the field naming follows the [.NET runtime conventions](https://github.com/dotnet/runtime/blob/main/docs/coding-guidelines/coding-style.md).

- Indent C# with four spaces, put braces on separate lines (Allman style), and use braces for control-flow bodies, including single statements. Keep one statement per line.
- Put `using` directives outside namespaces and sort `System` directives first.
- Use C# type keywords (`string`, `int`, etc.). Use `var` when the type is apparent from the initializer; otherwise write the type explicitly.
- Use PascalCase for types, members, and constants; camelCase for parameters and locals; `_camelCase` for private instance fields; and `s_camelCase` for private static fields. Specify accessibility explicitly outside interfaces.
- Preserve public API names (including the `zoft` namespace), framework overrides, binding/XAML names, generated members, and descriptive test/event-handler names. Keep existing file-scoped or block-scoped namespace declarations. Style work must preserve behavior and resource lifetimes.
- These rules cover library, native platform, test, and sample C# files. Do not format generated files, `bin`, or `obj`. XML/XAML and project files retain their existing layout.

From the repository root, format all source folders, including platform code excluded by the current host's project evaluation:

```powershell
$sourceFiles = @(git ls-files -- '*.cs')
dotnet format whitespace . --folder --include $sourceFiles
```

Run targeted semantic style fixes against the solution and sample after restoring them:

```powershell
dotnet format style src/AutoCompleteEntry.sln --no-restore --severity info --diagnostics IDE0003 IDE0009 IDE0011 IDE0040 IDE0049 IDE0007 IDE0008 IDE0065 IDE1006
dotnet format style sample/AutoCompleteEntry.Sample/AutoCompleteEntry.Sample.csproj --no-restore --severity info --diagnostics IDE0003 IDE0009 IDE0011 IDE0040 IDE0049 IDE0007 IDE0008 IDE0065 IDE1006
```

Append `--verify-no-changes` to check each command without modifying files. Include any new untracked C# files explicitly in `$sourceFiles`. Review the diff, run `git diff --check`, and perform the change-specific validation below. Project-based style analysis only checks successfully loaded targets and active preprocessor branches; review the other platform implementations as well, on a supported host when semantic analysis requires it. The solution already includes the sample; the separate sample command is useful for sample-only changes. Naming fixes may need manual review, particularly for partial classes and bindings. Multi-target and linked files can produce formatter conflict markers: resolve them and rerun verification before considering formatting complete. No additional analyzer package is required.

## Validation

Tests use VSTest with `xunit.v3.mtp-off` 4.0.1, the xUnit Visual Studio adapter,
and `coverlet.collector`. The explicit `mtp-off` variant preserves the existing
`dotnet test`, TRX logger and coverage collector workflow: the default `xunit.v3`
4.x package instead enables Microsoft Testing Platform v2, which requires a
different .NET 10 test configuration. See the [xUnit platform documentation](https://xunit.net/docs/getting-started/v3/microsoft-testing-platform).

Run commands from the repository root. The library Release build above matches CI and generates NuGet packages locally; it does not publish them. For a targeted platform build, pass `-f <target-framework>` using a framework declared in the project. A targeted build is useful feedback but does not replace the full CI build.

| Change | Validation |
| --- | --- |
| Documentation/instructions only | Check relative links, referenced paths, commands against project/workflow configuration, and `git diff --check`; no application build required. |
| Shared control behavior or helpers | Add or update meaningful regression tests, run the unit-test command above, and build the library. |
| Native handler/view behavior | Build the affected platform target and sample on a supported host; exercise the relevant scenarios below. Run shared tests if shared state/event behavior is affected. |
| Public API or binding usage | Build the library and sample, run relevant tests, and update the README/sample usage and changelog. |
| Project/dependency/build configuration | Build the library, run unit tests, and build the affected sample targets; inspect package output when packaging changes. |

The test project targets plain `net10.0`. It exercises shared control state, selection collections, mode transitions, query lifecycle and event order, plus linked Android density/template/placement helpers and the Windows suggestion snapshot helper. It does not instantiate native controls or validate dropdown rendering, keyboard behavior, or platform event subscriptions. A project reference to the multi-target library may still require MAUI workload restoration.

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
- For multiple selection, run the [native checklist](docs/multiple-selection.md#native-manual-script), including default/custom/selector rows, checkbox activation, query refinement and programmatic collection updates. Both sample pages scroll and group multiple-selection options in a bordered section.
- On Windows, check outside dismissal and reopening while still focused in both modes, clear after selection, light/dark popup appearance, and default-row vertical alignment.
- On Android, start at the top of With Events and focus the editor. With the sample's resize keyboard mode, the list must not cover the editor or keyboard. Repeat in both modes on a small viewport.
- For Apple changes, check both iOS and MacCatalyst implementations and validate on each affected platform when available.

Report the commands/results and the platforms actually exercised. Document any unavailable device, workload, or host checks as unverified.

## Changelog

Every user-visible change (new feature, bug fix, behavior change, deprecation) must be recorded in `CHANGELOG.md` under the `## [Unreleased]` section before the work is committed. See [Keep a Changelog](https://keepachangelog.com/en/1.1.0/) for the format.

Pure internal changes (refactoring, test additions, CI/tooling updates) do not require a changelog entry.

## Public API

This library is published as a NuGet package. Prefer additive or opt-in changes over renaming or changing existing behavior. When in doubt, open an issue to discuss the change before implementing it.
