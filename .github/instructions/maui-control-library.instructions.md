---
applyTo: "src/AutoCompleteEntry/**/*.cs,sample/AutoCompleteEntry.Sample/**/*.cs,sample/AutoCompleteEntry.Sample/**/*.xaml,sample/AutoCompleteEntry.Sample/**/*.csproj"
description: "Repo-specific guidance for the AutoCompleteEntry MAUI control library and sample app."
---

# AutoCompleteEntry MAUI library guidance

## Control library rules
- Keep the shared control contract in `src/AutoCompleteEntry/AutoCompleteEntry.cs`: bindable properties, event args, and cross-platform behavior belong there.
- Keep platform behavior in `src/AutoCompleteEntry/Platforms/<Platform>/...` and in the partial `AutoCompleteEntryHandler` implementation for that platform.
- If a platform needs cleanup, unsubscribe events in `DisconnectHandler` and release native resources in the platform view's cleanup method.
- Do not move filtering logic into the control unless the feature explicitly requires it. The intended model is: app code reacts to text changes and updates `ItemsSource`.

## API compatibility
- This library is shipped as `zoft.MauiExtensions.Controls.AutoCompleteEntry`; preserve public member names and default property behavior unless the change is clearly opt-in.
- Keep `UseZoftAutoCompleteEntry()` working as the single MAUI registration entry point.
- Preserve the XAML namespace contract: `xmlns:zoft="http://zoft.MauiExtensions/Controls"`.

## Sample app expectations
- The sample app is the closest thing this repo has to integration coverage. Use `WithBindingsPage` for binding-first scenarios and `WithEventsPage` for event-driven scenarios.
- If a change affects platform behavior or public API usage, update the sample page that demonstrates that path.

## Platform-specific notes
- Windows uses `AutoSuggestBox`, so MAUI `Entry` features are not always 1:1 there.
- iOS and MacCatalyst share the custom `IOSAutoCompleteEntry` implementation and are the most relevant files when changing dropdown layout, keyboard behavior, or templated rendering.
- Keep the README's documented limitation in mind: Windows support for `ItemTemplate` and `ShowBottomBorder` is still incomplete.
