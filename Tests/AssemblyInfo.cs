using Xunit;
using LordKuper.Common.Tests;

// Force the AssemblyResolverInitialize static constructor to run before test discovery
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA1806", Justification = "Forces static constructor")]

[assembly: TestFramework("LordKuper.Common.Tests.RimWorldTestFramework", "LordKuper.Common.Tests")]

// Trigger the assembly resolver initialization
internal static class _InitializerTrigger
{
    static _InitializerTrigger() => _ = AssemblyResolverInitialize.Ready;
}
