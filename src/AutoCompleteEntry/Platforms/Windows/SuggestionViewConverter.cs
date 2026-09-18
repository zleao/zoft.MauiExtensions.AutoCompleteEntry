using Microsoft.Maui.Platform;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WSize = Windows.Foundation.Size;
using MView = Microsoft.Maui.Controls.View;
using DataTemplate = Microsoft.Maui.Controls.DataTemplate;

namespace zoft.MauiExtensions.Controls.Platform;

/// <summary>Infrastructure for converting realized WinUI suggestions into MAUI views.</summary>
public sealed class SuggestionViewConverter : Microsoft.UI.Xaml.Data.IValueConverter, IDisposable
{
    private AutoCompleteEntry? _owner;
    private IMauiContext? _context;
    private DataTemplate? _template;
    private readonly List<WeakReference<SuggestionRow>> _rows = new();

    internal void Initialize(AutoCompleteEntry owner, IMauiContext context)
    {
        _owner = owner;
        _context = context;
        _template = owner.ItemTemplate;
    }

    /// <inheritdoc />
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (_owner is null || _context is null || _template is null)
            return null!; // A retired native template can finish an outstanding binding update.

        _rows.RemoveAll(reference => !reference.TryGetTarget(out _));
        var row = new SuggestionRow(_owner, _context, _template, value);
        _rows.Add(new(row));
        return row;
    }

    /// <inheritdoc />
    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotSupportedException();

    /// <summary>Releases all surviving rows when the template or handler is discarded.</summary>
    public void Dispose()
    {
        foreach (var reference in _rows)
            if (reference.TryGetTarget(out var row))
                row.Dispose();
        _rows.Clear();
        _owner = null;
        _context = null;
        _template = null;
    }

    private sealed class SuggestionRow : Panel, IDisposable
    {
        private AutoCompleteEntry? _owner;
        private IMauiContext? _context;
        private DataTemplate? _template;
        private object? _item;
        private MView? _view;

        internal SuggestionRow(AutoCompleteEntry owner, IMauiContext context, DataTemplate template, object item)
        {
            _owner = owner;
            _context = context;
            _template = template;
            _item = item;
            // Suggestions remain passive content: the native list owns focus and selection.
            IsHitTestVisible = false;
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
            CreateView();
        }

        private void CreateView()
        {
            if (_view is not null || _owner is null || _context is null || _template is null)
                return;

            _view = SuggestionTemplateContent.Create(_template, _item!, _owner);
            // Parent supplies inherited resources; BindingContext remains the original item.
            _view.Parent = _owner;
            _view.MeasureInvalidated += OnMeasureInvalidated;
            try
            {
                Children.Add(_view.ToPlatform(_context));
            }
            catch
            {
                ReleaseView();
                throw;
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs args)
        {
            CreateView();
            // Layouts need a fresh measure after their native children have loaded.
            _view?.InvalidateMeasure();
            InvalidateMeasure();
        }

        private void OnUnloaded(object sender, RoutedEventArgs args) => ReleaseView();
        private void OnMeasureInvalidated(object? sender, EventArgs args) => InvalidateMeasure();

        protected override WSize MeasureOverride(WSize availableSize)
        {
            if (_view is not IView view)
                return new WSize();
            var measured = view.Measure(availableSize.Width, availableSize.Height);
            return new WSize(double.IsFinite(availableSize.Width) ? availableSize.Width : measured.Width,
                measured.Height);
        }

        protected override WSize ArrangeOverride(WSize finalSize)
        {
            (_view as IView)?.Arrange(new Microsoft.Maui.Graphics.Rect(0, 0, finalSize.Width, finalSize.Height));
            return finalSize;
        }

        private void ReleaseView()
        {
            if (_view is null)
                return;
            var view = _view;
            _view = null;
            view.MeasureInvalidated -= OnMeasureInvalidated;
            Children.Clear();
            view.DisconnectHandlers();
            view.BindingContext = null;
            view.Parent = null;
        }

        public void Dispose()
        {
            Loaded -= OnLoaded;
            Unloaded -= OnUnloaded;
            ReleaseView();
            _owner = null;
            _context = null;
            _template = null;
            _item = null;
        }
    }
}
