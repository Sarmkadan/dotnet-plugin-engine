#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using PluginEngine.Domain.Entities;
using Xunit;

namespace PluginEngine.Tests;

public sealed class PluginEntityTests
{
    private static Plugin CreateValidPlugin(string name = "TestPlugin", string version = "1.0.0")
    {
        return new Plugin
        {
            Id = Guid.NewGuid(),
            Name = name,
            Version = version,
            AssemblyPath = "/plugins/test.dll"
        };
    }

    private static PluginDependency CreateDependency(
        Guid? pluginId = null,
        Guid? dependencyPluginId = null,
        string minimumVersion = "1.0.0",
        string? maximumVersion = null)
    {
        return new PluginDependency
        {
            PluginId = pluginId ?? Guid.NewGuid(),
            DependencyPluginId = dependencyPluginId ?? Guid.NewGuid(),
            MinimumVersion = minimumVersion,
            MaximumVersion = maximumVersion ?? string.Empty
        };
    }

    // ── Plugin entity ──────────────────────────────────────────────────

    [Fact]
    public void AddDependency_WithNullDependency_ThrowsArgumentNullException()
    {
        var plugin = CreateValidPlugin();

        var act = () => plugin.AddDependency(null!);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("dependency");
    }

    [Fact]
    public void AddDependency_WithValidDependency_AppendsToDependencyList()
    {
        var plugin = CreateValidPlugin();
        var dependency = CreateDependency(pluginId: plugin.Id);

        plugin.AddDependency(dependency);

        plugin.Dependencies.Should().ContainSingle()
            .Which.DependencyPluginId.Should().Be(dependency.DependencyPluginId);
    }

    [Fact]
    public void AddCapability_WithNullCapability_ThrowsArgumentNullException()
    {
        var plugin = CreateValidPlugin();

        var act = () => plugin.AddCapability(null!);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("capability");
    }

    [Fact]
    public void AddCapability_WithDuplicateName_DoesNotAddSecondEntry()
    {
        var plugin = CreateValidPlugin();
        var capabilityA = new PluginCapability { PluginId = plugin.Id, Name = "DataTransform", Version = "1.0.0", InterfaceTypeName = "ITransform" };
        var capabilityB = new PluginCapability { PluginId = plugin.Id, Name = "DataTransform", Version = "2.0.0", InterfaceTypeName = "ITransform" };

        plugin.AddCapability(capabilityA);
        plugin.AddCapability(capabilityB);

        plugin.Capabilities.Should().HaveCount(1);
    }

    [Fact]
    public void RemoveDependency_WhenDependencyExists_RemovesAndReturnsTrue()
    {
        var plugin = CreateValidPlugin();
        var dependency = CreateDependency(pluginId: plugin.Id);
        plugin.AddDependency(dependency);

        var removed = plugin.RemoveDependency(dependency.Id);

        removed.Should().BeTrue();
        plugin.Dependencies.Should().BeEmpty();
    }

    [Fact]
    public void RemoveDependency_WithNonExistentId_ReturnsFalse()
    {
        var plugin = CreateValidPlugin();

        var removed = plugin.RemoveDependency(Guid.NewGuid());

        removed.Should().BeFalse();
    }

    [Fact]
    public void IsValid_WithAllRequiredFields_ReturnsTrue()
    {
        var plugin = CreateValidPlugin();

        plugin.IsValid().Should().BeTrue();
    }

    [Fact]
    public void IsValid_WithEmptyName_ReturnsFalse()
    {
        var plugin = CreateValidPlugin();
        plugin.Name = string.Empty;

        plugin.IsValid().Should().BeFalse();
    }

    [Fact]
    public void IsValid_WithEmptyGuid_ReturnsFalse()
    {
        var plugin = CreateValidPlugin();
        plugin.Id = Guid.Empty;

        plugin.IsValid().Should().BeFalse();
    }

    [Fact]
    public void GetValidationError_WithMissingAssemblyPath_ReturnsDescriptiveMessage()
    {
        var plugin = CreateValidPlugin();
        plugin.AssemblyPath = string.Empty;

        var error = plugin.GetValidationError();

        error.Should().Contain("Assembly path");
    }

    // ── PluginDependency entity ────────────────────────────────────────

    [Fact]
    public void PluginDependency_IsSatisfiedBy_VersionAtMinimum_ReturnsTrue()
    {
        var dep = CreateDependency(minimumVersion: "2.0.0");

        dep.IsSatisfiedBy("2.0.0").Should().BeTrue();
    }

    [Fact]
    public void PluginDependency_IsSatisfiedBy_VersionAboveMinimum_ReturnsTrue()
    {
        var dep = CreateDependency(minimumVersion: "1.0.0");

        dep.IsSatisfiedBy("2.5.0").Should().BeTrue();
    }

    [Fact]
    public void PluginDependency_IsSatisfiedBy_VersionBelowMinimum_ReturnsFalse()
    {
        var dep = CreateDependency(minimumVersion: "2.0.0");

        dep.IsSatisfiedBy("1.9.9").Should().BeFalse();
    }

    [Fact]
    public void PluginDependency_IsSatisfiedBy_VersionExceedsMaximum_ReturnsFalse()
    {
        var dep = CreateDependency(minimumVersion: "1.0.0", maximumVersion: "1.5.0");

        dep.IsSatisfiedBy("2.0.0").Should().BeFalse();
    }

    [Fact]
    public void PluginDependency_IsSatisfiedBy_WithInvalidVersionString_ReturnsFalse()
    {
        var dep = CreateDependency(minimumVersion: "1.0.0");

        dep.IsSatisfiedBy("not-a-version").Should().BeFalse();
    }

    [Fact]
    public void PluginDependency_GetVersionConstraint_WithoutMaximum_ReturnsGreaterThanOrEqualExpression()
    {
        var dep = CreateDependency(minimumVersion: "3.2.1");

        var constraint = dep.GetVersionConstraint();

        constraint.Should().Be(">= 3.2.1");
    }

    [Fact]
    public void PluginDependency_GetVersionConstraint_WithMaximum_IncludesBothBounds()
    {
        var dep = CreateDependency(minimumVersion: "1.0.0", maximumVersion: "2.0.0");

        var constraint = dep.GetVersionConstraint();

        constraint.Should().Contain(">=").And.Contain("<=").And.Contain("2.0.0");
    }

    // ── PluginCapability entity ────────────────────────────────────────

    [Fact]
    public void PluginCapability_HasTag_WithMatchingTagIgnoringCase_ReturnsTrue()
    {
        var capability = new PluginCapability
        {
            PluginId = Guid.NewGuid(),
            Name = "Encryption",
            Version = "1.0.0",
            InterfaceTypeName = "IEncryption",
            Tags = "security,encryption,aes"
        };

        capability.HasTag("SECURITY").Should().BeTrue();
        capability.HasTag("aes").Should().BeTrue();
    }

    [Fact]
    public void PluginCapability_HasTag_WithNonExistentTag_ReturnsFalse()
    {
        var capability = new PluginCapability
        {
            PluginId = Guid.NewGuid(),
            Name = "Logging",
            Version = "1.0.0",
            InterfaceTypeName = "ILogger",
            Tags = "logging,audit"
        };

        capability.HasTag("encryption").Should().BeFalse();
    }
}
