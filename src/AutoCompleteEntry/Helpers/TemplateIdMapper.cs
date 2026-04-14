namespace zoft.MauiExtensions.Controls.Platform;

/// <summary>
/// Maps <see cref="DataTemplate"/> instances to stable integer IDs for use in
/// platform adapter view-type pools (e.g. Android <c>BaseAdapter.GetItemViewType</c>).
/// </summary>
internal static class TemplateIdMapper
{
    /// <summary>
    /// Returns the integer view-type ID for the given item.
    /// <para>
    /// For a plain <see cref="DataTemplate"/>, always returns <c>0</c> (single pool).
    /// For a <see cref="DataTemplateSelector"/>, resolves the template for the item and assigns
    /// a stable sequential ID, registering new templates in <paramref name="idMap"/> as encountered.
    /// </para>
    /// </summary>
    internal static int GetViewType(
        DataTemplate template,
        object item,
        BindableObject container,
        Dictionary<DataTemplate, int> idMap)
    {
        if (template is DataTemplateSelector selector)
        {
            var resolved = selector.SelectTemplate(item, container);

            if (!idMap.ContainsKey(resolved))
                idMap[resolved] = idMap.Count;

            return idMap[resolved];
        }

        return 0;
    }
}
