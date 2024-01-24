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
    public static bool ImplementsInterface<T>(this Type type) where T : class
    {
        return typeof(T).IsAssignableFrom(type) &&
               type != typeof(T) &&
               !type.IsInterface;
    }

    /// <summary>
    /// Finds all types in an assembly that implement a specific interface.
    /// </summary>
    public static IEnumerable<Type> GetTypesImplementing<T>(this Assembly assembly) where T : class
    {
        var interfaceType = typeof(T);
        return assembly.GetLoadableTypes()
            .Where(t => interfaceType.IsAssignableFrom(t) &&
                       !t.IsInterface &&
                       !t.IsAbstract);
    }

    /// <summary>
    /// Gets all loadable types from an assembly, handling exceptions gracefully.
    /// </summary>
    public static IEnumerable<Type> GetLoadableTypes(this Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            return ex.Types.Where(t => t != null)!;
        }
    }

    /// <summary>
    /// Determines if a type has a specific attribute.
    /// </summary>
    public static bool HasAttribute<T>(this Type type) where T : Attribute
    {
        return type.GetCustomAttribute<T>() != null;
    }

    /// <summary>
    /// Gets the custom attribute value from a type.
    /// </summary>
    public static T? GetAttribute<T>(this Type type) where T : Attribute
    {
        return type.GetCustomAttribute<T>();
    }

    /// <summary>
    /// Determines if a type is a concrete class that can be instantiated.
    /// </summary>
    public static bool IsConcreteClass(this Type type)
    {
        return !type.IsAbstract &&
               !type.IsInterface &&
               !type.IsGenericTypeDefinition &&
               type.IsClass;
    }

    /// <summary>
    /// Gets the full metadata about a type including namespace and assembly.
    /// </summary>
    public static string GetFullMetadata(this Type type)
    {
        return $"{type.FullName} ({type.Assembly.GetName().Name})";
    }

    /// <summary>
    /// Determines if a type can be serialized to JSON.
    /// </summary>
    public static bool IsJsonSerializable(this Type type)
    {
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
    public static Dictionary<string, object?> GetPropertyValues(this object obj)
    {
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
