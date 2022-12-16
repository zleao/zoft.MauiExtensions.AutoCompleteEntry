#if __IOS__ || MACCATALYST
using PlatformView = zoft.MauiExtensions.Controls.Platform.IOSAutoCompleteEntry;
#elif ANDROID
using PlatformView = zoft.MauiExtensions.Controls.Platform.AndroidAutoCompleteEntry;
#elif WINDOWS
using PlatformView = Microsoft.UI.Xaml.Controls.AutoSuggestBox;
#elif TIZEN
using PlatformView = Microsoft.Maui.Platform.MauiSearchBar;
#else
using PlatformView = System.Object;
#endif

namespace zoft.MauiExtensions.Controls.Handlers
{
    /// <summary>
    /// Handler interface for the <see cref="AutoCompleteEntry"/>
    /// </summary>
    public interface IAutoCompleteEntryHandler : IViewHandler
    {
        /// <summary>
        /// Maui view
        /// </summary>
        new AutoCompleteEntry VirtualView { get; }

        /// <summary>
        /// Platform specific implementation
        /// </summary>
        new PlatformView PlatformView { get; }
    }
}