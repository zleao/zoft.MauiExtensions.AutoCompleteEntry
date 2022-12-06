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
    }
}