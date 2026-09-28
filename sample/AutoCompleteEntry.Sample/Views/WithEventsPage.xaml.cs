using AutoCompleteEntry.Sample.ViewModels;

namespace AutoCompleteEntry.Sample.Views
{
    public partial class WithEventsPage : ContentPage
    {
        private SampleViewModel ViewModel => BindingContext as SampleViewModel;

        public WithEventsPage()
        {
            BindingContext = new SampleViewModel("");

            InitializeComponent();
        }

        private void AutoCompleteEntry_TextChanged(object sender, zoft.MauiExtensions.Controls.AutoCompleteEntryTextChangedEventArgs e)
        {
            // Only get results when it was a user typing, 
            // otherwise assume the value got filled in by TextMemberPath 
            // or the handler for SuggestionChosen.
            if (e.Reason == zoft.MauiExtensions.Controls.AutoCompleteEntryTextChangeReason.UserInput)
            {
                //Set the ItemsSource to be your filtered dataset
                ViewModel.FilterList((sender as zoft.MauiExtensions.Controls.AutoCompleteEntry).Text);
            }
        }

        private void AutoCompleteEntry_SuggestionChosen(object sender, zoft.MauiExtensions.Controls.AutoCompleteEntrySuggestionChosenEventArgs e)
        {
            Log($"SuggestionChosen: {(e.SelectedItem as ListItem)?.Country}, selected={e.IsSelected}");
            if (CountryEntry.SelectionMode == zoft.MauiExtensions.Controls.AutoCompleteEntrySelectionMode.Multiple) return;
            // Set sender.Text. You can use args.SelectedItem to build your text string.
            ViewModel.SelectedItem = e.SelectedItem as ListItem;
        }

        private void AutoCompleteEntry_CursorPositionChanged(object sender, zoft.MauiExtensions.Controls.AutoCompleteEntryCursorPositionChangedEventArgs e)
        {
            ViewModel.CursorPosition = e.CursorPosition;
        }

        private void AutoCompleteEntry_Completed(object sender, EventArgs e)
        {
            if (sender is zoft.MauiExtensions.Controls.AutoCompleteEntry autoCompleteEntry &&
                autoCompleteEntry.SelectionMode == zoft.MauiExtensions.Controls.AutoCompleteEntrySelectionMode.Single)
            {
                ViewModel.SelectedItem = ViewModel.GetExactMatch(autoCompleteEntry.Text);
            }
        }

        private void CountryEntry_Opening(object sender, EventArgs e)
        {
            Log($"Opening: query='{CountryEntry.Text}'");
            ViewModel.FilterList(CountryEntry.Text);
        }

        private void CountryEntry_SelectionChanged(object sender, zoft.MauiExtensions.Controls.AutoCompleteEntrySelectionChangedEventArgs e)
        {
            if (SelectionStatus is not null) SelectionStatus.Text = CountryEntry.SelectionSummary;
            Log($"SelectionChanged: +{e.AddedItems.Count}, -{e.RemovedItems.Count}");
        }

        private void MultipleSelection_Toggled(object sender, ToggledEventArgs e)
            => CountryEntry.SelectionMode = e.Value
                ? zoft.MauiExtensions.Controls.AutoCompleteEntrySelectionMode.Multiple
                : zoft.MauiExtensions.Controls.AutoCompleteEntrySelectionMode.Single;

        private void OpenSuggestions_Clicked(object sender, EventArgs e)
            => CountryEntry.IsSuggestionListOpen = true;

        private void CloseSuggestions_Clicked(object sender, EventArgs e)
            => CountryEntry.IsSuggestionListOpen = false;

        private void AddSelection_Clicked(object sender, EventArgs e)
        {
            var item = ViewModel.GetExactMatch("Portugal");
            if (!CountryEntry.SelectedSuggestions.Contains(item)) CountryEntry.SelectedSuggestions.Add(item);
        }

        private void RemoveSelection_Clicked(object sender, EventArgs e)
        {
            if (CountryEntry.SelectedSuggestions.Count > 0) CountryEntry.SelectedSuggestions.RemoveAt(0);
        }

        private void ReplaceSelection_Clicked(object sender, EventArgs e)
            => CountryEntry.SelectedSuggestions = new System.Collections.ObjectModel.ObservableCollection<ListItem>
            { ViewModel.GetExactMatch("Portugal"), ViewModel.GetExactMatch("Japan") };

        private void ClearSelection_Clicked(object sender, EventArgs e) => CountryEntry.SelectedSuggestions.Clear();

        private void Log(string message)
        {
            if (EventLog is not null)
                EventLog.Text = string.Join(Environment.NewLine,
                    ((EventLog.Text ?? "") + Environment.NewLine + message).Split(Environment.NewLine).TakeLast(5));
        }
    }
}
