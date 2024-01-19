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
public class FileSystemHelper
{
    private readonly ILogger<FileSystemHelper> _logger;

    public FileSystemHelper(ILogger<FileSystemHelper> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Ensures a directory exists, creating it if necessary.
    /// </summary>
    public bool EnsureDirectoryExists(string path)
    {
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
    public IEnumerable<string> DiscoverPlugins(string directory)
    {
        try
        {
            if (!Directory.Exists(directory))
                return [];

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
                return null;

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
    public bool SafeCopyFile(string source, string destination, bool overwrite = false)
    {
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
    public bool DeleteDirectoryRecursive(string path)
    {
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
    public long GetDirectorySize(string path)
    {
        try
        {
            if (!Directory.Exists(path))
                return 0;

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
    public string? CreateBackup(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
                return null;

            var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
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
    public bool IsDirectoryWritable(string path)
    {
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
