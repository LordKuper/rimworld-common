using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace LordKuper.Common.UI;

public static partial class Fields
{
    /// <summary>
    ///     The height of a row containing a slider field.
    /// </summary>
    private const float SliderRowHeight = Layout.RowHeightLarge;

    /// <summary>
    ///     Draws a float range slider within the specified rectangle and updates its value.
    /// </summary>
    /// <param name="rect">The rectangle in which to draw the slider.</param>
    /// <param name="inputId">The unique input ID for the slider.</param>
    /// <param name="value">A reference to the <see cref="FloatRange" /> value to be modified by the slider.</param>
    /// <param name="minValue">The minimum value of the slider.</param>
    /// <param name="maxValue">The maximum value of the slider.</param>
    /// <param name="step">The step size for the slider.</param>
    private static void DoFloatRangeSlider(Rect rect, int inputId, ref FloatRange value, float minValue, float maxValue,
        float step)
    {
        Verse.Widgets.FloatRange(rect, inputId, ref value, minValue, maxValue, valueStyle: ToStringStyle.FloatMaxTwo,
            roundTo: step);
    }

    /// <summary>
    ///     Draws a float slider within the specified rectangle and updates its value.
    /// </summary>
    /// <param name="rect">The rectangle in which to draw the slider.</param>
    /// <param name="value">A reference to the float value to be modified by the slider.</param>
    /// <param name="minValue">The minimum value of the slider.</param>
    /// <param name="maxValue">The maximum value of the slider.</param>
    /// <param name="step">The step size for the slider.</param>
    private static void DoFloatSlider(Rect rect, ref float value, float minValue, float maxValue, float step)
    {
        value = Verse.Widgets.HorizontalSlider(rect, value, minValue, maxValue, true, $"{value:N2}", $"{minValue:N2}",
            $"{maxValue:N2}", step);
    }

    /// <summary>
    ///     Draws a frequency slider within the specified rectangle and updates its value.
    /// </summary>
    /// <param name="rect">The rectangle in which to draw the slider.</param>
    /// <param name="value">A reference to the float value to be modified by the slider.</param>
    /// <param name="minValue">The minimum value of the slider.</param>
    /// <param name="maxValue">The maximum value of the slider.</param>
    /// <param name="roundToInt">Whether to round the slider value to an integer.</param>
    private static void DoFrequencySlider(Rect rect, ref float value, float minValue, float maxValue, bool roundToInt)
    {
        value = Verse.Widgets.FrequencyHorizontalSlider(rect, value, minValue, maxValue, roundToInt);
    }

    /// <summary>
    ///     Draws an integer range slider within the specified rectangle and updates its value.
    /// </summary>
    /// <param name="rect">The rectangle in which to draw the slider.</param>
    /// <param name="inputId">The unique input ID for the slider.</param>
    /// <param name="value">A reference to the <see cref="IntRange" /> value to be modified by the slider.</param>
    /// <param name="minValue">The minimum value of the slider.</param>
    /// <param name="maxValue">The maximum value of the slider.</param>
    /// <param name="step">The step size for the slider.</param>
    private static void DoIntegerRangeSlider(Rect rect, int inputId, ref IntRange value, int minValue, int maxValue,
        float step)
    {
        var buffer = new FloatRange(value.min, value.max);
        Verse.Widgets.FloatRange(rect, inputId, ref buffer, minValue, maxValue, valueStyle: ToStringStyle.FloatMaxTwo,
            roundTo: step);
        value.min = Mathf.RoundToInt(buffer.min);
        value.max = Mathf.RoundToInt(buffer.max);
    }

    /// <summary>
    ///     Draws an integer slider within the specified rectangle and updates its value.
    /// </summary>
    /// <param name="rect">The rectangle in which to draw the slider.</param>
    /// <param name="value">A reference to the integer value to be modified by the slider.</param>
    /// <param name="minValue">The minimum value of the slider.</param>
    /// <param name="maxValue">The maximum value of the slider.</param>
    /// <param name="step">The step size for the slider.</param>
    private static void DoIntegerSlider(Rect rect, ref int value, int minValue, int maxValue, int step)
    {
        value = (int)Verse.Widgets.HorizontalSlider(rect, value, minValue, maxValue, true, $"{value:N0}",
            $"{minValue:N0}", $"{maxValue:N0}", step);
    }

    /// <summary>
    ///     Draws a labeled float range slider with optional action buttons.
    /// </summary>
    /// <param name="rect">The rectangle in which to draw the field.</param>
    /// <param name="inputId">The unique input ID for the slider.</param>
    /// <param name="indentationLevel">The level of indentation applied to the field label.</param>
    /// <param name="actionButtons">A collection of action buttons to display next to the label.</param>
    /// <param name="label">The label text.</param>
    /// <param name="labelTooltip">The tooltip for the label.</param>
    /// <param name="value">The <see cref="FloatRange" /> value to be modified by the slider.</param>
    /// <param name="minValue">The minimum value of the slider.</param>
    /// <param name="maxValue">The maximum value of the slider.</param>
    /// <param name="step">The step size for the slider.</param>
    /// <param name="icon">The icon to display next to the label, or null for none.</param>
    /// <param name="remRect">The remaining rectangle after drawing the field.</param>
    /// <returns>The height of the row used for the field.</returns>
    public static float DoLabeledFloatRangeSlider(Rect rect, int inputId, int indentationLevel,
        IReadOnlyCollection<IconButton>? actionButtons, string label, string? labelTooltip, ref FloatRange value,
        float minValue, float maxValue, float step, Texture? icon, out Rect remRect)
    {
        var rowRect = GetFieldRowRect(rect, SliderRowHeight, out remRect);
        DoFieldLabelWithButtons(rowRect, indentationLevel, actionButtons, label, labelTooltip, icon, out var inputRect);
        DoFloatRangeSlider(inputRect, inputId, ref value, minValue, maxValue, step);
        return SliderRowHeight;
    }

    /// <summary>
    ///     Draws a labeled float slider with optional action buttons.
    /// </summary>
    /// <param name="rect">The rectangle in which to draw the field.</param>
    /// <param name="indentationLevel">The level of indentation applied to the field label.</param>
    /// <param name="actionButtons">A collection of action buttons to display next to the label.</param>
    /// <param name="label">The label text.</param>
    /// <param name="labelTooltip">The tooltip for the label.</param>
    /// <param name="value">The float value to be modified by the slider.</param>
    /// <param name="minValue">The minimum value of the slider.</param>
    /// <param name="maxValue">The maximum value of the slider.</param>
    /// <param name="step">The step size for the slider.</param>
    /// <param name="icon">The icon to display next to the label, or null for none.</param>
    /// <param name="remRect">The remaining rectangle after drawing the field.</param>
    /// <returns>The height of the row used for the field.</returns>
    public static float DoLabeledFloatSlider(Rect rect, int indentationLevel,
        IReadOnlyCollection<IconButton>? actionButtons, string label, string? labelTooltip, ref float value,
        float minValue, float maxValue, float step, Texture? icon, out Rect remRect)
    {
        var rowRect = GetFieldRowRect(rect, SliderRowHeight, out remRect);
        DoFieldLabelWithButtons(rowRect, indentationLevel, actionButtons, label, labelTooltip, icon, out var inputRect);
        DoFloatSlider(inputRect, ref value, minValue, maxValue, step);
        return SliderRowHeight;
    }

    /// <summary>
    ///     Draws a labeled frequency slider with optional action buttons.
    /// </summary>
    /// <param name="rect">The rectangle in which to draw the field.</param>
    /// <param name="indentationLevel">The level of indentation applied to the field label.</param>
    /// <param name="actionButtons">A collection of action buttons to display next to the label.</param>
    /// <param name="label">The label text.</param>
    /// <param name="labelTooltip">The tooltip for the label.</param>
    /// <param name="value">The float value to be modified by the slider.</param>
    /// <param name="minValue">The minimum value of the slider.</param>
    /// <param name="maxValue">The maximum value of the slider.</param>
    /// <param name="roundToInt">Whether to round the slider value to an integer.</param>
    /// <param name="icon">The icon to display next to the label, or null for none.</param>
    /// <param name="remRect">The remaining rectangle after drawing the field.</param>
    /// <returns>The height of the row used for the field.</returns>
    public static float DoLabeledFrequencySlider(Rect rect, int indentationLevel,
        IReadOnlyCollection<IconButton>? actionButtons, string label, string? labelTooltip, ref float value,
        float minValue, float maxValue, bool roundToInt, Texture? icon, out Rect remRect)
    {
        var rowRect = GetFieldRowRect(rect, SliderRowHeight, out remRect);
        DoFieldLabelWithButtons(rowRect, indentationLevel, actionButtons, label, labelTooltip, icon, out var inputRect);
        DoFrequencySlider(inputRect, ref value, minValue, maxValue, roundToInt);
        return SliderRowHeight;
    }

    /// <summary>
    ///     Draws a labeled integer range slider with optional action buttons.
    /// </summary>
    /// <param name="rect">The rectangle in which to draw the field.</param>
    /// <param name="inputId">The unique input ID for the slider.</param>
    /// <param name="indentationLevel">The level of indentation applied to the field label.</param>
    /// <param name="actionButtons">A collection of action buttons to display next to the label.</param>
    /// <param name="label">The label text.</param>
    /// <param name="labelTooltip">The tooltip for the label.</param>
    /// <param name="value">The <see cref="IntRange" /> value to be modified by the slider.</param>
    /// <param name="minValue">The minimum value of the slider.</param>
    /// <param name="maxValue">The maximum value of the slider.</param>
    /// <param name="step">The step size for the slider.</param>
    /// <param name="icon">The icon to display next to the label, or null for none.</param>
    /// <param name="remRect">The remaining rectangle after drawing the field.</param>
    /// <returns>The height of the row used for the field.</returns>
    public static float DoLabeledIntegerRangeSlider(Rect rect, int inputId, int indentationLevel,
        IReadOnlyCollection<IconButton>? actionButtons, string label, string? labelTooltip, ref IntRange value,
        int minValue, int maxValue, int step, Texture? icon, out Rect remRect)
    {
        var rowRect = GetFieldRowRect(rect, SliderRowHeight, out remRect);
        DoFieldLabelWithButtons(rowRect, indentationLevel, actionButtons, label, labelTooltip, icon, out var inputRect);
        DoIntegerRangeSlider(inputRect, inputId, ref value, minValue, maxValue, step);
        return SliderRowHeight;
    }

    /// <summary>
    ///     Draws a labeled integer slider with optional action buttons.
    /// </summary>
    /// <param name="rect">The rectangle in which to draw the field.</param>
    /// <param name="indentationLevel">The level of indentation applied to the field label.</param>
    /// <param name="actionButtons">A collection of action buttons to display next to the label.</param>
    /// <param name="label">The label text.</param>
    /// <param name="labelTooltip">The tooltip for the label.</param>
    /// <param name="value">The integer value to be modified by the slider.</param>
    /// <param name="minValue">The minimum value of the slider.</param>
    /// <param name="maxValue">The maximum value of the slider.</param>
    /// <param name="step">The step size for the slider.</param>
    /// <param name="icon">The icon to display next to the label, or null for none.</param>
    /// <param name="remRect">The remaining rectangle after drawing the field.</param>
    /// <returns>The height of the row used for the field.</returns>
    public static float DoLabeledIntegerSlider(Rect rect, int indentationLevel,
        IReadOnlyCollection<IconButton>? actionButtons, string label, string? labelTooltip, ref int value, int minValue,
        int maxValue, int step, Texture? icon, out Rect remRect)
    {
        var rowRect = GetFieldRowRect(rect, SliderRowHeight, out remRect);
        DoFieldLabelWithButtons(rowRect, indentationLevel, actionButtons, label, labelTooltip, icon, out var inputRect);
        DoIntegerSlider(inputRect, ref value, minValue, maxValue, step);
        return SliderRowHeight;
    }

    /// <summary>
    ///     Draws a labeled percent range slider with optional action buttons.
    /// </summary>
    /// <param name="rect">The rectangle in which to draw the field.</param>
    /// <param name="inputId">The unique input ID for the slider.</param>
    /// <param name="indentationLevel">The level of indentation applied to the field label.</param>
    /// <param name="actionButtons">A collection of action buttons to display next to the label.</param>
    /// <param name="label">The label text.</param>
    /// <param name="labelTooltip">The tooltip for the label.</param>
    /// <param name="value">The <see cref="FloatRange" /> value to be modified by the slider.</param>
    /// <param name="minValue">The minimum value of the slider.</param>
    /// <param name="maxValue">The maximum value of the slider.</param>
    /// <param name="step">The step size for the slider.</param>
    /// <param name="icon">The icon to display next to the label, or null for none.</param>
    /// <param name="remRect">The remaining rectangle after drawing the field.</param>
    /// <param name="style">The style to use for displaying the value.</param>
    /// <returns>The height of the row used for the field.</returns>
    public static float DoLabeledPercentRangeSlider(Rect rect, int inputId, int indentationLevel,
        IReadOnlyCollection<IconButton>? actionButtons, string label, string? labelTooltip, ref FloatRange value,
        float minValue, float maxValue, float step, ToStringStyle style, Texture? icon, out Rect remRect)
    {
        var rowRect = GetFieldRowRect(rect, SliderRowHeight, out remRect);
        DoFieldLabelWithButtons(rowRect, indentationLevel, actionButtons, label, labelTooltip, icon, out var inputRect);
        DoPercentRangeSlider(inputRect, inputId, ref value, minValue, maxValue, step, style);
        return SliderRowHeight;
    }

    /// <summary>
    ///     Draws a labeled percent slider with optional action buttons.
    /// </summary>
    /// <param name="rect">The rectangle in which to draw the field.</param>
    /// <param name="indentationLevel">The level of indentation applied to the field label.</param>
    /// <param name="actionButtons">A collection of action buttons to display next to the label.</param>
    /// <param name="label">The label text.</param>
    /// <param name="labelTooltip">The tooltip for the label.</param>
    /// <param name="value">The float value to be modified by the slider.</param>
    /// <param name="minValue">The minimum value of the slider.</param>
    /// <param name="maxValue">The maximum value of the slider.</param>
    /// <param name="step">The step size for the slider.</param>
    /// <param name="icon">The icon to display next to the label, or null for none.</param>
    /// <param name="remRect">The remaining rectangle after drawing the field.</param>
    /// <returns>The height of the row used for the field.</returns>
    public static float DoLabeledPercentSlider(Rect rect, int indentationLevel,
        IReadOnlyCollection<IconButton>? actionButtons, string label, string? labelTooltip, ref float value,
        float minValue, float maxValue, float step, Texture? icon, out Rect remRect)
    {
        var rowRect = GetFieldRowRect(rect, SliderRowHeight, out remRect);
        DoFieldLabelWithButtons(rowRect, indentationLevel, actionButtons, label, labelTooltip, icon, out var inputRect);
        DoPercentSlider(inputRect, ref value, minValue, maxValue, step);
        return SliderRowHeight;
    }

    /// <summary>
    ///     Draws a percent range slider within the specified rectangle and updates its value.
    /// </summary>
    /// <param name="rect">The rectangle in which to draw the slider.</param>
    /// <param name="inputId">The unique input ID for the slider.</param>
    /// <param name="value">A reference to the <see cref="FloatRange" /> value to be modified by the slider.</param>
    /// <param name="minValue">The minimum value of the slider.</param>
    /// <param name="maxValue">The maximum value of the slider.</param>
    /// <param name="step">The step size for the slider.</param>
    /// <param name="style">The style to use for displaying the value.</param>
    private static void DoPercentRangeSlider(Rect rect, int inputId, ref FloatRange value, float minValue,
        float maxValue, float step, ToStringStyle style)
    {
        Verse.Widgets.FloatRange(rect, inputId, ref value, minValue, maxValue, valueStyle: style, roundTo: step);
    }

    /// <summary>
    ///     Draws a percent slider within the specified rectangle and updates its value.
    /// </summary>
    /// <param name="rect">The rectangle in which to draw the slider.</param>
    /// <param name="value">A reference to the float value to be modified by the slider.</param>
    /// <param name="minValue">The minimum value of the slider.</param>
    /// <param name="maxValue">The maximum value of the slider.</param>
    /// <param name="step">The step size for the slider.</param>
    private static void DoPercentSlider(Rect rect, ref float value, float minValue, float maxValue, float step)
    {
        value = Verse.Widgets.HorizontalSlider(rect, value, minValue, maxValue, true, $"{value:P0}", $"{minValue:P0}",
            $"{maxValue:P0}", step);
    }

    /// <summary>
    ///     Determines the appropriate step size for a float slider based on the specified <see cref="ToStringStyle" />.
    /// </summary>
    /// <param name="style">
    ///     The <see cref="ToStringStyle" /> that defines the formatting style for the slider value.
    /// </param>
    /// <returns>
    ///     Returns <c>0.01f</c> for <see cref="ToStringStyle.PercentZero" />, <c>0.001f</c> for
    ///     <see cref="ToStringStyle.PercentOne" />,
    ///     <c>0.0001f</c> for <see cref="ToStringStyle.PercentTwo" />, and <c>0.1f</c> for all other styles.
    /// </returns>
    internal static float GetFloatSliderStepByValueStyle(ToStringStyle style)
    {
        return style switch
        {
            ToStringStyle.PercentZero => 0.01f,
            ToStringStyle.PercentOne => 0.001f,
            ToStringStyle.PercentTwo => 0.0001f,
            _ => 0.1f
        };
    }
}