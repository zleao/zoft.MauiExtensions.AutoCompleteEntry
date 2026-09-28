namespace zoft.MauiExtensions.Controls.Platform;

// Screen-coordinate placement, independent of Android's automatic anchor scrolling.
internal readonly record struct SuggestionPopupPlacement(int X, int Y, int Width, int Height)
{
    internal static SuggestionPopupPlacement Calculate(int anchorX, int anchorY, int anchorWidth,
        int anchorHeight, int viewportLeft, int viewportTop, int viewportRight, int viewportBottom,
        int preferredHeight)
    {
        var width = Math.Min(anchorWidth, Math.Max(0, viewportRight - viewportLeft));
        var x = Math.Clamp(anchorX, viewportLeft, viewportRight - width);
        var above = Math.Max(0, Math.Min(anchorY, viewportBottom) - viewportTop);
        var below = Math.Max(0, viewportBottom - Math.Max(anchorY + anchorHeight, viewportTop));
        var useBelow = below >= preferredHeight || below >= above;
        var height = Math.Min(preferredHeight, useBelow ? below : above);
        var y = useBelow ? Math.Max(viewportTop, anchorY + anchorHeight) : Math.Min(anchorY, viewportBottom) - height;
        return new(x, y, width, height);
    }
}
