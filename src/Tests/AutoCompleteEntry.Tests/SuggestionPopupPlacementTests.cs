using zoft.MauiExtensions.Controls.Platform;

namespace AutoCompleteEntry.Tests;

public class SuggestionPopupPlacementTests
{
    [Theory]
    [InlineData(1307, 1920)] // Events page before the keyboard appears.
    [InlineData(1307, 1048)] // Keyboard is visible; parent has not panned yet.
    [InlineData(930, 1048)]  // Captured editor bounds after keyboard-driven panning.
    [InlineData(600, 1048)]  // Further parent scrolling while editing.
    public void KeyboardPanKeepsBoundedDropdownOutsideEditor(int anchorY, int viewportBottom)
    {
        var result = SuggestionPopupPlacement.Calculate(27, anchorY, 1027, 132,
            0, 108, 1080, viewportBottom, 848);
        Assert.InRange(result.Y, 108, viewportBottom);
        Assert.True(result.Y + result.Height <= viewportBottom);
        Assert.True(result.Y + result.Height <= anchorY || result.Y >= anchorY + 132);
        Assert.InRange(result.Height, 1, 848);
    }

    [Fact]
    public void KeyboardAppearanceMovesPopupAboveWithoutChangingItsHeight()
    {
        var closedKeyboard = SuggestionPopupPlacement.Calculate(10, 500, 300, 50, 0, 20, 400, 1000, 320);
        var openKeyboard = SuggestionPopupPlacement.Calculate(10, 500, 300, 50, 0, 20, 400, 600, 320);
        Assert.Equal(new SuggestionPopupPlacement(10, 550, 300, 320), closedKeyboard);
        Assert.Equal(new SuggestionPopupPlacement(10, 180, 300, 320), openKeyboard);
    }

    [Theory]
    [InlineData(500, 600, 180, 320)]
    [InlineData(550, 600, 230, 320)]
    [InlineData(100, 250, 150, 100)]
    public void ScrollingAndSmallViewportsKeepPopupOutsideEditorAndKeyboard(int anchorY, int bottom, int expectedY, int expectedHeight)
    {
        var result = SuggestionPopupPlacement.Calculate(10, anchorY, 300, 50, 0, 20, 400, bottom, 320);
        Assert.Equal(expectedY, result.Y);
        Assert.Equal(expectedHeight, result.Height);
        Assert.True(result.Y >= 20 && result.Y + result.Height <= bottom);
        Assert.True(result.Y + result.Height <= anchorY || result.Y >= anchorY + 50);
    }

    [Fact]
    public void NarrowWindowClampsWidthAndHorizontalPosition()
    {
        var result = SuggestionPopupPlacement.Calculate(200, 100, 300, 50, 10, 20, 250, 800, 320);
        Assert.Equal(10, result.X);
        Assert.Equal(240, result.Width);
    }

    [Fact]
    public void EmptyResultPanelDoesNotCoverEditor()
    {
        var result = SuggestionPopupPlacement.Calculate(10, 500, 300, 50, 0, 20, 400, 600, 1);
        Assert.Equal(550, result.Y);
        Assert.Equal(1, result.Height);
    }
}
