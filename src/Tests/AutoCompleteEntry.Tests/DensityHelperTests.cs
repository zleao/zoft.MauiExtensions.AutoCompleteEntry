using zoft.MauiExtensions.Controls.Platform;

namespace AutoCompleteEntry.Tests;

/// <summary>
/// Tests for <see cref="DensityHelper"/> — the DIP ↔ pixel conversion logic
/// used by the Android adapter when measuring suggestion row heights.
/// </summary>
public class DensityHelperTests
{
    #region WidthPixelsToDipConstraint

    [Theory]
    [InlineData(1080, 2.75, 392.727)] // typical 1080p Android phone
    [InlineData(1440, 3.5, 411.429)]  // typical 1440p Android phone
    [InlineData(720, 2.0, 360.0)]     // hdpi device
    [InlineData(320, 1.0, 320.0)]     // mdpi (1:1)
    public void WidthPixelsToDipConstraint_PositiveWidth_ReturnsDipValue(
        int parentWidthPx, double density, double expectedDip)
    {
        var result = DensityHelper.WidthPixelsToDipConstraint(parentWidthPx, density);

        Assert.Equal(expectedDip, result, precision: 3);
    }

    [Theory]
    [InlineData(0, 2.75)]
    [InlineData(-1, 3.0)]
    [InlineData(-100, 1.0)]
    public void WidthPixelsToDipConstraint_ZeroOrNegativeWidth_ReturnsPositiveInfinity(
        int parentWidthPx, double density)
    {
        var result = DensityHelper.WidthPixelsToDipConstraint(parentWidthPx, density);

        Assert.Equal(double.PositiveInfinity, result);
    }

    [Theory]
    [InlineData(1080, 0.0)]
    [InlineData(1080, -1.0)]
    public void WidthPixelsToDipConstraint_ZeroOrNegativeDensity_ReturnsPositiveInfinity(
        int parentWidthPx, double density)
    {
        var result = DensityHelper.WidthPixelsToDipConstraint(parentWidthPx, density);

        Assert.Equal(double.PositiveInfinity, result);
    }

    #endregion

    #region HeightDipToPixels

    [Theory]
    [InlineData(44.0, 2.75, 121)]  // 44 * 2.75 = 121.0 → ceil = 121
    [InlineData(44.0, 3.5, 154)]   // 44 * 3.5  = 154.0 → ceil = 154
    [InlineData(44.0, 2.0, 88)]    // 44 * 2.0  = 88.0  → ceil = 88
    [InlineData(44.0, 1.0, 44)]    // 1:1 mdpi
    public void HeightDipToPixels_StandardDensities_ReturnsCorrectPixels(
        double heightDip, double density, int expectedPx)
    {
        var result = DensityHelper.HeightDipToPixels(heightDip, density);

        Assert.Equal(expectedPx, result);
    }

    [Fact]
    public void HeightDipToPixels_FractionalResult_RoundsUp()
    {
        // 45.0 * 2.75 = 123.75 → ceil = 124 (never clip content)
        var result = DensityHelper.HeightDipToPixels(45.0, 2.75);

        Assert.Equal(124, result);
    }

    [Fact]
    public void HeightDipToPixels_ZeroHeight_ReturnsZero()
    {
        var result = DensityHelper.HeightDipToPixels(0.0, 3.0);

        Assert.Equal(0, result);
    }

    [Theory]
    [InlineData(44.0, 0.0)]
    [InlineData(44.0, -2.0)]
    public void HeightDipToPixels_ZeroOrNegativeDensity_ReturnsZero(
        double heightDip, double density)
    {
        var result = DensityHelper.HeightDipToPixels(heightDip, density);

        Assert.Equal(0, result);
    }

    [Fact]
    public void HeightDipToPixels_VerySmallHeight_RoundsUpToAtLeastOne()
    {
        // 0.1 * 2.0 = 0.2 → ceil = 1
        var result = DensityHelper.HeightDipToPixels(0.1, 2.0);

        Assert.Equal(1, result);
    }

    #endregion
}
