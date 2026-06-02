using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace LordKuper.Common;

/// <summary>
///     Provides a cache for storing stat values of a <see cref="Thing" /> and manages cache updates based on a timer.
/// </summary>
[PublicAPI]
public class ThingCache
{
    /// <summary>
    ///     The interval in hours between cache updates.
    /// </summary>
    private readonly float _updateTimer;

    /// <summary>
    ///     Stores cached stat values for the associated <see cref="Thing" />.
    /// </summary>
    protected readonly Dictionary<StatDef, float> StatValues = new();

    /// <summary>
    ///     The last time the cache was updated.
    /// </summary>
    private RimWorldTime _updateTime = new(0, 0, 0);

    /// <summary>
    ///     Initializes a new instance of the <see cref="ThingCache" /> class.
    /// </summary>
    /// <param name="thing">The <see cref="Thing" /> to cache stat values for.</param>
    /// <param name="updateTimer">The interval in hours between cache updates.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="thing" /> is null.</exception>
    public ThingCache(Thing thing, float updateTimer)
    {
        Thing = thing ?? throw new ArgumentNullException(nameof(thing));
        _updateTimer = updateTimer;
    }

    /// <summary>
    ///     Gets the <see cref="Thing" /> associated with this cache.
    /// </summary>
    public Thing Thing { get; }

    /// <summary>
    ///     Updates the cache if the specified time interval has passed since the last update.
    /// </summary>
    /// <param name="time">The current <see cref="RimWorldTime" />.</param>
    /// <returns>
    ///     <c>true</c> if the cache was updated; otherwise, <c>false</c>.
    /// </returns>
    public virtual bool Update(RimWorldTime time)
    {
        var hoursPassed = time - _updateTime;
        if (hoursPassed < _updateTimer) return false;
        _updateTime = time;
        StatValues.Clear();
        return true;
    }
}