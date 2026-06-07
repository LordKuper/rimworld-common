using LordKuper.Common.Cache;

namespace LordKuper.Common.Tests.Cache;

/// <summary>
///     Tests for <see cref="TimedCache" /> update interval logic.
/// </summary>
public class TimedCacheTests
{
    [Test]
    public void Update_AfterDueReset_NewInterval()
    {
        // After update is due and processed, new interval starts
        var cache = new TestTimedCache(10f);
        var time1 = new RimWorldTime(0, 0, 0f);
        var time2 = new RimWorldTime(0, 0, 15f); // Due (15 - 0 = 15 >= 10)
        var time3 = new RimWorldTime(0, 0, 20f); // (20 - 15 = 5 < 10) Not due
        cache.Update(time1);
        var isDue2 = cache.Update(time2);
        var isDue3 = cache.Update(time3);
        isDue2.Should().BeTrue();
        isDue3.Should().BeFalse(); // New interval starts from time2
    }

    [Test]
    public void Update_CrossesMultipleDays()
    {
        // Time can span multiple days
        var cache = new TestTimedCache(50f);
        var time1 = new RimWorldTime(0, 0, 0f);
        var time2 = new RimWorldTime(0, 3, 0f); // 3 days = 72 hours later
        cache.Update(time1);
        var isDue = cache.Update(time2);
        isDue.Should().BeTrue(); // 72 > 50
    }

    [Test]
    public void Update_CrossesYears()
    {
        // Time can span multiple years
        var cache = new TestTimedCache(1000f);
        var time1 = new RimWorldTime(0, 0, 0f);
        var time2 = new RimWorldTime(1, 0, 0f); // 1 year = 1440 hours later
        cache.Update(time1);
        var isDue = cache.Update(time2);
        isDue.Should().BeTrue(); // 1440 > 1000
    }

    [Test]
    public void Update_FirstCallWithTime_RecordsTime()
    {
        // First call records the time, regardless of updateOnFirstAccess
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
        isDue1.Should().BeFalse();
        isDue2.Should().BeFalse();
    }

    [Test]
    public void Update_FirstCall_WithUpdateOnFirstAccess_ReturnsTrue()
    {
        // First call returns true if updateOnFirstAccess is true
        var cache = new TestTimedCache(10f, true);
        var time = new RimWorldTime(0, 0, 0f);
        var isDue = cache.Update(time);
        isDue.Should().BeTrue();
    }

    [Test]
    public void Update_FirstCall_WithoutUpdateOnFirstAccess_ReturnsFalse()
    {
        // First call returns false if updateOnFirstAccess is false
        var cache = new TestTimedCache(10f);
        var time = new RimWorldTime(0, 0, 0f);
        var isDue = cache.Update(time);
        isDue.Should().BeFalse();
    }

    [Test]
    public void Update_LargeInterval()
    {
        // Large intervals work correctly
        var cache = new TestTimedCache(10000f);
        var time1 = new RimWorldTime(0, 0, 0f);
        var time2 = new RimWorldTime(10, 0, 0f); // ~14400 hours
        cache.Update(time1);
        var isDue = cache.Update(time2);
        isDue.Should().BeTrue();
    }

    [Test]
    public void Update_MultipleCallsTracksLastUpdateTime()
    {
        // Update time advances with each call and interval is tracked from last update
        var cache = new TestTimedCache(10f);
        var time1 = new RimWorldTime(0, 0, 0f);
        var time2 = new RimWorldTime(0, 0, 5f); // (5 - 0 = 5 < 10) Not due
        var time3 = new RimWorldTime(0, 0, 16f); // (16 - 0 = 16 >= 10) Due from time1
        cache.Update(time1);
        var isDue2 = cache.Update(time2); // Tracks from time1, not due yet
        var isDue3 = cache.Update(time3); // (16 - 0 = 16 >= 10) Should be due
        isDue2.Should().BeFalse();
        isDue3.Should().BeTrue(); // (16 - 0) = 16 >= 10, so due
    }

    [Test]
    public void Update_SecondCallAtExactInterval_ReturnsTrue()
    {
        // Update at exact interval boundary returns true
        var cache = new TestTimedCache(10f);
        var time1 = new RimWorldTime(0, 0, 0f);
        var time2 = new RimWorldTime(0, 0, 10f); // Exactly 10 hours later
        cache.Update(time1);
        var isDue = cache.Update(time2);
        isDue.Should().BeTrue();
    }

    [Test]
    public void Update_SecondCallBeyondInterval_ReturnsTrue()
    {
        // Update beyond interval returns true
        var cache = new TestTimedCache(10f);
        var time1 = new RimWorldTime(0, 0, 0f);
        var time2 = new RimWorldTime(0, 0, 15f); // 15 hours later (more than 10 hour interval)
        cache.Update(time1);
        var isDue = cache.Update(time2);
        isDue.Should().BeTrue();
    }

    [Test]
    public void Update_SecondCallWithinInterval_ReturnsFalse()
    {
        // Update within interval returns false
        var cache = new TestTimedCache(10f);
        var time1 = new RimWorldTime(0, 0, 0f);
        var time2 = new RimWorldTime(0, 0, 5f); // 5 hours later (less than 10 hour interval)
        cache.Update(time1);
        var isDue = cache.Update(time2);
        isDue.Should().BeFalse();
    }

    [Test]
    public void Update_WithZeroInterval_AlwaysDue()
    {
        // Zero interval means always due (after first)
        var cache = new TestTimedCache(0f);
        var time1 = new RimWorldTime(0, 0, 0f);
        var time2 = new RimWorldTime(0, 0, 0f); // Same time
        var time3 = new RimWorldTime(0, 0, 0.001f); // Microscopically later
        cache.Update(time1);
        var isDue2 = cache.Update(time2); // 0 >= 0 interval
        var isDue3 = cache.Update(time3); // 0.001 >= 0 interval
        isDue2.Should().BeTrue();
        isDue3.Should().BeTrue();
    }

    private class TestTimedCache : TimedCache
    {
        public TestTimedCache(float updateInterval, bool updateOnFirstAccess = false) : base(
            updateInterval, updateOnFirstAccess)
        {
        }
    }
}