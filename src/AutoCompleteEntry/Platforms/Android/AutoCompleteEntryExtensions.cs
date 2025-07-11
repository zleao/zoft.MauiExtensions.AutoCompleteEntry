using Android.Graphics.Drawables;
using Android.Text;
using Android.Views;
using zoft.MauiExtensions.Controls.Handlers;
using zoft.MauiExtensions.Core.Extensions;

namespace zoft.MauiExtensions.Controls.Platform;

/// <summary>
/// Extensions for <see cref="AutoCompleteEntry"/>
/// </summary>
public static class AutoCompleteEntryExtensions
{
    /// <summary>
    /// Checks whether the touched position on the EditText is inbounds with clear button and clears if so.
    /// This will return True to handle OnTouch to prevent re-activating keyboard after clearing the text.
    /// </summary>
    /// <returns>True if clear button is clicked and Text is cleared. False if not.</returns>
    public static bool HandleClearButtonTouched(this AndroidAutoCompleteEntry platformView,
                                                Android.Views.View.TouchEventArgs touchEvent,
                                                Func<Drawable> getClearButtonDrawable)
    {
        if (platformView is null)
            return false;

        var motionEvent = touchEvent?.Event;
        if (motionEvent is null)
            return false;

        if (motionEvent.Action != MotionEventActions.Up)
            return false;

        var rBounds = getClearButtonDrawable?.Invoke()?.Bounds;

        if (rBounds is null)
        {
            // The button doesn't exist, or we can't retrieve it. 
            return false;
        }

        var buttonRect = GetClearButtonLocation(rBounds, platformView);

        if (!RectContainsMotionEvent(buttonRect, motionEvent))
        {
            return false;
        }

        platformView.Text = null;
        return true;
    }
    // Android.Graphics.Rect has a Containts(x,y) method, but it only takes `int` and the coordinates from
    // the motion event are `float`. The we use GetX() and GetY() so our coordinates are relative to the
    // bounds of the EditText.
    private static bool RectContainsMotionEvent(Android.Graphics.Rect rect, MotionEvent motionEvent)
    {
        var x = motionEvent.GetX();

        if (x < rect.Left || x > rect.Right)
        {
            return false;
        }

        var y = motionEvent.GetY();

        if (y < rect.Top || y > rect.Bottom)
        {
            return false;
        }

        return true;
    }
    // Gets the location of the "Clear" button relative to the bounds of the EditText
    private static Android.Graphics.Rect GetClearButtonLocation(Android.Graphics.Rect buttonRect, AndroidAutoCompleteEntry platformView)
    {
        // Determine the top and bottom edges of the button
        // This assumes the button is vertically centered within the padded area of the EditText

        var topEdge = platformView.PaddingTop;
        var bottomEdge = platformView.Height - platformView.PaddingBottom;

        // The horizontal location of the button depends on the layout direction
        var flowDirection = platformView.LayoutDirection;

        if (flowDirection == Android.Views.LayoutDirection.Ltr)
        {
            var rightEdge = platformView.Width - platformView.PaddingRight;
            var leftEdge = rightEdge - buttonRect.Width();

            return new Android.Graphics.Rect(leftEdge, topEdge, rightEdge, bottomEdge);
        }
        else
        {
            var leftEdge = platformView.PaddingLeft;
            var rightEdge = leftEdge + buttonRect.Width();

            return new Android.Graphics.Rect(leftEdge, topEdge, rightEdge, bottomEdge);
        }
    }


    /// <summary>
    /// Update the clear button visibility
    /// </summary>
    /// <param name="platformView"></param>
    /// <param name="virtualView"></param>
    public static void UpdateClearButtonVisibility(this AndroidAutoCompleteEntry platformView, AutoCompleteEntry virtualView)
    {
        if (virtualView?.Handler is not AutoCompleteEntryHandler autoCompleteEntryHandler)
        {
            return;
        }

        bool isFocused = platformView.IsFocused;
        bool hasText = virtualView.Text?.Length > 0;
        bool shouldDisplayClearButton = virtualView.ClearButtonVisibility == ClearButtonVisibility.WhileEditing
                                        &&
                                        hasText
                                        &&
                                        isFocused;

        if (shouldDisplayClearButton)
        {
            autoCompleteEntryHandler.ShowClearButton();
        }
        else
        {
            autoCompleteEntryHandler.HideClearButton();
        }
    }

    /// <summary>
    /// Update the DisplayMemberPath
    /// </summary>
    /// <param name="platformView"></param>
    /// <param name="virtualView"></param>
    public static void UpdateDisplayMemberPath(this AndroidAutoCompleteEntry platformView, AutoCompleteEntry virtualView)
    {
        platformView.SetItems(
            virtualView.ItemsSource,
            virtualView?.DisplayMemberPath,
            (o) => !string.IsNullOrEmpty(virtualView?.TextMemberPath) ? o.GetPropertyValueAsString(virtualView?.TextMemberPath) : o?.ToString());
    }

    /// <summary>
    /// Update the IsSuggestionListOpen
    /// </summary>
    /// <param name="platformView"></param>
    /// <param name="virtualView"></param>
    public static void UpdateIsSuggestionListOpen(this AndroidAutoCompleteEntry platformView, AutoCompleteEntry virtualView)
    {
        platformView.IsSuggestionListOpen = virtualView.IsSuggestionListOpen;
    }

    /// <summary>
    /// Update the IsTextPredictionEnabled
    /// </summary>
    /// <param name="platformView"></param>
    /// <param name="virtualView"></param>
    public static void UpdateIsTextPredictionEnabled(this AndroidAutoCompleteEntry platformView, AutoCompleteEntry virtualView)
    {
        if (virtualView.IsTextPredictionEnabled)
            platformView.InputType &= ~InputTypes.TextFlagNoSuggestions;
        else
            platformView.InputType |= InputTypes.TextFlagNoSuggestions;
    }

    /// <summary>
    /// Update the ItemsSource
    /// </summary>
    /// <param name="platformView"></param>
    /// <param name="virtualView"></param>
    public static void UpdateItemsSource(this AndroidAutoCompleteEntry platformView, AutoCompleteEntry virtualView)
    {
        platformView.SetItems(
            virtualView?.ItemsSource,
            virtualView?.DisplayMemberPath,
            (o) => !string.IsNullOrEmpty(virtualView?.TextMemberPath) ? o.GetPropertyValueAsString(virtualView?.TextMemberPath) : o?.ToString());
    }

    /// <summary>
    /// Update the SelectedSuggestion
    /// </summary>
    /// <param name="platformView"></param>
    /// <param name="virtualView"></param>
    public static void UpdateSelectedSuggestion(this AndroidAutoCompleteEntry platformView, AutoCompleteEntry virtualView)
    {
        if (virtualView.SelectedSuggestion is null)
        {
            return;
        }

        platformView.Text = 
            !string.IsNullOrEmpty(virtualView.TextMemberPath) ?
            virtualView.SelectedSuggestion.GetPropertyValueAsString(virtualView.TextMemberPath) 
            :
            virtualView.SelectedSuggestion.ToString();

        platformView.SetSelection(platformView.Text?.Length ?? 0);
    }

    /// <summary>
    /// Update the Text
    /// </summary>
    /// <param name="platformView"></param>
    /// <param name="virtualView"></param>
    public static void UpdateText(this AndroidAutoCompleteEntry platformView, AutoCompleteEntry virtualView)
    {
        if (platformView.Text != virtualView.Text)
        {
            platformView.Text = virtualView.Text;
            platformView.SetSelection(platformView.Text?.Length ?? 0);
        }
    }

    /// <summary>
    /// Update the UpdateTextOnSelect
    /// </summary>
    /// <param name="platformView"></param>
    /// <param name="virtualView"></param>
    public static void UpdateUpdateTextOnSelect(this AndroidAutoCompleteEntry platformView, AutoCompleteEntry virtualView)
    {
        platformView.UpdateTextOnSelect = virtualView.UpdateTextOnSelect;
    }

    /// <summary>
    /// Update the ShowBottomBorder
    /// </summary>
    /// <param name="platformView"></param>
    /// <param name="virtualView"></param>
    public static void UpdateShowBottomBorder(this AndroidAutoCompleteEntry platformView, AutoCompleteEntry virtualView)
    {
        platformView.ShowBottomBorder = virtualView.ShowBottomBorder;
    }

    /// <summary>
    /// Update the ItemTemplate
    /// </summary>
    /// <param name="platformView"></param>
    /// <param name="virtualView"></param>
    public static void UpdateItemTemplate(this AndroidAutoCompleteEntry platformView, AutoCompleteEntry virtualView)
    {
        platformView.SetItemTemplate(virtualView.ItemTemplate);
        platformView.SetItems(virtualView.ItemsSource,
                              virtualView?.DisplayMemberPath,
                              (o) => !string.IsNullOrEmpty(virtualView?.TextMemberPath) ? o.GetPropertyValueAsString(virtualView?.TextMemberPath) : o?.ToString());
    }
}