#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace PluginEngine.Exceptions;

/// <summary>
/// Base exception for all plugin engine related errors.
/// </summary>
public class PluginException : Exception
{
    /// <summary>
    /// Gets or sets the error code.
    /// </summary>
    public string ErrorCode { get; set; } = "PLUGIN_ERROR";

    /// <summary>
    /// Gets or sets the entity ID related to this error.
    /// </summary>
    public Guid? EntityId { get; set; }

    /// <summary>
    /// Gets or sets additional context information.
    /// </summary>
    public Dictionary<string, object> Context { get; set; } = new();

    /// <summary>
    /// Initializes a new instance of the PluginException class.
    /// </summary>
    public PluginException() : base()
    {
    }

    /// <summary>
    /// Initializes a new instance with a message.
    /// </summary>
    public PluginException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance with a message and error code.
    /// </summary>
    public PluginException(string message, string errorCode) : base(message)
    {
        ErrorCode = errorCode;
    }

    /// <summary>
    /// Initializes a new instance with a message and inner exception.
    /// </summary>
    public PluginException(string message, Exception innerException) : base(message, innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance with a message, error code, and inner exception.
    /// </summary>
    public PluginException(string message, string errorCode, Exception innerException)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
    }

    /// <summary>
    /// Gets a detailed error description.
    /// </summary>
    public override string ToString()
    {
        var result = $"[{ErrorCode}] {Message}";

        if (EntityId.HasValue)
            result += $" (Entity: {EntityId})";

        if (Context.Count > 0)
        {
            result += " | Context: ";
            var contextItems = string.Join(", ", Context.Select(kv => $"{kv.Key}={kv.Value}"));
            result += contextItems;
        }

        if (InnerException is not null)
            result += $"\nInner: {InnerException.Message}";

        return result;
    }

    /// <summary>
    /// Adds context information to the exception.
    /// </summary>
    public PluginException WithContext(string key, object value)
    {
        Context[key] = value;
        return this;
    }

    /// <summary>
    /// Sets the entity ID for this exception.
    /// </summary>
    public PluginException WithEntityId(Guid entityId)
    {
        EntityId = entityId;
        return this;
    }
}
