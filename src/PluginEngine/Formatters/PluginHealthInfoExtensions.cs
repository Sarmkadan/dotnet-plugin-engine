#nullable enable

namespace PluginEngine.Formatters;

/// <summary>
/// Extension methods for <see cref="PluginHealthInfo"/> providing
/// convenience checks on plugin health state.
/// </summary>
public static class PluginHealthInfoExtensions
{
    /// <summary>
    /// Determines whether the plugin health info contains any reported issues.
    /// </summary>
    /// <param name="health">The plugin health info to check.</param>
    /// <returns><c>true</c> if the <see cref="PluginHealthInfo.Issues"/> list is non-empty; otherwise <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="health"/> is <c>null</c>.</exception>
    public static bool HasIssues(this PluginHealthInfo health)
    {
        ArgumentNullException.ThrowIfNull(health);
        return health.Issues.Count > 0;
    }

    /// <summary>
    /// Returns a semicolon-separated summary of all issues, or an empty string when there are none.
    /// </summary>
    /// <param name="health">The plugin health info whose issues to summarise.</param>
    /// <returns>A string joining each issue with <c>"; "</c>, or <see cref="string.Empty"/> if no issues exist.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="health"/> is <c>null</c>.</exception>
    public static string GetIssueSummary(this PluginHealthInfo health)
    {
        ArgumentNullException.ThrowIfNull(health);
        return health.Issues.Count == 0
            ? string.Empty
            : string.Join("; ", health.Issues);
    }

    /// <summary>
    /// Determines whether the plugin's last access time is older than the specified maximum age
    /// relative to <see cref="DateTime.UtcNow"/>.
    /// </summary>
    /// <param name="health">The plugin health info to check.</param>
    /// <param name="maxAge">The maximum acceptable age since <see cref="PluginHealthInfo.LastAccessedUtc"/>.</param>
    /// <returns><c>true</c> if the elapsed time since <see cref="PluginHealthInfo.LastAccessedUtc"/> exceeds <paramref name="maxAge"/>; otherwise <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="health"/> is <c>null</c>.</exception>
    public static bool IsStale(this PluginHealthInfo health, TimeSpan maxAge)
    {
        ArgumentNullException.ThrowIfNull(health);
        return DateTime.UtcNow - health.LastAccessedUtc > maxAge;
    }
}
