using System.Text;
using JetBrains.Annotations;
using LordKuper.Common.Helpers;
using RimWorld;
using UnityEngine;
using static LordKuper.Common.Resources;

namespace LordKuper.Common.Cache;

/// <summary>
///     Represents a cache entry for a passion, encapsulating its properties and related data.
/// </summary>
/// <remarks>
///     Initializes a new instance of the <see cref="PassionCache" /> class with the specified parameters.
/// </remarks>
/// <param name="passion">The passion type.</param>
/// <param name="defName">The definition name of the passion.</param>
/// <param name="label">The display label for the passion.</param>
/// <param name="learnRateFactor">The learning rate factor associated with the passion.</param>
/// <param name="forgetRateFactor">The forget rate factor associated with the passion.</param>
[PublicAPI]
public class PassionCache(
    Passion passion,
    string defName,
    string label,
    float learnRateFactor,
    float forgetRateFactor)
{
    /// <summary>
    ///     Gets the definition name of the passion.
    /// </summary>
    public string DefName { get; } = defName;

    /// <summary>
    ///     Gets the description for the passion, including label, learning rate, and forget rate.
    /// </summary>
    public string Description { get; } = new StringBuilder().AppendLine(label).AppendLine()
        .AppendLine(string.Format(Strings.Passions.LearnRateFactorDescription, learnRateFactor))
        .AppendLine(string.Format(Strings.Passions.ForgetRateFactorDescription, forgetRateFactor))
        .ToString();

    /// <summary>
    ///     Gets the forget rate factor associated with the passion.
    /// </summary>
    public float ForgetRateFactor { get; } = forgetRateFactor;

    /// <summary>
    ///     Gets the icon representing the passion. The icon is cached after the first retrieval.
    /// </summary>
    public Texture2D? Icon => field ? field : field = PassionHelper.GetPassionIcon(Passion);

    /// <summary>
    ///     Gets the display label for the passion.
    /// </summary>
    public string Label { get; } = label;

    /// <summary>
    ///     Gets the learning rate factor associated with the passion.
    /// </summary>
    public float LearnRateFactor { get; } = learnRateFactor;

    /// <summary>
    ///     Gets the passion type.
    /// </summary>
    public Passion Passion { get; } = passion;
}