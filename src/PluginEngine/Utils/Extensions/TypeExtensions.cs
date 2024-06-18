#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace PluginEngine.Utils.Extensions;

/// <summary>
/// Extension methods for reflection and type operations in the plugin system.
/// Provides utilities for discovering interfaces, attributes, and type metadata.
/// </summary>
public static class TypeExtensions
{
    /// <summary>
    /// Determines if a type implements a specific interface, including base types.
    /// </summary>
    /// <typeparam name="T">The interface type to check for.</typeparam>
    /// <param name="type">The type to check.</param>
    /// <returns>True if the type implements the interface; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="type"/> is null.</exception>
    public static bool ImplementsInterface<T>(this Type type) where T : class
    {
        ArgumentNullException.ThrowIfNull(type);

        return typeof(T).IsAssignableFrom(type) &&
               type != typeof(T) &&
               !type.IsInterface;
    }

    /// <summary>
    /// Finds all types in an assembly that implement a specific interface.
    /// </summary>
    /// <typeparam name="T">The interface type to search for.</typeparam>
    /// <param name="assembly">The assembly to search.</param>
    /// <returns>Collection of types implementing the interface.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="assembly"/> is null.</exception>
    public static IEnumerable<Type> GetTypesImplementing<T>(this Assembly assembly) where T : class
    {
        ArgumentNullException.ThrowIfNull(assembly);

        var interfaceType = typeof(T);
        return assembly.GetLoadableTypes()
            .Where(t => interfaceType.IsAssignableFrom(t) &&
                        !t.IsInterface &&
                        !t.IsAbstract);
    }

    /// <summary>
    /// Gets all loadable types from an assembly, handling exceptions gracefully.
    /// </summary>
    /// <param name="assembly">The assembly to get types from.</param>
    /// <returns>Collection of loadable types.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="assembly"/> is null.</exception>
    public static IEnumerable<Type> GetLoadableTypes(this Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(assembly);

        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            return ex.Types.Where(t => t is not null)!;
        }
    }

    /// <summary>
    /// Determines if a type has a specific attribute.
    /// </summary>
    /// <typeparam name="T">The attribute type to check for.</typeparam>
    /// <param name="type">The type to check.</param>
    /// <returns>True if the type has the attribute; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="type"/> is null.</exception>
    public static bool HasAttribute<T>(this Type type) where T : Attribute
    {
        ArgumentNullException.ThrowIfNull(type);

        return type.GetCustomAttribute<T>() is not null;
    }

    /// <summary>
    /// Gets the custom attribute value from a type.
    /// </summary>
    /// <typeparam name="T">The attribute type to retrieve.</typeparam>
    /// <param name="type">The type to check.</param>
    /// <returns>The attribute instance, or null if not found.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="type"/> is null.</exception>
    public static T? GetAttribute<T>(this Type type) where T : Attribute
    {
        ArgumentNullException.ThrowIfNull(type);

        return type.GetCustomAttribute<T>();
    }

    /// <summary>
    /// Determines if a type is a concrete class that can be instantiated.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns>True if the type is a concrete class; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="type"/> is null.</exception>
    public static bool IsConcreteClass(this Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        return !type.IsAbstract &&
               !type.IsInterface &&
               !type.IsGenericTypeDefinition &&
               type.IsClass;
    }

    /// <summary>
    /// Gets the full metadata about a type including namespace and assembly.
    /// </summary>
    /// <param name="type">The type to get metadata for.</param>
    /// <returns>String containing type metadata.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="type"/> is null.</exception>
    public static string GetFullMetadata(this Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        return $"{type.FullName} ({type.Assembly.GetName().Name})";
    }

    /// <summary>
    /// Determines if a type can be serialized to JSON.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns>True if the type is JSON serializable; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="type"/> is null.</exception>
    public static bool IsJsonSerializable(this Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        if (type.IsPrimitive || type == typeof(string) || type == typeof(decimal))
            return true;

        if (type.IsGenericType && (
            type.GetGenericTypeDefinition() == typeof(List<>) ||
            type.GetGenericTypeDefinition() == typeof(Dictionary<,>) ||
            type.GetGenericTypeDefinition() == typeof(IEnumerable<>)))
            return true;

        return !type.IsInterface && !type.IsAbstract;
    }

    /// <summary>
    /// Gets public property values from an object as a dictionary.
    /// </summary>
    /// <param name="obj">The object to get properties from.</param>
    /// <returns>Dictionary of property names and values.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="obj"/> is null.</exception>
    public static Dictionary<string, object?> GetPropertyValues(this object obj)
    {
        ArgumentNullException.ThrowIfNull(obj);

        var properties = new Dictionary<string, object?>();

        foreach (var property in obj.GetType().GetProperties(
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
        {
            try
            {
                properties[property.Name] = property.GetValue(obj);
            }
            catch
            {
                properties[property.Name] = null;
            }
        }

        return properties;
    }
}