using Microsoft.Maui.Handlers;

namespace zoft.MauiExtensions.Controls.Handlers
{
	public partial class AutoCompleteEntryHandler : ViewHandler<IAutoCompleteEntry, Microsoft.Maui.Platform.MauiSearchBar>
	{
		public UIKit.UITextField QueryEditor => throw new NotImplementedException();

		protected override Microsoft.Maui.Platform.MauiSearchBar CreatePlatformView() => throw new NotImplementedException();

		public static void MapBackground(IAutoCompleteEntryHandler handler, ISearchBar searchBar) { }

		public static void MapIsEnabled(IAutoCompleteEntryHandler handler, ISearchBar searchBar) { }

		public static void MapText(IAutoCompleteEntryHandler handler, ISearchBar searchBar) { }

		public static void MapPlaceholder(IAutoCompleteEntryHandler handler, ISearchBar searchBar) { }

		public static void MapVerticalTextAlignment(IAutoCompleteEntryHandler handler, ISearchBar searchBar) { }

		public static void MapPlaceholderColor(IAutoCompleteEntryHandler handler, ISearchBar searchBar) { }

		public static void MapHorizontalTextAlignment(IAutoCompleteEntryHandler handler, ISearchBar searchBar) { }

		public static void MapFont(IAutoCompleteEntryHandler handler, ISearchBar searchBar) { }

		public static void MapCharacterSpacing(IAutoCompleteEntryHandler handler, ISearchBar searchBar) { }

		public static void MapTextColor(IAutoCompleteEntryHandler handler, ISearchBar searchBar) { }

		public static void MapIsTextPredictionEnabled(IAutoCompleteEntryHandler handler, ISearchBar searchBar) { }

		public static void MapMaxLength(IAutoCompleteEntryHandler handler, ISearchBar searchBar) { }

		public static void MapIsReadOnly(IAutoCompleteEntryHandler handler, ISearchBar searchBar) { }

		public static void MapCancelButtonColor(IAutoCompleteEntryHandler handler, ISearchBar searchBar) { }

		public static void MapTextMemberPath(IAutoCompleteEntryHandler handler, IAutoCompleteEntry autoCompleteEntry) { }

		public static void MapDisplayMemberPath(IAutoCompleteEntryHandler handler, IAutoCompleteEntry autoCompleteEntry) { }

		public static void MapIsSuggestionListOpen(IAutoCompleteEntryHandler handler, IAutoCompleteEntry autoCompleteEntry) { }

		public static void MapUpdateTextOnSelect(IAutoCompleteEntryHandler handler, IAutoCompleteEntry autoCompleteEntry) { }

		public static void MapItemsSource(IAutoCompleteEntryHandler handler, IAutoCompleteEntry autoCompleteEntry) { }

        public static void MapSelectedSuggestion(IAutoCompleteEntryHandler handler, IAutoCompleteEntry autoCompleteEntry) { }
    }
}
