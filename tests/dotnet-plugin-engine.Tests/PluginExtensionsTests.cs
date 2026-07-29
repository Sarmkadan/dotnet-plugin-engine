using System;
using System.Collections.Generic;
using Xunit;
using PluginEngine.Domain.Entities;

namespace PluginEngine.Tests
{
    public class PluginExtensionsTests
    {
        #region Helper

        private static Plugin CreatePlugin(
            PluginStatus status = PluginStatus.Inactive,
            string name = "TestPlugin",
            string version = "1.0.0",
            string? author = null,
            DateTime? createdAt = null,
            DateTime? modifiedAt = null,
            string? description = null,
            IEnumerable<Guid>? dependencies = null,
            IEnumerable<string>? capabilities = null)
        {
            return new Plugin
            {
                Status = status,
                Name = name,
                Version = version,
                Author = author,
                CreatedAt = createdAt ?? DateTime.UtcNow.AddDays(-10),
                ModifiedAt = modifiedAt ?? DateTime.UtcNow.AddDays(-5),
                Description = description,
                Dependencies = dependencies != null ? new List<Guid>(dependencies) : new List<Guid>(),
                Capabilities = capabilities != null ? new List<string>(capabilities) : new List<string>()
            };
        }

        #endregion

        [Fact]
        public void IsActive_WhenStatusIsActive_ReturnsTrue()
        {
            var plugin = CreatePlugin(status: PluginStatus.Active);
            Assert.True(plugin.IsActive());
        }

        [Fact]
        public void IsActive_WhenStatusIsNotActive_ReturnsFalse()
        {
            var plugin = CreatePlugin(status: PluginStatus.Failed);
            Assert.False(plugin.IsActive());
        }

        [Fact]
        public void IsActive_NullPlugin_ThrowsArgumentNullException()
        {
            Plugin? plugin = null;
            Assert.Throws<ArgumentNullException>(() => plugin!.IsActive());
        }

        [Fact]
        public void IsFailed_WhenStatusIsFailed_ReturnsTrue()
        {
            var plugin = CreatePlugin(status: PluginStatus.Failed);
            Assert.True(plugin.IsFailed());
        }

        [Fact]
        public void IsFailed_WhenStatusIsNotFailed_ReturnsFalse()
        {
            var plugin = CreatePlugin(status: PluginStatus.Active);
            Assert.False(plugin.IsFailed());
        }

        [Fact]
        public void IsFailed_NullPlugin_ThrowsArgumentNullException()
        {
            Plugin? plugin = null;
            Assert.Throws<ArgumentNullException>(() => plugin!.IsFailed());
        }

        [Fact]
        public void GetDisplayName_WithAuthor_IncludesAuthor()
        {
            var plugin = CreatePlugin(author: "Jane Doe");
            var displayName = plugin.GetDisplayName();
            Assert.Equal($"{plugin.Name} v{plugin.Version} by {plugin.Author}", displayName);
        }

        [Fact]
        public void GetDisplayName_WithoutAuthor_OmitsAuthor()
        {
            var plugin = CreatePlugin(author: null);
            var displayName = plugin.GetDisplayName();
            Assert.Equal($"{plugin.Name} v{plugin.Version}", displayName);
        }

        [Fact]
        public void GetDisplayName_NullPlugin_ThrowsArgumentNullException()
        {
            Plugin? plugin = null;
            Assert.Throws<ArgumentNullException>(() => plugin!.GetDisplayName());
        }

        [Fact]
        public void GetFormattedCreationDate_ReturnsIsoDate()
        {
            var created = new DateTime(2023, 4, 5, 12, 30, 0, DateTimeKind.Utc);
            var plugin = CreatePlugin(createdAt: created);
            var formatted = plugin.GetFormattedCreationDate();
            Assert.Equal("2023-04-05", formatted);
        }

        [Fact]
        public void GetFormattedCreationDate_NullPlugin_ThrowsArgumentNullException()
        {
            Plugin? plugin = null;
            Assert.Throws<ArgumentNullException>(() => plugin!.GetFormattedCreationDate());
        }

        [Fact]
        public void GetFormattedModificationDate_ReturnsIsoDate()
        {
            var modified = new DateTime(2023, 7, 1, 8, 15, 0, DateTimeKind.Utc);
            var plugin = CreatePlugin(modifiedAt: modified);
            var formatted = plugin.GetFormattedModificationDate();
            Assert.Equal("2023-07-01", formatted);
        }

        [Fact]
        public void GetFormattedModificationDate_NullPlugin_ThrowsArgumentNullException()
        {
            Plugin? plugin = null;
            Assert.Throws<ArgumentNullException>(() => plugin!.GetFormattedModificationDate());
        }

        [Fact]
        public void HasDependencies_WhenCollectionHasItems_ReturnsTrue()
        {
            var deps = new[] { Guid.NewGuid(), Guid.NewGuid() };
            var plugin = CreatePlugin(dependencies: deps);
            Assert.True(plugin.HasDependencies());
        }

        [Fact]
        public void HasDependencies_WhenCollectionIsEmpty_ReturnsFalse()
        {
            var plugin = CreatePlugin(dependencies: Array.Empty<Guid>());
            Assert.False(plugin.HasDependencies());
        }

        [Fact]
        public void HasDependencies_NullPlugin_ThrowsArgumentNullException()
        {
            Plugin? plugin = null;
            Assert.Throws<ArgumentNullException>(() => plugin!.HasDependencies());
        }

        [Fact]
        public void HasCapabilities_WhenCollectionHasItems_ReturnsTrue()
        {
            var caps = new[] { "CapA", "CapB" };
            var plugin = CreatePlugin(capabilities: caps);
            Assert.True(plugin.HasCapabilities());
        }

        [Fact]
        public void HasCapabilities_WhenCollectionIsEmpty_ReturnsFalse()
        {
            var plugin = CreatePlugin(capabilities: Array.Empty<string>());
            Assert.False(plugin.HasCapabilities());
        }

        [Fact]
        public void HasCapabilities_NullPlugin_ThrowsArgumentNullException()
        {
            Plugin? plugin = null;
            Assert.Throws<ArgumentNullException>(() => plugin!.HasCapabilities());
        }

        [Fact]
        public void GetMetadataSummary_WithDescription_IncludesDescription()
        {
            var plugin = CreatePlugin(description: "A sample plugin");
            var summary = plugin.GetMetadataSummary();
            var expected = $"{plugin.GetDisplayName()} - {plugin.Description}";
            Assert.Equal(expected, summary);
        }

        [Fact]
        public void GetMetadataSummary_WithoutDescription_ReturnsDisplayNameOnly()
        {
            var plugin = CreatePlugin(description: null);
            var summary = plugin.GetMetadataSummary();
            var expected = plugin.GetDisplayName();
            Assert.Equal(expected, summary);
        }

        [Fact]
        public void GetMetadataSummary_NullPlugin_ThrowsArgumentNullException()
        {
            Plugin? plugin = null;
            Assert.Throws<ArgumentNullException>(() => plugin!.GetMetadataSummary());
        }

        [Fact]
        public void Touch_UpdatesModifiedAtToNow()
        {
            var before = DateTime.UtcNow.AddHours(-1);
            var plugin = CreatePlugin(modifiedAt: before);
            plugin.Touch();
            var after = DateTime.UtcNow;
            Assert.InRange(plugin.ModifiedAt, after.AddSeconds(-1), after.AddSeconds(1));
        }

        [Fact]
        public void Touch_NullPlugin_ThrowsArgumentNullException()
        {
            Plugin? plugin = null;
            Assert.Throws<ArgumentNullException>(() => plugin!.Touch());
        }

        [Fact]
        public void IsTransitioning_WhenLoading_ReturnsTrue()
        {
            var plugin = CreatePlugin(status: PluginStatus.Loading);
            Assert.True(plugin.IsTransitioning());
        }

        [Fact]
        public void IsTransitioning_WhenUnloading_ReturnsTrue()
        {
            var plugin = CreatePlugin(status: PluginStatus.Unloading);
            Assert.True(plugin.IsTransitioning());
        }

        [Fact]
        public void IsTransitioning_WhenNotLoadingOrUnloading_ReturnsFalse()
        {
            var plugin = CreatePlugin(status: PluginStatus.Active);
            Assert.False(plugin.IsTransitioning());
        }

        [Fact]
        public void IsTransitioning_NullPlugin_ThrowsArgumentNullException()
        {
            Plugin? plugin = null;
            Assert.Throws<ArgumentNullException>(() => plugin!.IsTransitioning());
        }
    }
}
