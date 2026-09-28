namespace zoft.MauiExtensions.Controls;

// Passive content: the native row owns pointer/keyboard activation, so tapping the
// checkbox and tapping the content follow exactly the same selection path.
internal sealed class SelectionRow : Grid
{
    private readonly CheckBox _check = new() { InputTransparent = true, VerticalOptions = LayoutOptions.Center };
    private readonly View _content;

    internal SelectionRow(View content)
    {
        _content = content;
        ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        MinimumHeightRequest = 44;
        InputTransparent = true;
        CascadeInputTransparent = true;
        Add(_check);
        content.SetValue(ColumnProperty, 1);
        Add(content);
    }

    internal void Update(AutoCompleteEntry owner, object item)
    {
        BindingContext = item;
        _content.BindingContext = item;
        FlowDirection = owner.FlowDirection;
        _check.IsChecked = owner.IsSuggestionSelected(item);
        SemanticProperties.SetDescription(_check, owner.GetSelectionText(item));
    }
}
