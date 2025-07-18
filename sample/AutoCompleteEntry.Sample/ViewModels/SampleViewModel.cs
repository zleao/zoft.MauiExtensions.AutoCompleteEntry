using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using zoft.MauiExtensions.Core.Models;

namespace AutoCompleteEntry.Sample.ViewModels
{
    internal partial class ListItem : ObservableObject
    {
        [ObservableProperty]
        private string _group;

        [ObservableProperty]
        private string _country;
    }

    internal partial class SampleViewModel : ZoftObservableObject
    {
        private readonly List<ListItem> _teams =
        [
            new ListItem { Group = "Group A", Country = "Ecuador" },
            new ListItem { Group = "Group A", Country = "Netherlands" },
            new ListItem { Group = "Group A", Country = "Qatar" },
            new ListItem { Group = "Group A", Country = "Senegal" },
            new ListItem { Group = "Group B", Country = "England" },
            new ListItem { Group = "Group B", Country = "Iran" },
            new ListItem { Group = "Group B", Country = "Usa" },
            new ListItem { Group = "Group B", Country = "Wales" },
            new ListItem { Group = "Group C", Country = "Argentina" },
            new ListItem { Group = "Group C", Country = "Mexico" },
            new ListItem { Group = "Group C", Country = "Poland" },
            new ListItem { Group = "Group C", Country = "Saudi Arabia" },
            new ListItem { Group = "Group D", Country = "Australia" },
            new ListItem { Group = "Group D", Country = "Denmark" },
            new ListItem { Group = "Group D", Country = "France" },
            new ListItem { Group = "Group D", Country = "Tunisia" },
            new ListItem { Group = "Group E", Country = "Costa Rica" },
            new ListItem { Group = "Group E", Country = "Germany" },
            new ListItem { Group = "Group E", Country = "Japan" },
            new ListItem { Group = "Group E", Country = "Spain" },
            new ListItem { Group = "Group F", Country = "Belgium" },
            new ListItem { Group = "Group F", Country = "Canada" },
            new ListItem { Group = "Group F", Country = "Croatia" },
            new ListItem { Group = "Group F", Country = "Morocco" },
            new ListItem { Group = "Group G", Country = "Brazil" },
            new ListItem { Group = "Group G", Country = "Cameroon" },
            new ListItem { Group = "Group G", Country = "Serbia" },
            new ListItem { Group = "Group G", Country = "Switzerland" },
            new ListItem { Group = "Group H", Country = "Ghana" },
            new ListItem { Group = "Group H", Country = "Portugal" },
            new ListItem { Group = "Group H", Country = "South Korea" },
            new ListItem { Group = "Group H", Country = "Uruguai" }
        ];

        [ObservableProperty]
        private ObservableCollection<ListItem> _filteredList;

        [ObservableProperty]
        private ListItem _selectedItem;

        [ObservableProperty]
        private int _cursorPosition;

        [ObservableProperty]
        private int _newCursorPosition;

        [ObservableProperty]
        private string _text;

        [ObservableProperty]
        private bool _showBottomBorder = true;

        public SampleViewModel(string text)
        {
            Text = text;
            FilterList(Text);
        }

        public void FilterList(string filter)
        {
            SelectedItem = null;
            
            FilteredList?.Clear();
            FilteredList = null;
            FilteredList = new ObservableCollection<ListItem>(
                _teams.Where(t => t.Group.Contains(filter ?? "", StringComparison.CurrentCultureIgnoreCase) ||
                                 t.Country.Contains(filter ?? "", StringComparison.CurrentCultureIgnoreCase)));
        }

        public ListItem GetExactMatch(string text)
        {
            return _teams.FirstOrDefault(t => t.Country.Equals(text, StringComparison.CurrentCultureIgnoreCase));
        }

        [RelayCommand]
        private void TextChanged(string text)
        {
            FilterList(text);
        }

        [RelayCommand]
        private void SetCursorPosition()
        {
            CursorPosition = NewCursorPosition;
        }
    }
}
