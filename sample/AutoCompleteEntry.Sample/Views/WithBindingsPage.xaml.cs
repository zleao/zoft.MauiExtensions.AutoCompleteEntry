using AutoCompleteEntry.Sample.ViewModels;

namespace AutoCompleteEntry.Sample.Views
{
    public partial class WithBindingsPage : ContentPage
    {
        private readonly DataTemplate _groupCountryTemplate;
        private readonly DataTemplate _wrappedTemplate;
        private readonly DataTemplate _selector;

        public WithBindingsPage()
        {
            BindingContext = new SampleViewModel("Port");

            InitializeComponent();
            _groupCountryTemplate = CountryEntry.ItemTemplate;
            _wrappedTemplate = (DataTemplate)Resources["WrappedSuggestion"];
            _selector = new CountryTemplateSelector(_groupCountryTemplate, _wrappedTemplate);
            TemplateMode.SelectedIndex = 1;
        }

        private void TemplateMode_Changed(object sender, EventArgs e)
        {
            if (CountryEntry is null || _groupCountryTemplate is null)
                return;
            CountryEntry.ItemTemplate = TemplateMode.SelectedIndex switch
            {
                1 => _groupCountryTemplate,
                2 => _wrappedTemplate,
                3 => _selector,
                _ => null
            };
        }

        private void UpdateTextOnSelect_Toggled(object sender, ToggledEventArgs e)
        {
            if (CountryEntry is not null)
                CountryEntry.UpdateTextOnSelect = e.Value;
        }

        private void AddSuggestion_Clicked(object sender, EventArgs e)
        {
            if (BindingContext is SampleViewModel viewModel)
                viewModel.FilteredList.Add(new ListItem
                {
                    Group = "Long row",
                    Country = "A long suggestion for checking wrapping, variable row heights and window resizing without replacing the observable collection."
                });
        }

        private sealed class CountryTemplateSelector(DataTemplate compact, DataTemplate wrapped) : DataTemplateSelector
        {
            protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
                => item is ListItem { Group: "Group A" or "Group H" } ? compact : wrapped;
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
