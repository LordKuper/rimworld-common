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
[PublicAPI]
public class StatLimit : DefCache<StatDef>, IExposable
{
    private const float DefaultLimitCap = 1000f;
    private const float PercentStatCap = 5f;

    /// <summary>
    ///     The allowed value range for the stat, clamped between <see cref="LimitMinCap" /> and <see cref="LimitMaxCap" />.
    /// </summary>
    public FloatRange Limit;

    /// <summary>
    ///     The upper bound that <see cref="Limit" /> may reach, derived from the stat definition during configuration.
    /// </summary>
    public float LimitMaxCap;

    /// <summary>
    ///     The lower bound that <see cref="Limit" /> may reach, derived from the stat definition during configuration.
    /// </summary>
    public float LimitMinCap;

    /// <summary>
    ///     The display style used to format the stat's values.
    /// </summary>
    public ToStringStyle ValueStyle;

    private bool _isConfigured;
    private string? _maxValueBuffer;
    private string? _minValueBuffer;

    /// <summary>
    ///     Initializes a new instance of the <see cref="StatLimit" /> class.
    /// </summary>
    public StatLimit() { }

    /// <summary>
    ///     Initializes a new instance of the <see cref="StatLimit" /> class for the stat with the specified def name.
    /// </summary>
    /// <param name="statDefName">The def name of the <see cref="StatDef" /> to limit.</param>
    public StatLimit(string statDefName) : base(statDefName) { }

    /// <summary>
    ///     Initializes a new instance of the <see cref="StatLimit" /> class for the stat with the specified def name and
    ///     value bounds.
    /// </summary>
    /// <param name="statDefName">The def name of the <see cref="StatDef" /> to limit.</param>
    /// <param name="minValue">The lower bound of the allowed range, or <c>null</c> for no lower bound.</param>
    /// <param name="maxValue">The upper bound of the allowed range, or <c>null</c> for no upper bound.</param>
    public StatLimit(string statDefName, float? minValue, float? maxValue) : this(statDefName)
    {
        MinValue = minValue;
        MaxValue = maxValue;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="StatLimit" /> class from the specified <see cref="StatDef" />
    ///     instance, configuring caps and value style from the definition.
    /// </summary>
    /// <param name="def">The stat definition to limit.</param>
    /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="def" /> is null.</exception>
    public StatLimit(StatDef def) : base(GetDefName(def))
    {
        Configure(def);
    }

    /// <summary>
    ///     Gets or sets the upper bound of the allowed range, or <c>null</c> when no upper bound is set
    ///     (the value is at <see cref="LimitMaxCap" />). Setting clamps the value between
    ///     <see cref="LimitMinCap" /> and <see cref="LimitMaxCap" />.
    /// </summary>
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

    /// <summary>
    ///     Gets or sets the UI text-input buffer for the upper bound. Parses valid numeric input into
    ///     <see cref="MaxValue" /> and otherwise retains the raw text.
    /// </summary>
    public string MaxValueBuffer
    {
        get
        {
            EnsureConfigured();
            return string.IsNullOrEmpty(_maxValueBuffer)
                ? MaxValue?.ToString("F2", CultureInfo.InvariantCulture) ?? string.Empty
                : _maxValueBuffer!; // non-null: IsNullOrEmpty was false, so _maxValueBuffer is a non-empty string
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

    /// <summary>
    ///     Gets or sets the lower bound of the allowed range, or <c>null</c> when no lower bound is set
    ///     (the value is at <see cref="LimitMinCap" />). Setting clamps the value between
    ///     <see cref="LimitMinCap" /> and <see cref="LimitMaxCap" />.
    /// </summary>
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

    /// <summary>
    ///     Gets or sets the UI text-input buffer for the lower bound. Parses valid numeric input into
    ///     <see cref="MinValue" /> and otherwise retains the raw text.
    /// </summary>
    public string MinValueBuffer
    {
        get
        {
            EnsureConfigured();
            return string.IsNullOrEmpty(_minValueBuffer)
                ? MinValue?.ToString("F2", CultureInfo.InvariantCulture) ?? string.Empty
                : _minValueBuffer!; // non-null: IsNullOrEmpty was false, so _minValueBuffer is a non-empty string
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

    /// <summary>
    ///     Gets the resolved <see cref="StatDef" /> for this limit, or <c>null</c> if it could not be resolved.
    /// </summary>
    public StatDef? StatDef => Def;

    /// <summary>
    ///     Gets the def name of the stat this limit targets, or <c>null</c> if none is set.
    /// </summary>
    public string? StatDefName => DefName;

    internal float ValueStep
    {
        get
        {
            if (field == 0f) field = Fields.GetFloatSliderStepByValueStyle(ValueStyle);
            return field;
        }
    }

    /// <summary>
    ///     Serializes the limit's state (def name, value range, caps, and value style) for saving and loading.
    /// </summary>
    public new void ExposeData()
    {
        base.ExposeData();
        float? minValue = null;
        float? maxValue = null;
        if (Scribe.mode == LoadSaveMode.Saving)
        {
            minValue = MinValue;
            maxValue = MaxValue;
        }
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

    private void Configure(StatDef? def)
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

    /// <summary>
    ///     Initializes the limit after its def is resolved, ensuring caps and value style are configured.
    /// </summary>
    protected override void Initialize()
    {
        base.Initialize();
        EnsureConfigured();
    }
}