#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace PluginEngine.Domain.Entities;

/// <summary>
/// Represents an assembly that belongs to a plugin.
/// </summary>
public sealed class PluginAssembly
{
    /// <summary>
    /// Gets the unique identifier for this assembly record.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the plugin ID this assembly belongs to.
    /// </summary>
    public Guid PluginId { get; set; }

    /// <summary>
    /// Gets or sets the assembly name.
    /// </summary>
    public string AssemblyName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the assembly version.
    /// </summary>
    public string AssemblyVersion { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the full file path to the assembly.
    /// </summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the file size in bytes.
    /// </summary>
    public long FileSizeBytes { get; set; } = 0;

    /// <summary>
    /// Gets or sets the SHA256 hash of the assembly file.
    /// </summary>
    public string FileHash { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the public key token if the assembly is signed.
    /// </summary>
    public string PublicKeyToken { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether the assembly is the main plugin assembly.
    /// </summary>
    public bool IsMainAssembly { get; set; } = false;

    /// <summary>
    /// Gets or sets the assembly load context identifier.
    /// </summary>
    public string LoadContextId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the last modified timestamp.
    /// </summary>
    public DateTime LastModifiedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the load timestamp.
    /// </summary>
    public DateTime? LoadedAt { get; set; }

    /// <summary>
    /// Gets or sets the assembly load status.
    /// </summary>
    public AssemblyLoadStatus Status { get; set; } = AssemblyLoadStatus.Unloaded;

    /// <summary>
    /// Gets or sets error information if loading failed.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Validates the assembly entity.
    /// </summary>
    public bool IsValid()
    {
        return PluginId != Guid.Empty &&
               !string.IsNullOrWhiteSpace(AssemblyName) &&
               !string.IsNullOrWhiteSpace(AssemblyVersion) &&
               !string.IsNullOrWhiteSpace(FilePath);
    }

    /// <summary>
    /// Gets the assembly qualified name.
    /// </summary>
    public string GetQualifiedName()
    {
        if (string.IsNullOrWhiteSpace(PublicKeyToken))
            return $"{AssemblyName}, Version={AssemblyVersion}";

        return $"{AssemblyName}, Version={AssemblyVersion}, PublicKeyToken={PublicKeyToken}";
    }

    /// <summary>
    /// Updates the assembly file information.
    /// </summary>
    public void UpdateFileInfo(string filePath, long fileSize, string fileHash)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be empty.", nameof(filePath));

        FilePath = filePath;
        FileSizeBytes = fileSize;
        FileHash = fileHash;
        LastModifiedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Records a successful load.
    /// </summary>
    public void MarkAsLoaded(string loadContextId)
    {
        Status = AssemblyLoadStatus.Loaded;
        LoadContextId = loadContextId;
        LoadedAt = DateTime.UtcNow;
        ErrorMessage = null;
    }

    /// <summary>
    /// Records a failed load attempt.
    /// </summary>
    public void MarkAsFailedLoad(string errorMessage)
    {
        Status = AssemblyLoadStatus.Failed;
        ErrorMessage = errorMessage ?? "Unknown error.";
        LoadedAt = null;
    }
}

/// <summary>
/// Represents the load status of an assembly.
/// </summary>
public enum AssemblyLoadStatus
{
    Unloaded = 0,
    Loading = 1,
    Loaded = 2,
    Failed = 3,
    Unloading = 4
}
