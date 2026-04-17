using System;
using System.Globalization;
using JetBrains.Annotations;
using LordKuper.Common.Cache;
using LordKuper.Common.UI;
using RimWorld;
using UnityEngine;
using Verse;

namespace LordKuper.Common.Filters.Limits;

/// <summary>
///     Represents a limit for a stat, including minimum and maximum values and their buffers for UI input.
/// </summary>
[UsedImplicitly]
public class StatLimit : DefCache<StatDef>, IExposable
{
    private const float DefaultLimitCap = 1000f;
    private const float PercentStatCap = 5f;
    private bool _isConfigured;
    private string _maxValueBuffer;
    private string _minValueBuffer;
    private float _valueStep;
    public FloatRange Limit;
    public float LimitMaxCap;
    public float LimitMinCap;
    public ToStringStyle ValueStyle;

    [UsedImplicitly]
    public StatLimit() { }

    [UsedImplicitly]
    public StatLimit([NotNull] string statDefName) : base(statDefName) { }

    [UsedImplicitly]
    public StatLimit([NotNull] string statDefName, float? minValue, float? maxValue) : this(
        statDefName)
    {
        MinValue = minValue;
        MaxValue = maxValue;
    }

    public StatLimit([NotNull] StatDef def) : base(def.defName)
    {
        if (def == null) throw new ArgumentNullException(nameof(def));
        Configure(def);
    }

    public float? MaxValue
    {
        get
        {
            EnsureConfigured();
            return string.IsNullOrEmpty(_maxValueBuffer) &&
                   Mathf.Approximately(Limit.max, LimitMaxCap)
                ? null
                : Limit.max;
        }
        set
        {
            EnsureConfigured();
            if (!value.HasValue)
            {
                Limit.max = LimitMaxCap;
                _maxValueBuffer = string.Empty;
                return;
            }
            var clamped = Mathf.Clamp(value.Value, LimitMinCap, LimitMaxCap);
            Limit.max = clamped;
            _maxValueBuffer = clamped.ToString("F2", CultureInfo.InvariantCulture);
        }
    }

    [UsedImplicitly]
    [NotNull]
    public string MaxValueBuffer
    {
        get
        {
            EnsureConfigured();
            return string.IsNullOrEmpty(_maxValueBuffer)
                ? MaxValue?.ToString("F2", CultureInfo.InvariantCulture) ?? string.Empty
                : _maxValueBuffer;
        }
        set
        {
            if (ReferenceEquals(value, _maxValueBuffer) || value == _maxValueBuffer) return;
            EnsureConfigured();
            if (string.IsNullOrEmpty(value))
            {
                MaxValue = null;
                return;
            }
            if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture,
                    out var maxValue))
                MaxValue = maxValue;
            else
                _maxValueBuffer = value;
        }
    }

    public float? MinValue
    {
        get
        {
            EnsureConfigured();
            return string.IsNullOrEmpty(_minValueBuffer) &&
                   Mathf.Approximately(Limit.min, LimitMinCap)
                ? null
                : Limit.min;
        }
        set
        {
            EnsureConfigured();
            if (!value.HasValue)
            {
                Limit.min = LimitMinCap;
                _minValueBuffer = string.Empty;
                return;
            }
            var clamped = Mathf.Clamp(value.Value, LimitMinCap, LimitMaxCap);
            Limit.min = clamped;
            _minValueBuffer = clamped.ToString("F2", CultureInfo.InvariantCulture);
        }
    }

    [UsedImplicitly]
    [NotNull]
    public string MinValueBuffer
    {
        get
        {
            EnsureConfigured();
            return string.IsNullOrEmpty(_minValueBuffer)
                ? MinValue?.ToString("F2", CultureInfo.InvariantCulture) ?? string.Empty
                : _minValueBuffer;
        }
        set
        {
            if (ReferenceEquals(value, _minValueBuffer) || value == _minValueBuffer) return;
            EnsureConfigured();
            if (string.IsNullOrEmpty(value))
            {
                MinValue = null;
                return;
            }
            if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture,
                    out var minValue))
                MinValue = minValue;
            else
                _minValueBuffer = value;
        }
    }

    public StatDef StatDef => Def;
    public string StatDefName => DefName;

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
        Scribe_Values.Look(ref LimitMinCap, nameof(LimitMinCap));
        Scribe_Values.Look(ref LimitMaxCap, nameof(LimitMaxCap));
        Scribe_Values.Look(ref ValueStyle, nameof(ValueStyle));
        if (Scribe.mode != LoadSaveMode.Saving)
        {
            _isConfigured = LimitMinCap != 0f || LimitMaxCap != 0f || Limit.min != 0f ||
                            Limit.max != 0f;
            MinValue = minValue;
            MaxValue = maxValue;
        }
    }

    private void Configure([CanBeNull] StatDef def)
    {
        if (def == null)
        {
            LimitMinCap = -1f * DefaultLimitCap;
            LimitMaxCap = DefaultLimitCap;
            ValueStyle = ToStringStyle.FloatTwo;
        }
        else
        {
            var style = def.toStringStyle;
            if (style is ToStringStyle.PercentZero or ToStringStyle.PercentOne
                or ToStringStyle.PercentTwo)
            {
                LimitMinCap = Mathf.Max(-1 * PercentStatCap, def.minValue);
                LimitMaxCap = Mathf.Min(PercentStatCap, def.maxValue);
                ValueStyle = style;
            }
            else
            {
                LimitMinCap = def.minValue;
                LimitMaxCap = def.maxValue;
                ValueStyle = ToStringStyle.FloatTwo;
            }
        }
        if (Mathf.Approximately(Limit.min, 0f) && Mathf.Approximately(Limit.max, 0f))
            Limit = new FloatRange(LimitMinCap, LimitMaxCap);
        _isConfigured = true;
    }

    private void EnsureConfigured()
    {
        if (_isConfigured) return;
        Configure(Def);
    }

    protected override void Initialize()
    {
        base.Initialize();
        EnsureConfigured();
    }
}