using zoft.MauiExtensions.Controls.Handlers;

namespace zoft.MauiExtensions.Controls
{
    /// <summary>
    /// Initialization logic to include in Maui AppBuilder
    /// </summary>
    public static class Initialization
    {
        /// <summary>
        /// Configure app to use this control
        /// </summary>
        /// <param name="mauiAppBuilder"></param>
        /// <returns></returns>
        public static MauiAppBuilder UseZoftAutoCompleteEntry(this MauiAppBuilder mauiAppBuilder)
        {
            return mauiAppBuilder.ConfigureMauiHandlers(handlers =>
            {
                handlers.AddHandler(typeof(AutoCompleteEntry), typeof(AutoCompleteEntryHandler));
            });
        }
    }
}
