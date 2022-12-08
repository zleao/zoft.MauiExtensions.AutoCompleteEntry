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
using Microsoft.Maui.Handlers;

namespace zoft.MauiExtensions.Controls.Handlers
{
	public partial class AutoCompleteEntryHandler : IAutoCompleteEntryHandler
	{
		public static IPropertyMapper<IAutoCompleteEntry, IAutoCompleteEntryHandler> Mapper =
			new PropertyMapper<IAutoCompleteEntry, IAutoCompleteEntryHandler>(ViewMapper)
			{
#if __IOS__
				[nameof(ISearchBar.IsEnabled)] = MapIsEnabled,
#endif
				[nameof(ISearchBar.Background)] = MapBackground,
				[nameof(ISearchBar.CharacterSpacing)] = MapCharacterSpacing,
				[nameof(ISearchBar.Font)] = MapFont,
				[nameof(ITextAlignment.HorizontalTextAlignment)] = MapHorizontalTextAlignment,
				[nameof(ITextAlignment.VerticalTextAlignment)] = MapVerticalTextAlignment,
				[nameof(ISearchBar.IsReadOnly)] = MapIsReadOnly,
				[nameof(ISearchBar.IsTextPredictionEnabled)] = MapIsTextPredictionEnabled,
				[nameof(ISearchBar.MaxLength)] = MapMaxLength,
				[nameof(ISearchBar.Placeholder)] = MapPlaceholder,
				[nameof(ISearchBar.PlaceholderColor)] = MapPlaceholderColor,
				[nameof(ISearchBar.Text)] = MapText,
				[nameof(ISearchBar.TextColor)] = MapTextColor,
				[nameof(ISearchBar.CancelButtonColor)] = MapCancelButtonColor,
				[nameof(IAutoCompleteEntry.TextMemberPath)] = MapTextMemberPath,
				[nameof(IAutoCompleteEntry.DisplayMemberPath)] = MapDisplayMemberPath,
				[nameof(IAutoCompleteEntry.IsSuggestionListOpen)] = MapIsSuggestionListOpen,
				[nameof(IAutoCompleteEntry.UpdateTextOnSelect)] = MapUpdateTextOnSelect,
				[nameof(IAutoCompleteEntry.ItemsSource)] = MapItemsSource,
				[nameof(IAutoCompleteEntry.SelectedSuggestion)] = MapSelectedSuggestion,
			};

		public static CommandMapper<ISearchBar, ISearchBarHandler> CommandMapper = new(ViewCommandMapper)
		{
		};

		public AutoCompleteEntryHandler() : base(Mapper)
		{
		}

		public AutoCompleteEntryHandler(IPropertyMapper mapper)
			: base(mapper ?? Mapper, CommandMapper)
		{
		}

		public AutoCompleteEntryHandler(IPropertyMapper mapper, CommandMapper commandMapper)
			: base(mapper ?? Mapper, commandMapper ?? CommandMapper)
		{
		}

		IAutoCompleteEntry IAutoCompleteEntryHandler.VirtualView => VirtualView;

		PlatformView IAutoCompleteEntryHandler.PlatformView => PlatformView;
	}
}
