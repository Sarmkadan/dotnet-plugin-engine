using System;
using PluginEngine.Domain.Entities;
using Xunit;

namespace PluginEngine.Tests
{
    public class PluginMetadataTests
    {
        [Fact]
        public void Constructor_InitializesWithDefaultValues()
        {
            var metadata = new PluginMetadata();

            Assert.NotEqual(Guid.Empty, metadata.Id);
            Assert.Equal(Guid.Empty, metadata.PluginId);
            Assert.Equal(string.Empty, metadata.PluginName);
            Assert.Equal(string.Empty, metadata.PluginVersion);
            Assert.Equal(string.Empty, metadata.AssemblyName);
            Assert.Equal(string.Empty, metadata.AssemblyVersion);
            Assert.Equal("net10.0", metadata.TargetFramework);
            Assert.Equal(string.Empty, metadata.Author);
            Assert.Equal(string.Empty, metadata.Company);
            Assert.Equal(string.Empty, metadata.Description);
            Assert.Equal(string.Empty, metadata.License);
            Assert.Equal(string.Empty, metadata.RepositoryUrl);
            Assert.Equal("10.0", metadata.MinimumClrVersion);
            Assert.Equal(string.Empty, metadata.EngineVersionConstraint);
            Assert.False(metadata.IsSigned);
            Assert.Equal(string.Empty, metadata.PublicKeyToken);
            Assert.NotEqual(default, metadata.CreatedAt);
            Assert.NotNull(metadata.CustomProperties);
        }

        [Fact]
        public void SetCustomProperty_AddsOrUpdatesValue()
        {
            var metadata = new PluginMetadata();

            metadata.SetCustomProperty("key1", "value1");
            Assert.Equal("value1", metadata.GetCustomProperty("key1"));

            metadata.SetCustomProperty("key1", "updatedValue");
            Assert.Equal("updatedValue", metadata.GetCustomProperty("key1"));

            metadata.SetCustomProperty("key2", null);
            Assert.Equal(string.Empty, metadata.GetCustomProperty("key2"));
        }

        [Fact]
        public void GetCustomProperty_ReturnsNullForMissingKey()
        {
            var metadata = new PluginMetadata();
            Assert.Null(metadata.GetCustomProperty("nonexistent"));
        }

        [Fact]
        public void RemoveCustomProperty_RemovesExistingKey()
        {
            var metadata = new PluginMetadata();
            metadata.SetCustomProperty("key1", "value1");
            Assert.True(metadata.RemoveCustomProperty("key1"));
            Assert.Null(metadata.GetCustomProperty("key1"));
        }

        [Fact]
        public void RemoveCustomProperty_ReturnsFalseForMissingKey()
        {
            var metadata = new PluginMetadata();
            Assert.False(metadata.RemoveCustomProperty("nonexistent"));
        }

        [Fact]
        public void ClearCustomProperties_RemovesAll()
        {
            var metadata = new PluginMetadata();
            metadata.SetCustomProperty("key1", "value1");
            metadata.SetCustomProperty("key2", "value2");
            Assert.Equal(2, metadata.CustomProperties.Count);
            metadata.ClearCustomProperties();
            Assert.Empty(metadata.CustomProperties);
        }

        [Fact]
        public void GetSummary_ReturnsFormattedString()
        {
            var metadata = new PluginMetadata
            {
                PluginName = "TestPlugin",
                PluginVersion = "1.2.3",
                AssemblyName = "TestPlugin.Assembly",
                AssemblyVersion = "1.2.3.0",
                Description = "A test plugin"
            };

            var summary = metadata.GetSummary();
            Assert.Contains("TestPlugin v1.2.3", summary);
            Assert.Contains("TestPlugin.Assembly v1.2.3.0", summary);
            Assert.Contains("A test plugin", summary);
        }

        [Fact]
        public void IsValid_ReturnsFalseWhenRequiredFieldsAreEmpty()
        {
            var metadata = new PluginMetadata
            {
                PluginId = Guid.Empty,
                PluginName = string.Empty,
                PluginVersion = string.Empty,
                AssemblyName = string.Empty,
                TargetFramework = string.Empty
            };

            Assert.False(metadata.IsValid());
        }

        [Fact]
        public void IsValid_ReturnsTrueWhenRequiredFieldsAreSet()
        {
            var metadata = new PluginMetadata
            {
                PluginId = Guid.NewGuid(),
                PluginName = "TestPlugin",
                PluginVersion = "1.0.0",
                AssemblyName = "TestPlugin.Assembly",
                TargetFramework = "net10.0"
            };

            Assert.True(metadata.IsValid());
        }
    }
}