using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LordKuper.Common.Cache;
using LordKuper.Common.Compatibility;
using RimWorld;
using UnityEngine;

namespace LordKuper.Common.Helpers;

/// <summary>
///     Provides helper methods and caching for handling <see cref="Passion" /> values and their associated data.
/// </summary>
public static class PassionHelper
{
    /// <summary>
    ///     Backing field for the cached list of <see cref="PassionCache" /> entries.
    /// </summary>
    private static List<PassionCache> _cachedPassions;

    /// <summary>
    ///     Indicates whether the passion cache has been initialized.
    /// </summary>
    private static bool _isInitialized;

    /// <summary>
    ///     Stores the mapping between <see cref="Passion" /> values and their corresponding <see cref="PassionCache" />
    ///     entries.
    /// </summary>
    private static readonly Dictionary<Passion, PassionCache> PassionCache = new();

    /// <summary>
    ///     Gets an ordered, cached list of all <see cref="PassionCache" /> entries.
    /// </summary>
    [NotNull]
    [UsedImplicitly]
    public static IReadOnlyList<PassionCache> Passions
    {
        get
        {
            Initialize();
            return _cachedPassions ??= PassionCache.Values
                .OrderByDescending(pc => pc.Passion == Passion.None)
                .ThenBy(pc => pc.LearnRateFactor).ThenBy(pc => pc.ForgetRateFactor)
                .ThenBy(pc => pc.DefName).ToList();
        }
    }

    /// <summary>
    ///     Retrieves the <see cref="PassionCache" /> entry for the specified <see cref="Passion" />.
    /// </summary>
    /// <param name="passion">The <see cref="Passion" /> value to look up.</param>
    /// <returns>
    ///     The <see cref="PassionCache" /> entry if found; otherwise, <c>null</c>.
    /// </returns>
    [UsedImplicitly]
    [CanBeNull]
    public static PassionCache GetPassionCache(Passion passion)
    {
        Initialize();
        PassionCache.TryGetValue(passion, out var passionCache);
        if (passionCache == null)
            Logger.LogError($"Passion '{passion}' not found in cache.");
        return passionCache;
    }

    /// <summary>
    ///     Retrieves a cached passion object based on the specified definition name.
    /// </summary>
    /// <param name="defName">The definition name of the passion.</param>
    /// <returns>
    ///     The <see cref="PassionCache" /> entry if found; otherwise, <c>null</c>.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="defName" /> is null or empty.</exception>
    [CanBeNull]
    [UsedImplicitly]
    public static PassionCache GetPassionCache([NotNull] string defName)
    {
        Initialize();
        if (string.IsNullOrEmpty(defName))
            throw new ArgumentNullException(nameof(defName), "DefName cannot be null or empty.");
        return Passions.FirstOrDefault(p =>
            p.DefName.Equals(defName, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    ///     Gets the icon associated with the specified <see cref="Passion" />.
    /// </summary>
    /// <param name="passion">The <see cref="Passion" /> value for which to retrieve the icon.</param>
    /// <returns>
    ///     The <see cref="Texture2D" /> representing the icon for the specified passion.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     Thrown if the specified <paramref name="passion" /> is not recognized.
    /// </exception>
    internal static Texture2D GetPassionIcon(Passion passion)
    {
        if (Vse.VanillaSkillsExpandedActive) return Vse.GetIcon(passion);
        return passion switch
        {
            Passion.None => Resources.Textures.Passions.PassionIconNone,
            Passion.Minor => Resources.Textures.Passions.PassionIconMinor,
            Passion.Major => Resources.Textures.Passions.PassionIconMajor,
            _ => throw new ArgumentOutOfRangeException(nameof(passion), passion, null)
        };
    }

    /// <summary>
    ///     Initializes the passion cache if it has not already been initialized.
    /// </summary>
    private static void Initialize()
    {
        if (_isInitialized) return;
        _isInitialized = true;
        PassionCache.Clear();
        _cachedPassions = null;
        if (Vse.VanillaSkillsExpandedActive)
        {
            foreach (var passion in Vse.GetPassions())
            {
                if (!PassionCache.ContainsKey(passion))
                    PassionCache[passion] = new PassionCache(passion, Vse.GetDefName(passion),
                        Vse.GetLabel(passion), Vse.GetLearnRateFactor(passion),
                        Vse.GetForgetRateFactor(passion));
            }
        }
        else
        {
            foreach (Passion passion in Enum.GetValues(typeof(Passion)))
            {
                if (!PassionCache.ContainsKey(passion))
                    PassionCache[passion] = new PassionCache(passion, passion.ToString(),
                        passion.GetLabel(), passion.GetLearningFactor(), 1f);
            }
        }
    }
}