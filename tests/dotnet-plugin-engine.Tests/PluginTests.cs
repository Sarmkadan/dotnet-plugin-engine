using System;
using System.Linq;
using PluginEngine.Domain.Entities;
using Xunit;

namespace dotnet_plugin_engine.Tests;

public class PluginTests
{
    [Fact]
    public void NewPlugin_HasDefaultValues_AndIsInvalid()
    {
        var plugin = new Plugin();

        Assert.Equal(Guid.Empty, plugin.Id);
        Assert.Equal(string.Empty, plugin.Name);
        Assert.Equal(string.Empty, plugin.Description);
        Assert.Equal("1.0.0", plugin.Version);
        Assert.Equal(string.Empty, plugin.Author);
        Assert.Equal(string.Empty, plugin.AssemblyPath);
        Assert.Equal(PluginStatus.Unloaded, plugin.Status);
        Assert.True(plugin.SupportsHotReload);
        Assert.Empty(plugin.Dependencies);
        Assert.Empty(plugin.Capabilities);
        Assert.False(plugin.IsValid());

        var error = plugin.GetValidationError();
        Assert.Equal("Plugin name is required.", error);
    }

    [Fact]
    public void SettingRequiredProperties_MakesPluginValid()
    {
        var plugin = new Plugin
        {
            Id = Guid.NewGuid(),
            Name = "Test Plugin",
            Version = "2.3.4",
            AssemblyPath = "/tmp/plugin.dll"
        };

        Assert.True(plugin.IsValid());
        Assert.Equal(string.Empty, plugin.GetValidationError());
    }

    [Fact]
    public void AddDependency_Null_ThrowsArgumentNullException()
    {
        var plugin = new Plugin();
        Assert.Throws<ArgumentNullException>(() => plugin.AddDependency(null!));
    }

    [Fact]
    public void AddAndRemoveDependency_WorksCorrectly()
    {
        var plugin = new Plugin();
        var dep = new PluginDependency
        {
            Id = Guid.NewGuid(),
            DependencyPluginId = Guid.NewGuid(),
            VersionConstraint = ">=1.0.0"
        };

        plugin.AddDependency(dep);
        Assert.Single(plugin.Dependencies);
        Assert.Contains(dep, plugin.Dependencies);

        var removed = plugin.RemoveDependency(dep.Id);
        Assert.True(removed);
        Assert.Empty(plugin.Dependencies);
    }

    [Fact]
    public void RemoveDependency_NonExisting_ReturnsFalse()
    {
        var plugin = new Plugin();
        var result = plugin.RemoveDependency(Guid.NewGuid());
        Assert.False(result);
    }

    [Fact]
    public void AddCapability_Null_ThrowsArgumentNullException()
    {
        var plugin = new Plugin();
        Assert.Throws<ArgumentNullException>(() => plugin.AddCapability(null!));
    }

    [Fact]
    public void AddCapability_Duplicate_IsIgnored()
    {
        var plugin = new Plugin();
        var cap = new PluginCapability { Name = "FeatureX" };
        plugin.AddCapability(cap);
        plugin.AddCapability(new PluginCapability { Name = "FeatureX" }); // duplicate

        Assert.Single(plugin.Capabilities);
        Assert.Equal("FeatureX", plugin.Capabilities.First().Name);
    }

    [Fact]
    public void GetValidationError_ReturnsFirstMissingField()
    {
        var plugin = new Plugin(); // all missing
        var error1 = plugin.GetValidationError();
        Assert.Equal("Plugin name is required.", error1);

        plugin.Name = "Name";
        var error2 = plugin.GetValidationError();
        Assert.Equal("Plugin version is required.", error2);

        plugin.Version = "1.0.0";
        var error3 = plugin.GetValidationError();
        Assert.Equal("Assembly path is required.", error3);

        plugin.AssemblyPath = "path.dll";
        var error4 = plugin.GetValidationError();
        Assert.Equal("Plugin ID cannot be empty.", error4);
    }
}
