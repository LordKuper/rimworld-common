using System;
using JetBrains.Annotations;
using LordKuper.Common.Helpers;
using RimWorld;
using Verse;

namespace LordKuper.Common.Cache;

/// <summary>
///     Provides a base class for caching RimWorld <see cref="Def" /> objects by name.
///     Handles lazy initialization and serialization of the def reference.
/// </summary>
/// <typeparam name="T">The type of <see cref="Def" /> to cache.</typeparam>
[PublicAPI]
public abstract class DefCache<T> : IExposable where T : Def
{
    /// <summary>
    ///     The cached <see cref="Def" /> instance.
    /// </summary>
    [CanBeNull] private T _def;

    /// <summary>
    ///     The name of the <see cref="Def" /> to cache.
    /// </summary>
    [CanBeNull] private string _defName;

    /// <summary>
    ///     Indicates whether the cache has been initialized.
    /// </summary>
    private bool _isInitialized;

    /// <summary>
    ///     Default constructor for serialization.
    /// </summary>
    protected DefCache() { }

    /// <summary>
    ///     Initializes a new instance of <see cref="DefCache{T}" /> with the specified def name.
    /// </summary>
    /// <param name="defName">The name of the def to cache.</param>
    protected DefCache([CanBeNull] string defName)
    {
        _defName = defName;
    }

    /// <summary>
    ///     Gets the cached <see cref="Def" /> instance, initializing it if necessary.
    /// </summary>
    [CanBeNull]
    public T Def
    {
        get
        {
            Initialize();
            return _def;
        }
    }

    /// <summary>
    ///     Gets the name of the cached <see cref="Def" />.
    /// </summary>
    [CanBeNull]
    public string DefName => _defName;

    /// <summary>
    ///     Gets the label for the cached <see cref="Def" />, or the def name if not found.
    /// </summary>
    [CanBeNull]
    public virtual string Label => field ??= Def == null ? _defName : Def.GetLabel();

    /// <summary>
    ///     Serializes the def name for saving/loading.
    /// </summary>
    public virtual void ExposeData()
    {
        Scribe_Values.Look(ref _defName, nameof(DefName));
    }

    /// <summary>
    ///     Returns the def name of the specified def, throwing if the def is <see langword="null" />.
    /// </summary>
    /// <param name="def">The def whose name to return.</param>
    /// <returns>The def name.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="def" /> is <see langword="null" />.</exception>
    [NotNull]
    protected static string GetDefName([NotNull] T def)
    {
        return def == null ? throw new ArgumentNullException(nameof(def)) : def.defName;
    }

    /// <summary>
    ///     Initializes the cached <see cref="Def" /> instance if it has not been initialized.
    /// </summary>
    protected virtual void Initialize()
    {
        if (_isInitialized) return;
        _isInitialized = true;
        if (string.IsNullOrEmpty(_defName)) return;
        if (typeof(T) == typeof(StatDef))
            _def = (T)(object)StatHelper.GetStatDef(_defName);
        else _def = DefDatabase<T>.GetNamedSilentFail(_defName);
    }
}