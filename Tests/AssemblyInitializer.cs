using System;
using System.Reflection;
using Xunit;

namespace LordKuper.Common.Tests;

/// <summary>
///     Initializes the RimWorld assembly resolver and defines a collection marker for all tests
///     that use RimWorld context. This fixture runs once per test collection, ensuring that
///     Assembly-CSharp and UnityEngine modules are available for all tests in the collection.
/// </summary>
public class AssemblyInitializerFixture : IDisposable
{
    public AssemblyInitializerFixture()
    {
        // Initialize the assembly resolver when this fixture is created
        // (which happens before any test in the collection runs)
        RegisterRimWorldAssemblyResolver();
    }

    public void Dispose()
    {
        // No cleanup needed
    }

    private static void RegisterRimWorldAssemblyResolver()
    {
        AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
        {
            var assemblyName = new AssemblyName(args.Name);
            var rimWorldDir = Environment.GetEnvironmentVariable("RIMWORLD_DIR")
                           ?? Environment.GetEnvironmentVariable("RimWorldDir")
                           ?? "D:\\Games\\Steam\\steamapps\\common\\RimWorld";

            var managedDir = Path.Combine(rimWorldDir, "RimWorldWin64_Data", "Managed");
            var assemblyPath = Path.Combine(managedDir, $"{assemblyName.Name}.dll");

            if (File.Exists(assemblyPath) && IsRimWorldAssembly(assemblyName.Name))
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
        };
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

/// <summary>
///     Collection definition that uses the <see cref="AssemblyInitializerFixture" />.
///     All test classes that reference RimWorld types should use this collection.
/// </summary>
[CollectionDefinition("RimWorldContext", DisableParallelization = true)]
public class RimWorldContextCollection : ICollectionFixture<AssemblyInitializerFixture>
{
    // Marker only
}
