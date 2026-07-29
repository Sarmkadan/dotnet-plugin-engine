using System;
using System.Collections.Generic;
using System.Linq;
using PluginEngine.Domain.Entities;
using Xunit;

namespace dotnet_plugin_engine.Tests;

public class PluginCapabilityTests
{
    [Fact]
    public void Constructor_Sets_Default_Values()
    {
        var capability = new PluginCapability();

        Assert.NotEqual(Guid.Empty, capability.Id);
        Assert.Equal(Guid.Empty, capability.PluginId);
        Assert.Equal(string.Empty, capability.Name);
        Assert.Equal("1.0.0", capability.Version);
        Assert.Equal(string.Empty, capability.InterfaceTypeName);
        Assert.Equal(string.Empty, capability.ImplementationTypeName);
        Assert.Equal(string.Empty, capability.Description);
        Assert.Equal(string.Empty, capability.Tags);
        Assert.False(capability.IsMandatory);
        Assert.True((DateTime.UtcNow - capability.CreatedAt).TotalSeconds < 5);
        Assert.True((DateTime.UtcNow - capability.ModifiedAt).TotalSeconds < 5);
    }

    [Fact]
    public void GetTags_Parses_CommaSeparated_Tags()
    {
        var capability = new PluginCapability
        {
            Tags = "alpha,  beta , , gamma,,"
        };

        var tags = capability.GetTags().ToList();

        Assert.Equal(3, tags.Count);
        Assert.Contains("alpha", tags);
        Assert.Contains("beta", tags);
        Assert.Contains("gamma", tags);
    }

    [Fact]
    public void SetTags_Null_Clears_Tags()
    {
        var capability = new PluginCapability
        {
            Tags = "shouldBeCleared"
        };

        capability.SetTags(null);

        Assert.Equal(string.Empty, capability.Tags);
        Assert.Empty(capability.GetTags());
    }

    [Fact]
    public void SetTags_Filters_Empty_And_Whitespace()
    {
        var capability = new PluginCapability();

        capability.SetTags(new[] { "first", "", "  ", "second", null! });

        Assert.Equal("first,second", capability.Tags);
        var tags = capability.GetTags().ToList();
        Assert.Equal(2, tags.Count);
        Assert.Contains("first", tags);
        Assert.Contains("second", tags);
    }

    [Fact]
    public void HasTag_Is_Case_Insensitive()
    {
        var capability = new PluginCapability
        {
            Tags = "Alpha,Beta"
        };

        Assert.True(capability.HasTag("alpha"));
        Assert.True(capability.HasTag("BETA"));
        Assert.False(capability.HasTag("gamma"));
    }

    [Fact]
    public void IsValid_Returns_True_For_Valid_Capability()
    {
        var capability = new PluginCapability
        {
            PluginId = Guid.NewGuid(),
            Name = "TestCapability",
            Version = "2.3.4",
            InterfaceTypeName = "ITestInterface"
        };

        Assert.True(capability.IsValid());
    }

    [Theory]
    [MemberData(nameof(InvalidCapabilityData))]
    public void IsValid_Returns_False_For_Invalid_Capability(PluginCapability capability)
    {
        Assert.False(capability.IsValid());
    }

    public static IEnumerable<object[]> InvalidCapabilityData()
    {
        // Missing PluginId
        yield return new object[] { new PluginCapability { PluginId = Guid.Empty, Name = "Name", Version = "1.0.0", InterfaceTypeName = "IInterface" } };
        // Missing Name
        yield return new object[] { new PluginCapability { PluginId = Guid.NewGuid(), Name = "", Version = "1.0.0", InterfaceTypeName = "IInterface" } };
        // Missing Version
        yield return new object[] { new PluginCapability { PluginId = Guid.NewGuid(), Name = "Name", Version = "", InterfaceTypeName = "IInterface" } };
        // Invalid Version format
        yield return new object[] { new PluginCapability { PluginId = Guid.NewGuid(), Name = "Name", Version = "invalid", InterfaceTypeName = "IInterface" } };
        // Missing InterfaceTypeName
        yield return new object[] { new PluginCapability { PluginId = Guid.NewGuid(), Name = "Name", Version = "1.0.0", InterfaceTypeName = "" } };
    }

    [Fact]
    public void GetDisplayName_Formats_Correctly()
    {
        var capability = new PluginCapability
        {
            Name = "SuperFeature",
            Version = "3.1.0"
        };

        var displayName = capability.GetDisplayName();

        Assert.Equal("SuperFeature v3.1.0", displayName);
    }
}
