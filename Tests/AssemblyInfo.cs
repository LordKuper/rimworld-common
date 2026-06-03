using System.Diagnostics.CodeAnalysis;

// Force the AssemblyResolverInitialize static constructor to run before test discovery
[assembly: SuppressMessage("Usage", "CA1806", Justification = "Forces static constructor")]
[assembly: TestFramework("LordKuper.Common.Tests.RimWorldTestFramework", "LordKuper.Common.Tests")]

// Trigger the assembly resolver initialization
namespace LordKuper.Common.Tests;

internal static class InitializerTrigger
{
    static InitializerTrigger()
    {
        _ = AssemblyResolverInitialize.Ready;
    }
}