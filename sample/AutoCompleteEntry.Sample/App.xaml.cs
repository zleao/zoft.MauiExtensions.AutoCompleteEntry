namespace AutoCompleteEntry.Sample
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            // Let the page ScrollView reveal the focused editor. Window panning
            // displaces Android's separately positioned native suggestion popup.
            Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.Application.SetWindowSoftInputModeAdjust(
                this, Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.WindowSoftInputModeAdjust.Resize);
        }
        protected override Window CreateWindow(IActivationState activationState)
        {
            return new Window(new AppShell());
        }
    }
}
