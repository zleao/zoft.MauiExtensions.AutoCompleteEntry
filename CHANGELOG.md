# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this project follows [Semantic Versioning](https://semver.org/).

Historical entries before this file was added may be summarized from package metadata instead of full release notes.

## [Unreleased]

Planned for **6.0.0** (major release). See the [migration guide](docs/migration-6.0.md).

### Added
- Opt-in multiple selection on Android, Windows, iOS, and MacCatalyst: two-way `SelectedSuggestions`, checkbox suggestion rows, persistent query sessions, and an ellipsized selection summary separate from `Text` ([#74](https://github.com/zleao/zoft.MauiExtensions.AutoCompleteEntry/issues/74)).
- `SelectionChanged`, `SuggestionListOpening`, and `SuggestionChosen.IsSelected`, with observable selection synchronization and deterministic mode transitions. Single selection remains the default.
- Scrollable binding/event samples with grouped multiple-selection options, initial-list population, programmatic selection changes, and event-order reporting.

### Changed
- Updated `Microsoft.Maui.Controls` in the library/sample and the MacCatalyst `Microsoft.Maui.Graphics` dependency to `10.0.110`.
- **Breaking behavior (Windows):** outside clicks now dismiss single-selection suggestions even on non-focusable page background; clicking the still-focused editor reopens them. Text and selection are preserved on dismissal.
- Windows native suggestion updates are deferred and coalesced through snapshots rather than applied synchronously during consumer collection notifications.
- The Android sample uses keyboard resize mode with its ScrollViews. Applications using window panning should review the documented keyboard configuration; the control does not change the host's keyboard mode globally.

### Fixed
- iOS and MacCatalyst suggestions become visible when an initially empty observable result collection is populated during an open session, without resetting the query or repeating the opening event.
- Android sample pages resize for the keyboard so suggestions do not cover the editor during window panning. Single-selection dropdowns also refresh their measurements when viewport or anchor bounds change.
- Vertically centered the default Windows multiple-selection suggestion text beside its checkbox.
- Windows multiple-selection suggestions reopen when clicking the still-focused editor after outside dismissal.
- Windows multiple selection preserves the native editor border/background when displaying its summary and uses the native suggestion template's popup styling.
- Windows multiple-selection suggestions use an opaque, theme-aware background. Suggestion updates are deferred through snapshots to prevent reentrant WinUI collection changes when clearing or refiltering the query.
- Android multiple-selection queries no longer reset when filtering produces no results. Popup placement uses available space above or below the editor without covering the keyboard, and selection refresh ignores disposed native rows.

## [5.2.0] - 2026-09-18

### Added
- Windows support for MAUI suggestion `ItemTemplate` and `DataTemplateSelector`, including runtime template changes and restoring native text rendering ([#38](https://github.com/zleao/zoft.MauiExtensions.AutoCompleteEntry/issues/38)).

## [5.1.0]

### Changed
- Updated `zoft.MauiExtensions.Core` to `6.1.0`.
- Updated the library's .NET MAUI dependencies to `10.0.101`.

## [5.0.0]

### Changed
- Upgraded target frameworks to .NET MAUI 10 (`net10.0-android`, `net10.0-ios`, `net10.0-maccatalyst`, `net10.0-windows10.0.19041.0`).
- Bumped `Microsoft.Maui.Controls` to `10.0.70`, `CommunityToolkit.WinUI.Extensions` to `8.2.251219`, and `CommunityToolkit.Maui` (sample) to `14.1.0`.

## [4.0.5]

### Changed
- Improved MacCatalyst support, including `ItemTemplate`, width/height handling, and related platform behavior.

### Fixed
- Fired the base `TextChanged` event consistently.
