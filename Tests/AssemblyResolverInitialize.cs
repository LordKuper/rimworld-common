using System;
using System.Reflection;

namespace LordKuper.Common.Tests;

/// <summary>
///     Initializes the RimWorld assembly resolver at the earliest possible moment
///     (when this class is first referenced), before test discovery or coverage instrumentation.
///     This is referenced from the test project's project file to ensure it's loaded early.
/// </summary>
internal static class AssemblyResolverInitialize
{
    static AssemblyResolverInitialize()
    {
        // Register the assembly resolver before anything else tries to load RimWorld assemblies
        AppDomain.CurrentDomain.AssemblyResolve += ResolveRimWorldAssembly;
    }

    /// <summary>
    ///     Dummy property to force the static constructor to run when referenced.
    /// </summary>
    internal static bool Ready { get; } = true;

    private static Assembly? ResolveRimWorldAssembly(object? sender, ResolveEventArgs args)
    {
        var assemblyName = new AssemblyName(args.Name);

        // Only handle RimWorld assemblies
        if (!IsRimWorldAssembly(assemblyName.Name))
            return null;

        var rimWorldDir = Environment.GetEnvironmentVariable("RIMWORLD_DIR")
                       ?? Environment.GetEnvironmentVariable("RimWorldDir")
                       ?? "D:\\Games\\Steam\\steamapps\\common\\RimWorld";

        var managedDir = Path.Combine(rimWorldDir, "RimWorldWin64_Data", "Managed");
        var assemblyPath = Path.Combine(managedDir, $"{assemblyName.Name}.dll");

        if (File.Exists(assemblyPath))
        {
            try
            {
                return Assembly.LoadFrom(assemblyPath);
            }
            catch
            {
                return null;
            }
        }

        return null;
    }

    private static bool IsRimWorldAssembly(string assemblyName)
    {
        return assemblyName == "Assembly-CSharp"
            || assemblyName == "Assembly-CSharp-firstpass"
            || assemblyName.StartsWith("UnityEngine", StringComparison.Ordinal)
            || assemblyName == "Unity.Burst"
            || assemblyName == "Unity.Collections"
            || assemblyName == "Unity.Mathematics"
            || assemblyName == "com.rlabrecque.steamworks.net";
    }
}
