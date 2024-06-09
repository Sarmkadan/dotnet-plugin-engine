namespace PluginEngine.Domain.Entities;

/// <summary>
/// Provides extension methods for <see cref="PluginMetadata"/>.
/// </summary>
public static class PluginMetadataExtensions
{
    /// <summary>
    /// Determines whether the plugin metadata has a custom property with the specified key.
    /// </summary>
    /// <param name="metadata">The plugin metadata.</param>
    /// <param name="key">The key of the custom property.</param>
    /// <returns><c>true</c> if the plugin metadata has a custom property with the specified key; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="metadata"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="key"/> is <c>null</c> or empty.</exception>
    public static bool HasCustomProperty(this PluginMetadata metadata, string key)
    {
        ArgumentNullException.ThrowIfNull(metadata);
        ArgumentException.ThrowIfNullOrEmpty(key);

        return metadata.GetCustomProperty(key) != null;
    }

    /// <summary>
    /// Gets all custom properties of the plugin metadata.
    /// </summary>
    /// <param name="metadata">The plugin metadata.</param>
    /// <returns>An <see cref="IReadOnlyDictionary{TKey,TValue}"/> containing all custom properties.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="metadata"/> is <c>null</c>.</exception>
    public static IReadOnlyDictionary<string, string> GetAllCustomProperties(this PluginMetadata metadata)
    {
        ArgumentNullException.ThrowIfNull(metadata);

        var customProperties = new Dictionary<string, string>();

        // assume we don't know the exact number of custom properties
        // this could be optimized if we knew the number

        for (int i = 0; i < 100; i++)
        {
            var key = $"customProperty{i}";
            var value = metadata.GetCustomProperty(key);
            if (value != null)
            {
                customProperties.Add(key, value);
            }
            else
            {
                break;
            }
        }

        // We have to actually walk through GetCustomProperty calls
        var keys = new List<string>();
        for (int i = 0; ; i++)
        {
            var key = $"customProperty{i}";
            if (metadata.GetCustomProperty(key) == null)
                break;
            keys.Add(key);
        }

        return keys.ToDictionary(key => key, key => metadata.GetCustomProperty(key)!);
    }

    /// <summary>
    /// Clears all custom properties of the plugin metadata.
    /// </summary>
    /// <param name="metadata">The plugin metadata.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="metadata"/> is <c>null</c>.</exception>
    public static void ClearCustomProperties(this PluginMetadata metadata)
    {
        ArgumentNullException.ThrowIfNull(metadata);

        var keys = new List<string>();
        for (int i = 0; ; i++)
        {
            var key = $"customProperty{i}";
            if (metadata.GetCustomProperty(key) == null)
                break;
            keys.Add(key);
        }

        foreach (var key in keys)
        {
            metadata.RemoveCustomProperty(key);
        }
    }
}
