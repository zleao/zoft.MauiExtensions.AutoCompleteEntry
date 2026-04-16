namespace zoft.MauiExtensions.Controls.Platform;

/// <summary>
/// Provides conversion helpers between MAUI device-independent pixels (DIPs) and physical pixels.
/// </summary>
internal static class DensityHelper
{
    /// <summary>
    /// Converts a pixel-based width to a DIP-based measurement constraint.
    /// Returns <see cref="double.PositiveInfinity"/> when the pixel width is unavailable (≤ 0),
    /// allowing MAUI to measure with an unconstrained width.
    /// </summary>
    internal static double WidthPixelsToDipConstraint(int parentWidthPx, double density)
    {
        return parentWidthPx > 0 && density > 0
            ? parentWidthPx / density
            : double.PositiveInfinity;
    }

    /// <summary>
    /// Converts a DIP-based measured height to physical pixels, rounding up
    /// so content is never clipped.
    /// </summary>
    internal static int HeightDipToPixels(double heightDip, double density)
    {
        if (density <= 0)
            return 0;

        var px = (int)System.Math.Ceiling(heightDip * density);
        return System.Math.Max(px, 0);
    }
}
