using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using Windows.System;
using WGrid = Microsoft.UI.Xaml.Controls.Grid;
using ListView = Microsoft.UI.Xaml.Controls.ListView;
using SizeChangedEventArgs = Microsoft.UI.Xaml.SizeChangedEventArgs;
using ListViewSelectionMode = Microsoft.UI.Xaml.Controls.ListViewSelectionMode;
using WBorder = Microsoft.UI.Xaml.Controls.Border;

namespace zoft.MauiExtensions.Controls.Platform;

// AutoSuggestBox's SuggestionChosen includes keyboard highlight changes. Multiple
// mode therefore owns a ListView popup and commits only ItemClick / Enter.
internal sealed class MultipleSelectionPresentation : IDisposable
{
    private readonly AutoSuggestBox _editor;
    private readonly AutoCompleteEntry _owner;
    private readonly IMauiContext _context;
    private readonly Popup _popup = new();
    private readonly WBorder _surface = new();
    private readonly ListView _list = new() { IsItemClickEnabled = true, SelectionMode = ListViewSelectionMode.Single, IsTabStop = false, MaxHeight = 320 };
    private readonly TextBlock _summary = new() { TextTrimming = TextTrimming.CharacterEllipsis, IsHitTestVisible = false, Visibility = Microsoft.UI.Xaml.Visibility.Collapsed };
    private ScrollViewer? _textContent;
    private FrameworkElement? _placeholder;
    private double _contentOpacity;
    private double _placeholderOpacity;
    private SuggestionViewConverter? _converter;
    private bool _changing;
    private bool _disposed;
    private int _highlight = -1;
    private UIElement? _root;
    private readonly PointerEventHandler _rootPointerHandler;
    private readonly PointerEventHandler _editorPointerHandler;

    internal MultipleSelectionPresentation(AutoSuggestBox editor, AutoCompleteEntry owner, IMauiContext context)
    {
        _editor = editor;
        _owner = owner;
        _context = context;
        _rootPointerHandler = OnRootPointerPressed;
        _editorPointerHandler = OnPointerPressed;
        _surface.Child = _list;
        _popup.Child = _surface;
        _popup.Closed += OnClosed;
        _list.ItemClick += OnItemClick;
        _editor.PreviewKeyDown += OnKeyDown;
        _editor.SizeChanged += OnSizeChanged;
        _editor.Loaded += OnLoaded;
        // TextBox handles pointer presses itself. Outside dismissal can leave
        // it focused, so GotFocus will not run when the user clicks it again.
        _editor.AddHandler(UIElement.PointerPressedEvent, _editorPointerHandler, true);
        _editor.ActualThemeChanged += OnThemeChanged;
        UpdateBackground();
        SetTemplate();
        AttachSummary();
    }

    private void OnLoaded(object sender, RoutedEventArgs e) { AttachSummary(); Update(); }

    private void OnThemeChanged(FrameworkElement sender, object args) => UpdateBackground();

    private void UpdateBackground()
    {
        // Never use the editor's potentially transparent background. Use the
        // native suggestion surface, with an opaque fallback before templating.
        _surface.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(
            _editor.ActualTheme == ElementTheme.Dark
                ? Windows.UI.Color.FromArgb(255, 32, 32, 32) : Microsoft.UI.Colors.White);
        _surface.RequestedTheme = _editor.ActualTheme;
        // Read the installed AutoSuggestBox template, including application
        // overrides, rather than maintaining a second approximation of WinUI.
        if (_editor.FindDescendant<Popup>()?.Child is WBorder nativeSurface &&
            nativeSurface.FindDescendant<ListView>() is { } nativeList)
        {
            _surface.Background = nativeSurface.Background ?? _surface.Background;
            _surface.BorderBrush = nativeSurface.BorderBrush;
            _surface.BorderThickness = nativeSurface.BorderThickness;
            _surface.CornerRadius = nativeSurface.CornerRadius;
            _surface.Padding = nativeSurface.Padding;
            _surface.Shadow = nativeSurface.Shadow;
            _surface.Translation = nativeSurface.Translation;
            _list.Style = nativeList.Style;
            _list.Background = nativeList.Background;
            _list.BorderBrush = nativeList.BorderBrush;
            _list.BorderThickness = nativeList.BorderThickness;
            _list.CornerRadius = nativeList.CornerRadius;
            _list.Margin = nativeList.Margin;
            _list.Padding = nativeList.Padding;
            _list.MaxHeight = nativeList.MaxHeight;
            _list.ItemContainerStyle = nativeList.ItemContainerStyle;
        }
    }

    internal void SetItems(System.Collections.IList? items)
    {
        if (_disposed) return;
        _list.ItemsSource = items;
        _highlight = -1;
        Update();
    }

    private void AttachSummary()
    {
        if (_root is null && _editor.XamlRoot?.Content is { } root)
        {
            _root = root;
            _root.AddHandler(UIElement.PointerPressedEvent, _rootPointerHandler, true);
        }
        var textBox = _editor.FindDescendant<TextBox>();
        textBox?.ApplyTemplate();
        var content = textBox?.FindDescendant<ScrollViewer>();
        if (_summary.Parent is null && content?.Parent is WGrid grid)
        {
            _textContent = content;
            _contentOpacity = content.Opacity;
            _placeholder = grid.Children.OfType<FrameworkElement>()
                .FirstOrDefault(child => child.Name == "PlaceholderTextContentPresenter");
            _placeholderOpacity = _placeholder?.Opacity ?? 1;
            grid.Children.Add(_summary);
            WGrid.SetRow(_summary, WGrid.GetRow(content));
            WGrid.SetColumn(_summary, WGrid.GetColumn(content));
            WGrid.SetRowSpan(_summary, WGrid.GetRowSpan(content));
            WGrid.SetColumnSpan(_summary, WGrid.GetColumnSpan(content));
        }
    }

    internal void SetTemplate()
    {
        _list.ItemTemplate = null;
        _converter?.Dispose();
        var resources = new SuggestionTemplates();
        _converter = (SuggestionViewConverter)resources["SuggestionViewConverter"];
        _converter.Initialize(_owner, _context);
        _list.ItemTemplate = (Microsoft.UI.Xaml.DataTemplate)resources["SuggestionTemplate"];
    }

    internal void Update()
    {
        if (_changing || _disposed) return;
        _changing = true;
        try
        {
            AttachSummary();
            UpdateBackground();
            _editor.UpdateTextOnSelect = false;
            _editor.IsSuggestionListOpen = false;
            _editor.ItemsSource = null;
            _converter?.RefreshSelection();
            _summary.Text = _owner.SelectionSummary;
            _summary.Foreground = _editor.Foreground;
            _summary.FontSize = _editor.FontSize;
            _summary.FontFamily = _editor.FontFamily;
            _summary.FontWeight = _editor.FontWeight;
            _summary.FontStyle = _editor.FontStyle;
            _summary.CharacterSpacing = _editor.CharacterSpacing;
            _summary.FlowDirection = _editor.FlowDirection;
            _summary.Visibility = _owner.ShowsSelectionSummary ? Microsoft.UI.Xaml.Visibility.Visible : Microsoft.UI.Xaml.Visibility.Collapsed;
            if (_textContent is { } content)
            {
                // Keep native chrome and buttons visible. Only replace the text
                // area, using its exact cell, padding and alignment.
                content.Opacity = _owner.ShowsSelectionSummary ? 0 : _contentOpacity;
                _summary.Margin = content.Margin;
                _summary.Padding = content.Padding;
                _summary.VerticalAlignment = content.VerticalAlignment;
                _summary.HorizontalAlignment = content.HorizontalAlignment;
                if (_editor.FindDescendant<TextBox>() is { } textBox)
                    _summary.TextAlignment = textBox.TextAlignment;
            }
            if (_placeholder is not null)
                _placeholder.Opacity = _owner.ShowsSelectionSummary ? 0 : _placeholderOpacity;
            Microsoft.UI.Xaml.Automation.AutomationProperties.SetHelpText(_editor,
                _owner.ShowsSelectionSummary ? _owner.SelectionSummary : string.Empty);
            if (_editor.XamlRoot is not null)
            {
                _popup.XamlRoot = _editor.XamlRoot;
                PositionPopup();
                _popup.IsOpen = _owner.IsSuggestionListOpen;
            }
        }
        finally { _changing = false; }
    }

    private void PositionPopup()
    {
        var point = _editor.TransformToVisual(null).TransformPoint(new Windows.Foundation.Point(0, _editor.ActualHeight));
        _popup.HorizontalOffset = point.X;
        _popup.VerticalOffset = point.Y;
        _surface.Width = Math.Max(1, _editor.ActualWidth);
        _list.FlowDirection = _editor.FlowDirection;
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e) => Update();
    private void OnPointerPressed(object sender, PointerRoutedEventArgs e)
    {
        if (_disposed) return;
        _owner.IsSuggestionListOpen = true;
        // Also reconcile native visibility if the shared value was already true.
        // Do not handle the event: the TextBox still owns caret/selection input.
        Update();
    }
    private void OnRootPointerPressed(object sender, PointerRoutedEventArgs e)
    {
        // Light-dismiss popups consider the editor "outside" and would erase the
        // query when the user taps to position the caret. Exclude both surfaces.
        var source = e.OriginalSource as DependencyObject;
        while (source is not null)
        {
            if (ReferenceEquals(source, _editor) || ReferenceEquals(source, _surface)) return;
            source = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetParent(source);
        }
        _owner.IsSuggestionListOpen = false;
    }
    private void OnClosed(object? sender, object e)
    {
        // A queued Closed notification from an earlier dismissal must not close
        // a session that has already been reopened by another pointer press.
        if (!_changing && !_disposed && !_popup.IsOpen) _owner.IsSuggestionListOpen = false;
    }

    internal bool ContainsFocus()
    {
        var focused = FocusManager.GetFocusedElement(_editor.XamlRoot) as DependencyObject;
        while (focused is not null)
        {
            if (ReferenceEquals(focused, _editor) || ReferenceEquals(focused, _list)) return true;
            focused = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetParent(focused);
        }
        return false;
    }

    private void OnItemClick(object sender, ItemClickEventArgs e)
    {
        _owner.OnSuggestionSelected(e.ClickedItem);
        _editor.Focus(FocusState.Programmatic);
    }

    private void OnKeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == VirtualKey.Escape) { _owner.IsSuggestionListOpen = false; e.Handled = true; return; }
        if (e.Key is VirtualKey.Down or VirtualKey.Up)
        {
            _owner.IsSuggestionListOpen = true;
            if (_list.Items.Count > 0)
            {
                _highlight = Math.Clamp(_highlight + (e.Key == VirtualKey.Down ? 1 : -1), 0, _list.Items.Count - 1);
                _list.SelectedIndex = _highlight;
                _list.ScrollIntoView(_list.SelectedItem);
            }
            e.Handled = true;
        }
        else if (e.Key == VirtualKey.Enter && _owner.IsSuggestionListOpen && _highlight >= 0 && _highlight < _list.Items.Count)
        {
            _owner.OnSuggestionSelected(_list.Items[_highlight]);
            e.Handled = true;
        }
    }

    public void Dispose()
    {
        _disposed = true;
        _popup.Closed -= OnClosed;
        _popup.IsOpen = false;
        _popup.Child = null;
        _list.ItemClick -= OnItemClick;
        _list.ItemsSource = null;
        _list.ItemTemplate = null;
        _converter?.Dispose();
        _editor.PreviewKeyDown -= OnKeyDown;
        _editor.SizeChanged -= OnSizeChanged;
        _editor.Loaded -= OnLoaded;
        _editor.RemoveHandler(UIElement.PointerPressedEvent, _editorPointerHandler);
        _editor.ActualThemeChanged -= OnThemeChanged;
        _root?.RemoveHandler(UIElement.PointerPressedEvent, _rootPointerHandler);
        _root = null;
        if (_summary.Parent is WGrid grid) grid.Children.Remove(_summary);
        if (_textContent is not null) _textContent.Opacity = _contentOpacity;
        if (_placeholder is not null) _placeholder.Opacity = _placeholderOpacity;
        Microsoft.UI.Xaml.Automation.AutomationProperties.SetHelpText(_editor, string.Empty);
    }
}
