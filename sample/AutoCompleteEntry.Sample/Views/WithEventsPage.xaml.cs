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
            // Set sender.Text. You can use args.SelectedItem to build your text string.
            ViewModel.SelectedItem = e.SelectedItem as ListItem;
        }

        private void AutoCompleteEntry_CursorPositionChanged(object sender, zoft.MauiExtensions.Controls.AutoCompleteEntryCursorPositionChangedEventArgs e)
        {
            ViewModel.CursorPosition = e.CursorPosition;
        }

        private void AutoCompleteEntry_Completed(object sender, EventArgs e)
        {
            if (sender is zoft.MauiExtensions.Controls.AutoCompleteEntry autoCompleteEntry)
            {
                ViewModel.SelectedItem = ViewModel.GetExactMatch(autoCompleteEntry.Text);
            }
        }
    }
}