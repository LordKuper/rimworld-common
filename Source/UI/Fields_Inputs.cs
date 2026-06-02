using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using UnityEngine;
using Verse;

namespace LordKuper.Common.UI;

/// <summary>
///     Provides input field rendering and logic for UI fields.
/// </summary>
public static partial class Fields
{
    private const float InputRowHeight = Layout.RowHeight;

    /// <summary>
    ///     Renders float range input fields for minimum and maximum values.
    /// </summary>
    private static void DoFloatRangeInputs(Rect rect, ref string minBuffer, ref string maxBuffer, float minValue,
        float maxValue, ToStringStyle style)
    {
        GetRangeInputsRects(rect, out var leftRect, out var dashRect, out var rightRect);
        DoFloatTextInput(leftRect, ref minBuffer, minValue, maxValue, style);
        var anchor = Text.Anchor;
        Text.Anchor = TextAnchor.MiddleCenter;
        Verse.Widgets.LabelFit(dashRect, "-");
        Text.Anchor = anchor;
        DoFloatTextInput(rightRect, ref maxBuffer, minValue, maxValue, style);
    }

    /// <summary>
    ///     Renders a float text input field and parses the input value.
    /// </summary>
    private static void DoFloatTextInput(Rect rect, ref string buffer, float minValue, float maxValue,
        ToStringStyle style)
    {
        buffer = Verse.Widgets.TextField(rect, buffer);
        if (!float.TryParse(buffer, NumberStyles.Float, CultureInfo.InvariantCulture, out var result)) return;
        result = Mathf.Clamp(result, minValue, maxValue);
        buffer = result.ToStringByStyle(style);
    }

    /// <summary>
    ///     Renders a labeled float range input row with optional icon and action buttons.
    /// </summary>
    public static float DoLabeledFloatRangeInputs(Rect rect, int indentationLevel,
        [CanBeNull] IReadOnlyCollection<IconButton> actionButtons, [NotNull] string label,
        [CanBeNull] string labelTooltip, ref string minBuffer, ref string maxBuffer, float minValue, float maxValue,
        ToStringStyle style, [CanBeNull] Texture icon, out Rect remRect)
    {
        var rowRect = GetFieldRowRect(rect, InputRowHeight, out remRect);
        DoFieldLabelWithButtons(rowRect, indentationLevel, actionButtons, label, labelTooltip, icon, out var inputRect);
        DoFloatRangeInputs(inputRect, ref minBuffer, ref maxBuffer, minValue, maxValue, style);
        return InputRowHeight;
    }

    /// <summary>
    ///     Renders a labeled text input row with optional icon and action buttons.
    /// </summary>
    public static float DoLabeledTextInput(Rect rect, int indentationLevel,
        [CanBeNull] IReadOnlyCollection<IconButton> actionButtons, [NotNull] string label,
        [CanBeNull] string labelTooltip, ref string value, [CanBeNull] Regex validRegex, int maxLength,
        [CanBeNull] Texture icon, out Rect remRect)
    {
        var rowRect = GetFieldRowRect(rect, InputRowHeight, out remRect);
        DoFieldLabelWithButtons(rowRect, indentationLevel, actionButtons, label, labelTooltip, icon, out var inputRect);
        DoTextInput(inputRect, ref value, validRegex, maxLength);
        return InputRowHeight;
    }

    /// <summary>
    ///     Renders a text input field.
    /// </summary>
    private static void DoTextInput(Rect rect, ref string value, [CanBeNull] Regex validRegex, int maxLength)
    {
        value = validRegex == null
            ? Verse.Widgets.TextField(rect, value, maxLength)
            : Verse.Widgets.TextField(rect, value, maxLength, validRegex);
    }

    /// <summary>
    ///     Calculates the rectangles for the left input, dash separator, and right input in a float range input row.
    /// </summary>
    private static void GetRangeInputsRects(Rect rect, out Rect leftRect, out Rect dashRect, out Rect rightRect)
    {
        var inputWidth = (rect.width - Layout.ElementGap - Layout.ElementGapSmall * 2) / 2f;
        leftRect = Layout.GetLeftColumnRect(rect, inputWidth, out rightRect);
        rightRect = Layout.GetRightColumnRect(rightRect, inputWidth, out dashRect);
    }
}