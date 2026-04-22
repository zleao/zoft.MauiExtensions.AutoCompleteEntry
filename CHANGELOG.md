# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this project follows [Semantic Versioning](https://semver.org/).

Historical entries before this file was added may be summarized from package metadata instead of full release notes.

## [Unreleased]

### Changed
- Upgraded target frameworks to .NET MAUI 10 (`net10.0-android`, `net10.0-ios`, `net10.0-maccatalyst`, `net10.0-windows10.0.19041.0`).
- Bumped `Microsoft.Maui.Controls` to `10.0.51`, `CommunityToolkit.WinUI.Extensions` to `8.2.251219`, and `CommunityToolkit.Maui` (sample) to `14.1.0`.

## [4.0.5]

### Changed
- Improved MacCatalyst support, including `ItemTemplate`, width/height handling, and related platform behavior.

### Fixed
- Fired the base `TextChanged` event consistently.
