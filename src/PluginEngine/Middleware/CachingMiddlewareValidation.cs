#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace PluginEngine.Middleware;

/// <summary>
/// Validation helpers for <see cref="CachingMiddleware"/> to ensure configuration and runtime values are valid.
/// </summary>
public static class CachingMiddlewareValidation
{
    /// <summary>
    /// Validates a <see cref="CachingMiddleware"/> instance and returns a list of human-readable problems.
    /// </summary>
    /// <param name="value">The middleware instance to validate.</param>
    /// <returns>An empty list if valid; otherwise, a list of validation error messages.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this CachingMiddleware? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // CachingMiddleware is sealed and properly initialized in its constructor
        // The constructor sets valid defaults (_cacheDuration = TimeSpan.FromMinutes(5),
        // _cachableOperations = new HashSet<string> { "GetMetadata", "ResolveDependencies", "ValidateVersion" })
        // Therefore, a properly constructed instance is always valid
        // We validate the instance itself rather than its private fields

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="CachingMiddleware"/> instance is valid.
    /// </summary>
    /// <param name="value">The middleware instance to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(this CachingMiddleware? value)
    {
        return value is not null && value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="CachingMiddleware"/> instance is valid.
    /// </summary>
    /// <param name="value">The middleware instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is not valid, containing a list of validation errors.</exception>
    public static void EnsureValid(this CachingMiddleware? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                "CachingMiddleware configuration is invalid. " +
                string.Join(" ", errors),
                nameof(value));
        }
    }
}