using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;

namespace zoft.MauiExtensions.Controls.Handlers;

public partial class AutoCompleteEntryHandler
{
    private UIElement? _singleSelectionRoot;
    private PointerEventHandler? _singleSelectionRootPointerHandler;
    private PointerEventHandler? _singleSelectionEditorPointerHandler;

    private void ConnectSingleSelectionPointerHandlers(AutoSuggestBox editor)
    {
        _singleSelectionRootPointerHandler = OnSingleSelectionRootPointerPressed;
        _singleSelectionEditorPointerHandler = OnSingleSelectionEditorPointerPressed;
        // The inner TextBox handles presses, including when it already has focus.
        editor.AddHandler(UIElement.PointerPressedEvent, _singleSelectionEditorPointerHandler, true);
        AttachSingleSelectionRootHandler();
    }

    private void AttachSingleSelectionRootHandler()
    {
        var root = PlatformView.XamlRoot?.Content;
        if (ReferenceEquals(root, _singleSelectionRoot)) return;
        DetachSingleSelectionRootHandler();
        if (root is not null && _singleSelectionRootPointerHandler is not null)
        {
            _singleSelectionRoot = root;
            root.AddHandler(UIElement.PointerPressedEvent, _singleSelectionRootPointerHandler, true);
        }
    }

    private void DetachSingleSelectionRootHandler()
    {
        if (_singleSelectionRootPointerHandler is not null)
            _singleSelectionRoot?.RemoveHandler(UIElement.PointerPressedEvent, _singleSelectionRootPointerHandler);
        _singleSelectionRoot = null;
    }

    private void DisconnectSingleSelectionPointerHandlers(AutoSuggestBox editor)
    {
        DetachSingleSelectionRootHandler();
        if (_singleSelectionEditorPointerHandler is not null)
            editor.RemoveHandler(UIElement.PointerPressedEvent, _singleSelectionEditorPointerHandler);
        _singleSelectionEditorPointerHandler = null;
        _singleSelectionRootPointerHandler = null;
    }

    private void OnSingleSelectionEditorPointerPressed(object sender, PointerRoutedEventArgs e)
    {
        if (VirtualView is null || VirtualView.IsMultiple) return;
        VirtualView.IsSuggestionListOpen = true;
        if (VirtualView.ItemsSource?.Count > 0) PlatformView.IsSuggestionListOpen = true;
        // Leave caret placement and clear-button processing to the native editor.
    }

    private void OnSingleSelectionRootPointerPressed(object sender, PointerRoutedEventArgs e)
    {
        if (VirtualView is null || VirtualView.IsMultiple ||
            (!VirtualView.IsSuggestionListOpen && !PlatformView.IsSuggestionListOpen)) return;

        // Popup content has a separate visual ancestry from the editor. Exclude
        // the complete suggestion surface so item/scrollbar clicks work normally.
        var popupContent = PlatformView.FindDescendant<Popup>()?.Child;
        var source = e.OriginalSource as DependencyObject;
        while (source is not null)
        {
            if (ReferenceEquals(source, PlatformView) || ReferenceEquals(source, popupContent)) return;
            source = VisualTreeHelper.GetParent(source);
        }

        VirtualView.IsSuggestionListOpen = false;
        PlatformView.IsSuggestionListOpen = false;
        // Do not consume the outside press or change single-mode text/selection.
    }
}
