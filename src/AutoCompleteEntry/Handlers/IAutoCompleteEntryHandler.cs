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
    public interface IAutoCompleteEntryHandler : IViewHandler
    {
        new AutoCompleteEntry VirtualView { get; }
        new PlatformView PlatformView { get; }
    }
}