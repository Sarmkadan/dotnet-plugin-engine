#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace PluginEngine.Utils.Validators;

/// <summary>
/// Validator for plugin metadata and configuration.
/// Enforces naming conventions, version requirements, and dependency constraints.
/// Provides detailed validation reports with specific error messages.
/// </summary>
public sealed class PluginValidator
{
    private readonly ILogger<PluginValidator> _logger;
    private readonly VersionHelper _versionHelper;

    public PluginValidator(ILogger<PluginValidator> logger, VersionHelper versionHelper)
    {
        _logger = logger;
        _versionHelper = versionHelper;
    }

    /// <summary>
    /// Validates a plugin entity for basic correctness.
    /// Returns a validation result with detailed error messages.
    /// </summary>
    public PluginValidationResult Validate(Plugin plugin)
    {
        var errors = new List<string>();

        ValidateName(plugin.Name, errors);
        ValidateVersion(plugin.Version, errors);
        ValidateMetadata(plugin.Metadata, errors);
        ValidateDependencies(plugin.Dependencies, errors);

        return new PluginValidationResult
        {
            IsValid = errors.Count == 0,
            Errors = errors,
            PluginId = plugin.Id,
            PluginName = plugin.Name
        };
    }

    /// <summary>
    /// Validates a plugin name against naming conventions.
    /// </summary>
    private void ValidateName(string name, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            errors.Add("Plugin name cannot be empty");
            return;
        }

        if (name.Length > 100)
        {
            errors.Add($"Plugin name exceeds maximum length of 100 characters: {name.Length}");
        }

        if (name.StartsWith("System.") || name.StartsWith("Microsoft."))
        {
            errors.Add("Plugin name cannot start with reserved prefixes (System., Microsoft.)");
        }

        if (!char.IsLetterOrDigit(name[0]))
        {
            errors.Add("Plugin name must start with a letter or digit");
        }

        if (name.Any(c => !char.IsLetterOrDigit(c) && c != '-' && c != '_' && c != '.'))
        {
            errors.Add("Plugin name contains invalid characters");
        }
    }

    /// <summary>
    /// Validates a version string for semantic versioning compliance.
    /// </summary>
    private void ValidateVersion(string version, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(version))
        {
            errors.Add("Plugin version cannot be empty");
            return;
        }

        if (!_versionHelper.IsValidSemanticVersion(version))
        {
            errors.Add($"Invalid semantic version format: {version}");
        }

        var versionInfo = _versionHelper.GetVersionInfo(version);
        if (versionInfo.Major == 0 && versionInfo.Minor == 0 && versionInfo.Patch == 0)
        {
            errors.Add("Plugin version cannot be 0.0.0");
        }
    }

    /// <summary>
    /// Validates plugin metadata structure.
    /// </summary>
    private void ValidateMetadata(PluginMetadata? metadata, List<string> errors)
    {
        if (metadata is null)
            return;

        if (string.IsNullOrWhiteSpace(metadata.Description))
        {
            errors.Add("Plugin description cannot be empty");
        }

        if (string.IsNullOrWhiteSpace(metadata.Author))
        {
            errors.Add("Plugin author cannot be empty");
        }
        else if (metadata.Author.Length > 100)
        {
            errors.Add("Plugin author name exceeds maximum length");
        }
    }

    /// <summary>
    /// Validates plugin dependencies for correctness.
    /// </summary>
    private void ValidateDependencies(IReadOnlyList<PluginDependency> dependencies, List<string> errors)
    {
        if (dependencies.Count > 50)
        {
            errors.Add($"Plugin has too many dependencies: {dependencies.Count} (max 50)");
        }

        var seenIds = new HashSet<Guid>();

        foreach (var dep in dependencies)
        {
            if (string.IsNullOrWhiteSpace(dep.MinimumVersion))
            {
                errors.Add($"Dependency {dep.DependencyPluginId} has invalid minimum version constraint");
            }
            else if (!_versionHelper.IsValidSemanticVersion(dep.MinimumVersion))
            {
                errors.Add($"Dependency {dep.DependencyPluginId} has invalid minimum version: {dep.MinimumVersion}");
            }

            if (!string.IsNullOrWhiteSpace(dep.MaximumVersion))
            {
                if (!_versionHelper.IsValidSemanticVersion(dep.MaximumVersion))
                {
                    errors.Add($"Dependency {dep.DependencyPluginId} has invalid maximum version: {dep.MaximumVersion}");
                }
                else if (_versionHelper.IsValidSemanticVersion(dep.MinimumVersion) &&
                         _versionHelper.CompareVersions(dep.MaximumVersion, dep.MinimumVersion) < 0)
                {
                    errors.Add($"Dependency {dep.DependencyPluginId} maximum version {dep.MaximumVersion} cannot be less than minimum version {dep.MinimumVersion}");
                }
            }

            if (!seenIds.Add(dep.DependencyPluginId))
            {
                errors.Add($"Duplicate dependency: {dep.DependencyPluginId}");
            }
        }
    }

    /// <summary>
    /// Validates a dependency relationship between two plugins.
    /// </summary>
    public bool ValidateDependencyRelationship(Plugin dependent, Plugin dependency, PluginDependency depSpec)
    {
        if (dependent.Id == dependency.Id)
        {
            _logger.LogError("Plugin cannot depend on itself: {PluginId}", dependent.Id);
            return false;
        }

        if (!_versionHelper.SatisfiesConstraint(dependency.Version, depSpec.MinimumVersion))
        {
            _logger.LogError(
                "Dependency version mismatch: {DependentPlugin} requires {Constraint}, but {DependencyPlugin} is v{ActualVersion}",
                dependent.Name, depSpec.MinimumVersion, dependency.Name, dependency.Version);
            return false;
        }

        return true;
    }
}

/// <summary>
/// Represents the result of a plugin validation.
/// </summary>
public sealed class PluginValidationResult
{
    public required Guid PluginId { get; set; }
    public required string PluginName { get; set; }
    public required bool IsValid { get; set; }
    public required List<string> Errors { get; set; }

    public string GetErrorSummary() => string.Join("\n  ", Errors);
}
