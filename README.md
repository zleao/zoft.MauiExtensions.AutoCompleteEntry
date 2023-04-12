# zoft.MauiExtensions.Controls.AutoCompleteEntry

Entry control that makes suggestions to users as they type.

**NOTE:** This control is based on the awesome [dotMortem/XamarinFormsControls/AutoSuggestBox](https://github.com/dotMorten/XamarinFormsControls/tree/main/AutoSuggestBox). with some simplifications and modifications of my own.

[![NuGet](https://img.shields.io/nuget/v/zoft.MauiExtensions.Controls.AutoCompleteEntry.svg)](https://www.nuget.org/packages/zoft.MauiExtensions.Controls.AutoCompleteEntry/)

## Getting Started

### Instalation

Add NuGet Package to your project: 
```
dotnet add package zoft.MauiExtensions.Controls.AutoCompleteEntry`
```
You can find the nuget package here [zoft.MauiExtensions.Controls.AutoCompleteEntry](https://www.nuget.org/packages/zoft.MauiExtensions.Controls.AutoCompleteEntry/)

<br/>

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

<br/>

### How to Use

The filtering of results, happens as the user types and you'll only need to respond to 2 actions:

**Binding based**
- `TextChangedCommand`: Triggered every time the user changes the text. Receives the current text as parameter;
- `SelectedSuggestion`: Holds the currently selected option;

**Event based**
- `TextChanged`: Event raised every time the user changes the text. The current text is part of the event arguments;
- `SuggestionChosen`: Event raised every time a suggestion is chosen. The selected option is part of the event arguments;

<br/>

### XAML Usage
In order to make use of the control within XAML you can use this namespace:

```xml
xmlns:zoft="http://zoft.MauiExtensions/Controls"
```

<br/>

### Sample Using Bindings

```xml
<ContentPage ...
             xmlns:zoft="http://zoft.MauiExtensions/Controls"
             ...>

    <zoft:AutoCompleteEntry Placeholder="Search for a country or group"
                            ItemsSource="{Binding FilteredList}"
                            TextMemberPath="Country"
                            DisplayMemberPath="Country"
                            TextChangedCommand="{Binding TextChangedCommand}"
                            SelectedSuggestion="{Binding SelectedItem}"/>
</ContentPage>

```
```csharp
internal partial class ListItem : ObservableObject
{
    [ObservableProperty]
    public string _group;

    [ObservableProperty]
    public string _country;
}

internal partial class SampleViewModel : CoreViewModel
{
    private readonly List<ListItem> Teams  = new List<ListItem>() { ... }
    
    [ObservableProperty]
    private ObservableCollection<ListItem> _filteredList;

    [ObservableProperty]
    private ListItem _selectedItem;

    public Command<string> TextChangedCommand { get; }

    public SampleViewModel()
    {
        FilteredList = new(Teams);
        SelectedItem = null;
        TextChangedCommand = new Command<string>(FilterList);
    }

    private void FilterList(string filter)
    {
        SelectedItem = null;
        FilteredList.Clear();

        FilteredList.AddRange(Teams.Where(t => t.Group.Contains(filter, StringComparison.CurrentCultureIgnoreCase) ||
                                               t.Country.Contains(filter, StringComparison.CurrentCultureIgnoreCase)));
    }
}
```


<br/>

### Sample Using Events

```xml
<ContentPage ...
             xmlns:zoft="http://zoft.MauiExtensions/Controls"
             ...>

    <zoft:AutoCompleteEntry Placeholder="Search for a country or group"
                            ItemsSource="{Binding FilteredList}"
                            TextMemberPath="Country"
                            DisplayMemberPath="Country"
                            TextChanged="AutoCompleteEntry_TextChanged"
                            SuggestionChosen="AutoCompleteEntry_SuggestionChosen"/>
</ContentPage>

```
```csharp
private void AutoCompleteEntry_TextChanged(object sender, zoft.MauiExtensions.Controls.AutoCompleteEntryTextChangedEventArgs e)
{
    // Filter only when the user is typing
    if (e.Reason == zoft.MauiExtensions.Controls.AutoCompleteEntryTextChangeReason.UserInput)
    {
        //Filter the ItemsSource, based on text
        ViewModel.FilterList((sender as zoft.MauiExtensions.Controls.AutoCompleteEntry).Text);
    }
}

private void AutoCompleteEntry_SuggestionChosen(object sender, zoft.MauiExtensions.Controls.AutoCompleteEntrySuggestionChosenEventArgs e)
{
    //Set the SelectedItem provided by the event arguments
    ViewModel.SelectedItem = e.SelectedItem as ListItem;
}
```

<br/>
<br/>

#### Windows

|![](docs/Windows_1.png)|![](docs/Windows_2.png)|![](docs/Windows_3.png)|![](docs/Windows_4.png)|
|:---:|:---:|:---:|:---:|

#### Android

|![](docs/Android_1.png)|![](docs/Android_2.png)|![](docs/Android_3.png)|![](docs/Android_4.png)|
|:---:|:---:|:---:|:---:|

#### iOS

|![](docs/iOS_1.png)|![](docs/iOS_2.png)|![](docs/iOS_3.png)|![](docs/iOS_4.png)|
|:---:|:---:|:---:|:---:|

#### MacCatalyst

|![](docs/MacCatalyst_1.png)|![](docs/MacCatalyst_2.png)|![](docs/MacCatalyst_3.png)|![](docs/MacCatalyst_4.png)|
|:---:|:---:|:---:|:---:|
