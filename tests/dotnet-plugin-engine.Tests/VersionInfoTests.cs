using System;
using PluginEngine.Domain.Entities;
using Xunit;

namespace PluginEngine.Tests
{
    public class VersionInfoTests
    {
        [Fact]
        public void Constructor_Sets_Default_Values()
        {
            var vi = new VersionInfo();

            Assert.NotEqual(Guid.Empty, vi.Id);
            Assert.Equal(Guid.Empty, vi.EntityId);
            Assert.Equal("1.0.0", vi.Version);
            Assert.NotEqual(default, vi.ReleaseDate);
            Assert.Equal(string.Empty, vi.ReleaseNotes);
            Assert.False(vi.IsPrerelease);
            Assert.Equal(string.Empty, vi.PrereleaseIdentifier);
            Assert.Equal(string.Empty, vi.BuildMetadata);
            Assert.Equal(string.Empty, vi.Compatibility);
            Assert.True(vi.IsActive);
            Assert.Equal(string.Empty, vi.DeprecationNotice);
            Assert.Equal(0, vi.DownloadCount);
        }

        [Fact]
        public void GetSemanticVersion_Basic_Returns_Version()
        {
            var vi = new VersionInfo { Version = "2.3.4" };
            var result = vi.GetSemanticVersion();

            Assert.Equal("2.3.4", result);
        }

        [Fact]
        public void GetSemanticVersion_WithPrereleaseAndBuild_Returns_Combined()
        {
            var vi = new VersionInfo
            {
                Version = "1.2.3",
                IsPrerelease = true,
                PrereleaseIdentifier = "beta",
                BuildMetadata = "exp.sha.5114f85"
            };

            var result = vi.GetSemanticVersion();

            Assert.Equal("1.2.3-beta+exp.sha.5114f85", result);
        }

        [Fact]
        public void GetDisplayString_Includes_Prerelase_And_Deprecation()
        {
            var vi = new VersionInfo
            {
                Version = "3.0.0",
                IsPrerelease = true,
                PrereleaseIdentifier = "rc",
                DeprecationNotice = "Use newer version"
            };

            var result = vi.GetDisplayString();

            Assert.Contains("3.0.0-rc", result);
            Assert.Contains("(pre-release)", result);
            Assert.Contains("[DEPRECATED]", result);
        }

        [Fact]
        public void IsCompatibleWith_SameMajor_Returns_True()
        {
            var vi = new VersionInfo { Version = "4.5.6" };
            Assert.True(vi.IsCompatibleWith("4.0.0"));
        }

        [Fact]
        public void IsCompatibleWith_DifferentMajor_Returns_False()
        {
            var vi = new VersionInfo { Version = "4.5.6" };
            Assert.False(vi.IsCompatibleWith("5.0.0"));
        }

        [Fact]
        public void IsCompatibleWith_NullOrEmpty_Returns_False()
        {
            var vi = new VersionInfo { Version = "1.0.0" };
            Assert.False(vi.IsCompatibleWith(null));
            Assert.False(vi.IsCompatibleWith(string.Empty));
        }

        [Fact]
        public void IncrementPatch_Updates_Build_Number()
        {
            var vi = new VersionInfo { Version = "1.2.3" };
            vi.IncrementPatch();
            Assert.Equal("1.2.4", vi.Version);
        }

        [Fact]
        public void IncrementMinor_Resets_Build_And_Increments_Minor()
        {
            var vi = new VersionInfo { Version = "1.2.3" };
            vi.IncrementMinor();
            Assert.Equal("1.3.0", vi.Version);
        }

        [Fact]
        public void IncrementMajor_Resets_Minor_And_Build()
        {
            var vi = new VersionInfo { Version = "1.2.3" };
            vi.IncrementMajor();
            Assert.Equal("2.0.0", vi.Version);
        }

        [Fact]
        public void IsValid_Returns_False_When_EntityId_Is_Empty()
        {
            var vi = new VersionInfo { Version = "1.0.0", EntityId = Guid.Empty };
            Assert.False(vi.IsValid());
        }

        [Fact]
        public void IsValid_Returns_True_When_All_Required_Fields_Are_Set()
        {
            var vi = new VersionInfo
            {
                Version = "2.1.0",
                EntityId = Guid.NewGuid()
            };
            Assert.True(vi.IsValid());
        }

        [Fact]
        public void IsValid_Returns_False_When_Version_Is_Invalid()
        {
            var vi = new VersionInfo
            {
                Version = "invalid.version",
                EntityId = Guid.NewGuid()
            };
            Assert.False(vi.IsValid());
        }
    }
}
