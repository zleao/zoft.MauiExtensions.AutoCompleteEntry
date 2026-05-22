# Contributing

Thank you for your interest in contributing to `zoft.MauiExtensions.Controls.AutoCompleteEntry`!

## Branching workflow

- **Never commit directly to `main`.** The `main` branch represents the last released (or release-ready) state.
- Create a dedicated branch for every change:
  - Features: `feature/<short-description>` (e.g. `feature/upgrade-to-net10`)
  - Bug fixes: `fix/<short-description>` (e.g. `fix/ios-suggestion-layout`)
- When your work is ready for review, open a **Pull Request targeting `main`**.
- CI runs automatically on every PR (build + unit tests + sample build on Windows).

## Development setup

1. Install the [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0).
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
   dotnet test src\Tests\AutoCompleteEntry.Tests\AutoCompleteEntry.Tests.csproj
   ```
5. Build the sample app on Windows:
   ```
   dotnet build sample/AutoCompleteEntry.Sample/AutoCompleteEntry.Sample.csproj -f net10.0-windows10.0.19041.0
   ```

## Changelog

Every user-visible change (new feature, bug fix, behavior change, deprecation) must be recorded in `CHANGELOG.md` under the `## [Unreleased]` section before the work is committed. See [Keep a Changelog](https://keepachangelog.com/en/1.1.0/) for the format.

Pure internal changes (refactoring, test additions, CI/tooling updates) do not require a changelog entry.

## Public API

This library is published as a NuGet package. Prefer additive or opt-in changes over renaming or changing existing behavior. When in doubt, open an issue to discuss the change before implementing it.
