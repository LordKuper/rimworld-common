using JetBrains.Annotations;

namespace LordKuper.Common.Cache;

/// <summary>
///     Provides a base class for caches that update at a specified interval of RimWorld time.
/// </summary>
/// <remarks>
///     The cache is updated only if the specified interval in in-game hours has passed since the last update.
/// </remarks>
[PublicAPI]
public abstract class TimedCache
{
    private readonly float _updateInterval;
    private readonly bool _updateOnFirstAccess;

    /// <summary>
    ///     Indicates whether the cache has already been updated at least once.
    /// </summary>
    private bool _hasUpdated;

    /// <summary>
    ///     The last time the cache was updated.
    /// </summary>
    private RimWorldTime _lastUpdate = new(0);

    /// <summary>
    ///     Initializes a new instance of the <see cref="TimedCache" /> class.
    /// </summary>
    /// <param name="updateInterval">The minimum interval, in in-game hours, between updates.</param>
    /// <param name="updateOnFirstAccess">
    ///     Whether the first call to <see cref="Update" /> should report that an update is due.
    /// </param>
    protected TimedCache(float updateInterval, bool updateOnFirstAccess = false)
    {
        _updateInterval = updateInterval;
        _updateOnFirstAccess = updateOnFirstAccess;
    }

    /// <summary>
    ///     Records the given time and reports whether the configured interval has elapsed since the last update.
    /// </summary>
    /// <param name="time">The current RimWorld time.</param>
    /// <returns>
    ///     <c>true</c> if the cache is due for an update; otherwise, <c>false</c>.
    /// </returns>
    public virtual bool Update(RimWorldTime time)
    {
        if (!_hasUpdated)
        {
            _hasUpdated = true;
            _lastUpdate = time;
            return _updateOnFirstAccess;
        }
        var hoursPassed = time - _lastUpdate;
        if (hoursPassed < _updateInterval) return false;
        _lastUpdate = time;
        return true;
    }
}