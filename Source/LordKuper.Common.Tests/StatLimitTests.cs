using LordKuper.Common.Filters.Limits;
using LordKuper.Common.Helpers;
using RimWorld;
using Verse;

namespace LordKuper.Common.Tests;

/// <summary>
///     Tests for <see cref="StatLimit" /> pure-logic paths: caps, clamping, buffers, and ctors.
///     Because <c>StatLimit(StatDef)</c> calls <c>Configure(def)</c> directly in the constructor,
///     it sets <c>_isConfigured = true</c> before any property is accessed and avoids the
///     recursive initialisation cycle that would occur via the parameterless constructor.
///     A hand-built <see cref="StatDef" /> with <c>minValue = -1000</c>, <c>maxValue = 1000</c>,
///     <c>toStringStyle = FloatTwo</c> replicates the <c>Configure(null)</c> defaults exactly.
/// </summary>
[NonParallelizable]
[SetCulture("en-US")]
public class StatLimitTests : StaticStateTestBase
{
    // ------------------------------------------------------------------
    // Helper: produces a StatDef that gives the same caps as Configure(null)
    // ------------------------------------------------------------------

    private static StatDef MakeStatDef(
        float min = -1000f,
        float max = 1000f,
        ToStringStyle style = ToStringStyle.FloatTwo)
    {
        return new StatDef
        {
            defName = "TestStatDef",
            label = "Test Stat",
            category = null,
            minValue = min,
            maxValue = max,
            toStringStyle = style
        };
    }

    // -------------------------------------------------------------------------
    // StatLimit(StatDef) constructor — initial state
    // -------------------------------------------------------------------------

    [Test]
    public void Ctor_StatDef_MinMaxReturnNull_WhenAtCaps()
    {
        // Fresh limit with caps ±1000 — Limit is initialised to {min=-1000, max=1000}
        // so both getters return null (both are at their respective caps).
        var limit = new StatLimit(MakeStatDef());
        limit.MinValue.Should().BeNull();
        limit.MaxValue.Should().BeNull();
    }

    [Test]
    public void Ctor_StatDef_CapsSetFromDef()
    {
        var limit = new StatLimit(MakeStatDef());
        limit.LimitMinCap.Should().BeApproximately(-1000f, 0.001f);
        limit.LimitMaxCap.Should().BeApproximately(1000f, 0.001f);
    }

    [Test]
    public void Ctor_StatDef_ValueStyle_IsFloatTwo()
    {
        var limit = new StatLimit(MakeStatDef());
        limit.ValueStyle.Should().Be(ToStringStyle.FloatTwo);
    }

    // -------------------------------------------------------------------------
    // MaxValue setter — in-range, clamping, null reset
    // -------------------------------------------------------------------------

    [Test]
    public void MaxValue_Set_InRange_Stored()
    {
        var limit = new StatLimit(MakeStatDef());
        limit.MaxValue = 500f;
        limit.MaxValue.Should().BeApproximately(500f, 0.001f);
    }

    [Test]
    public void MaxValue_Set_AboveCap_ClampsToMaxCap()
    {
        // Value above LimitMaxCap (1000) must clamp to 1000.
        var limit = new StatLimit(MakeStatDef());
        limit.MaxValue = 5000f;
        limit.MaxValue.Should().BeApproximately(1000f, 0.001f);
    }

    [Test]
    public void MaxValue_Set_BelowMinCap_ClampsToMinCap()
    {
        // Value below LimitMinCap (-1000) must clamp to -1000.
        var limit = new StatLimit(MakeStatDef());
        limit.MaxValue = -5000f;
        limit.MaxValue.Should().BeApproximately(-1000f, 0.001f);
    }

    [Test]
    public void MaxValue_SetNull_ResetsToCapAndReturnsNull()
    {
        var limit = new StatLimit(MakeStatDef());
        limit.MaxValue = 200f;
        limit.MaxValue.Should().NotBeNull();
        limit.MaxValue = null;
        limit.MaxValue.Should().BeNull();
    }

    // -------------------------------------------------------------------------
    // MinValue setter — in-range, clamping, null reset
    // -------------------------------------------------------------------------

    [Test]
    public void MinValue_Set_InRange_Stored()
    {
        var limit = new StatLimit(MakeStatDef());
        limit.MinValue = -300f;
        limit.MinValue.Should().BeApproximately(-300f, 0.001f);
    }

    [Test]
    public void MinValue_Set_AboveMaxCap_ClampsToMaxCap()
    {
        // Value above LimitMaxCap (1000) must clamp to 1000.
        var limit = new StatLimit(MakeStatDef());
        limit.MinValue = 5000f;
        limit.MinValue.Should().BeApproximately(1000f, 0.001f);
    }

    [Test]
    public void MinValue_Set_BelowMinCap_ClampsToMinCap()
    {
        // Value below LimitMinCap (-1000) must clamp to -1000.
        var limit = new StatLimit(MakeStatDef());
        limit.MinValue = -9999f;
        limit.MinValue.Should().BeApproximately(-1000f, 0.001f);
    }

    [Test]
    public void MinValue_SetNull_ResetsToCapAndReturnsNull()
    {
        var limit = new StatLimit(MakeStatDef());
        limit.MinValue = -100f;
        limit.MinValue.Should().NotBeNull();
        limit.MinValue = null;
        limit.MinValue.Should().BeNull();
    }

    // -------------------------------------------------------------------------
    // MaxValueBuffer
    // -------------------------------------------------------------------------

    [Test]
    public void MaxValueBuffer_SetEmptyString_ResetsMaxValueToNull()
    {
        var limit = new StatLimit(MakeStatDef());
        limit.MaxValue = 400f;
        limit.MaxValueBuffer = string.Empty;
        limit.MaxValue.Should().BeNull();
    }

    [Test]
    public void MaxValueBuffer_SetValidNumericString_ParsedIntoMaxValue()
    {
        var limit = new StatLimit(MakeStatDef());
        limit.MaxValueBuffer = "12.50";
        limit.MaxValue.Should().BeApproximately(12.5f, 0.001f);
    }

    [Test]
    public void MaxValueBuffer_SetInvalidString_RetainedVerbatim_MaxValueUnchanged()
    {
        // An invalid string must be kept verbatim; the underlying value must not change.
        var limit = new StatLimit(MakeStatDef());
        limit.MaxValue = 100f;
        limit.MaxValueBuffer = "abc";
        limit.MaxValueBuffer.Should().Be("abc");
        limit.MaxValue.Should().BeApproximately(100f, 0.001f);
    }

    [Test]
    public void MaxValueBuffer_Getter_WhenValueSet_ReturnsFormattedFloat()
    {
        // After MaxValue setter, _maxValueBuffer is set to the F2 string representation.
        var limit = new StatLimit(MakeStatDef());
        limit.MaxValue = 75f;
        limit.MaxValueBuffer.Should().Be("75.00");
    }

    // -------------------------------------------------------------------------
    // MinValueBuffer
    // -------------------------------------------------------------------------

    [Test]
    public void MinValueBuffer_SetEmptyString_ResetsMinValueToNull()
    {
        var limit = new StatLimit(MakeStatDef());
        limit.MinValue = -200f;
        limit.MinValueBuffer = string.Empty;
        limit.MinValue.Should().BeNull();
    }

    [Test]
    public void MinValueBuffer_SetValidNumericString_ParsedIntoMinValue()
    {
        var limit = new StatLimit(MakeStatDef());
        limit.MinValueBuffer = "-50.25";
        limit.MinValue.Should().BeApproximately(-50.25f, 0.001f);
    }

    [Test]
    public void MinValueBuffer_SetInvalidString_RetainedVerbatim_MinValueUnchanged()
    {
        var limit = new StatLimit(MakeStatDef());
        limit.MinValue = -100f;
        limit.MinValueBuffer = "xyz";
        limit.MinValueBuffer.Should().Be("xyz");
        limit.MinValue.Should().BeApproximately(-100f, 0.001f);
    }

    [Test]
    public void MinValueBuffer_Getter_WhenValueSet_ReturnsFormattedFloat()
    {
        var limit = new StatLimit(MakeStatDef());
        limit.MinValue = -33f;
        limit.MinValueBuffer.Should().Be("-33.00");
    }

    // -------------------------------------------------------------------------
    // Round-trip / independence checks
    // -------------------------------------------------------------------------

    [Test]
    public void MinMaxValues_Independent_DoNotAffectEachOther()
    {
        var limit = new StatLimit(MakeStatDef());
        limit.MinValue = -50f;
        limit.MaxValue = 50f;
        limit.MinValue.Should().BeApproximately(-50f, 0.001f);
        limit.MaxValue.Should().BeApproximately(50f, 0.001f);

        limit.MinValue = -80f;
        limit.MaxValue.Should().BeApproximately(50f, 0.001f);

        limit.MaxValue = 80f;
        limit.MinValue.Should().BeApproximately(-80f, 0.001f);
    }

    [Test]
    public void TwoInstances_HaveIndependentState()
    {
        var a = new StatLimit(MakeStatDef());
        var b = new StatLimit(MakeStatDef());
        a.MaxValue = 100f;
        b.MaxValue = 200f;
        a.MaxValue.Should().BeApproximately(100f, 0.001f);
        b.MaxValue.Should().BeApproximately(200f, 0.001f);
    }

    [Test]
    public void MaxValueBuffer_SetNumericString_OverridesPreviousValue()
    {
        // Buffer assignment via valid numeric string replaces the previously set value.
        var limit = new StatLimit(MakeStatDef());
        limit.MaxValue = 500f;
        limit.MaxValueBuffer = "123.45";
        limit.MaxValue.Should().BeApproximately(123.45f, 0.001f);
        limit.MaxValueBuffer.Should().Be("123.45");
    }

    [Test]
    public void CustomCaps_ClampingRespectsDef()
    {
        // A StatDef with narrower caps (0–100) should clamp values accordingly.
        var narrowDef = MakeStatDef(0f, 100f);
        var limit = new StatLimit(narrowDef);
        limit.MaxValue = 500f;
        limit.MaxValue.Should().BeApproximately(100f, 0.001f);
        limit.MinValue = -50f;
        limit.MinValue.Should().BeApproximately(0f, 0.001f);
    }

    [Test]
    public void NullResetAfterSet_BuffersAreCleared()
    {
        // After resetting via null, the buffer should be empty and getter should return null.
        var limit = new StatLimit(MakeStatDef());
        limit.MaxValue = 300f;
        limit.MinValue = -300f;
        limit.MaxValue = null;
        limit.MinValue = null;
        limit.MaxValue.Should().BeNull();
        limit.MinValue.Should().BeNull();
        // Buffers should also be empty after null reset.
        limit.MaxValueBuffer.Should().BeEmpty();
        limit.MinValueBuffer.Should().BeEmpty();
    }

    // -------------------------------------------------------------------------
    // Regression: EnsureConfigured re-entrancy guard (parameterless / string ctors)
    // Previously these paths triggered infinite recursion via
    //   property → EnsureConfigured → Configure(Def) → Def → Initialize
    //   → EnsureConfigured (again, _isConfigured still false) → StackOverflow.
    // The parameterless ctor leaves _defName null so Def resolves to null and Configure(null)
    // runs, producing caps ±1000. The string ctor with an unresolvable name also resolves Def
    // to null (StatHelper.GetStatDef returns null) and follows the same Configure(null) path.
    // Note: caps are populated lazily (the first property access triggers EnsureConfigured), so
    // LimitMinCap/LimitMaxCap assertions must follow at least one property access.
    // Note: string-ctor tests install a FakeDefProvider and call StatHelper.Rebuild() so that
    // the _statDefsByName dictionary (nulled by TearDownStaticState in prior tests) is restored
    // to a clean empty state before GetStatDef("SomeStat") is called.
    // -------------------------------------------------------------------------

    [Test]
    public void Ctor_Parameterless_MinValue_DoesNotStackOverflow_AndReturnsNull()
    {
        // Configure(null) path: no StatDef resolved; caps default to ±1000.
        // MinValue at LimitMinCap (-1000) with empty buffer → null.
        var limit = new StatLimit();
        limit.MinValue.Should().BeNull();
    }

    [Test]
    public void Ctor_Parameterless_MaxValue_DoesNotStackOverflow_AndReturnsNull()
    {
        var limit = new StatLimit();
        limit.MaxValue.Should().BeNull();
    }

    [Test]
    public void Ctor_Parameterless_MinValueBuffer_DoesNotStackOverflow_AndReturnsEmpty()
    {
        var limit = new StatLimit();
        limit.MinValueBuffer.Should().BeEmpty();
    }

    [Test]
    public void Ctor_Parameterless_MaxValueBuffer_DoesNotStackOverflow_AndReturnsEmpty()
    {
        var limit = new StatLimit();
        limit.MaxValueBuffer.Should().BeEmpty();
    }

    [Test]
    public void Ctor_Parameterless_CapsDefaultToConfigureNull()
    {
        // Caps are populated lazily on first property access; access MinValue first to trigger
        // EnsureConfigured, then assert the public cap fields reflect Configure(null) defaults.
        var limit = new StatLimit();
        _ = limit.MinValue; // trigger EnsureConfigured → Configure(null)
        limit.LimitMinCap.Should().BeApproximately(-1000f, 0.001f);
        limit.LimitMaxCap.Should().BeApproximately(1000f, 0.001f);
    }

    [Test]
    public void Ctor_String_MinValue_DoesNotStackOverflow_AndReturnsNull()
    {
        // "SomeStat" is not registered → Def resolves to null → Configure(null) → caps ±1000.
        // FakeDefProvider + Rebuild ensures _statDefsByName is non-null (restored from teardown).
        DefProvider.Current = new FakeDefProvider();
        StatHelper.Rebuild();
        var limit = new StatLimit("SomeStat");
        limit.MinValue.Should().BeNull();
    }

    [Test]
    public void Ctor_String_MaxValue_DoesNotStackOverflow_AndReturnsNull()
    {
        DefProvider.Current = new FakeDefProvider();
        StatHelper.Rebuild();
        var limit = new StatLimit("SomeStat");
        limit.MaxValue.Should().BeNull();
    }

    [Test]
    public void Ctor_String_MinValueBuffer_DoesNotStackOverflow_AndReturnsEmpty()
    {
        DefProvider.Current = new FakeDefProvider();
        StatHelper.Rebuild();
        var limit = new StatLimit("SomeStat");
        limit.MinValueBuffer.Should().BeEmpty();
    }

    [Test]
    public void Ctor_String_MaxValueBuffer_DoesNotStackOverflow_AndReturnsEmpty()
    {
        DefProvider.Current = new FakeDefProvider();
        StatHelper.Rebuild();
        var limit = new StatLimit("SomeStat");
        limit.MaxValueBuffer.Should().BeEmpty();
    }

    [Test]
    public void Ctor_String_CapsDefaultToConfigureNull()
    {
        // Caps are populated lazily; access MinValue first to trigger EnsureConfigured.
        DefProvider.Current = new FakeDefProvider();
        StatHelper.Rebuild();
        var limit = new StatLimit("SomeStat");
        _ = limit.MinValue; // trigger EnsureConfigured → Configure(null)
        limit.LimitMinCap.Should().BeApproximately(-1000f, 0.001f);
        limit.LimitMaxCap.Should().BeApproximately(1000f, 0.001f);
    }
}
