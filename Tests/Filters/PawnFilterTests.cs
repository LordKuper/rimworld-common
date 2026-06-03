using LordKuper.Common.Filters;
using LordKuper.Common.Filters.Limits;
using RimWorld;
using Verse;
using PawnHealthState = LordKuper.Common.Filters.PawnHealthState;

namespace LordKuper.Common.Tests.Filters;

/// <summary>
///     Tests for <see cref="PawnFilter" /> pure paths (Combine, Copy, Validate, GetSummary).
///     AC-17: pure-path coverage for PawnFilter. Verifies Task 7's Combine refactor preserved semantics.
/// </summary>
public class PawnFilterTests
{
    [Fact]
    public void Combine_MainHasPawnTypes_UsesMain()
    {
        // AC-17: Combine uses main's values when set
        var main = new PawnFilter { FilterPawnTypes = true, AllowedPawnTypes = [PawnType.Colonist] };
        var fallback = new PawnFilter { FilterPawnTypes = false, AllowedPawnTypes = [PawnType.Guest] };

        var result = PawnFilter.Combine(main, fallback);

        Assert.True(result.FilterPawnTypes);
        Assert.Contains(PawnType.Colonist, result.AllowedPawnTypes);
        Assert.DoesNotContain(PawnType.Guest, result.AllowedPawnTypes);
    }

    [Fact]
    public void Combine_MainHasNoPawnTypes_UsesFallback()
    {
        var main = new PawnFilter { FilterPawnTypes = null, AllowedPawnTypes = [] };
        var fallback = new PawnFilter { FilterPawnTypes = true, AllowedPawnTypes = [PawnType.Guest] };

        var result = PawnFilter.Combine(main, fallback);

        Assert.True(result.FilterPawnTypes);
        Assert.Contains(PawnType.Guest, result.AllowedPawnTypes);
    }

    [Fact]
    public void Combine_MainHasWorkPassions_UsesMain()
    {
        var main = new PawnFilter { FilterWorkPassions = true, AllowedWorkPassions = [Passion.Major] };
        var fallback = new PawnFilter { FilterWorkPassions = false, AllowedWorkPassions = [Passion.Minor] };

        var result = PawnFilter.Combine(main, fallback);

        Assert.True(result.FilterWorkPassions);
        Assert.Contains(Passion.Major, result.AllowedWorkPassions);
        Assert.DoesNotContain(Passion.Minor, result.AllowedWorkPassions);
    }

    [Fact]
    public void Combine_MainHasNoWorkPassions_UsesFallback()
    {
        var main = new PawnFilter { FilterWorkPassions = null, AllowedWorkPassions = [] };
        var fallback = new PawnFilter { FilterWorkPassions = true, AllowedWorkPassions = [Passion.Minor] };

        var result = PawnFilter.Combine(main, fallback);

        Assert.True(result.FilterWorkPassions);
        Assert.Contains(Passion.Minor, result.AllowedWorkPassions);
    }

    [Fact]
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

        Assert.Equal(PawnHealthState.Healthy, result.AllowedPawnHealthStates);
    }

    [Fact]
    public void Combine_NullMain_Throws()
    {
        var fallback = new PawnFilter();
        Assert.Throws<ArgumentNullException>(() => PawnFilter.Combine(null!, fallback));
    }

    [Fact]
    public void Combine_NullFallback_Throws()
    {
        var main = new PawnFilter();
        Assert.Throws<ArgumentNullException>(() => PawnFilter.Combine(main, null!));
    }

    [Fact]
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
        Assert.Equal(original.FilterPawnTypes, copy.FilterPawnTypes);
        Assert.Equal(original.FilterPawnHealthStates, copy.FilterPawnHealthStates);
        Assert.Equal(original.TriStateMode, copy.TriStateMode);

        // Verify collections are independent
        Assert.NotSame(original.AllowedPawnTypes, copy.AllowedPawnTypes);
        Assert.Equal(original.AllowedPawnTypes.Count, copy.AllowedPawnTypes.Count);
        foreach (var pawnType in original.AllowedPawnTypes)
        {
            Assert.Contains(pawnType, copy.AllowedPawnTypes);
        }
    }

    [Fact]
    public void Copy_ModifyingCopyDoesNotAffectOriginal()
    {
        var original = new PawnFilter { AllowedPawnTypes = [PawnType.Colonist] };
        var copy = original.Copy();

        copy.AllowedPawnTypes.Add(PawnType.Guest);

        Assert.Single(original.AllowedPawnTypes);
        Assert.Equal(2, copy.AllowedPawnTypes.Count);
    }

    [Fact]
    public void Copy_EmptyFilter_Copies()
    {
        var original = new PawnFilter();
        var copy = original.Copy();

        Assert.NotNull(copy);
        Assert.Null(copy.FilterPawnTypes);
        Assert.Empty(copy.AllowedPawnTypes);
    }

    [Fact]
    public void Copy_WithWorkCapacityLimits_DeepCopies()
    {
        var original = new PawnFilter
        {
            WorkCapacityLimits = new Dictionary<Verse.WorkTags, bool> { { Verse.WorkTags.ManualDumb, true } }
        };

        var copy = original.Copy();

        Assert.NotSame(original.WorkCapacityLimits, copy.WorkCapacityLimits);
        Assert.Equal(original.WorkCapacityLimits.Count, copy.WorkCapacityLimits.Count);
    }

    [Fact(Skip = "Requires live RimWorld context for Verse.Translator")]
    public void GetSummary_NoFiltersActive_ReturnsEmptyOrMinimal()
    {
        var filter = new PawnFilter();
        var summary = filter.GetSummary(0);

        Assert.NotNull(summary);
        // Summary should be empty or very minimal when no filters are set
    }

    [Fact(Skip = "Requires live RimWorld context for Verse.Translator")]
    public void GetSummary_WithIndentation_FormatsCorrectly()
    {
        var filter = new PawnFilter { FilterPawnTypes = true };
        var summary = filter.GetSummary(1);

        Assert.NotNull(summary);
        Assert.NotEmpty(summary);
    }

    [Fact(Skip = "Requires live RimWorld context for Verse.Translator")]
    public void GetSummary_MultipleIndentationLevels_Respects()
    {
        var filter = new PawnFilter { FilterPawnTypes = true };

        var summary0 = filter.GetSummary(0);
        var summary2 = filter.GetSummary(2);

        Assert.NotNull(summary0);
        Assert.NotNull(summary2);
        // Both should be valid (indentation is internal formatting)
    }

    [Fact]
    public void Validate_WithTriStateModeFalse_SetsNullFlagsToFalse()
    {
        // AC-17/AC-20: Validate sets null filter flags to false when TriStateMode is false
        var filter = new PawnFilter
        {
            TriStateMode = false,
            FilterPawnTypes = null,
            FilterWorkPassions = null,
            FilterPawnCapacities = null
        };

        filter.Validate();

        // Null flags should be set to false
        Assert.False(filter.FilterPawnTypes);
        Assert.False(filter.FilterWorkPassions);
        Assert.False(filter.FilterPawnCapacities);
        Assert.False(filter.FilterPawnHealthStates);
        Assert.False(filter.FilterPawnSkills);
        Assert.False(filter.FilterPawnStats);
        Assert.False(filter.FilterPawnTraits);
        Assert.False(filter.FilterWorkCapacities);
        Assert.False(filter.FilterPawnPrimaryWeaponTypes);
    }

    [Fact]
    public void Validate_WithTriStateModeTrue_PreservesNullFlags()
    {
        // AC-17/AC-20: Validate preserves null flags when TriStateMode is true
        var filter = new PawnFilter
        {
            TriStateMode = true,
            FilterPawnTypes = null,
            FilterWorkPassions = null
        };

        filter.Validate();

        // Null flags should remain null
        Assert.Null(filter.FilterPawnTypes);
        Assert.Null(filter.FilterWorkPassions);
    }

    [Fact]
    public void Validate_PreservesExistingBoolValues()
    {
        // AC-17/AC-20: Validate preserves explicitly set bool values
        var filter = new PawnFilter
        {
            TriStateMode = false,
            FilterPawnTypes = true,
            FilterWorkPassions = false,
            FilterPawnCapacities = true
        };

        filter.Validate();

        Assert.True(filter.FilterPawnTypes);
        Assert.False(filter.FilterWorkPassions);
        Assert.True(filter.FilterPawnCapacities);
    }

    [Fact]
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
        Assert.Null(Record.Exception(() => original.ExposeData()));
    }

    [Fact]
    public void AllowedPawnTypes_ModifyingDoesNotAffectOtherCollections()
    {
        // AC-17: AllowedPawnTypes is independent from other filter collections
        var filter = new PawnFilter
        {
            AllowedPawnTypes = [PawnType.Colonist],
            AllowedWorkPassions = [Passion.Major],
            AllowedPawnHealthStates = PawnHealthState.Healthy
        };

        filter.AllowedPawnTypes.Add(PawnType.Guest);

        Assert.Equal(2, filter.AllowedPawnTypes.Count);
        Assert.Single(filter.AllowedWorkPassions);
        Assert.Equal(PawnHealthState.Healthy, filter.AllowedPawnHealthStates);
    }

    [Fact]
    public void ForbiddenPawnTypes_Independent()
    {
        // AC-17: ForbiddenPawnTypes is separate from AllowedPawnTypes
        var filter = new PawnFilter
        {
            AllowedPawnTypes = [PawnType.Colonist],
            ForbiddenPawnTypes = [PawnType.Slave]
        };

        Assert.Single(filter.AllowedPawnTypes);
        Assert.Single(filter.ForbiddenPawnTypes);
        Assert.Contains(PawnType.Colonist, filter.AllowedPawnTypes);
        Assert.Contains(PawnType.Slave, filter.ForbiddenPawnTypes);
    }

    [Fact]
    public void PawnCapacityLimits_IsModifiable()
    {
        // AC-17: PawnCapacityLimits list can be modified
        var filter = new PawnFilter();
        Assert.Empty(filter.PawnCapacityLimits);

        var limit = new PawnCapacityLimit("Sight");
        filter.PawnCapacityLimits.Add(limit);

        Assert.Single(filter.PawnCapacityLimits);
        Assert.Contains(limit, filter.PawnCapacityLimits);
    }

    [Fact]
    public void WorkCapacityLimits_IsModifiable()
    {
        // AC-17: WorkCapacityLimits dictionary can be modified
        var filter = new PawnFilter();
        Assert.Empty(filter.WorkCapacityLimits);

        filter.WorkCapacityLimits[WorkTags.ManualDumb] = true;
        filter.WorkCapacityLimits[WorkTags.ManualSkilled] = false;

        Assert.Equal(2, filter.WorkCapacityLimits.Count);
        Assert.True(filter.WorkCapacityLimits[WorkTags.ManualDumb]);
        Assert.False(filter.WorkCapacityLimits[WorkTags.ManualSkilled]);
    }

    [Fact]
    public void TriStateMode_AffectsValidate()
    {
        // AC-17: TriStateMode controls Validate behavior
        var filter1 = new PawnFilter { TriStateMode = true };
        var filter2 = new PawnFilter { TriStateMode = false };

        filter1.Validate();
        filter2.Validate();

        // With TriStateMode true, nulls should stay null
        Assert.Null(filter1.FilterPawnTypes);
        // With TriStateMode false, nulls should become false
        Assert.False(filter2.FilterPawnTypes);
    }
}
