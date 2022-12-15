using Microsoft.Maui.Handlers;

namespace zoft.MauiExtensions.Controls.Handlers
{
	public partial class AutoCompleteEntryHandler : ViewHandler<AutoCompleteEntry, object>
	{
		protected override object CreatePlatformView() => throw new NotImplementedException();

		public static void MapBackground(IAutoCompleteEntryHandler handler, IEntry entry) { }

		public static void MapIsEnabled(IAutoCompleteEntryHandler handler, IEntry entry) { }

		public static void MapText(IAutoCompleteEntryHandler handler, IEntry entry) { }

		public static void MapPlaceholder(IAutoCompleteEntryHandler handler, IEntry entry) { }

		public static void MapVerticalTextAlignment(IAutoCompleteEntryHandler handler, IEntry entry) { }

		public static void MapPlaceholderColor(IAutoCompleteEntryHandler handler, IEntry entry) { }

		public static void MapHorizontalTextAlignment(IAutoCompleteEntryHandler handler, IEntry entry) { }

		public static void MapFont(IAutoCompleteEntryHandler handler, IEntry entry) { }

		public static void MapCharacterSpacing(IAutoCompleteEntryHandler handler, IEntry entry) { }

		public static void MapTextColor(IAutoCompleteEntryHandler handler, IEntry entry) { }

		public static void MapIsTextPredictionEnabled(IAutoCompleteEntryHandler handler, IEntry entry) { }

		public static void MapMaxLength(IAutoCompleteEntryHandler handler, IEntry entryr) { }

		public static void MapIsReadOnly(IAutoCompleteEntryHandler handler, IEntry entry) { }

		public static void MapTextMemberPath(IAutoCompleteEntryHandler handler, AutoCompleteEntry autoCompleteEntry) { }

		public static void MapDisplayMemberPath(IAutoCompleteEntryHandler handler, AutoCompleteEntry autoCompleteEntry) { }

		public static void MapIsSuggestionListOpen(IAutoCompleteEntryHandler handler, AutoCompleteEntry autoCompleteEntry) { }

		public static void MapUpdateTextOnSelect(IAutoCompleteEntryHandler handler, AutoCompleteEntry autoCompleteEntry) { }

		public static void MapItemsSource(IAutoCompleteEntryHandler handler, AutoCompleteEntry autoCompleteEntry) { }

        public static void MapSelectedSuggestion(IAutoCompleteEntryHandler handler, AutoCompleteEntry autoCompleteEntry) { }
    }
}
