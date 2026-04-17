using System;
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
[UsedImplicitly]
public class PawnCapacityLimit : DefCache<PawnCapacityDef>, IExposable
{
    internal const float LimitMaxCap = 5f;
    internal const float LimitMinCap = 0f;
    internal const ToStringStyle ValueStyle = ToStringStyle.PercentZero;
    private string _maxValueBuffer;
    private string _minValueBuffer;
    private float _valueStep;
    public FloatRange Limit = new(LimitMinCap, LimitMaxCap);

    [UsedImplicitly]
    public PawnCapacityLimit() { }

    [UsedImplicitly]
    public PawnCapacityLimit([NotNull] string pawnCapacityDefName) : base(pawnCapacityDefName) { }

    [UsedImplicitly]
    public PawnCapacityLimit([NotNull] string pawnCapacityDefName, float? minValue, float? maxValue)
        : this(pawnCapacityDefName)
    {
        MinValue = minValue;
        MaxValue = maxValue;
    }

    public PawnCapacityLimit([NotNull] PawnCapacityDef def) : base(def.defName)
    {
        if (def == null) throw new ArgumentNullException(nameof(def));
    }

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

    [NotNull]
    public string MaxValueBuffer
    {
        get => string.IsNullOrEmpty(_maxValueBuffer)
            ? MaxValue?.ToString("F2", CultureInfo.InvariantCulture) ?? string.Empty
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

    [NotNull]
    public string MinValueBuffer
    {
        get => string.IsNullOrEmpty(_minValueBuffer)
            ? MinValue?.ToString("F2", CultureInfo.InvariantCulture) ?? string.Empty
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

    public PawnCapacityDef PawnCapacityDef => Def;
    public string PawnCapacityDefName => DefName;

    internal float ValueStep
    {
        get
        {
            if (_valueStep == 0f) _valueStep = Fields.GetFloatSliderStepByValueStyle(ValueStyle);
            return _valueStep;
        }
    }

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