using System.Reflection;
using System.Runtime.CompilerServices;

// net472 polyfill: [ModuleInitializer] is a C# 9 / .NET 5+ built-in. The compiler only needs
// the attribute to exist in the right namespace; the CLR honours it on net472 as long as the
// compiler emits the .cctor wiring. This stub is internal, compile-only, and invisible to callers.
namespace System.Runtime.CompilerServices
{
    [AttributeUsage(AttributeTargets.Method)]
    internal sealed class ModuleInitializerAttribute : Attribute { }
}

// Global (namespace-less) NUnit SetUpFixture so this fixture applies to the whole assembly.
// NUnit runs the [OneTimeSetUp] method here before constructing or running any test fixture in
// the assembly, which gives the same "resolver live before type load" guarantee the previous
// xUnit [assembly: TestFramework] hook provided.
//
// ADR-0006 fallback: the [ModuleInitializer] below registers the resolver at module-load time,
// before the NUnit engine can call GetTypes() on the assembly. This is the documented contingency
// for the accepted risk that [OneTimeSetUp] runs at execution time (not discovery time) and cannot
// provably precede discovery-time type loading. Both paths share the same body; the idempotency
// guard ensures the handler is added exactly once regardless of which fires first.
[SetUpFixture]
public class RimWorldResolverSetup
{
    // Registers the resolver at CLR module-load time — before NUnit's engine calls GetTypes().
    // This is the ADR-0006 contingency path; [OneTimeSetUp] below is the NUnit-idiomatic path.
    // The idempotency guard inside RegisterRimWorldResolver() makes coexistence safe.
    [ModuleInitializer]
    public static void InitializeModule()
    {
        RegisterRimWorldResolver();
    }

    private static bool IsRimWorldAssembly(string? assemblyName)
    {
        return assemblyName == "Assembly-CSharp" || assemblyName == "Assembly-CSharp-firstpass" ||
               (assemblyName != null &&
                assemblyName.StartsWith("UnityEngine", StringComparison.Ordinal)) ||
               assemblyName == "Unity.Burst" || assemblyName == "Unity.Collections" ||
               assemblyName == "Unity.Mathematics" ||
               assemblyName == "com.rlabrecque.steamworks.net";
    }

    [OneTimeSetUp]
    public static void RegisterRimWorldResolver()
    {
        // Register the RimWorld assembly resolver only once across any registration path.
        // The idempotency guard is justified: both this [OneTimeSetUp] and the [ModuleInitializer]
        // above may run in the same process; the guard ensures the handler is added exactly once
        // regardless of which path fires first.
        if (AppDomain.CurrentDomain.GetData("RimWorldResolverInitialized") != null)
            return;
        AppDomain.CurrentDomain.SetData("RimWorldResolverInitialized", true);
        AppDomain.CurrentDomain.AssemblyResolve += (_, args) =>
        {
            var assemblyName = new AssemblyName(args.Name);

            // Only intercept RimWorld / Unity assemblies; let everything else resolve normally.
            if (!IsRimWorldAssembly(assemblyName.Name))
                return null;
            var rimWorldDir = Environment.GetEnvironmentVariable("RIMWORLD_DIR") ??
                              Environment.GetEnvironmentVariable("RimWorldDir") ??
                              "D:\\Games\\Steam\\steamapps\\common\\RimWorld";
            var managedDir = Path.Combine(rimWorldDir, "RimWorldWin64_Data", "Managed");
            var assemblyPath = Path.Combine(managedDir, $"{assemblyName.Name}.dll");
            if (File.Exists(assemblyPath))
                try
                {
                    return Assembly.LoadFrom(assemblyPath);
                }
                catch
                {
                    return null;
                }
            return null;
        };
    }
}