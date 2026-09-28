using System.Collections;
using System.Collections.ObjectModel;
using zoft.MauiExtensions.Controls.Platform;

namespace AutoCompleteEntry.Tests;

public class SuggestionItemsSnapshotTests
{
    [Fact]
    public void ClearAndReplacementDoNotModifyNativeItemsInsideCollectionNotification()
    {
        var queue = new Queue<Action>();
        IList? nativeItems = null;
        var changes = 0;
        using var bridge = new SuggestionItemsSnapshot(queue.Enqueue, items => { nativeItems = items; changes++; });
        var selected = new object();
        var source = new ObservableCollection<object> { selected };
        bridge.SetSource(source);
        queue.Dequeue()();
        source.CollectionChanged += (_, _) =>
        {
            Assert.Single(nativeItems!);
            Assert.Same(selected, nativeItems![0]);
            bridge.SetSource(new ObservableCollection<object> { "new result" });
        };
        source.Clear();
        Assert.Equal(1, changes);
        Assert.Single(queue);
        queue.Dequeue()();
        Assert.Equal("new result", Assert.Single(nativeItems!.Cast<object>()));
        Assert.Equal(2, changes);
    }

    [Fact]
    public void FilteringFromNativeApplyIsQueuedInsteadOfReenteringNativeCollectionUpdate()
    {
        var queue = new Queue<Action>();
        var source = new ObservableCollection<string> { "selected" };
        var applying = false;
        var snapshots = new List<IList?>();
        using var bridge = new SuggestionItemsSnapshot(queue.Enqueue, items =>
        {
            Assert.False(applying);
            applying = true;
            snapshots.Add(items);
            if (snapshots.Count == 1) { source.Clear(); source.Add("filtered"); }
            applying = false;
        });
        bridge.SetSource(source);
        queue.Dequeue()();
        Assert.Equal("selected", snapshots[0]![0]);
        Assert.Single(queue);
        queue.Dequeue()();
        Assert.Equal("filtered", snapshots[1]![0]);
    }

    [Fact]
    public void ReplacementUnsubscribesOldSourceAndCoalescesToLatestState()
    {
        var queue = new Queue<Action>();
        IList? snapshot = null;
        using var bridge = new SuggestionItemsSnapshot(queue.Enqueue, items => snapshot = items);
        var old = new ObservableCollection<string> { "old" };
        var current = new ObservableCollection<string> { "A" };
        bridge.SetSource(old);
        bridge.SetSource(null);
        bridge.SetSource(current);
        current.Add("B");
        Assert.Single(queue);
        queue.Dequeue()();
        Assert.Equal(new[] { "A", "B" }, snapshot!.Cast<string>());
        old.Clear();
        Assert.Empty(queue);
        current.Move(1, 0);
        queue.Dequeue()();
        Assert.Equal(new[] { "B", "A" }, snapshot!.Cast<string>());
    }

    [Fact]
    public void DisconnectCancelsPendingWorkAndDetachesObservableSource()
    {
        var queue = new Queue<Action>();
        var applied = false;
        var source = new ObservableCollection<string>();
        var bridge = new SuggestionItemsSnapshot(queue.Enqueue, _ => applied = true);
        bridge.SetSource(source);
        bridge.Dispose();
        queue.Dequeue()();
        source.Add("ignored");
        Assert.False(applied);
        Assert.Empty(queue);
    }
}
