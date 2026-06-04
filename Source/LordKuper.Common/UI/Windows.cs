using JetBrains.Annotations;
using UnityEngine;
using Verse;

namespace LordKuper.Common.UI;

/// <summary>
///     Provides utility methods for windows in the UI.
/// </summary>
[PublicAPI]
public static class Windows
{
    /// <summary>
    ///     Calculates the window size based on the current screen dimensions and UI scale,
    ///     clamped between the specified minimum and maximum sizes.
    /// </summary>
    /// <param name="minSize">The minimum allowed window size.</param>
    /// <param name="maxSize">The maximum allowed window size.</param>
    /// <returns>
    ///     A <see cref="Vector2" /> representing the calculated window width and height.
    /// </returns>
    public static Vector2 GetWindowSize(Vector2 minSize, Vector2 maxSize)
    {
        var width = Mathf.Clamp(Prefs.ScreenWidth / Prefs.UIScale * 0.9f, minSize.x, maxSize.x);
        var height = Mathf.Clamp(Prefs.ScreenHeight / Prefs.UIScale * 0.9f, minSize.y, maxSize.y);
        return new Vector2(width, height);
    }
}