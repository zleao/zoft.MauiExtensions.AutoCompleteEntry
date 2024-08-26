using AutoCompleteEntry.Sample.ViewModels;

namespace AutoCompleteEntry.Sample.Views
{
    public partial class WithBindingsPage : ContentPage
    {
        public WithBindingsPage()
        {
            BindingContext = new SampleViewModel();

            InitializeComponent();
        }

        private void AutoCompleteEntry_Completed(object sender, EventArgs e)
        {
            if (sender is zoft.MauiExtensions.Controls.AutoCompleteEntry autoCompleteEntry &&
                BindingContext is SampleViewModel viewModel)
            {
                viewModel.SelectedItem = viewModel.GetExactMatch(autoCompleteEntry.Text);
            }
        }
    }
}