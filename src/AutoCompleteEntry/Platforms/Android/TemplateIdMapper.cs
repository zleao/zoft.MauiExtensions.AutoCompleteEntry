namespace zoft.MauiExtensions.Controls.Platform;

/// <summary>
/// Maps <see cref="DataTemplate"/> instances to stable integer IDs for use in
/// platform adapter view-type pools (e.g. Android <c>BaseAdapter.GetItemViewType</c>).
/// </summary>
internal static class TemplateIdMapper
{
    /// <summary>
    /// Maximum number of distinct view types supported when using a <see cref="DataTemplateSelector"/>.
    /// This value must match the pool count declared in the platform adapter's <c>ViewTypeCount</c>.
    /// </summary>
    internal const int MaxViewTypes = 10;

    /// <summary>
    /// Returns the integer view-type ID for the given resolved <see cref="DataTemplate"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when more than <see cref="MaxViewTypes"/> distinct templates are registered.
    /// </exception>
    internal static int GetViewType(
        DataTemplate resolvedTemplate,
        Dictionary<DataTemplate, int> idMap)
    {
        if (!idMap.TryGetValue(resolvedTemplate, out int value))
        {
            if (idMap.Count >= MaxViewTypes)
            {
                throw new InvalidOperationException(
                    $"DataTemplateSelector returned more than {MaxViewTypes} distinct templates. " +
                    $"Increase {nameof(TemplateIdMapper)}.{nameof(MaxViewTypes)} to support more.");
            }

            value = idMap.Count;
            idMap[resolvedTemplate] = value;
        }

        return value;
    }

    /// <summary>
    /// Returns the integer view-type ID for the given item.
    /// <para>
    /// For a plain <see cref="DataTemplate"/>, always returns <c>0</c> (single pool).
    /// For a <see cref="DataTemplateSelector"/>, resolves the template for the item and assigns
    /// a stable sequential ID, registering new templates in <paramref name="idMap"/> as encountered.
    /// </para>
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when a <see cref="DataTemplateSelector"/> produces more than <see cref="MaxViewTypes"/> distinct templates.
    /// </exception>
    internal static int GetViewType(
        DataTemplate template,
        object item,
        BindableObject? container,
        Dictionary<DataTemplate, int> idMap)
    {
        if (template is DataTemplateSelector selector)
        {
            var resolved = selector.SelectTemplate(item, container)
                ?? throw new InvalidOperationException(
                    $"DataTemplateSelector '{selector.GetType().FullName}' returned null for item '{item}'.");
            return GetViewType(resolved, idMap);
        }

        return 0;
    }
}
