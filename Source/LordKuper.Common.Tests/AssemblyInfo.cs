// Assembly-level xUnit test-framework selection.
// RimWorldTestFramework (XunitExtensions.cs) registers the AssemblyResolve handler for RimWorld
// assemblies in its constructor, which runs before any test discovery or execution. No additional
// initializer trigger is needed. (Finding 2 / simplification iter-01)
[assembly: TestFramework("LordKuper.Common.Tests.RimWorldTestFramework", "LordKuper.Common.Tests")]
