#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace PluginEngine.Utils.Helpers;

/// <summary>
/// Helper class for file system operations related to plugins.
/// Provides utilities for directory management, file discovery, and cleanup.
/// Abstracts file system complexity and provides cross-platform compatibility.
/// </summary>
public sealed class FileSystemHelper
{
    private readonly ILogger<FileSystemHelper> _logger;

    /// <summary>Initializes a new instance of the <see cref="FileSystemHelper"/> class.</summary>
    public FileSystemHelper(ILogger<FileSystemHelper> logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;
    }

    /// <summary>
    /// Ensures a directory exists, creating it if necessary.
    /// </summary>
    /// <param name="path">The directory path to ensure.</param>
    /// <returns><c>true</c> if the directory exists or was created; otherwise <c>false</c>.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="path"/> is null, empty, or whitespace.</exception>
    public bool EnsureDirectoryExists(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        try
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                _logger.LogInformation("Created plugin directory: {Path}", path);
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create directory: {Path}", path);
            return false;
        }
    }

    /// <summary>
    /// Finds all plugin assemblies (.dll) in a directory.
    /// </summary>
    /// <param name="directory">The directory to search.</param>
    /// <returns>The paths of discovered plugin assemblies, or an empty sequence if none are found.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="directory"/> is null, empty, or whitespace.</exception>
    public IEnumerable<string> DiscoverPlugins(string directory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);

        try
        {
            if (!Directory.Exists(directory))
            {
                return [];
            }

            return Directory.GetFiles(directory, "*.dll", SearchOption.TopDirectoryOnly)
                .Where(f => !Path.GetFileName(f).StartsWith("_"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to discover plugins in {Directory}", directory);
            return [];
        }
    }

    /// <summary>
    /// Gets file information including size and modification time.
    /// </summary>
    public (long Size, DateTime Modified)? GetFileInfo(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                return null;
            }

            var info = new FileInfo(filePath);
            return (info.Length, info.LastWriteTimeUtc);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get file info: {FilePath}", filePath);
            return null;
        }
    }

    /// <summary>
    /// Safely copies a file with overwrite protection.
    /// </summary>
    /// <param name="source">The path of the file to copy.</param>
    /// <param name="destination">The path of the copy to create.</param>
    /// <param name="overwrite">Whether an existing destination file may be overwritten.</param>
    /// <returns><c>true</c> if the file was copied; otherwise <c>false</c>.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="source"/> or <paramref name="destination"/> is null, empty, or whitespace.</exception>
    public bool SafeCopyFile(string source, string destination, bool overwrite = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(source);
        ArgumentException.ThrowIfNullOrWhiteSpace(destination);

        try
        {
            if (!File.Exists(source))
            {
                _logger.LogWarning("Source file not found: {Source}", source);
                return false;
            }

            if (File.Exists(destination) && !overwrite)
            {
                _logger.LogWarning("Destination file exists and overwrite is disabled: {Destination}", destination);
                return false;
            }

            File.Copy(source, destination, overwrite);
            _logger.LogInformation("Copied file: {Source} -> {Destination}", source, destination);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to copy file: {Source} -> {Destination}", source, destination);
            return false;
        }
    }

    /// <summary>
    /// Recursively deletes a directory and all its contents.
    /// </summary>
    /// <param name="path">The directory to delete.</param>
    /// <returns><c>true</c> if the directory was deleted or did not exist; otherwise <c>false</c>.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="path"/> is null, empty, or whitespace.</exception>
    public bool DeleteDirectoryRecursive(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        try
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, recursive: true);
                _logger.LogInformation("Deleted directory: {Path}", path);
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete directory: {Path}", path);
            return false;
        }
    }

    /// <summary>
    /// Calculates the total size of all files in a directory.
    /// </summary>
    /// <param name="path">The directory to measure.</param>
    /// <returns>The total size in bytes, or 0 if the directory does not exist or cannot be read.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="path"/> is null, empty, or whitespace.</exception>
    public long GetDirectorySize(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        try
        {
            if (!Directory.Exists(path))
            {
                return 0;
            }

            var info = new DirectoryInfo(path);
            return info.EnumerateFiles("*", SearchOption.AllDirectories)
                .Sum(f => f.Length);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to calculate directory size: {Path}", path);
            return 0;
        }
    }

    /// <summary>
    /// Creates a backup of a file with timestamp suffix.
    /// </summary>
    /// <param name="filePath">The file to back up.</param>
    /// <returns>The path of the backup file, or <c>null</c> if the file does not exist or the backup failed.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="filePath"/> is null, empty, or whitespace.</exception>
    public string? CreateBackup(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        try
        {
            if (!File.Exists(filePath))
            {
                return null;
            }

            var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture);
            var backupPath = Path.Combine(
                Path.GetDirectoryName(filePath) ?? ".",
                $"{Path.GetFileNameWithoutExtension(filePath)}.backup.{timestamp}{Path.GetExtension(filePath)}");

            File.Copy(filePath, backupPath);
            _logger.LogInformation("Created backup: {BackupPath}", backupPath);
            return backupPath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create backup: {FilePath}", filePath);
            return null;
        }
    }

    /// <summary>
    /// Checks if a path is writable by attempting to create a temporary file.
    /// </summary>
    /// <param name="path">The directory to check.</param>
    /// <returns><c>true</c> if a test file could be written and removed; otherwise <c>false</c>.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="path"/> is null, empty, or whitespace.</exception>
    public bool IsDirectoryWritable(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        try
        {
            var testFile = Path.Combine(path, ".writetest");
            File.WriteAllText(testFile, "test");
            File.Delete(testFile);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
