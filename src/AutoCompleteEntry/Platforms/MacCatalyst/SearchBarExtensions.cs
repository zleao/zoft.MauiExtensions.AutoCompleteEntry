using UIKit;

namespace zoft.MauiExtensions.Controls.Platforms.Extensions
{
    public static class SearchBarExtensions
    {
        internal static UITextField GetSearchTextField(this UISearchBar searchBar)
        {
            if (OperatingSystem.IsMacCatalystVersionAtLeast(13))
                return searchBar.SearchTextField;
            else
                return searchBar.GetSearchTextField();
        }

        internal static bool ShouldShowCancelButton(this ISearchBar searchBar) =>
            !string.IsNullOrEmpty(searchBar.Text);
    }
}
