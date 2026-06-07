using System.Globalization;
using JetBrains.Annotations;
using LordKuper.Common.Cache;
using RimWorld;
using UnityEngine;
using Verse;

namespace LordKuper.Common.Filters.Limits;

/// <summary>
///     Represents a skill limit filter for a pawn, based on a specific <see cref="SkillDef" />.
///     Stores a range for the allowed skill level and supports serialization.
/// </summary>
[PublicAPI]
public class PawnSkillLimit : DefCache<SkillDef>, IExposable
{
    internal const int LimitMaxCap = 20;
    internal const int LimitMinCap = 0;
    internal const int ValueStep = 1;

    /// <summary>
    ///     The allowed skill level range, clamped between <see cref="LimitMinCap" /> and <see cref="LimitMaxCap" />.
    /// </summary>
    public IntRange Limit = new(LimitMinCap, LimitMaxCap);

    private string? _maxValueBuffer;
    private string? _minValueBuffer;

    /// <summary>
    ///     Initializes a new instance of the <see cref="PawnSkillLimit" /> class.
    /// </summary>
    public PawnSkillLimit()
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="PawnSkillLimit" /> class for the skill with the specified def name.
    /// </summary>
    /// <param name="skillDefName">The def name of the <see cref="SkillDef" /> to limit.</param>
    public PawnSkillLimit(string skillDefName) : base(skillDefName)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="PawnSkillLimit" /> class for the skill with the specified def name
    ///     and value bounds.
    /// </summary>
    /// <param name="skillDefName">The def name of the <see cref="SkillDef" /> to limit.</param>
    /// <param name="minValue">The lower bound of the allowed range, or <c>null</c> for no lower bound.</param>
    /// <param name="maxValue">The upper bound of the allowed range, or <c>null</c> for no upper bound.</param>
    public PawnSkillLimit(string skillDefName, float? minValue, float? maxValue) : this(
        skillDefName)
    {
        MinValue = minValue;
        MaxValue = maxValue;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="PawnSkillLimit" /> class from the specified <see cref="SkillDef" />
    ///     instance.
    /// </summary>
    /// <param name="def">The skill definition to limit.</param>
    /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="def" /> is null.</exception>
    public PawnSkillLimit(SkillDef def) : base(GetDefName(def))
    {
    }

    /// <summary>
    ///     Gets or sets the upper bound of the allowed skill level, or <c>null</c> when no upper bound is set
    ///     (the value is at <see cref="LimitMaxCap" />). Setting clamps and rounds the value between
    ///     <see cref="LimitMinCap" /> and <see cref="LimitMaxCap" />.
    /// </summary>
    public float? MaxValue
    {
        get => string.IsNullOrEmpty(_maxValueBuffer) && Limit.max == LimitMaxCap ? null : Limit.max;
        set
        {
            if (!value.HasValue)
            {
                Limit.max = LimitMaxCap;
                _maxValueBuffer = string.Empty;
                return;
            }

            Limit.max = Mathf.RoundToInt(Mathf.Clamp(value.Value, LimitMinCap, LimitMaxCap));
            _maxValueBuffer = Limit.max.ToString(CultureInfo.InvariantCulture);
        }
    }

    /// <summary>
    ///     Gets or sets the UI text-input buffer for the upper bound. Parses valid numeric input into
    ///     <see cref="MaxValue" /> and otherwise retains the raw text.
    /// </summary>
    public string MaxValueBuffer
    {
        get => string.IsNullOrEmpty(_maxValueBuffer)
            ? MaxValue?.ToString(CultureInfo.InvariantCulture) ?? string.Empty
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
    ///     Gets or sets the lower bound of the allowed skill level, or <c>null</c> when no lower bound is set
    ///     (the value is at <see cref="LimitMinCap" />). Setting clamps and rounds the value between
    ///     <see cref="LimitMinCap" /> and <see cref="LimitMaxCap" />.
    /// </summary>
    public float? MinValue
    {
        get => string.IsNullOrEmpty(_minValueBuffer) && Limit.min == LimitMinCap ? null : Limit.min;
        set
        {
            if (!value.HasValue)
            {
                Limit.min = LimitMinCap;
                _minValueBuffer = string.Empty;
                return;
            }

            Limit.min = Mathf.RoundToInt(Mathf.Clamp(value.Value, LimitMinCap, LimitMaxCap));
            _minValueBuffer = Limit.min.ToString(CultureInfo.InvariantCulture);
        }
    }

    /// <summary>
    ///     Gets or sets the UI text-input buffer for the lower bound. Parses valid numeric input into
    ///     <see cref="MinValue" /> and otherwise retains the raw text.
    /// </summary>
    public string MinValueBuffer
    {
        get => string.IsNullOrEmpty(_minValueBuffer)
            ? MinValue?.ToString(CultureInfo.InvariantCulture) ?? string.Empty
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
    ///     Gets the resolved <see cref="SkillDef" /> for this limit, or <c>null</c> if it could not be resolved.
    /// </summary>
    public SkillDef? SkillDef => Def;

    /// <summary>
    ///     Gets the def name of the skill this limit targets, or <c>null</c> if none is set.
    /// </summary>
    public string? SkillDefName => DefName;

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