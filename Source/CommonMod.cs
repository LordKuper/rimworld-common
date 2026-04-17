using System.Reflection;
using HarmonyLib;
using JetBrains.Annotations;
using LordKuper.Common.Compatibility;
using Verse;

namespace LordKuper.Common;

/// <summary>
///     Represents the core mod class for the LordKuper.Common mod.
///     Handles initialization and provides a unique mod identifier.
/// </summary>
[UsedImplicitly]
public class CommonMod : Mod
{
    /// <summary>
    ///     The unique identifier for the LordKuper.Common mod.
    /// </summary>
    internal const string ModId = "LordKuper.Common";

    /// <summary>
    ///     Initializes a new instance of the <see cref="CommonMod" /> class and logs the initialization message.
    /// </summary>
    /// <param name="content">The mod content pack.</param>
    public CommonMod(ModContentPack content) : base(content)
    {
        Logger.LogMessage(
            $"Initializing (v.{Assembly.GetExecutingAssembly().GetName().Version})...");
        Harmony harmony = new(ModId);
        harmony.PatchAll(Assembly.GetExecutingAssembly());
        Vse.Initialize();
    }
}