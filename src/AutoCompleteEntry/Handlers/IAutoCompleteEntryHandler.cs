#if __IOS__
using PlatformView = zoft.MauiExtensions.Controls.Platforms.iOS.IOSAutoCompleteEntry;
#elif MACCATALYST
using PlatformView = Microsoft.Maui.Platform.MauiSearchBar;
#elif ANDROID
using PlatformView = zoft.MauiExtensions.Controls.Platforms.Android.AndroidAutoCompleteEntry;
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
        new IAutoCompleteEntry VirtualView { get; }
        new PlatformView PlatformView { get; }
    }
}