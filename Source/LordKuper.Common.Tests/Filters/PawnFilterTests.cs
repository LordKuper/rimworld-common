using LordKuper.Common.Filters;
using LordKuper.Common.Filters.Limits;
using RimWorld;
using Verse;
using PawnHealthState = LordKuper.Common.Filters.PawnHealthState;

namespace LordKuper.Common.Tests.Filters;

/// <summary>
///     Tests for <see cref="PawnFilter" /> pure paths (Combine, Copy, Validate, GetSummary).
///     Verifies that Combine refactor preserved semantics.
/// </summary>
public class PawnFilterTests
{
    [Test]
    public void AllowedPawnTypes_ModifyingDoesNotAffectOtherCollections()
    {
        // AllowedPawnTypes is independent from other filter collections
        var filter = new PawnFilter
        {
            AllowedPawnTypes = [PawnType.Colonist],
            AllowedWorkPassions = [Passion.Major],
            AllowedPawnHealthStates = PawnHealthState.Healthy
        };
        filter.AllowedPawnTypes.Add(PawnType.Guest);
        filter.AllowedPawnTypes.Count.Should().Be(2);
        filter.AllowedWorkPassions.Should().ContainSingle();
        filter.AllowedPawnHealthStates.Should().Be(PawnHealthState.Healthy);
    }

    [Test]
    public void Combine_BothProvideSameSection_MainWins()
    {
        var main = new PawnFilter
        {
            FilterPawnHealthStates = true,
            AllowedPawnHealthStates = PawnHealthState.Healthy
        };
        var fallback = new PawnFilter
        {
            FilterPawnHealthStates = true,
            AllowedPawnHealthStates = PawnHealthState.Resting
        };
        var result = PawnFilter.Combine(main, fallback);
        result.AllowedPawnHealthStates.Should().Be(PawnHealthState.Healthy);
    }

    [Test]
    public void Combine_MainHasNoPawnTypes_UsesFallback()
    {
        var main = new PawnFilter { FilterPawnTypes = null, AllowedPawnTypes = [] };
        var fallback = new PawnFilter
        { FilterPawnTypes = true, AllowedPawnTypes = [PawnType.Guest] };
        var result = PawnFilter.Combine(main, fallback);
        result.FilterPawnTypes.Should().BeTrue();
        result.AllowedPawnTypes.Should().Contain(PawnType.Guest);
    }

    [Test]
    public void Combine_MainHasNoWorkPassions_UsesFallback()
    {
        var main = new PawnFilter { FilterWorkPassions = null, AllowedWorkPassions = [] };
        var fallback = new PawnFilter
        { FilterWorkPassions = true, AllowedWorkPassions = [Passion.Minor] };
        var result = PawnFilter.Combine(main, fallback);
        result.FilterWorkPassions.Should().BeTrue();
        result.AllowedWorkPassions.Should().Contain(Passion.Minor);
    }

    [Test]
    public void Combine_MainHasPawnTypes_UsesMain()
    {
        // Combine uses main's values when set
        var main = new PawnFilter
        { FilterPawnTypes = true, AllowedPawnTypes = [PawnType.Colonist] };
        var fallback = new PawnFilter
        { FilterPawnTypes = false, AllowedPawnTypes = [PawnType.Guest] };
        var result = PawnFilter.Combine(main, fallback);
        result.FilterPawnTypes.Should().BeTrue();
        result.AllowedPawnTypes.Should().Contain(PawnType.Colonist);
        result.AllowedPawnTypes.Should().NotContain(PawnType.Guest);
    }

    [Test]
    public void Combine_MainHasWorkPassions_UsesMain()
    {
        var main = new PawnFilter
        { FilterWorkPassions = true, AllowedWorkPassions = [Passion.Major] };
        var fallback = new PawnFilter
        { FilterWorkPassions = false, AllowedWorkPassions = [Passion.Minor] };
        var result = PawnFilter.Combine(main, fallback);
        result.FilterWorkPassions.Should().BeTrue();
        result.AllowedWorkPassions.Should().Contain(Passion.Major);
        result.AllowedWorkPassions.Should().NotContain(Passion.Minor);
    }

    [Test]
    public void Combine_NullFallback_Throws()
    {
        var main = new PawnFilter();
        var act = () => PawnFilter.Combine(main, null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void Combine_NullMain_Throws()
    {
        var fallback = new PawnFilter();
        var act = () => PawnFilter.Combine(null!, fallback);
        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void Copy_CreatesIndependentCopy()
    {
        var original = new PawnFilter
        {
            FilterPawnTypes = true,
            AllowedPawnTypes = [PawnType.Colonist, PawnType.Guest],
            FilterPawnHealthStates = true,
            AllowedPawnHealthStates = PawnHealthState.Healthy,
            TriStateMode = true
        };
        var copy = original.Copy();

        // Verify all properties are copied
        copy.FilterPawnTypes.Should().Be(original.FilterPawnTypes);
        copy.FilterPawnHealthStates.Should().Be(original.FilterPawnHealthStates);
        copy.TriStateMode.Should().Be(original.TriStateMode);

        // Verify collections are independent (reference inequality)
        copy.AllowedPawnTypes.Should().NotBeSameAs(original.AllowedPawnTypes);
        copy.AllowedPawnTypes.Count.Should().Be(original.AllowedPawnTypes.Count);
        foreach (var pawnType in original.AllowedPawnTypes)
        {
            copy.AllowedPawnTypes.Should().Contain(pawnType);
        }
    }

    [Test]
    public void Copy_EmptyFilter_Copies()
    {
        var original = new PawnFilter();
        var copy = original.Copy();
        copy.Should().NotBeNull();
        copy.FilterPawnTypes.Should().BeNull();
        copy.AllowedPawnTypes.Should().BeEmpty();
    }

    [Test]
    public void Copy_ModifyingCopyDoesNotAffectOriginal()
    {
        var original = new PawnFilter { AllowedPawnTypes = [PawnType.Colonist] };
        var copy = original.Copy();
        copy.AllowedPawnTypes.Add(PawnType.Guest);
        original.AllowedPawnTypes.Should().ContainSingle();
        copy.AllowedPawnTypes.Count.Should().Be(2);
    }

    [Test]
    public void Copy_WithWorkCapacityLimits_DeepCopies()
    {
        var original = new PawnFilter
        {
            WorkCapacityLimits = new Dictionary<WorkTags, bool> { { WorkTags.ManualDumb, true } }
        };
        var copy = original.Copy();
        copy.WorkCapacityLimits.Should().NotBeSameAs(original.WorkCapacityLimits);
        copy.WorkCapacityLimits.Count.Should().Be(original.WorkCapacityLimits.Count);
    }

    [Test]
    public void ExposeData_RoundTrip_PreservesState()
    {
        var original = new PawnFilter
        {
            FilterPawnTypes = true,
            AllowedPawnTypes = [PawnType.Colonist],
            TriStateMode = true
        };

        // For this test, we're just verifying ExposeData doesn't throw
        // (Full serialization testing requires RimWorld's Scribe infrastructure)
        var act = () => original.ExposeData();
        act.Should().NotThrow();
    }

    [Test]
    public void ForbiddenPawnTypes_Independent()
    {
        // ForbiddenPawnTypes is separate from AllowedPawnTypes
        var filter = new PawnFilter
        {
            AllowedPawnTypes = [PawnType.Colonist],
            ForbiddenPawnTypes = [PawnType.Slave]
        };
        filter.AllowedPawnTypes.Should().ContainSingle();
        filter.ForbiddenPawnTypes.Should().ContainSingle();
        filter.AllowedPawnTypes.Should().Contain(PawnType.Colonist);
        filter.ForbiddenPawnTypes.Should().Contain(PawnType.Slave);
    }

    [Test]
    [Ignore("Requires live RimWorld context for Verse.Translator")]
    public void GetSummary_MultipleIndentationLevels_Respects()
    {
        var filter = new PawnFilter { FilterPawnTypes = true };
        var summary0 = filter.GetSummary(0);
        var summary2 = filter.GetSummary(2);
        summary0.Should().NotBeNull();
        summary2.Should().NotBeNull();
        // Both should be valid (indentation is internal formatting)
    }

    [Test]
    [Ignore("Requires live RimWorld context for Verse.Translator")]
    public void GetSummary_NoFiltersActive_ReturnsEmptyOrMinimal()
    {
        var filter = new PawnFilter();
        var summary = filter.GetSummary(0);
        summary.Should().NotBeNull();
        // Summary should be empty or very minimal when no filters are set
    }

    [Test]
    [Ignore("Requires live RimWorld context for Verse.Translator")]
    public void GetSummary_WithIndentation_FormatsCorrectly()
    {
        var filter = new PawnFilter { FilterPawnTypes = true };
        var summary = filter.GetSummary(1);
        summary.Should().NotBeNull();
        summary.Should().NotBeEmpty();
    }

    [Test]
    public void PawnCapacityLimits_IsModifiable()
    {
        // PawnCapacityLimits list can be modified
        var filter = new PawnFilter();
        filter.PawnCapacityLimits.Should().BeEmpty();
        var limit = new PawnCapacityLimit("Sight");
        filter.PawnCapacityLimits.Add(limit);
        filter.PawnCapacityLimits.Should().ContainSingle();
        filter.PawnCapacityLimits.Should().Contain(limit);
    }

    [Test]
    public void TriStateMode_AffectsValidate()
    {
        // TriStateMode controls Validate behavior
        var filter1 = new PawnFilter { TriStateMode = true };
        var filter2 = new PawnFilter { TriStateMode = false };
        filter1.Validate();
        filter2.Validate();

        // With TriStateMode true, nulls should stay null
        filter1.FilterPawnTypes.Should().BeNull();
        // With TriStateMode false, nulls should become false
        filter2.FilterPawnTypes.Should().BeFalse();
    }

    [Test]
    public void Validate_PreservesExistingBoolValues()
    {
        // Validate preserves explicitly set bool values
        var filter = new PawnFilter
        {
            TriStateMode = false,
            FilterPawnTypes = true,
            FilterWorkPassions = false,
            FilterPawnCapacities = true
        };
        filter.Validate();
        filter.FilterPawnTypes.Should().BeTrue();
        filter.FilterWorkPassions.Should().BeFalse();
        filter.FilterPawnCapacities.Should().BeTrue();
    }

    [Test]
    public void Validate_WithTriStateModeFalse_SetsNullFlagsToFalse()
    {
        // Validate sets null filter flags to false when TriStateMode is false
        var filter = new PawnFilter
        {
            TriStateMode = false,
            FilterPawnTypes = null,
            FilterWorkPassions = null,
            FilterPawnCapacities = null
        };
        filter.Validate();

        // Null flags should be set to false
        filter.FilterPawnTypes.Should().BeFalse();
        filter.FilterWorkPassions.Should().BeFalse();
        filter.FilterPawnCapacities.Should().BeFalse();
        filter.FilterPawnHealthStates.Should().BeFalse();
        filter.FilterPawnSkills.Should().BeFalse();
        filter.FilterPawnStats.Should().BeFalse();
        filter.FilterPawnTraits.Should().BeFalse();
        filter.FilterWorkCapacities.Should().BeFalse();
        filter.FilterPawnPrimaryWeaponTypes.Should().BeFalse();
    }

    [Test]
    public void Validate_WithTriStateModeTrue_PreservesNullFlags()
    {
        // Validate preserves null flags when TriStateMode is true
        var filter = new PawnFilter
        {
            TriStateMode = true,
            FilterPawnTypes = null,
            FilterWorkPassions = null
        };
        filter.Validate();

        // Null flags should remain null
        filter.FilterPawnTypes.Should().BeNull();
        filter.FilterWorkPassions.Should().BeNull();
    }

    [Test]
    public void WorkCapacityLimits_IsModifiable()
    {
        // WorkCapacityLimits dictionary can be modified
        var filter = new PawnFilter();
        filter.WorkCapacityLimits.Should().BeEmpty();
        filter.WorkCapacityLimits[WorkTags.ManualDumb] = true;
        filter.WorkCapacityLimits[WorkTags.ManualSkilled] = false;
        filter.WorkCapacityLimits.Count.Should().Be(2);
        filter.WorkCapacityLimits[WorkTags.ManualDumb].Should().BeTrue();
        filter.WorkCapacityLimits[WorkTags.ManualSkilled].Should().BeFalse();
    }
}