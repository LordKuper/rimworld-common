using LordKuper.Common.Cache;

namespace LordKuper.Common.Tests.Cache;

/// <summary>
///     Tests for <see cref="TimedCache" /> update interval logic (AC-20).
/// </summary>
public class TimedCacheTests
{
    private class TestTimedCache : TimedCache
    {
        public TestTimedCache(float updateInterval, bool updateOnFirstAccess = false) : base(
            updateInterval, updateOnFirstAccess) { }
    }

    [Fact]
    public void Update_AfterDueReset_NewInterval()
    {
        // AC-20: After update is due and processed, new interval starts
        var cache = new TestTimedCache(10f);
        var time1 = new RimWorldTime(0, 0, 0f);
        var time2 = new RimWorldTime(0, 0, 15f); // Due (15 - 0 = 15 >= 10)
        var time3 = new RimWorldTime(0, 0, 20f); // (20 - 15 = 5 < 10) Not due
        cache.Update(time1);
        var isDue2 = cache.Update(time2);
        var isDue3 = cache.Update(time3);
        Assert.True(isDue2);
        Assert.False(isDue3); // New interval starts from time2
    }

    [Fact]
    public void Update_CrossesMultipleDays()
    {
        // AC-20: Time can span multiple days
        var cache = new TestTimedCache(50f);
        var time1 = new RimWorldTime(0, 0, 0f);
        var time2 = new RimWorldTime(0, 3, 0f); // 3 days = 72 hours later
        cache.Update(time1);
        var isDue = cache.Update(time2);
        Assert.True(isDue); // 72 > 50
    }

    [Fact]
    public void Update_CrossesYears()
    {
        // AC-20: Time can span multiple years
        var cache = new TestTimedCache(1000f);
        var time1 = new RimWorldTime(0, 0, 0f);
        var time2 = new RimWorldTime(1, 0, 0f); // 1 year = 1440 hours later
        cache.Update(time1);
        var isDue = cache.Update(time2);
        Assert.True(isDue); // 1440 > 1000
    }

    [Fact]
    public void Update_FirstCallWithTime_RecordsTime()
    {
        // AC-20: First call records the time, regardless of updateOnFirstAccess
        var cache1 = new TestTimedCache(10f, true);
        var cache2 = new TestTimedCache(10f);
        var time1 = new RimWorldTime(0, 0, 0f);
        var time2 = new RimWorldTime(0, 0, 5f);

        // Cache1: first call is due, second is within interval
        cache1.Update(time1);
        var isDue1 = cache1.Update(time2); // 5 < 10, so not due

        // Cache2: first call is not due, second is within interval
        cache2.Update(time1);
        var isDue2 = cache2.Update(time2); // 5 < 10, so not due
        Assert.False(isDue1);
        Assert.False(isDue2);
    }

    [Fact]
    public void Update_FirstCall_WithUpdateOnFirstAccess_ReturnsTrue()
    {
        // AC-20: First call returns true if updateOnFirstAccess is true
        var cache = new TestTimedCache(10f, true);
        var time = new RimWorldTime(0, 0, 0f);
        var isDue = cache.Update(time);
        Assert.True(isDue);
    }

    [Fact]
    public void Update_FirstCall_WithoutUpdateOnFirstAccess_ReturnsFalse()
    {
        // AC-20: First call returns false if updateOnFirstAccess is false
        var cache = new TestTimedCache(10f);
        var time = new RimWorldTime(0, 0, 0f);
        var isDue = cache.Update(time);
        Assert.False(isDue);
    }

    [Fact]
    public void Update_LargeInterval()
    {
        // AC-20: Large intervals work correctly
        var cache = new TestTimedCache(10000f);
        var time1 = new RimWorldTime(0, 0, 0f);
        var time2 = new RimWorldTime(10, 0, 0f); // ~14400 hours
        cache.Update(time1);
        var isDue = cache.Update(time2);
        Assert.True(isDue);
    }

    [Fact]
    public void Update_MultipleCallsTracksLastUpdateTime()
    {
        // AC-20: Update time advances with each call and interval is tracked from last update
        var cache = new TestTimedCache(10f);
        var time1 = new RimWorldTime(0, 0, 0f);
        var time2 = new RimWorldTime(0, 0, 5f); // (5 - 0 = 5 < 10) Not due
        var time3 = new RimWorldTime(0, 0, 16f); // (16 - 0 = 16 >= 10) Due from time1
        cache.Update(time1);
        var isDue2 = cache.Update(time2); // Tracks from time1, not due yet
        var isDue3 = cache.Update(time3); // (16 - 0 = 16 >= 10) Should be due
        Assert.False(isDue2);
        Assert.True(isDue3); // (16 - 0) = 16 >= 10, so due
    }

    [Fact]
    public void Update_SecondCallAtExactInterval_ReturnsTrue()
    {
        // AC-20: Update at exact interval boundary returns true
        var cache = new TestTimedCache(10f);
        var time1 = new RimWorldTime(0, 0, 0f);
        var time2 = new RimWorldTime(0, 0, 10f); // Exactly 10 hours later
        cache.Update(time1);
        var isDue = cache.Update(time2);
        Assert.True(isDue);
    }

    [Fact]
    public void Update_SecondCallBeyondInterval_ReturnsTrue()
    {
        // AC-20: Update beyond interval returns true
        var cache = new TestTimedCache(10f);
        var time1 = new RimWorldTime(0, 0, 0f);
        var time2 = new RimWorldTime(0, 0, 15f); // 15 hours later (more than 10 hour interval)
        cache.Update(time1);
        var isDue = cache.Update(time2);
        Assert.True(isDue);
    }

    [Fact]
    public void Update_SecondCallWithinInterval_ReturnsFalse()
    {
        // AC-20: Update within interval returns false
        var cache = new TestTimedCache(10f);
        var time1 = new RimWorldTime(0, 0, 0f);
        var time2 = new RimWorldTime(0, 0, 5f); // 5 hours later (less than 10 hour interval)
        cache.Update(time1);
        var isDue = cache.Update(time2);
        Assert.False(isDue);
    }

    [Fact]
    public void Update_WithZeroInterval_AlwaysDue()
    {
        // AC-20: Zero interval means always due (after first)
        var cache = new TestTimedCache(0f);
        var time1 = new RimWorldTime(0, 0, 0f);
        var time2 = new RimWorldTime(0, 0, 0f); // Same time
        var time3 = new RimWorldTime(0, 0, 0.001f); // Microscopically later
        cache.Update(time1);
        var isDue2 = cache.Update(time2); // 0 >= 0 interval
        var isDue3 = cache.Update(time3); // 0.001 >= 0 interval
        Assert.True(isDue2);
        Assert.True(isDue3);
    }
}