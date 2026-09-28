using System.Collections;
using System.Collections.Specialized;

namespace zoft.MauiExtensions.Controls.Platform;

// WinUI must not observe the consumer's collection directly: a native clear or
// selection callback can filter it while WinUI is still processing VectorChanged.
internal sealed class SuggestionItemsSnapshot(Action<Action> enqueue, Action<IList?> apply) : IDisposable
{
    private IList? _source;
    private bool _pending;
    private bool _disposed;

    internal void SetSource(IList? source)
    {
        if (_disposed) return;
        if (!ReferenceEquals(source, _source))
        {
            if (_source is INotifyCollectionChanged old) old.CollectionChanged -= OnChanged;
            _source = source;
            if (_source is INotifyCollectionChanged current) current.CollectionChanged += OnChanged;
        }
        Schedule();
    }

    private void OnChanged(object? sender, NotifyCollectionChangedEventArgs e) => Schedule();

    private void Schedule()
    {
        if (_pending || _disposed) return;
        _pending = true;
        enqueue(() =>
        {
            _pending = false;
            if (!_disposed) apply(_source?.Cast<object?>().ToArray());
        });
    }

    public void Dispose()
    {
        _disposed = true;
        if (_source is INotifyCollectionChanged source) source.CollectionChanged -= OnChanged;
        _source = null;
    }
}
