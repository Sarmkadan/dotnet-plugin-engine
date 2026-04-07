#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace PluginEngine.Exceptions;

/// <summary>
/// Exception thrown when a plugin fails to load.
/// </summary>
public sealed class PluginLoadException : PluginException
{
    /// <summary>
    /// Gets or sets the plugin name.
    /// </summary>
    public string PluginName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the assembly path.
    /// </summary>
    public string AssemblyPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the load error stage.
    /// </summary>
    public PluginLoadStage LoadStage { get; set; } = PluginLoadStage.Unknown;

    /// <summary>
    /// Initializes a new instance of the PluginLoadException class.
    /// </summary>
    public PluginLoadException() : base()
    {
        ErrorCode = "PLUGIN_LOAD_ERROR";
    }

    /// <summary>
    /// Initializes a new instance with a message.
    /// </summary>
    public PluginLoadException(string message) : base(message)
    {
        ErrorCode = "PLUGIN_LOAD_ERROR";
    }

    /// <summary>
    /// Initializes a new instance with a message and inner exception.
    /// </summary>
    public PluginLoadException(string message, Exception innerException)
        : base(message, innerException)
    {
        ErrorCode = "PLUGIN_LOAD_ERROR";
    }

    /// <summary>
    /// Initializes a new instance with full details.
    /// </summary>
    public PluginLoadException(string message, string pluginName, string assemblyPath, PluginLoadStage stage)
        : base(message)
    {
        ErrorCode = "PLUGIN_LOAD_ERROR";
        PluginName = pluginName;
        AssemblyPath = assemblyPath;
        LoadStage = stage;
    }

    /// <summary>
    /// Initializes a new instance with full details and inner exception.
    /// </summary>
    public PluginLoadException(string message, string pluginName, string assemblyPath, PluginLoadStage stage, Exception innerException)
        : base(message, innerException)
    {
        ErrorCode = "PLUGIN_LOAD_ERROR";
        PluginName = pluginName;
        AssemblyPath = assemblyPath;
        LoadStage = stage;
    }

    /// <summary>
    /// Gets a detailed error description.
    /// </summary>
    public override string ToString()
    {
        var result = base.ToString();
        result = $"Plugin: {PluginName}\nStage: {LoadStage}\nPath: {AssemblyPath}\n{result}";
        return result;
    }
}

/// <summary>
/// Represents the stage at which a plugin load failed.
/// </summary>
public enum PluginLoadStage
{
    Unknown = 0,
    MetadataValidation = 1,
    AssemblyResolution = 2,
    TypeLoading = 3,
    InterfaceValidation = 4,
    Instantiation = 5,
    Initialization = 6,
    DependencyResolution = 7
}
