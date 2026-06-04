using System.Reflection;

// Global (namespace-less) NUnit SetUpFixture so this fixture applies to the whole assembly.
// NUnit runs the [OneTimeSetUp] method here before constructing or running any test fixture in
// the assembly, which gives the resolver-live-before-type-load guarantee needed so that
// RimWorld / Unity assemblies can be resolved at discovery time.
[SetUpFixture]
public class RimWorldResolverSetup
{
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
        // Fail fast with an actionable message when the RimWorld directory is not configured.
        // The managed dir must exist before we can resolve RimWorld assemblies.
        var rimWorldDir = Environment.GetEnvironmentVariable("RIMWORLD_DIR") ??
                          Environment.GetEnvironmentVariable("RimWorldDir");
        if (rimWorldDir == null)
            throw new InvalidOperationException(
                "RimWorld directory is not configured. " +
                "Set the RIMWORLD_DIR (or RimWorldDir) environment variable to the RimWorld installation folder " +
                "before running tests (e.g. RIMWORLD_DIR=C:\\Program Files\\Steam\\steamapps\\common\\RimWorld).");

        var managedDir = Path.Combine(rimWorldDir, "RimWorldWin64_Data", "Managed");
        if (!Directory.Exists(managedDir))
            throw new InvalidOperationException(
                $"RimWorld Managed directory not found at '{managedDir}'. " +
                "Ensure RIMWORLD_DIR (or RimWorldDir) points to a valid RimWorld installation.");

        AppDomain.CurrentDomain.AssemblyResolve += (_, args) =>
        {
            var assemblyName = new AssemblyName(args.Name);

            // Only intercept RimWorld / Unity assemblies; let everything else resolve normally.
            if (!IsRimWorldAssembly(assemblyName.Name))
                return null;
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
