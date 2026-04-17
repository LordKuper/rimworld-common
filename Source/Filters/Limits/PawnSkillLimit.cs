using System;
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
[UsedImplicitly]
public class PawnSkillLimit : DefCache<SkillDef>, IExposable
{
    internal const int LimitMaxCap = 20;
    internal const int LimitMinCap = 0;
    internal const int ValueStep = 1;
    private string _maxValueBuffer;
    private string _minValueBuffer;
    public IntRange Limit = new(LimitMinCap, LimitMaxCap);

    [UsedImplicitly]
    public PawnSkillLimit() { }

    [UsedImplicitly]
    public PawnSkillLimit([NotNull] string skillDefName) : base(skillDefName) { }

    [UsedImplicitly]
    public PawnSkillLimit([NotNull] string skillDefName, float? minValue, float? maxValue) : this(
        skillDefName)
    {
        MinValue = minValue;
        MaxValue = maxValue;
    }

    public PawnSkillLimit([NotNull] SkillDef def) : base(def.defName)
    {
        if (def == null) throw new ArgumentNullException(nameof(def));
    }

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

    [NotNull]
    public string MaxValueBuffer
    {
        get => string.IsNullOrEmpty(_maxValueBuffer)
            ? MaxValue?.ToString(CultureInfo.InvariantCulture) ?? string.Empty
            : _maxValueBuffer;
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

    [NotNull]
    public string MinValueBuffer
    {
        get => string.IsNullOrEmpty(_minValueBuffer)
            ? MinValue?.ToString(CultureInfo.InvariantCulture) ?? string.Empty
            : _minValueBuffer;
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

    public SkillDef SkillDef => Def;
    public string SkillDefName => DefName;

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