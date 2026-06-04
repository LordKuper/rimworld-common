using System.Globalization;
using JetBrains.Annotations;
using LordKuper.Common.Cache;
using LordKuper.Common.UI;
using UnityEngine;
using Verse;

namespace LordKuper.Common.Filters.Limits;

/// <summary>
///     Represents a limit for a specific <see cref="PawnCapacityDef" />.
///     Provides range constraints and value step for UI sliders.
/// </summary>
[PublicAPI]
public class PawnCapacityLimit : DefCache<PawnCapacityDef>, IExposable
{
    internal const float LimitMaxCap = 5f;
    internal const float LimitMinCap = 0f;
    internal const ToStringStyle ValueStyle = ToStringStyle.PercentZero;

    /// <summary>
    ///     The allowed value range for the capacity, clamped between <see cref="LimitMinCap" /> and
    ///     <see cref="LimitMaxCap" />.
    /// </summary>
    public FloatRange Limit = new(LimitMinCap, LimitMaxCap);

    private string? _maxValueBuffer;
    private string? _minValueBuffer;

    /// <summary>
    ///     Initializes a new instance of the <see cref="PawnCapacityLimit" /> class.
    /// </summary>
    public PawnCapacityLimit() { }

    /// <summary>
    ///     Initializes a new instance of the <see cref="PawnCapacityLimit" /> class for the capacity with the specified
    ///     def name.
    /// </summary>
    /// <param name="pawnCapacityDefName">The def name of the <see cref="PawnCapacityDef" /> to limit.</param>
    public PawnCapacityLimit(string pawnCapacityDefName) : base(pawnCapacityDefName) { }

    /// <summary>
    ///     Initializes a new instance of the <see cref="PawnCapacityLimit" /> class for the capacity with the specified
    ///     def name and value bounds.
    /// </summary>
    /// <param name="pawnCapacityDefName">The def name of the <see cref="PawnCapacityDef" /> to limit.</param>
    /// <param name="minValue">The lower bound of the allowed range, or <c>null</c> for no lower bound.</param>
    /// <param name="maxValue">The upper bound of the allowed range, or <c>null</c> for no upper bound.</param>
    public PawnCapacityLimit(string pawnCapacityDefName, float? minValue, float? maxValue) : this(
        pawnCapacityDefName)
    {
        MinValue = minValue;
        MaxValue = maxValue;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="PawnCapacityLimit" /> class from the specified
    ///     <see cref="PawnCapacityDef" /> instance.
    /// </summary>
    /// <param name="def">The capacity definition to limit.</param>
    /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="def" /> is null.</exception>
    public PawnCapacityLimit(PawnCapacityDef def) : base(GetDefName(def)) { }

    /// <summary>
    ///     Gets or sets the upper bound of the allowed range, or <c>null</c> when no upper bound is set
    ///     (the value is at <see cref="LimitMaxCap" />). Setting clamps the value between
    ///     <see cref="LimitMinCap" /> and <see cref="LimitMaxCap" />.
    /// </summary>
    public float? MaxValue
    {
        get => string.IsNullOrEmpty(_maxValueBuffer) && Mathf.Approximately(Limit.max, LimitMaxCap)
            ? null
            : Limit.max;
        set
        {
            if (!value.HasValue)
            {
                Limit.max = LimitMaxCap;
                _maxValueBuffer = string.Empty;
                return;
            }
            Limit.max = Mathf.Clamp(value.Value, LimitMinCap, LimitMaxCap);
            _maxValueBuffer = Limit.max.ToString("F2", CultureInfo.InvariantCulture);
        }
    }

    /// <summary>
    ///     Gets or sets the UI text-input buffer for the upper bound. Parses valid numeric input into
    ///     <see cref="MaxValue" /> and otherwise retains the raw text.
    /// </summary>
    public string MaxValueBuffer
    {
        get => string.IsNullOrEmpty(_maxValueBuffer)
            ? MaxValue?.ToString("F2", CultureInfo.InvariantCulture) ?? string.Empty
            : _maxValueBuffer!; // non-null: IsNullOrEmpty was false, so _maxValueBuffer is a non-empty string
        set
        {
            if (ReferenceEquals(value, _maxValueBuffer) || value == _maxValueBuffer) return;
            if (string.IsNullOrEmpty(value))
            {
                MaxValue = null;
                return;
            }
            if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture,
                    out var parsed))
                MaxValue = parsed;
            else
                _maxValueBuffer = value;
        }
    }

    /// <summary>
    ///     Gets or sets the lower bound of the allowed range, or <c>null</c> when no lower bound is set
    ///     (the value is at <see cref="LimitMinCap" />). Setting clamps the value between
    ///     <see cref="LimitMinCap" /> and <see cref="LimitMaxCap" />.
    /// </summary>
    public float? MinValue
    {
        get => string.IsNullOrEmpty(_minValueBuffer) && Mathf.Approximately(Limit.min, LimitMinCap)
            ? null
            : Limit.min;
        set
        {
            if (!value.HasValue)
            {
                Limit.min = LimitMinCap;
                _minValueBuffer = string.Empty;
                return;
            }
            Limit.min = Mathf.Clamp(value.Value, LimitMinCap, LimitMaxCap);
            _minValueBuffer = Limit.min.ToString("F2", CultureInfo.InvariantCulture);
        }
    }

    /// <summary>
    ///     Gets or sets the UI text-input buffer for the lower bound. Parses valid numeric input into
    ///     <see cref="MinValue" /> and otherwise retains the raw text.
    /// </summary>
    public string MinValueBuffer
    {
        get => string.IsNullOrEmpty(_minValueBuffer)
            ? MinValue?.ToString("F2", CultureInfo.InvariantCulture) ?? string.Empty
            : _minValueBuffer!; // non-null: IsNullOrEmpty was false, so _minValueBuffer is a non-empty string
        set
        {
            if (ReferenceEquals(value, _minValueBuffer) || value == _minValueBuffer) return;
            if (string.IsNullOrEmpty(value))
            {
                MinValue = null;
                return;
            }
            if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture,
                    out var parsed))
                MinValue = parsed;
            else
                _minValueBuffer = value;
        }
    }

    /// <summary>
    ///     Gets the resolved <see cref="PawnCapacityDef" /> for this limit, or <c>null</c> if it could not be resolved.
    /// </summary>
    public PawnCapacityDef? PawnCapacityDef => Def;

    /// <summary>
    ///     Gets the def name of the capacity this limit targets, or <c>null</c> if none is set.
    /// </summary>
    public string? PawnCapacityDefName => DefName;

    internal float ValueStep
    {
        get
        {
            if (field == 0f) field = Fields.GetFloatSliderStepByValueStyle(ValueStyle);
            return field;
        }
    }

    /// <summary>
    ///     Serializes the limit's state (def name and value range) for saving and loading.
    /// </summary>
    public new void ExposeData()
    {
        base.ExposeData();
        var minValue = MinValue;
        var maxValue = MaxValue;
        Scribe_Values.Look(ref minValue, nameof(MinValue));
        Scribe_Values.Look(ref maxValue, nameof(MaxValue));
        Scribe_Values.Look(ref Limit, nameof(Limit));
        if (Scribe.mode != LoadSaveMode.Saving)
        {
            MinValue = minValue;
            MaxValue = maxValue;
        }
    }
}