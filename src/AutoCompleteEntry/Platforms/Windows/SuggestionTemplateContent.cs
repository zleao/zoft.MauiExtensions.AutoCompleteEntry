namespace zoft.MauiExtensions.Controls.Platform;

// Kept independent of WinUI so template selection and binding can be regression tested.
internal static class SuggestionTemplateContent
{
    internal static View Create(DataTemplate template, object item, BindableObject container)
    {
        var resolved = template is DataTemplateSelector selector
            ? selector.SelectTemplate(item, container)
            : template;
        if (resolved is null || resolved is DataTemplateSelector)
            throw new InvalidOperationException("The suggestion DataTemplateSelector must return a concrete DataTemplate.");

        var content = resolved.CreateContent();
        if (content is not View view)
            throw new InvalidOperationException(
                $"The suggestion ItemTemplate must create a Microsoft.Maui.Controls.View, but created '{content?.GetType().FullName ?? "null"}'.");
        if (view.Parent is not null || view.Handler is not null)
            throw new InvalidOperationException("The suggestion ItemTemplate must create a new, unparented View for each row.");

        view.BindingContext = item;
        return view;
    }
}
