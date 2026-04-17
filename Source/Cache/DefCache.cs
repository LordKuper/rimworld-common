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
public abstract class DefCache<T> : IExposable where T : Def
{
    /// <summary>
    ///     The cached <see cref="Def" /> instance.
    /// </summary>
    private T _def;

    /// <summary>
    ///     The name of the <see cref="Def" /> to cache.
    /// </summary>
    private string _defName;

    /// <summary>
    ///     Indicates whether the cache has been initialized.
    /// </summary>
    private bool _isInitialized;

    /// <summary>
    ///     Gets the label for the cached <see cref="Def" />, or the def name if not found.
    /// </summary>
    [UsedImplicitly] private string _label;

    /// <summary>
    ///     Default constructor for serialization.
    /// </summary>
    [UsedImplicitly]
    protected DefCache() { }

    /// <summary>
    ///     Initializes a new instance of <see cref="DefCache{T}" /> with the specified def name.
    /// </summary>
    /// <param name="defName">The name of the def to cache.</param>
    protected DefCache(string defName)
    {
        _defName = defName;
    }

    /// <summary>
    ///     Gets the cached <see cref="Def" /> instance, initializing it if necessary.
    /// </summary>
    [UsedImplicitly]
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
    public string DefName => _defName;

    public virtual string Label => _label ??= Def == null ? _defName : Def.GetLabel();

    /// <summary>
    ///     Serializes the def name for saving/loading.
    /// </summary>
    public void ExposeData()
    {
        Scribe_Values.Look(ref _defName, nameof(DefName));
    }

    /// <summary>
    ///     Initializes the cached <see cref="Def" /> instance if it has not been initialized.
    /// </summary>
    [UsedImplicitly]
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