#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace PluginEngine.Utils.Helpers;

/// <summary>
/// Service for discovering plugins on the file system.
/// Scans directories for valid plugin assemblies and extracts metadata.
/// </summary>
public sealed class PluginDiscoveryService
{
    private readonly FileSystemHelper _fileSystemHelper;
    private readonly VersionHelper _versionHelper;
    private readonly ILogger<PluginDiscoveryService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="PluginDiscoveryService"/> class.
    /// </summary>
    /// <param name="fileSystemHelper">The file system helper used for discovering plugin files.</param>
    /// <param name="versionHelper">The version helper used for comparing plugin versions.</param>
    /// <param name="logger">The logger for recording discovery events.</param>
    public PluginDiscoveryService(
        FileSystemHelper fileSystemHelper,
        VersionHelper versionHelper,
        ILogger<PluginDiscoveryService> logger)
    {
        _fileSystemHelper = fileSystemHelper;
        _versionHelper = versionHelper;
        _logger = logger;
    }

    /// <summary>
    /// Discovers all plugins in a directory.
    /// </summary>
    public async Task<List<PluginCandidateInfo>> DiscoverPluginsAsync(string directory)
    {
        try
        {
            if (!Directory.Exists(directory))
            {
                _logger.LogWarning("Plugin directory does not exist: {Path}", directory);
                return [];
            }

            var pluginFiles = _fileSystemHelper.DiscoverPlugins(directory).ToList();

            if (pluginFiles.Count == 0)
            {
                _logger.LogInformation("No plugins found in directory: {Path}", directory);
                return [];
            }

            _logger.LogInformation("Discovered {Count} potential plugins in {Path}", pluginFiles.Count, directory);

            var candidates = new List<PluginCandidateInfo>();

            foreach (var filePath in pluginFiles)
            {
                var candidate = await InspectPluginAsync(filePath);
                if (candidate is not null)
                {
                    candidates.Add(candidate);
                }
            }

            return candidates;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error discovering plugins in directory: {Path}", directory);
            return [];
        }
    }

    /// <summary>
    /// Inspects a single plugin file for metadata.
    /// </summary>
    public async Task<PluginCandidateInfo?> InspectPluginAsync(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                _logger.LogWarning("Plugin file not found: {FilePath}", filePath);
                return null;
            }

            var fileInfo = _fileSystemHelper.GetFileInfo(filePath);
            if (fileInfo is null)
                return null;

            var assemblyName = Path.GetFileNameWithoutExtension(filePath);
            var (size, modified) = fileInfo.Value;

            // Attempt to load assembly metadata
            Assembly? assembly = null;
            try
            {
                assembly = Assembly.LoadFrom(filePath);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not load assembly for inspection: {FilePath}", filePath);
            }

            var candidate = new PluginCandidateInfo
            {
                FilePath = filePath,
                FileName = Path.GetFileName(filePath),
                AssemblyName = assemblyName,
                FileSize = size,
                ModifiedAtUtc = modified,
                IsValid = assembly is not null,
                DiscoveredAtUtc = DateTime.UtcNow
            };

            if (assembly is not null)
            {
                ExtractAssemblyMetadata(assembly, candidate);
            }

            return candidate;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inspecting plugin: {FilePath}", filePath);
            return null;
        }
    }

    /// <summary>
    /// Filters discovered plugins based on criteria.
    /// </summary>
    public List<PluginCandidateInfo> FilterPlugins(
        List<PluginCandidateInfo> candidates,
        PluginDiscoveryFilter? filter = null)
    {
        filter ??= new PluginDiscoveryFilter();

        return candidates
            .Where(c => filter.ValidOnly ? c.IsValid : true)
            .Where(c => filter.MinimumVersionInfo is null ||
                       c.Version is not null &&
                       _versionHelper.CompareVersions(c.Version, filter.MinimumVersionInfo) >= 0)
            .Where(c => filter.NamePattern is null ||
                       System.Text.RegularExpressions.Regex.IsMatch(c.AssemblyName, filter.NamePattern))
            .Where(c => filter.MaxFileSizeBytes is null || c.FileSize <= filter.MaxFileSizeBytes)
            .ToList();
    }

    private void ExtractAssemblyMetadata(Assembly assembly, PluginCandidateInfo candidate)
    {
        try
        {
            // Extract version
            var versionAttr = assembly.GetCustomAttribute<AssemblyVersionAttribute>();
            if (versionAttr is not null)
            {
                candidate.Version = versionAttr.Version;
            }

            // Extract product name
            var productAttr = assembly.GetCustomAttribute<AssemblyProductAttribute>();
            if (productAttr is not null)
            {
                candidate.ProductName = productAttr.Product;
            }

            // Extract company
            var companyAttr = assembly.GetCustomAttribute<AssemblyCompanyAttribute>();
            if (companyAttr is not null)
            {
                candidate.Company = companyAttr.Company;
            }

            // Extract description
            var descAttr = assembly.GetCustomAttribute<AssemblyDescriptionAttribute>();
            if (descAttr is not null)
            {
                candidate.Description = descAttr.Description;
            }

            // Extract custom attributes
            candidate.CustomAttributes.AddRange(
                assembly.GetCustomAttributes()
                    .Select(a => a.GetType().Name)
            );
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Error extracting assembly metadata");
        }
    }

    /// <summary>
    /// Gets discovery statistics.
    /// </summary>
    public DiscoveryStatistics GetStatistics(List<PluginCandidateInfo> candidates)
    {
        return new DiscoveryStatistics
        {
            TotalCandidates = candidates.Count,
            ValidPlugins = candidates.Count(c => c.IsValid),
            InvalidPlugins = candidates.Count(c => !c.IsValid),
            TotalSizeBytes = candidates.Sum(c => c.FileSize),
            DiscoveredAtUtc = DateTime.UtcNow
        };
    }
}

/// <summary>
/// Information about a discovered plugin candidate.
/// </summary>
public sealed class PluginCandidateInfo
{
    /// <summary>
    /// Gets or sets the full path to the plugin file.
    /// </summary>
    public required string FilePath { get; set; }
    /// <summary>
    /// Gets or sets the name of the plugin file.
    /// </summary>
    public required string FileName { get; set; }
    /// <summary>
    /// Gets or sets the name of the assembly.
    /// </summary>
    public required string AssemblyName { get; set; }
    /// <summary>
    /// Gets or sets the size of the plugin file in bytes.
    /// </summary>
    public required long FileSize { get; set; }
    /// <summary>
    /// Gets or sets the last modified date and time of the plugin file in UTC.
    /// </summary>
    public required DateTime ModifiedAtUtc { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether the plugin is valid.
    /// </summary>
    public required bool IsValid { get; set; }
    /// <summary>
    /// Gets or sets the date and time the plugin was discovered in UTC.
    /// </summary>
    public required DateTime DiscoveredAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the version of the plugin.
    /// </summary>
    public string? Version { get; set; }
    /// <summary>
    /// Gets or sets the product name of the plugin.
    /// </summary>
    public string? ProductName { get; set; }
    /// <summary>
    /// Gets or sets the company that produced the plugin.
    /// </summary>
    public string? Company { get; set; }
    /// <summary>
    /// Gets or sets the description of the plugin.
    /// </summary>
    public string? Description { get; set; }
    /// <summary>
    /// Gets the list of custom attributes found in the plugin assembly.
    /// </summary>
    public List<string> CustomAttributes { get; } = [];
}

/// <summary>
/// Filter criteria for plugin discovery.
/// </summary>
public sealed class PluginDiscoveryFilter
{
    /// <summary>
    /// Gets or sets a value indicating whether to filter only valid plugins.
    /// </summary>
    public bool ValidOnly { get; set; } = true;
    /// <summary>
    /// Gets or sets the minimum version string to filter plugins by.
    /// </summary>
    public string? MinimumVersionInfo { get; set; }
    /// <summary>
    /// Gets or sets the regular expression pattern to filter plugins by name.
    /// </summary>
    public string? NamePattern { get; set; }
    /// <summary>
    /// Gets or sets the maximum allowed file size in bytes for a plugin.
    /// </summary>
    public long? MaxFileSizeBytes { get; set; }
}

/// <summary>
/// Statistics from plugin discovery.
/// </summary>
public sealed class DiscoveryStatistics
{
    /// <summary>
    /// Gets or sets the total number of plugin candidates discovered.
    /// </summary>
    public int TotalCandidates { get; set; }
    /// <summary>
    /// Gets or sets the number of valid plugins discovered.
    /// </summary>
    public int ValidPlugins { get; set; }
    /// <summary>
    /// Gets or sets the number of invalid plugins discovered.
    /// </summary>
    public int InvalidPlugins { get; set; }
    /// <summary>
    /// Gets or sets the total size of all discovered plugins in bytes.
    /// </summary>
    public long TotalSizeBytes { get; set; }
    /// <summary>
    /// Gets or sets the date and time the statistics were generated in UTC.
    /// </summary>
    public DateTime DiscoveredAtUtc { get; set; }

    /// <summary>
    /// Gets the percentage of valid plugins out of the total candidates.
    /// </summary>
    public double ValidPercentage => TotalCandidates > 0
        ? (ValidPlugins / (double)TotalCandidates) * 100
        : 0;
}
