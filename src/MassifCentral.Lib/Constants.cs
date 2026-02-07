namespace MassifCentral.Lib;

/// <summary>
/// Contains application-wide constants used throughout the system.
/// 
/// IMPORTANT: All public constants are defined as <c>static readonly</c> to prevent compile-time inlining.
/// This ensures that all consuming assemblies reference the actual value from the library, not a hardcoded copy.
/// If constants were defined as <c>const</c>, they would be inlined at compile time, causing consuming applications
/// to use stale values if the library is updated without recompiling the consumer.
/// 
/// For more information, see: docs/CONST_VISIBILITY_ANALYSIS.md
/// </summary>
public static class Constants
{
    /// <summary>
    /// The application version following semantic versioning (Major.Minor.Patch).
    /// 
    /// Updated when releasing new versions. Using <c>static readonly</c> ensures all
    /// applications reference the current version without requiring rebuilds.
    /// </summary>
    public static readonly string Version = "1.0.0";

    /// <summary>
    /// The application name used for identification, logging, and display purposes.
    /// 
    /// Consistent across all projects in the solution. Using <c>static readonly</c>
    /// allows for future centralization of this value if needed.
    /// </summary>
    public static readonly string ApplicationName = "MassifCentral";
}
