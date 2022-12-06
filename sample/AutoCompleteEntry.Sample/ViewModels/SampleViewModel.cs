using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using zoft.MauiExtensions.Core.Extensions;
using zoft.MauiExtensions.Core.ViewModels;

namespace AutoCompleteEntry.Sample.ViewModels
{
    internal partial class ListItem : ObservableObject
    {
        [ObservableProperty]
        public string _group;

        [ObservableProperty]
        public string _country;
    }

    internal partial class SampleViewModel : CoreViewModel
    {
        private readonly List<ListItem> Teams  = new List<ListItem>()
        {
            new ListItem() { Group = "Group A", Country = "Ecuador" },
            new ListItem() { Group = "Group A", Country = "Netherlands" },
            new ListItem() { Group = "Group A", Country = "Qatar" },
            new ListItem() { Group = "Group A", Country = "Senegal" },
            new ListItem() { Group = "Group B", Country = "England" },
            new ListItem() { Group = "Group B", Country = "Iran" },
            new ListItem() { Group = "Group B", Country = "Usa" },
            new ListItem() { Group = "Group B", Country = "Wales" },
            new ListItem() { Group = "Group C", Country = "Argentina" },
            new ListItem() { Group = "Group C", Country = "Mexico" },
            new ListItem() { Group = "Group C", Country = "Poland" },
            new ListItem() { Group = "Group C", Country = "Saudi Arabia" },
            new ListItem() { Group = "Group D", Country = "Australia" },
            new ListItem() { Group = "Group D", Country = "Denmark" },
            new ListItem() { Group = "Group D", Country = "France" },
            new ListItem() { Group = "Group D", Country = "Tunisia" },
            new ListItem() { Group = "Group E", Country = "Costa Rica" },
            new ListItem() { Group = "Group E", Country = "Germany" },
            new ListItem() { Group = "Group E", Country = "Japan" },
            new ListItem() { Group = "Group E", Country = "Spain" },
            new ListItem() { Group = "Group F", Country = "Belgium" },
            new ListItem() { Group = "Group F", Country = "Canada" },
            new ListItem() { Group = "Group F", Country = "Croatia" },
            new ListItem() { Group = "Group F", Country = "Morocco" },
            new ListItem() { Group = "Group G", Country = "Brazil" },
            new ListItem() { Group = "Group G", Country = "Cameroon" },
            new ListItem() { Group = "Group G", Country = "Serbia" },
            new ListItem() { Group = "Group G", Country = "Switzerland" },
            new ListItem() { Group = "Group H", Country = "Ghana" },
            new ListItem() { Group = "Group H", Country = "Portugal" },
            new ListItem() { Group = "Group H", Country = "South Korea" },
            new ListItem() { Group = "Group H", Country = "Uruguai" }
        };

        [ObservableProperty]
        private ObservableCollection<ListItem> _filteredList;

        [ObservableProperty]
        private ListItem _selectedItem;

        public Command<string> TextChangedCommand { get; }

        public SampleViewModel()
        {
            FilteredList = new(Teams);
            SelectedItem = null;

            TextChangedCommand = new Command<string>(OnTextChanged);
        }

        public void FilterList(string filter)
        {
            SelectedItem = null;
            FilteredList.Clear();

            FilteredList.AddRange(Teams.Where(t => t.Group.Contains(filter, StringComparison.CurrentCultureIgnoreCase) ||
                                                   t.Country.Contains(filter, StringComparison.CurrentCultureIgnoreCase)));
        }

        private void OnTextChanged(string text)
        {
            FilterList(text);
        }
    }
}
