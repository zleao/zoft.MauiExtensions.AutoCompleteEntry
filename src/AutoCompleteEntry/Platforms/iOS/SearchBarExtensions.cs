using UIKit;

namespace zoft.MauiExtensions.Controls.Platform
{
    public static class SearchBarExtensions
    {
        internal static UITextField GetSearchTextField(this UISearchBar searchBar)
        {
            if (OperatingSystem.IsIOSVersionAtLeast(13))
                return searchBar.SearchTextField;
            else
                return searchBar.GetSearchTextField();
        }

        internal static bool ShouldShowCancelButton(this ISearchBar searchBar) =>
            !string.IsNullOrEmpty(searchBar.Text);
    }
}
