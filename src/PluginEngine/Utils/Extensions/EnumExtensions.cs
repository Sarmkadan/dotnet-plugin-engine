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
    public static string GetDescription(this Enum value)
    {
        var field = value.GetType().GetField(value.ToString());
        if (field == null)
            return value.ToString();

        var attribute = field.GetCustomAttribute<System.ComponentModel.DescriptionAttribute>();
        return attribute?.Description ?? value.ToString();
    }

    /// <summary>
    /// Gets the display name of an enum value from Display attribute.
    /// </summary>
    public static string GetDisplayName(this Enum value)
    {
        var field = value.GetType().GetField(value.ToString());
        if (field == null)
            return value.ToString();

        var attribute = field.GetCustomAttribute<System.ComponentModel.DataAnnotations.DisplayAttribute>();
        return attribute?.Name ?? value.ToString();
    }

    /// <summary>
    /// Converts plugin status to user-friendly text.
    /// </summary>
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
    public static bool IsHealthy(this PluginStatus status)
    {
        return status == PluginStatus.Loaded;
    }

    /// <summary>
    /// Determines if a plugin status is a transient state.
    /// </summary>
    public static bool IsTransient(this PluginStatus status)
    {
        return status == PluginStatus.Loading || status == PluginStatus.Unloading;
    }

    /// <summary>
    /// Gets all enum values of a specific type.
    /// </summary>
    public static IEnumerable<T> GetAllValues<T>() where T : Enum
    {
        return Enum.GetValues(typeof(T)).Cast<T>();
    }

    /// <summary>
    /// Converts a string to an enum value with case-insensitive matching.
    /// </summary>
    public static T? TryParse<T>(string value) where T : struct, Enum
    {
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
    public static int GetIntValue(this Enum value)
    {
        try
        {
            return Convert.ToInt32(value);
        }
        catch
        {
            return 0;
        }
    }

    /// <summary>
    /// Creates a user-friendly list of enum values with descriptions.
    /// </summary>
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
    public static string ToColorHex(this ExecutionState state)
    {
        return state switch
        {
            ExecutionState.Running => "#0066CC",   // Blue
            ExecutionState.Completed => "#00AA00",  // Green
            ExecutionState.Failed => "#CC0000",     // Red
            ExecutionState.Cancelled => "#999999",  // Gray
            ExecutionState.Timeout => "#FF6600",    // Orange
            _ => "#000000"
        };
    }

    /// <summary>
    /// Determines if an execution state represents completion.
    /// </summary>
    public static bool IsTerminal(this ExecutionState state)
    {
        return state is ExecutionState.Completed or
                       ExecutionState.Failed or
                       ExecutionState.Cancelled or
                       ExecutionState.Timeout;
    }
}
