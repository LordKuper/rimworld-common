using System;
using System.Reflection;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace LordKuper.Common.Tests;

/// <summary>
///     Custom xUnit test framework that initializes the RimWorld assembly resolver
///     before any test discovery or execution occurs.
/// </summary>
public class RimWorldTestFramework : XunitTestFramework
{
    public RimWorldTestFramework(IMessageSink messageSink) : base(messageSink)
    {
        // Initialize the RimWorld assembly resolver when the framework is created
        InitializeRimWorldResolver();
    }

    private static void InitializeRimWorldResolver()
    {
        // Register the assembly resolver only once
        if (AppDomain.CurrentDomain.GetData("RimWorldResolverInitialized") != null)
            return;

        AppDomain.CurrentDomain.SetData("RimWorldResolverInitialized", true);

        AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
        {
            var assemblyName = new AssemblyName(args.Name);

            // Only handle RimWorld assemblies - let other resolution proceed normally
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
///     Configuration attribute that specifies the custom test framework.
///     Apply this to the test assembly via:
///     [assembly: TestFramework("LordKuper.Common.Tests.RimWorldTestFramework", "LordKuper.Common.Tests")]
/// </summary>
[AttributeUsage(AttributeTargets.Assembly)]
public class RimWorldTestFrameworkAttribute : Attribute;
