# Copilot instructions

You are contributing to **zoft.MauiExtensions.Controls.AutoCompleteEntry**, a **.NET MAUI custom control library** that provides an `AutoCompleteEntry` (an `Entry`-like control that shows suggestions while the user types). :contentReference[oaicite:3]{index=3}

## Core goals
- Keep the control **reliable, lightweight, and dependency-minimal**.
- Preserve **backwards compatibility** wherever possible (this is a published NuGet control).
- Prefer **clean MAUI patterns** (handlers, bindable properties, XAML-friendly APIs).
- Ensure behavior is consistent across supported platforms, and document any unavoidable differences.

## Platform support
- Follow the repo’s stated supported platforms (as documented in the README).
- Only introduce platform-conditional code when necessary:
  - Use `Platforms/<PlatformName>/...` with partial classes or platform services.
  - Avoid scattering `#if ANDROID` / `#if WINDOWS` / `#if IOS` throughout shared code.

## Public API rules (VERY IMPORTANT)
- Treat the public API surface as stable:
  - Do **not** rename public types/members or change behavior of existing properties/events without strong justification.
  - If you must break behavior, add an **opt-in** property or a new overload instead.
- Keep XAML usage ergonomic:
  - Bindable properties must have correct default values, binding modes, and change callbacks.
  - Events should have clear event args types and be fired deterministically.

## Implementation guidance
- Prefer **MAUI Handler architecture**:
  - Keep control logic (filtering, selection, open/close state) in shared code.
  - Put native view wiring / rendering in handlers/platform implementations.
- Avoid doing expensive work on the UI thread.
- Avoid unnecessary allocations in hot paths (e.g., text-changed filtering).
- Always unsubscribe from events and dispose native resources properly.

## Feature-specific notes for this control
- The README documents current features, setup (`UseZoftAutoCompleteEntry()`), and known platform limitations.
  - Do not implement features in ways that conflict with documented behavior.
- Windows currently has limitations around `ItemTemplate` (per README). When touching templating:
  - Keep existing behavior working.
  - If you add/extend Windows support, do it behind tests and clear docs. :contentReference[oaicite:7]{index=7}

## Coding standards
- Use modern C# and .NET practices:
  - Nullable enabled, no null-forgiving unless justified.
  - Prefer small, single-purpose methods.
  - Avoid synchronous blocking on async (`.Result`, `.Wait()`).
  - Use `CancellationToken` in potentially long-running operations (e.g., async suggestion fetching scenarios).
- Keep formatting consistent with repo conventions and `.editorconfig` (if present).

## Testing & sample app
- If you add behavior, add coverage where feasible:
  - Unit tests for non-UI logic (filtering, state transitions).
  - If UI tests aren’t available, at minimum update/extend the sample app usage to exercise the change.
- Keep the sample app as a reference implementation; don’t hardcode sample-only hacks into the library.

## Documentation expectations
- Update README / docs when you add or change:
  - Bindable properties
  - Events
  - Platform differences/limitations
  - Setup steps (`UseZoftAutoCompleteEntry()` / XAML namespace)
- Add concise XML docs for public APIs and any non-obvious logic.

## Dependency policy
- Avoid introducing new third-party dependencies unless explicitly requested.
- Prefer using MAUI/CommunityToolkit primitives already in use by the repo’s samples (when appropriate).

## When generating code changes (Copilot behavior)
Always:
- Follow existing naming, folder structure, and patterns already present in the repo.
- Keep the solution compiling (no placeholder types or TODO-only stubs).
- Consider cross-platform impact and test on at least one mobile + one desktop target when feasible.
Avoid:
- Large refactors unrelated to the requested change.
- Broad API changes without strong compatibility reasoning.
