using zoft.MauiExtensions.Controls.Platform;

namespace AutoCompleteEntry.Tests;

/// <summary>
/// Tests for <see cref="TemplateIdMapper"/> — the template-to-integer-ID mapping logic
/// used by the Android adapter to maintain separate recycling pools per DataTemplate type.
/// </summary>
public class TemplateIdMapperTests
{
    // Minimal concrete DataTemplateSelector for testing — DataTemplateSelector.OnSelectTemplate
    // is protected abstract and cannot be configured via NSubstitute.
    private sealed class TestSelector : DataTemplateSelector
    {
        private readonly Func<object, DataTemplate> _select;

        internal TestSelector(Func<object, DataTemplate> select) => _select = select;

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
            => _select(item);
    }

    // Returns a new distinct DataTemplate instance each call — used as unique dictionary keys.
    private static DataTemplate NewTemplate() => new();

    #region Plain DataTemplate (no selector)

    [Fact]
    public void GetViewType_PlainTemplate_ReturnsZero()
    {
        var result = TemplateIdMapper.GetViewType(NewTemplate(), new object(), null, new());

        Assert.Equal(0, result);
    }

    [Fact]
    public void GetViewType_PlainTemplate_DoesNotPopulateMap()
    {
        var idMap = new Dictionary<DataTemplate, int>();

        TemplateIdMapper.GetViewType(NewTemplate(), new object(), null, idMap);

        Assert.Empty(idMap);
    }

    [Theory]
    [InlineData("a")]
    [InlineData("b")]
    [InlineData("c")]
    public void GetViewType_PlainTemplate_MultipleCalls_AlwaysReturnsZero(string item)
    {
        var template = NewTemplate();
        var idMap = new Dictionary<DataTemplate, int>();

        var firstResult = TemplateIdMapper.GetViewType(template, item, null, idMap);
        var secondResult = TemplateIdMapper.GetViewType(template, item, null, idMap);
        var thirdResult = TemplateIdMapper.GetViewType(template, item, null, idMap);

        Assert.Equal(0, firstResult);
        Assert.Equal(0, secondResult);
        Assert.Equal(0, thirdResult);
        Assert.Empty(idMap);
    }

    #endregion

    #region DataTemplateSelector — ID assignment

    [Fact]
    public void GetViewType_Selector_FirstEncounteredTemplate_AssignsIdZero()
    {
        var templateA = NewTemplate();
        var selector = new TestSelector(_ => templateA);
        var idMap = new Dictionary<DataTemplate, int>();

        var result = TemplateIdMapper.GetViewType(selector, "item", null, idMap);

        Assert.Equal(0, result);
    }

    [Fact]
    public void GetViewType_Selector_TwoDistinctTemplates_AssignsDifferentIds()
    {
        var templateA = NewTemplate();
        var templateB = NewTemplate();
        var selector = new TestSelector(item => item is "a" ? templateA : templateB);
        var idMap = new Dictionary<DataTemplate, int>();

        var idA = TemplateIdMapper.GetViewType(selector, "a", null, idMap);
        var idB = TemplateIdMapper.GetViewType(selector, "b", null, idMap);

        Assert.NotEqual(idA, idB);
    }

    [Fact]
    public void GetViewType_Selector_ThreeDistinctTemplates_AssignsSequentialIds()
    {
        var templateA = NewTemplate();
        var templateB = NewTemplate();
        var templateC = NewTemplate();
        var selector = new TestSelector(item => item switch
        {
            "a" => templateA,
            "b" => templateB,
            _ => templateC
        });
        var idMap = new Dictionary<DataTemplate, int>();

        var idA = TemplateIdMapper.GetViewType(selector, "a", null, idMap);
        var idB = TemplateIdMapper.GetViewType(selector, "b", null, idMap);
        var idC = TemplateIdMapper.GetViewType(selector, "c", null, idMap);

        Assert.Equal(3, new[] { idA, idB, idC }.Distinct().Count());
        Assert.Equivalent(new[] { 0, 1, 2 }, new[] { idA, idB, idC });
    }

    #endregion

    #region DataTemplateSelector — ID stability

    [Fact]
    public void GetViewType_Selector_SameTemplateInstance_ReturnsSameIdEachTime()
    {
        var templateA = NewTemplate();
        var selector = new TestSelector(_ => templateA);
        var idMap = new Dictionary<DataTemplate, int>();

        var first = TemplateIdMapper.GetViewType(selector, "x", null, idMap);
        var second = TemplateIdMapper.GetViewType(selector, "y", null, idMap);

        Assert.Equal(first, second);
    }

    [Fact]
    public void GetViewType_Selector_IdsDoNotChangeAfterNewTemplateAdded()
    {
        var templateA = NewTemplate();
        var templateB = NewTemplate();
        var selector = new TestSelector(item => item is "a" ? templateA : templateB);
        var idMap = new Dictionary<DataTemplate, int>();

        var idA_before = TemplateIdMapper.GetViewType(selector, "a", null, idMap);
        TemplateIdMapper.GetViewType(selector, "b", null, idMap); // register B
        var idA_after = TemplateIdMapper.GetViewType(selector, "a", null, idMap);

        Assert.Equal(idA_before, idA_after);
    }

    #endregion

    #region DataTemplateSelector — map state

    [Fact]
    public void GetViewType_Selector_PopulatesMapWithResolvedTemplates()
    {
        var templateA = NewTemplate();
        var templateB = NewTemplate();
        var selector = new TestSelector(item => item is "a" ? templateA : templateB);
        var idMap = new Dictionary<DataTemplate, int>();

        TemplateIdMapper.GetViewType(selector, "a", null, idMap);
        TemplateIdMapper.GetViewType(selector, "b", null, idMap);

        Assert.Equal(2, idMap.Count);
        Assert.Contains(templateA, idMap.Keys);
        Assert.Contains(templateB, idMap.Keys);
    }

    [Fact]
    public void GetViewType_Selector_RepeatedSameTemplate_DoesNotGrowMap()
    {
        var templateA = NewTemplate();
        var selector = new TestSelector(_ => templateA);
        var idMap = new Dictionary<DataTemplate, int>();

        TemplateIdMapper.GetViewType(selector, "x", null, idMap);
        TemplateIdMapper.GetViewType(selector, "y", null, idMap);
        TemplateIdMapper.GetViewType(selector, "z", null, idMap);

        Assert.Single(idMap);
    }

    #endregion

    #region MaxViewTypes cap enforcement

    [Fact]
    public void GetViewType_Selector_ExceedsMaxViewTypes_Throws()
    {
        var templates = Enumerable.Range(0, TemplateIdMapper.MaxViewTypes + 1)
                                  .Select(_ => NewTemplate())
                                  .ToArray();
        var selector = new TestSelector(item => templates[(int)item]);
        var idMap = new Dictionary<DataTemplate, int>();

        // Fill up to the limit — all should succeed
        for (var i = 0; i < TemplateIdMapper.MaxViewTypes; i++)
            TemplateIdMapper.GetViewType(selector, i, null, idMap);

        // One more distinct template must throw
        Assert.Throws<InvalidOperationException>(
            () => TemplateIdMapper.GetViewType(selector, TemplateIdMapper.MaxViewTypes, null, idMap));
    }

    [Fact]
    public void GetViewType_Selector_AtExactLimit_DoesNotThrow()
    {
        var templates = Enumerable.Range(0, TemplateIdMapper.MaxViewTypes)
                                  .Select(_ => NewTemplate())
                                  .ToArray();
        var selector = new TestSelector(item => templates[(int)item]);
        var idMap = new Dictionary<DataTemplate, int>();

        // Exactly MaxViewTypes distinct templates must all succeed
        for (var i = 0; i < TemplateIdMapper.MaxViewTypes; i++)
            TemplateIdMapper.GetViewType(selector, i, null, idMap);

        Assert.Equal(TemplateIdMapper.MaxViewTypes, idMap.Count);
    }

    #endregion

    #region Null template from selector

    [Fact]
    public void GetViewType_Selector_ReturnsNull_Throws()
    {
        var selector = new TestSelector(_ => null!);
        var idMap = new Dictionary<DataTemplate, int>();

        Assert.Throws<InvalidOperationException>(
            () => TemplateIdMapper.GetViewType(selector, "item", null, idMap));
    }

    #endregion
}
