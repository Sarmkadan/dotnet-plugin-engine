#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace PluginEngine.Utils.Extensions;

/// <summary>
/// Extension methods for enum operations in the plugin system.
/// Provides utilities for enum conversion and description extraction.
/// </summary>
public static class EnumExtensions
{
    /// <summary>
    /// Gets the description of an enum value from its Display attribute.
    /// Falls back to the enum name if no description is found.
    /// </summary>
    /// <param name="value">The enum value to get description for.</param>
    /// <returns>The description from <see cref="System.ComponentModel.DescriptionAttribute"/> if present; otherwise the enum name.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/></exception>
    public static string GetDescription(this Enum value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var field = value.GetType().GetField(value.ToString());
        if (field is null)
            return value.ToString();

        var attribute = field.GetCustomAttribute<System.ComponentModel.DescriptionAttribute>();
        return attribute?.Description ?? value.ToString();
    }

    /// <summary>
    /// Gets the display name of an enum value from Display attribute.
    /// </summary>
    /// <param name="value">The enum value to get display name for.</param>
    /// <returns>The display name from <see cref="System.ComponentModel.DataAnnotations.DisplayAttribute"/> if present; otherwise the enum name.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/></exception>
    public static string GetDisplayName(this Enum value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var field = value.GetType().GetField(value.ToString());
        if (field is null)
            return value.ToString();

        var attribute = field.GetCustomAttribute<System.ComponentModel.DataAnnotations.DisplayAttribute>();
        return attribute?.Name ?? value.ToString();
    }

    /// <summary>
    /// Converts plugin status to user-friendly text.
    /// </summary>
    /// <param name="status">The plugin status to convert.</param>
    /// <returns>A user-friendly string representation of the status.</returns>
    public static string ToUserFriendlyString(this PluginStatus status)
    {
        return status switch
        {
            PluginStatus.Unloaded => "Not Loaded",
            PluginStatus.Loading => "Loading...",
            PluginStatus.Loaded => "Loaded",
            PluginStatus.Unloading => "Unloading...",
            PluginStatus.Failed => "Error",
            PluginStatus.Inactive => "Disabled",
            _ => status.ToString()
        };
    }

    /// <summary>
    /// Converts plugin status to HTML CSS class name.
    /// </summary>
    /// <param name="status">The plugin status to convert.</param>
    /// <returns>A CSS class name representing the status.</returns>
    public static string ToCssClass(this PluginStatus status)
    {
        return status switch
        {
            PluginStatus.Loaded => "status-loaded",
            PluginStatus.Unloaded => "status-unloaded",
            PluginStatus.Failed => "status-error",
            PluginStatus.Loading => "status-loading",
            PluginStatus.Inactive => "status-disabled",
            _ => "status-unknown"
        };
    }

    /// <summary>
    /// Determines if a plugin status represents a healthy state.
    /// </summary>
    /// <param name="status">The plugin status to check.</param>
    /// <returns><see langword="true"/> if the status is <see cref="PluginStatus.Loaded"/>; otherwise <see langword="false"/>.</returns>
    public static bool IsHealthy(this PluginStatus status)
    {
        return status == PluginStatus.Loaded;
    }

    /// <summary>
    /// Determines if a plugin status is a transient state.
    /// </summary>
    /// <param name="status">The plugin status to check.</param>
    /// <returns><see langword="true"/> if the status is <see cref="PluginStatus.Loading"/> or <see cref="PluginStatus.Unloading"/>; otherwise <see langword="false"/>.</returns>
    public static bool IsTransient(this PluginStatus status)
    {
        return status == PluginStatus.Loading || status == PluginStatus.Unloading;
    }

    /// <summary>
    /// Gets all enum values of a specific type.
    /// </summary>
    /// <typeparam name="T">The enum type to get values for.</typeparam>
    /// <returns>An enumerable of all enum values.</returns>
    /// <exception cref="ArgumentException"><typeparamref name="T"/> is not an enum type.</exception>
    public static IEnumerable<T> GetAllValues<T>() where T : Enum
    {
        return Enum.GetValues(typeof(T)).Cast<T>();
    }

    /// <summary>
    /// Converts a string to an enum value with case-insensitive matching.
    /// </summary>
    /// <typeparam name="T">The enum type to parse.</typeparam>
    /// <param name="value">The string value to parse.</param>
    /// <returns>The parsed enum value if successful; otherwise <see langword="null"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/></exception>
    public static T? TryParse<T>(string value) where T : struct, Enum
    {
        ArgumentNullException.ThrowIfNull(value);

        if (string.IsNullOrEmpty(value))
            return null;

        try
        {
            return (T)Enum.Parse(typeof(T), value, ignoreCase: true);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Gets the int value of an enum member.
    /// </summary>
    /// <param name="value">The enum value to convert.</param>
    /// <returns>The integer representation of the enum value.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/></exception>
    public static int GetIntValue(this Enum value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return Convert.ToInt32(value);
    }

    /// <summary>
    /// Creates a user-friendly list of enum values with descriptions.
    /// </summary>
    /// <typeparam name="T">The enum type to get values and descriptions for.</typeparam>
    /// <returns>A list of tuples containing enum values and their descriptions.</returns>
    /// <exception cref="ArgumentException"><typeparamref name="T"/> is not an enum type.</exception>
    public static List<(T Value, string Description)> GetValueDescriptions<T>()
        where T : Enum
    {
        return Enum.GetValues(typeof(T))
            .Cast<T>()
            .Select(e => (e, e.GetDescription()))
            .ToList();
    }

    /// <summary>
    /// Converts execution state to a color code for UI display.
    /// </summary>
    /// <param name="state">The execution state to convert.</param>
    /// <returns>A hex color code representing the execution state.</returns>
    public static string ToColorHex(this ExecutionState state)
    {
        return state switch
        {
            ExecutionState.Running => "#0066CC", // Blue
            ExecutionState.Completed => "#00AA00", // Green
            ExecutionState.Failed => "#CC0000", // Red
            ExecutionState.Cancelled => "#999999", // Gray
            ExecutionState.Timeout => "#FF6600", // Orange
            _ => "#000000"
        };
    }

    /// <summary>
    /// Determines if an execution state represents completion.
    /// </summary>
    /// <param name="state">The execution state to check.</param>
    /// <returns><see langword="true"/> if the state is terminal; otherwise <see langword="false"/>.</returns>
    public static bool IsTerminal(this ExecutionState state)
    {
        return state is ExecutionState.Completed or
               ExecutionState.Failed or
               ExecutionState.Cancelled or
               ExecutionState.Timeout;
    }
}