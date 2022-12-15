using zoft.MauiExtensions.Controls.Handlers;

namespace zoft.MauiExtensions.Controls
{
    public static class Initialization
    {
        public static MauiAppBuilder UseZoftAutoCompleteEntry(this MauiAppBuilder mauiAppBuilder)
        {
            return mauiAppBuilder.ConfigureMauiHandlers(handlers =>
            {
                handlers.AddHandler(typeof(AutoCompleteEntry), typeof(AutoCompleteEntryHandler));
            });
        }
    }
}
