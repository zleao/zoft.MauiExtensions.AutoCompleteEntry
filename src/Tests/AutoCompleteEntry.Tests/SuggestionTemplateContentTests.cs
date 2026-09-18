using zoft.MauiExtensions.Controls.Platform;

namespace AutoCompleteEntry.Tests;

public class SuggestionTemplateContentTests
{
    [Fact]
    public void CreatesSeparateRowsAndBindsOriginalItems()
    {
        var template = new DataTemplate(() =>
        {
            var label = new Label();
            label.SetBinding(Label.TextProperty, nameof(Item.Name));
            return label;
        });
        var owner = new Entry();
        var firstItem = new Item("first");
        var secondItem = new Item("second");

        var first = (Label)SuggestionTemplateContent.Create(template, firstItem, owner);
        var second = (Label)SuggestionTemplateContent.Create(template, secondItem, owner);

        Assert.NotSame(first, second);
        Assert.Same(firstItem, first.BindingContext);
        Assert.Same(secondItem, second.BindingContext);
        Assert.Equal("first", first.Text);
        Assert.Equal("second", second.Text);
    }

    [Fact]
    public void ResolvesSelectorPerItemWithOwningContainer()
    {
        var owner = new Entry();
        var firstItem = new Item("first");
        var seen = new List<object>();
        var selector = new Selector((item, container) =>
        {
            Assert.Same(owner, container);
            seen.Add(item);
            return ReferenceEquals(item, firstItem)
                ? new DataTemplate(() => new Label())
                : new DataTemplate(() => new Grid());
        });

        Assert.IsType<Label>(SuggestionTemplateContent.Create(selector, firstItem, owner));
        Assert.IsType<Grid>(SuggestionTemplateContent.Create(selector, new Item("second"), owner));
        Assert.Equal(2, seen.Count);
        Assert.Same(firstItem, seen[0]);
    }

    [Fact]
    public void ReportsInvalidContent()
    {
        var error = Assert.Throws<InvalidOperationException>(() =>
            SuggestionTemplateContent.Create(new DataTemplate(() => new object()), new object(), new Entry()));
        Assert.Contains("must create a Microsoft.Maui.Controls.View", error.Message);
    }

    [Fact]
    public void ReportsNullSelectorResult()
    {
        var error = Assert.Throws<InvalidOperationException>(() =>
            SuggestionTemplateContent.Create(new Selector((_, _) => null!), new object(), new Entry()));
        Assert.Contains("concrete DataTemplate", error.Message);
    }

    [Fact]
    public void RejectsAlreadyParentedContent()
    {
        var label = new Label { Parent = new ContentView() };
        Assert.Throws<InvalidOperationException>(() =>
            SuggestionTemplateContent.Create(new DataTemplate(() => label), new object(), new Entry()));
    }

    private sealed record Item(string Name);
    private sealed class Selector(Func<object, BindableObject, DataTemplate> select) : DataTemplateSelector
    {
        protected override DataTemplate OnSelectTemplate(object item, BindableObject container) => select(item, container);
    }
}
