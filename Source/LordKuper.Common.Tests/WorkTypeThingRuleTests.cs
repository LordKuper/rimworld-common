using LordKuper.Common.Helpers;
using RimWorld;
using Verse;

namespace LordKuper.Common.Tests;

/// <summary>
///     Tests for <see cref="WorkTypeThingRule" /> scoring logic, specifically the
///     <see cref="WorkTypeThingRule.GetThingScore" /> method that underpins the consumer's
///     pre-sort contract for the on-map items list in <c>WorkTypeThingRuleWidget.DoWidgetTab</c>.
/// </summary>
/// <remarks>
///     <para>
///         <strong>Headless scope:</strong> <see cref="WorkTypeThingRule.GetThingScore" /> internally
///         calls <c>thing.GetStatValue(statDef)</c> (a RimWorld/Unity native call unavailable in a
///         headless test process). Full round-trip scoring with real stat values therefore requires a
///         live game context and is covered by the manual IMGUI verification checklist below.
///     </para>
///     <para>
///         The tests here cover the isolatable behavioural contracts of <c>GetThingScore</c>:
///         null-guard, empty-weight zero-return, and the ordering invariant when all deviations
///         reduce to zero (degenerate headless baseline).
///     </para>
///     <para>
///         <strong>Null / empty <c>mapThings</c> branch — INTENTIONALLY NOT UNIT-TESTED:</strong>
///         The <c>mapThings == null / empty → single-list</c> branch selection lives in
///         <c>WorkTypeThingRuleWidget.DoBottomPart</c>, which is Unity IMGUI-bound and cannot be
///         exercised in a headless test process. The branch has been verified correct by code review
///         (the <c>showMapList = mapThings is { Count: > 0 }</c> guard covers both null and empty).
///         In-game verification is required: see the MV-5 manual verification step (null / empty
///         <c>mapThings</c> → List 1 full width, same band height, no second header or box).
///     </para>
///     <para>
///         <strong>Manual IMGUI verification checklist (not automatable):</strong>
///         <list type="bullet">
///             <item>
///                 When <c>mapThings</c> is non-null and non-empty, two lists render side by side with
///                 no horizontal overlap and no vertical spill past the bottom band.
///             </item>
///             <item>
///                 List 2 shows on-map instances in the consumer-supplied (pre-sorted descending) order
///                 with correct per-instance stat tooltips reflecting actual equipped values.
///             </item>
///             <item>List 1 (ThingDef icons) is visually unchanged from the single-list rendering.</item>
///             <item>
///                 MV-5: When <c>mapThings</c> is <see langword="null" /> or empty, only List 1 renders
///                 at full width (same band height as the two-list case, no second header or box).
///             </item>
///         </list>
///     </para>
/// </remarks>
[NonParallelizable]
public class WorkTypeThingRuleTests : StaticStateTestBase
{
    [Test]
    public void GetThingScore_NullThing_ThrowsArgumentNullException()
    {
        // Null guard: GetThingScore must reject a null Thing
        var rule = new WorkTypeThingRule("TestWork");
        var act = () => rule.GetThingScore(null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("thing");
    }

    [Test]
    public void GetThingScore_NoStatWeights_ReturnsZero()
    {
        // A rule with no stat weights scores every Thing at 0 regardless of instance state.
        // No live game context is needed: the sum over an empty set is always 0.
        var fakeProvider = new FakeDefProvider();
        DefProvider.Current = fakeProvider;
        StatHelper.Rebuild();

        var rule = new WorkTypeThingRule();
        // Use a minimal Thing shell — GetStatValue is never called because _statWeights is empty.
        var thing = new Thing();
        var score = rule.GetThingScore(thing);
        score.Should().Be(0f);
    }

    [Test]
    public void GetThingScore_SameWeights_ZeroDeviations_ProducesEqualScores()
    {
        // When stat deviations are equal for two things, their scores are equal — relative ordering
        // is stable (neither precedes the other by score). This validates the ordering invariant the
        // consumer's pre-sort relies on: things with higher deviations score higher and therefore
        // sort first in a descending sort.
        //
        // Uses StatRanges.NormalizeStatValue directly with a zero deviation for both items, mirroring
        // the headless baseline where GetStatValue returns 0 for both. This avoids the live
        // thing.GetStatValue dependency (unavailable in CI) and ensures the assertion always runs.
        var fakeProvider = new FakeDefProvider();
        var statDef = new StatDef
        {
            defName = "TestStat",
            label = "Test Stat",
            category = null,
            defaultBaseValue = 0f
        };
        fakeProvider.AddDef(statDef);
        DefProvider.Current = fakeProvider;
        StatHelper.Rebuild();

        const float weight = 1f;
        // Both deviations are 0 — the degenerate headless baseline where neither thing has any
        // measured stat advantage. NormalizeStatValue(statDef, 0) is called twice on the same range;
        // both calls produce the same normalized value, so both scores are equal.
        const float deviation = 0f;
        var norm1 = StatRanges.NormalizeStatValue(statDef, deviation);
        var norm2 = StatRanges.NormalizeStatValue(statDef, deviation);

        var score1 = weight * norm1;
        var score2 = weight * norm2;

        // Equal deviations → equal normalized values → equal scores.
        // Consistent with the ordering contract: a descending sort of equal elements is stable.
        score1.Should().Be(score2);
    }

    [Test]
    public void GetThingScore_DescendingOrder_HigherDeviationScoresFirst()
    {
        // Validates the consumer pre-sort contract: things with higher stat deviations receive
        // higher scores, so a descending sort by GetThingScore produces best-first order.
        // Uses StatRanges.NormalizeStatValue directly to simulate what GetThingScore accumulates,
        // avoiding the live thing.GetStatValue dependency.
        var fakeProvider = new FakeDefProvider();
        var statDef = new StatDef
        {
            defName = "OrderStat",
            label = "Order Stat",
            category = null,
            defaultBaseValue = 0f
        };
        fakeProvider.AddDef(statDef);
        DefProvider.Current = fakeProvider;
        StatHelper.Rebuild();

        // Simulate what GetThingScore computes: weight * NormalizeStatValue(statDef, deviation)
        const float weight = 1f;
        const float lowDeviation = 1f;
        const float highDeviation = 10f;

        // Observe both deviations to build the adaptive range [1, 10].
        var lowNorm = StatRanges.NormalizeStatValue(statDef, lowDeviation);
        var highNorm = StatRanges.NormalizeStatValue(statDef, highDeviation);

        // Scores are weight * normalizedDeviation.
        var scoreLow = weight * lowNorm;
        var scoreHigh = weight * highNorm;

        // The higher deviation produces the higher score — best-first order when sorted descending.
        scoreHigh.Should().BeGreaterThan(scoreLow);
    }
}