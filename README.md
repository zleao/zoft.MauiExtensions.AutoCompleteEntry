# zoft.MauiExtensions.Controls.AutoCompleteEntry

A .NET MAUI `Entry`-derived control that shows app-provided suggestions while the user types.

The control is responsible for displaying suggestions, handling selection, and preserving familiar `Entry` behavior. **Your app remains responsible for filtering data and updating `ItemsSource`.**

[![NuGet](https://img.shields.io/nuget/v/zoft.MauiExtensions.Controls.AutoCompleteEntry.svg)](https://www.nuget.org/packages/zoft.MauiExtensions.Controls.AutoCompleteEntry/)

## 📚 Table of Contents

- [✨ Features](#-features)
- [🚀 Getting Started](#-getting-started)
- [📋 Properties Reference](#-properties-reference)  
- [🎯 Basic Usage](#-basic-usage)
- [💡 Usage Examples](#-usage-examples)
- [🏗️ Platform Support Matrix](#️-platform-support-matrix)
- [🎨 Advanced Customization](#-advanced-customization)
- [🐛 Troubleshooting](#-troubleshooting)
- [🚀 Releasing](#-releasing)
- [💖 Support](#-support)
- [🤝 Contributing](#-contributing)
- [📄 License](#-license)
- [🙏 Acknowledgments](#-acknowledgments)

## ✨ Features

- 🔍 **Real-time suggestions** driven by your app's filtering logic
- 🎨 **Custom item templates** for rich suggestion rendering
- 🔄 **Binding-first and event-based APIs**
- 📱 **Cross-platform support** for Android, iOS, Windows, and MacCatalyst
- 🎯 **Entry compatibility** with familiar MAUI `Entry` properties and events
- ⚙️ **Selection and dropdown control** through bindable properties

## 🚀 Getting Started

### Installation

Add the NuGet package to your project:

```bash
dotnet add package zoft.MauiExtensions.Controls.AutoCompleteEntry
```

📦 [View on NuGet](https://www.nuget.org/packages/zoft.MauiExtensions.Controls.AutoCompleteEntry/)

### Setup

Register the control in `MauiProgram.cs`:

```csharp
using zoft.MauiExtensions.Controls;

namespace AutoCompleteEntry.Sample
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseZoftAutoCompleteEntry()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            return builder.Build();
        }
    }
}
```

### XAML Namespace

Add the control namespace to the XAML file where you want to use the control:

```xml
xmlns:zoft="http://zoft.MauiExtensions/Controls"
```

### First working example

```xml
<zoft:AutoCompleteEntry
    Placeholder="Search for a country"
    ItemsSource="{Binding FilteredList}"
    DisplayMemberPath="Country"
    TextMemberPath="Country"
    SelectedSuggestion="{Binding SelectedItem}"
    TextChangedCommand="{Binding TextChangedCommand}" />
```

```csharp
public sealed class CountryItem
{
    public string Group { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
}

public sealed partial class SampleViewModel : ObservableObject
{
    private readonly List<CountryItem> _allCountries =
    [
        new() { Group = "Group A", Country = "Ecuador" },
        new() { Group = "Group A", Country = "Netherlands" },
        new() { Group = "Group B", Country = "England" }
    ];

    [ObservableProperty]
    public partial ObservableCollection<CountryItem> FilteredList { get; set; } = [];

    [ObservableProperty]
    public partial CountryItem? SelectedItem { get; set; }

    [RelayCommand]
    private void TextChanged(string? text)
    {
        var filter = text ?? string.Empty;

        FilteredList = new ObservableCollection<CountryItem>(
            _allCountries.Where(item =>
                item.Country.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                item.Group.Contains(filter, StringComparison.OrdinalIgnoreCase)));
    }
}
```

This is the key pattern for the control:

1. The user types.
2. `TextChangedCommand` or `TextChanged` fires.
3. Your app filters its data.
4. Your app assigns the filtered results to `ItemsSource`.

For complete working examples, see the sample app in `sample\AutoCompleteEntry.Sample`.

## 📋 Properties Reference

### AutoCompleteEntry-Specific Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `ItemsSource` | `IList` | `null` | Collection of suggestion items to display |
| `SelectedSuggestion` | `object` | `null` | Currently selected suggestion item (two-way binding) |
| `DisplayMemberPath` | `string` | `""` | Property path for displaying items in the suggestion list |
| `TextMemberPath` | `string` | `""` | Property path for the text value when an item is selected |
| `ItemTemplate` | `DataTemplate` | `null` | Custom template for rendering suggestion items |
| `IsSuggestionListOpen` | `bool` | `false` | Controls whether the suggestion dropdown is open |
| `UpdateTextOnSelect` | `bool` | `true` | Whether selecting an item updates the text field |
| `ShowBottomBorder` | `bool` | `true` | Controls the visibility of the bottom border |
| `TextChangedCommand` | `ICommand` | `null` | Command executed when the user types (receives the current text as parameter) |

### Inherited Entry Properties

AutoCompleteEntry inherits from `Entry`, so all standard Entry properties are available:

| Property | Description |
|----------|-------------|
| `Text` | The current text value |
| `Placeholder` | Placeholder text when empty |
| `PlaceholderColor` | Color of the placeholder text |
| `TextColor` | Color of the input text |
| `FontSize`, `FontFamily`, `FontAttributes` | Text formatting |
| `IsReadOnly` | Whether the text can be edited |
| `MaxLength` | Maximum character length |
| `CursorPosition` | Current cursor position |
| `ClearButtonVisibility` | When to show the clear button |
| `HorizontalTextAlignment`, `VerticalTextAlignment` | Text alignment |
| `CharacterSpacing` | Spacing between characters |
| `IsTextPredictionEnabled` | Enable/disable text prediction |
| `ReturnType` | Keyboard return key type |

### Events

| Event | EventArgs | Description |
|-------|-----------|-------------|
| `TextChanged` | `AutoCompleteEntryTextChangedEventArgs` | Fired when text changes and includes the reason |
| `SuggestionChosen` | `AutoCompleteEntrySuggestionChosenEventArgs` | Fired when a suggestion is selected |
| `CursorPositionChanged` | `AutoCompleteEntryCursorPositionChangedEventArgs` | Fired when cursor position changes |

Plus all inherited Entry events: `Completed`, `Focused`, `Unfocused`

### Text change reasons

`AutoCompleteEntryTextChangedEventArgs.Reason` helps you distinguish why `TextChanged` fired:

- `UserInput`: the user typed in the control
- `ProgrammaticChange`: your code changed `Text`
- `SuggestionChosen`: the user picked an item from the suggestions list

`TextChangedCommand` only runs for `UserInput`, which makes it the recommended hook for filtering.

## 🎯 Basic Usage

`AutoCompleteEntry` does **not** filter your data source internally. Instead, it acts as a UI shell around your own filtering logic.

The most common setup is:

1. Bind `ItemsSource` to a filtered collection.
2. Use `DisplayMemberPath` to control how items appear in the suggestion list.
3. Use `TextMemberPath` to control what text is written back into the entry when an item is selected.
4. Use either:
   - **binding-based filtering** with `TextChangedCommand` (**recommended**)
   - **event-based filtering** with `TextChanged`

## 💡 Usage Examples

### Binding-based example

```xml
<zoft:AutoCompleteEntry
    Placeholder="Search for a country or group"
    ItemsSource="{Binding FilteredList}"
    TextMemberPath="Country"
    DisplayMemberPath="Country"
    TextChangedCommand="{Binding TextChangedCommand}"
    SelectedSuggestion="{Binding SelectedItem}"
    HeightRequest="50" />
```

**ViewModel implementation:**

```csharp
public partial class SampleViewModel : ObservableObject
{
    private readonly List<CountryItem> _allCountries = new()
    {
        new CountryItem { Group = "Group A", Country = "Ecuador" },
        new CountryItem { Group = "Group B", Country = "Netherlands" },
        // ... more items
    };
    
    [ObservableProperty]
    private ObservableCollection<CountryItem> _filteredList = new();

    [ObservableProperty]
    private CountryItem? _selectedItem;

    public SampleViewModel()
    {
        FilteredList = new ObservableCollection<CountryItem>(_allCountries);
    }

    [RelayCommand]
    private void TextChanged(string? text)
    {
        var filter = text ?? string.Empty;

        var filtered = _allCountries.Where(item => 
            item.Country.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
            item.Group.Contains(filter, StringComparison.OrdinalIgnoreCase));

        FilteredList = new ObservableCollection<CountryItem>(filtered);
    }
}

public sealed class CountryItem
{
    public string Group { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
}
```

### Binding-based example with `ItemTemplate`

```xml
<zoft:AutoCompleteEntry
    Placeholder="Search countries with custom display"
    ItemsSource="{Binding FilteredList}"
    TextMemberPath="Country"
    DisplayMemberPath="Country"
    TextChangedCommand="{Binding TextChangedCommand}"
    SelectedSuggestion="{Binding SelectedItem}"
    ShowBottomBorder="{Binding ShowBottomBorder}"
    HeightRequest="50">

    <zoft:AutoCompleteEntry.ItemTemplate>
        <DataTemplate x:DataType="vm:CountryItem">
            <Grid ColumnDefinitions="Auto,*"
                  Padding="12,8"
                  HeightRequest="44">
                <Border Grid.Column="0"
                        BackgroundColor="Red"
                        WidthRequest="4"
                        HeightRequest="28"
                        StrokeShape="RoundRectangle 2" />

                <VerticalStackLayout Grid.Column="1"
                                     Margin="12,0">
                    <Label Text="{Binding Country}"
                           FontSize="16"
                           FontAttributes="Bold"
                           TextColor="Black" />
                    <Label Text="{Binding Group}"
                           FontSize="12"
                           TextColor="Gray" />
                </VerticalStackLayout>
            </Grid>
        </DataTemplate>
    </zoft:AutoCompleteEntry.ItemTemplate>
</zoft:AutoCompleteEntry>
```

### Event-based example

```xml
<zoft:AutoCompleteEntry
    Placeholder="Search for a country or group"
    ItemsSource="{Binding FilteredList}"
    TextMemberPath="Country"
    DisplayMemberPath="Country"
    TextChanged="AutoCompleteEntry_TextChanged"
    SuggestionChosen="AutoCompleteEntry_SuggestionChosen"
    CursorPositionChanged="AutoCompleteEntry_CursorPositionChanged"
    ClearButtonVisibility="WhileEditing"
    HeightRequest="50" />
```

**Code-behind implementation:**

```csharp
private void AutoCompleteEntry_TextChanged(object sender, AutoCompleteEntryTextChangedEventArgs e)
{
    // Only filter when the user is actually typing
    if (e.Reason == AutoCompleteEntryTextChangeReason.UserInput)
    {
        var autoComplete = sender as AutoCompleteEntry;
        ViewModel.FilterList(autoComplete.Text);
    }
}

private void AutoCompleteEntry_SuggestionChosen(object sender, AutoCompleteEntrySuggestionChosenEventArgs e)
{
    // Handle the selected suggestion
    if (e.SelectedItem is CountryItem selectedCountry)
    {
        ViewModel.SelectedItem = selectedCountry;
        // Perform additional actions like navigation or validation
    }
}

private void AutoCompleteEntry_CursorPositionChanged(object sender, AutoCompleteEntryCursorPositionChangedEventArgs e)
{
    // Track cursor position for analytics or custom behavior
    Console.WriteLine($"Cursor moved to position: {e.CursorPosition}");
}

private void AutoCompleteEntry_Completed(object sender, EventArgs e)
{
    if (sender is AutoCompleteEntry autoCompleteEntry)
    {
        ViewModel.SelectedItem = ViewModel.GetExactMatch(autoCompleteEntry.Text);
    }
}
```

### Programmatic control examples

```csharp
// Programmatically open/close the suggestion list
autoCompleteEntry.IsSuggestionListOpen = true;

// Control text updates on selection
autoCompleteEntry.UpdateTextOnSelect = false; // Keep original text when selecting

// Customize appearance
autoCompleteEntry.ShowBottomBorder = false; // Remove bottom border
autoCompleteEntry.ClearButtonVisibility = ClearButtonVisibility.WhileEditing;

// Handle selection programmatically
autoCompleteEntry.SelectedSuggestion = mySelectedItem;
```

## 🏗️ Platform Support Matrix

| Feature | Windows | Android | iOS | MacCatalyst | Notes |
|---------|---------|---------|-----|-------------|-------|
| **Core Functionality** |
| Text Input & Filtering | ✅ | ✅ | ✅ | ✅ | Full support |
| ItemsSource Binding | ✅ | ✅ | ✅ | ✅ | Full support |
| Selection Events | ✅ | ✅ | ✅ | ✅ | Full support |
| **Appearance & Styling** |
| ItemTemplate | ❌ | ✅ | ✅ | ✅ | Windows: Planned for future release |
| ShowBottomBorder | ❌ | ✅ | ✅ | ✅ | Windows: Planned for future release |
| Text Styling | ✅ | ✅ | ✅ | ✅ | Fonts, colors, alignment |
| **Behavior** |
| UpdateTextOnSelect | ✅ | ✅ | ✅ | ✅ | Full support |
| IsSuggestionListOpen | ✅ | ✅ | ✅ | ✅ | Full support |
| CursorPosition | ✅ | ✅ | ✅ | ✅ | Full support |
| **Entry Features** |
| ClearButtonVisibility | ✅ | ✅ | ✅ | ✅ | Full support |
| Placeholder | ✅ | ✅ | ✅ | ✅ | Full support |
| IsReadOnly | ✅ | ✅ | ✅ | ✅ | Full support |
| MaxLength | ✅ | ✅ | ✅ | ✅ | Full support |

### Legend
- ✅ **Fully Supported** - Feature works as expected
- ❌ **Not Implemented** - Feature exists in API but not yet implemented on this platform
- ⚠️ **Limited Support** - Feature works with some limitations

### Windows Platform Notes

While the AutoCompleteEntry works great on Windows, some advanced styling features are still in development:

- **ItemTemplate**: Currently displays items using their string representation. Custom templates are planned for a future release.
- **ShowBottomBorder**: This styling option doesn't affect the Windows presentation currently.

All core functionality including filtering, selection, and data binding works perfectly on Windows.

## 🎨 Advanced Customization

### Custom Item Templates

Use `ItemTemplate` when you want richer suggestion rows than plain text:

```xml
<zoft:AutoCompleteEntry.ItemTemplate>
    <DataTemplate x:DataType="models:Product">
        <Grid ColumnDefinitions="60,*,Auto" 
              RowDefinitions="Auto,Auto"
              Padding="16,12">
            
            <!-- Product Image -->
            <Image Grid.RowSpan="2"
                   Source="{Binding ImageUrl}"
                   WidthRequest="50"
                   HeightRequest="50"
                   Aspect="AspectFill" />
            
            <!-- Product Info -->
            <Label Grid.Column="1"
                   Text="{Binding Name}"
                   FontSize="16"
                   FontAttributes="Bold" />
            
            <Label Grid.Column="1" Grid.Row="1"
                   Text="{Binding Category}"
                   FontSize="12"
                   TextColor="Gray" />
            
            <!-- Price -->
            <Label Grid.Column="2" Grid.RowSpan="2"
                   Text="{Binding Price, StringFormat='${0:F2}'}"
                   FontSize="14"
                   FontAttributes="Bold"
                   VerticalOptions="Center"
                   HorizontalOptions="End" />
        </Grid>
    </DataTemplate>
</zoft:AutoCompleteEntry.ItemTemplate>
```

### Styling and Theming

```xml
<Style x:Key="CustomAutoCompleteStyle" TargetType="zoft:AutoCompleteEntry">
    <Setter Property="BackgroundColor" Value="{DynamicResource SurfaceColor}" />
    <Setter Property="TextColor" Value="{DynamicResource OnSurfaceColor}" />
    <Setter Property="PlaceholderColor" Value="{DynamicResource OnSurfaceVariantColor}" />
    <Setter Property="FontSize" Value="16" />
    <Setter Property="HeightRequest" Value="56" />
    <Setter Property="Margin" Value="16,8" />
    <Setter Property="ShowBottomBorder" Value="True" />
</Style>
```

### Performance Tips

1. **Efficient Filtering**: Use proper indexing and async operations for large datasets
2. **Template Complexity**: Keep ItemTemplates lightweight for smooth scrolling  
3. **Data Virtualization**: Consider implementing virtualization for very large lists
4. **Debouncing**: Implement debouncing in your TextChangedCommand for better UX

```csharp
// Example: Debounced filtering
private CancellationTokenSource _filterCancellation;

[RelayCommand]
private async Task TextChanged(string text)
{
    _filterCancellation?.Cancel();
    _filterCancellation = new CancellationTokenSource();
    
    try
    {
        await Task.Delay(300, _filterCancellation.Token); // Debounce
        await FilterItemsAsync(text);
    }
    catch (TaskCanceledException)
    {
        // Filtering was cancelled by newer input
    }
}
```

## 🐛 Troubleshooting

### Common Issues

**Issue**: Suggestions not appearing
- ✅ Ensure `ItemsSource` is properly bound and contains data
- ✅ Check `DisplayMemberPath` matches your data model properties
- ✅ Verify the control has sufficient height to display suggestions

**Issue**: Selection not working
- ✅ Confirm `SelectedSuggestion` binding is two-way
- ✅ Check `TextMemberPath` property is correctly set
- ✅ Ensure the selected item exists in the current `ItemsSource`

**Issue**: Custom templates not rendering (Windows)
- ⚠️ ItemTemplate is not yet implemented on Windows platform
- ✅ Use DisplayMemberPath for basic text display on Windows

**Issue**: Performance issues with large datasets
- ✅ Implement efficient filtering logic
- ✅ Use proper async/await patterns
- ✅ Consider pagination or virtual scrolling

## 💖 Support

If you find this project helpful, please consider supporting its development:

[![Sponsor](https://img.shields.io/badge/Sponsor-❤️-blue?style=for-the-badge&logo=github-sponsors)](https://github.com/sponsors/zleao)

Your support helps maintain and improve this project for the entire .NET MAUI community. Thank you! 🙏

## 🚀 Releasing

This repository uses **manual GitHub Releases** plus a **manual GitHub Actions publish workflow**.

### Maintainer release flow

1. Update the package version in `src\Directory.build.props`.
2. Move the pending notes from `CHANGELOG.md` into a new version section.
3. Merge the release changes to `main`.
4. Create a **draft GitHub Release** with tag `vX.Y.Z`.
5. Click **Generate release notes** and refine the result into a short human-friendly summary.
6. Run the **Publish package** workflow manually from the Actions tab.
7. Provide:
   - `ref`: the branch or tag to build
   - `release_tag`: the existing GitHub release tag if you want validation and release asset upload
   - `publish_to_nuget`: enable only when you want to push to NuGet.org
   - `attach_to_release`: enable only when you want the built `.nupkg` / `.snupkg` attached to the release
8. Publish the GitHub Release after the notes and assets look correct.

### Notes

- Pull requests to `main` only run CI; they do **not** create releases or publish packages.
- GitHub's generated release notes are grouped by `.github\release.yml`.
- Publishing to NuGet requires the `NUGET_API_KEY` repository secret.
- `CHANGELOG.md` is the long-lived human changelog; GitHub Releases are the release-specific summary.

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request. For major changes, please open an issue first to discuss what you would like to change.

## 📄 License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details.

## 🙏 Acknowledgments

- Inspired by platform-native autocomplete controls
- Built with ❤️ for the .NET MAUI community
