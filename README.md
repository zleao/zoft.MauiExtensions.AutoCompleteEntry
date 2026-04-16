# zoft.MauiExtensions.Controls.AutoCompleteEntry

A powerful AutoCompleteEntry control for .NET MAUI that makes suggestions to users as they type. This control provides rich customization options, data templating support, and works consistently across all supported platforms.

[![NuGet](https://img.shields.io/nuget/v/zoft.MauiExtensions.Controls.AutoCompleteEntry.svg)](https://www.nuget.org/packages/zoft.MauiExtensions.Controls.AutoCompleteEntry/)

## 📚 Table of Contents

- [✨ Features](#-features)
- [🚀 Getting Started](#-getting-started)
- [📋 Properties Reference](#-properties-reference)  
- [🎯 Basic Usage](#-basic-usage)
- [💡 Usage Examples](#-usage-examples)
- [🏗️ Platform Support Matrix](#️-platform-support-matrix)
- [📱 Platform Screenshots](#-platform-screenshots)
- [🎨 Advanced Customization](#-advanced-customization)
- [🐛 Troubleshooting](#-troubleshooting)
- [🚀 Releasing](#-releasing)
- [💖 Support](#-support)
- [🤝 Contributing](#-contributing)
- [📄 License](#-license)
- [🙏 Acknowledgments](#-acknowledgments)

## ✨ Features

- 🔍 **Real-time filtering** as the user types
- 🎨 **Custom item templates** for rich suggestion display
- 📱 **Cross-platform support** (iOS, Android, Windows, MacCatalyst)
- 🔄 **Flexible data binding** with command and event-based approaches
- ⚙️ **Highly customizable** appearance and behavior
- 🎯 **Full Entry compatibility** - inherits all Entry properties and behaviors

## 🚀 Getting Started

### Installation

Add the NuGet package to your project:

```bash
dotnet add package zoft.MauiExtensions.Controls.AutoCompleteEntry
```

📦 [View on NuGet](https://www.nuget.org/packages/zoft.MauiExtensions.Controls.AutoCompleteEntry/)

### Setup

Initialize the library in your `MauiProgram.cs` file:

```csharp
using CommunityToolkit.Maui;
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
                .UseMauiCommunityToolkit()
                .UseZoftAutoCompleteEntry()  // 👈 Add this line
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

Add this namespace to your XAML files:

```xml
xmlns:zoft="http://zoft.MauiExtensions/Controls"
```

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
| `TextChangedCommand` | `ICommand` | `null` | Command executed when text changes (receives text as parameter) |

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
| `TextChanged` | `AutoCompleteEntryTextChangedEventArgs` | Fired when text changes (includes change reason) |
| `SuggestionChosen` | `AutoCompleteEntrySuggestionChosenEventArgs` | Fired when a suggestion is selected |
| `CursorPositionChanged` | `AutoCompleteEntryCursorPositionChangedEventArgs` | Fired when cursor position changes |

Plus all inherited Entry events: `Completed`, `Focused`, `Unfocused`

## 🎯 Basic Usage

The filtering of results happens as the user types. You can respond to text changes using either:

**🔗 Binding-Based Approach** (Recommended)
- Use `TextChangedCommand` for filtering logic
- Bind `SelectedSuggestion` for the selected item

**⚡ Event-Based Approach**
- Handle `TextChanged` event for filtering
- Handle `SuggestionChosen` event for selection

## 💡 Usage Examples

### Basic Example with Bindings

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

**ViewModel Implementation:**

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
    private ObservableCollection<CountryItem> _filteredList;

    [ObservableProperty]
    private CountryItem _selectedItem;

    [ObservableProperty]
    private int _cursorPosition;

    public SampleViewModel()
    {
        FilteredList = new(_allCountries);
    }

    [RelayCommand]
    private void TextChanged(string text)
    {
        FilteredList.Clear();
        
        var filtered = _allCountries.Where(item => 
            item.Country.Contains(text, StringComparison.OrdinalIgnoreCase) ||
            item.Group.Contains(text, StringComparison.OrdinalIgnoreCase));
            
        foreach (var item in filtered)
            FilteredList.Add(item);
    }
}

public class CountryItem
{
    public string Group { get; set; }
    public string Country { get; set; }
}
```

### Advanced Example with Custom ItemTemplate

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
    
    <!-- 🎨 Custom item template for rich display -->
    <zoft:AutoCompleteEntry.ItemTemplate>
        <DataTemplate x:DataType="vm:CountryItem">
            <Grid ColumnDefinitions="Auto,*,Auto" 
                  Padding="12,8" 
                  HeightRequest="60">
                
                <!-- Flag or Group Indicator -->
                <Border Grid.Column="0"
                        BackgroundColor="{Binding GroupColor}"
                        WidthRequest="4"
                        HeightRequest="40"
                        StrokeShape="RoundRectangle 2" />
                
                <!-- Country Details -->
                <StackLayout Grid.Column="1" 
                            Margin="12,0">
                    <Label Text="{Binding Country}"
                           FontSize="16"
                           FontAttributes="Bold"
                           TextColor="Black" />
                    <Label Text="{Binding Group}"
                           FontSize="12"
                           TextColor="Gray" />
                </StackLayout>
                
                <!-- Population or other info -->
                <Label Grid.Column="2"
                       Text="{Binding Population, StringFormat='{0:N0}'}"
                       FontSize="12"
                       TextColor="DarkGray"
                       VerticalOptions="Center" />
            </Grid>
        </DataTemplate>
    </zoft:AutoCompleteEntry.ItemTemplate>
</zoft:AutoCompleteEntry>
```

### Event-Based Example

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

**Code-Behind Implementation:**

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
    Console.WriteLine($"Cursor moved to position: {e.NewCursorPosition}");
}
```

### Programmatic Control Examples

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

Create rich, interactive suggestion lists with custom DataTemplates:

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
