using System;
using System.Collections.Generic;
using System.Linq;
using PluginEngine.Domain.Entities;
using Xunit;

namespace dotnet_plugin_engine.Tests
{
    public class VersionInfoValidationTests
    {
        [Fact]
        public void Validate_Happy_PATH()
        {
            // Arrange
            var versionInfo = new VersionInfo
            {
                Id = Guid.NewGuid(),
                EntityId = Guid.NewGuid(),
                Version = "1.0.0",
                ReleaseDate = DateTime.UtcNow,
                ReleaseNotes = "Release notes",
                IsPrerelease = false,
                PrereleaseIdentifier = null,
                BuildMetadata = null,
                Compatibility = null,
                DeprecationNotice = null,
                DownloadCount = 0
            };

            // Act
            var errors = VersionInfoValidation.Validate(versionInfo);

            // Assert
            Assert.Empty(errors);
        }

        [Fact]
        public void Validate_NULL_VERSION_INFO()
        {
            // Arrange
            VersionInfo versionInfo = null;

            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => VersionInfoValidation.Validate(versionInfo));
        }

        [Fact]
        public void Validate_EMPTY_GUID_ID()
        {
            // Arrange
            var versionInfo = new VersionInfo
            {
                Id = Guid.Empty,
                EntityId = Guid.NewGuid(),
                Version = "1.0.0",
                ReleaseDate = DateTime.UtcNow,
                ReleaseNotes = "Release notes",
                IsPrerelease = false,
                PrereleaseIdentifier = null,
                BuildMetadata = null,
                Compatibility = null,
                DeprecationNotice = null,
                DownloadCount = 0
            };

            // Act
            var errors = VersionInfoValidation.Validate(versionInfo);

            // Assert
            Assert.Contains("Id must not be empty.", errors);
        }

        [Fact]
        public void Validate_EMPTY_GUID_ENTITYID()
        {
            // Arrange
            var versionInfo = new VersionInfo
            {
                Id = Guid.NewGuid(),
                EntityId = Guid.Empty,
                Version = "1.0.0",
                ReleaseDate = DateTime.UtcNow,
                ReleaseNotes = "Release notes",
                IsPrerelease = false,
                PrereleaseIdentifier = null,
                BuildMetadata = null,
                Compatibility = null,
                DeprecationNotice = null,
                DownloadCount = 0
            };

            // Act
            var errors = VersionInfoValidation.Validate(versionInfo);

            // Assert
            Assert.Contains("EntityId must not be empty.", errors);
        }

        [Fact]
        public void Validate_NULL_OR_WHITESPACE_VERSION()
        {
            // Arrange
            var versionInfo = new VersionInfo
            {
                Id = Guid.NewGuid(),
                EntityId = Guid.NewGuid(),
                Version = null,
                ReleaseDate = DateTime.UtcNow,
                ReleaseNotes = "Release notes",
                IsPrerelease = false,
                PrereleaseIdentifier = null,
                BuildMetadata = null,
                Compatibility = null,
                DeprecationNotice = null,
                DownloadCount = 0
            };

            // Act
            var errors = VersionInfoValidation.Validate(versionInfo);

            // Assert
            Assert.Contains("Version must not be null or whitespace.", errors);
        }

        [Fact]
        public void Validate_INVALID_VERSION_FORMAT()
        {
            // Arrange
            var versionInfo = new VersionInfo
            {
                Id = Guid.NewGuid(),
                EntityId = Guid.NewGuid(),
                Version = "invalid.version",
                ReleaseDate = DateTime.UtcNow,
                ReleaseNotes = "Release notes",
                IsPrerelease = false,
                PrereleaseIdentifier = null,
                BuildMetadata = null,
                Compatibility = null,
                DeprecationNotice = null,
                DownloadCount = 0
            };

            // Act
            var errors = VersionInfoValidation.Validate(versionInfo);

            // Assert
            Assert.Contains("Version must be a valid semantic version string (e.g., 1.0.0).", errors);
        }

        [Fact]
        public void Validate_DEFAULT_RELEASEDATE()
        {
            // Arrange
            var versionInfo = new VersionInfo
            {
                Id = Guid.NewGuid(),
                EntityId = Guid.NewGuid(),
                Version = "1.0.0",
                ReleaseDate = default,
                ReleaseNotes = "Release notes",
                IsPrerelease = false,
                PrereleaseIdentifier = null,
                BuildMetadata = null,
                Compatibility = null,
                DeprecationNotice = null,
                DownloadCount = 0
            };

            // Act
            var errors = VersionInfoValidation.Validate(versionInfo);

            // Assert
            Assert.Contains("ReleaseDate must not be the default DateTime value.", errors);
        }

        [Fact]
        public void Validate_FUTURE_RELEASEDATE()
        {
            // Arrange
            var versionInfo = new VersionInfo
            {
                Id = Guid.NewGuid(),
                EntityId = Guid.NewGuid(),
                Version = "1.0.0",
                ReleaseDate = DateTime.UtcNow.AddDays(2),
                ReleaseNotes = "Release notes",
                IsPrerelease = false,
                PrereleaseIdentifier = null,
                BuildMetadata = null,
                Compatibility = null,
                DeprecationNotice = null,
                DownloadCount = 0
            };

            // Act
            var errors = VersionInfoValidation.Validate(versionInfo);

            // Assert
            Assert.Contains("ReleaseDate cannot be in the future.", errors);
        }

        [Fact]
        public void Validate_OLD_RELEASEDATE()
        {
            // Arrange
            var versionInfo = new VersionInfo
            {
                Id = Guid.NewGuid(),
                EntityId = Guid.NewGuid(),
                Version = "1.0.0",
                ReleaseDate = new DateTime(1999, 1, 1),
                ReleaseNotes = "Release notes",
                IsPrerelease = false,
                PrereleaseIdentifier = null,
                BuildMetadata = null,
                Compatibility = null,
                DeprecationNotice = null,
                DownloadCount = 0
            };

            // Act
            var errors = VersionInfoValidation.Validate(versionInfo);

            // Assert
            Assert.Contains("ReleaseDate must be a valid date after year 2000.", errors);
        }

        [Fact]
        public void Validate_NULL_RELEASENOTES()
        {
            // Arrange
            var versionInfo = new VersionInfo
            {
                Id = Guid.NewGuid(),
                EntityId = Guid.NewGuid(),
                Version = "1.0.0",
                ReleaseDate = DateTime.UtcNow,
                ReleaseNotes = null,
                IsPrerelease = false,
                PrereleaseIdentifier = null,
                BuildMetadata = null,
                Compatibility = null,
                DeprecationNotice = null,
                DownloadCount = 0
            };

            // Act
            var action = () => VersionInfoValidation.Validate(versionInfo);

            // Assert
            Assert.Throws<ArgumentNullException>(action);
        }

        [Fact]
        public void Validate_PRERELEASE_TRUE_NULL_IDENTIFIER()
        {
            // Arrange
            var versionInfo = new VersionInfo
            {
                Id = Guid.NewGuid(),
                EntityId = Guid.NewGuid(),
                Version = "1.0.0",
                ReleaseDate = DateTime.UtcNow,
                ReleaseNotes = "Release notes",
                IsPrerelease = true,
                PrereleaseIdentifier = null,
                BuildMetadata = null,
                Compatibility = null,
                DeprecationNotice = null,
                DownloadCount = 0
            };

            // Act
            var errors = VersionInfoValidation.Validate(versionInfo);

            // Assert
            Assert.Contains("PrereleaseIdentifier must not be null or whitespace when IsPrerelease is true.", errors);
        }

        [Fact]
        public void Validate_PRERELEASE_TRUE_WHITESPACE_IDENTIFIER()
        {
            // Arrange
            var versionInfo = new VersionInfo
            {
                Id = Guid.NewGuid(),
                EntityId = Guid.NewGuid(),
                Version = "1.0.0",
                ReleaseDate = DateTime.UtcNow,
                ReleaseNotes = "Release notes",
                IsPrerelease = true,
                PrereleaseIdentifier = "   ",
                BuildMetadata = null,
                Compatibility = null,
                DeprecationNotice = null,
                DownloadCount = 0
            };

            // Act
            var errors = VersionInfoValidation.Validate(versionInfo);

            // Assert
            Assert.Contains("PrereleaseIdentifier must not be null or whitespace when IsPrerelease is true.", errors);
        }

        [Fact]
        public void Validate_NULL_BUILDMETADATA()
        {
            // Arrange
            var versionInfo = new VersionInfo
            {
                Id = Guid.NewGuid(),
                EntityId = Guid.NewGuid(),
                Version = "1.0.0",
                ReleaseDate = DateTime.UtcNow,
                ReleaseNotes = "Release notes",
                IsPrerelease = false,
                PrereleaseIdentifier = null,
                BuildMetadata = null,
                Compatibility = null,
                DeprecationNotice = null,
                DownloadCount = 0
            };

            // Act
            var action = () => VersionInfoValidation.Validate(versionInfo);

            // Assert
            Assert.Throws<ArgumentNullException>(action);
        }

        [Fact]
        public void Validate_NULL_COMPATIBILITY()
        {
            // Arrange
            var versionInfo = new VersionInfo
            {
                Id = Guid.NewGuid(),
                EntityId = Guid.NewGuid(),
                Version = "1.0.0",
                ReleaseDate = DateTime.UtcNow,
                ReleaseNotes = "Release notes",
                IsPrerelease = false,
                PrereleaseIdentifier = null,
                BuildMetadata = null,
                Compatibility = null,
                DeprecationNotice = null,
                DownloadCount = 0
            };

            // Act
            var action = () => VersionInfoValidation.Validate(versionInfo);

            // Assert
            Assert.Throws<ArgumentNullException>(action);
        }

        [Fact]
        public void Validate_NULL_DEPRECATIONNOTICE()
        {
            // Arrange
            var versionInfo = new VersionInfo
            {
                Id = Guid.NewGuid(),
                EntityId = Guid.NewGuid(),
                Version = "1.0.0",
                ReleaseDate = DateTime.UtcNow,
                ReleaseNotes = "Release notes",
                IsPrerelease = false,
                PrereleaseIdentifier = null,
                BuildMetadata = null,
                Compatibility = null,
                DeprecationNotice = null,
                DownloadCount = 0
            };

            // Act
            var action = () => VersionInfoValidation.Validate(versionInfo);

            // Assert
            Assert.Throws<ArgumentNullException>(action);
        }

        [Fact]
        public void Validate_NEGATIVE_DOWNLOADCOUNT()
        {
            // Arrange
            var versionInfo = new VersionInfo
            {
                Id = Guid.NewGuid(),
                EntityId = Guid.NewGuid(),
                Version = "1.0.0",
                ReleaseDate = DateTime.UtcNow,
                ReleaseNotes = "Release notes",
                IsPrerelease = false,
                PrereleaseIdentifier = null,
                BuildMetadata = null,
                Compatibility = null,
                DeprecationNotice = null,
                DownloadCount = -1
            };

            // Act
            var errors = VersionInfoValidation.Validate(versionInfo);

            // Assert
            Assert.Contains("DownloadCount must not be negative.", errors);
        }

        [Fact]
        public void IsValid_TRUE_FOR_VALID_VERSIONINFO()
        {
            // Arrange
            var versionInfo = new VersionInfo
            {
                Id = Guid.NewGuid(),
                EntityId = Guid.NewGuid(),
                Version = "1.0.0",
                ReleaseDate = DateTime.UtcNow,
                ReleaseNotes = "Release notes",
                IsPrerelease = false,
                PrereleaseIdentifier = null,
                BuildMetadata = null,
                Compatibility = null,
                DeprecationNotice = null,
                DownloadCount = 0
            };

            // Act
            var result = VersionInfoValidation.IsValid(versionInfo);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsValid_FALSE_FOR_INVALID_VERSIONINFO()
        {
            // Arrange
            var versionInfo = new VersionInfo
            {
                Id = Guid.NewGuid(),
                EntityId = Guid.Empty, // Invalid
                Version = "1.0.0",
                ReleaseDate = DateTime.UtcNow,
                ReleaseNotes = "Release notes",
                IsPrerelease = false,
                PrereleaseIdentifier = null,
                BuildMetadata = null,
                Compatibility = null,
                DeprecationNotice = null,
                DownloadCount = 0
            };

            // Act
            var result = VersionInfoValidation.IsValid(versionInfo);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsValid_NULL_VERSIONINFO_THROWS()
        {
            // Arrange
            VersionInfo versionInfo = null;

            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => VersionInfoValidation.IsValid(versionInfo));
        }

        [Fact]
        public void EnsureValid_DOES_NOTHING_FOR_VALID_VERSIONINFO()
        {
            // Arrange
            var versionInfo = new VersionInfo
            {
                Id = Guid.NewGuid(),
                EntityId = Guid.NewGuid(),
                Version = "1.0.0",
                ReleaseDate = DateTime.UtcNow,
                ReleaseNotes = "Release notes",
                IsPrerelease = false,
                PrereleaseIdentifier = null,
                BuildMetadata = null,
                Compatibility = null,
                DeprecationNotice = null,
                DownloadCount = 0
            };

            // Act
            var exception = Record.Exception(() => VersionInfoValidation.EnsureValid(versionInfo));

            // Assert
            Assert.Null(exception);
        }

        [Fact]
        public void EnsureValid_THROWS_ARGUMENTEXCEPTION_FOR_INVALID_VERSIONINFO()
        {
            // Arrange
            var versionInfo = new VersionInfo
            {
                Id = Guid.NewGuid(),
                EntityId = Guid.Empty, // Invalid
                Version = "1.0.0",
                ReleaseDate = DateTime.UtcNow,
                ReleaseNotes = "Release notes",
                IsPrerelease = false,
                PrereleaseIdentifier = null,
                BuildMetadata = null,
                Compatibility = null,
                DeprecationNotice = null,
                DownloadCount = 0
            };

            // Act
            var exception = Record.Exception(() => VersionInfoValidation.EnsureValid(versionInfo));

            // Assert
            Assert.IsType<ArgumentException>(exception);
            Assert.Contains("VersionInfo is invalid", exception.Message);
            Assert.Contains("EntityId must not be empty.", exception.Message);
        }

        [Fact]
        public void EnsureValid_THROWS_ARGUMENTNULLEXCEPTION_FOR_NULL_VERSIONINFO()
        {
            // Arrange
            VersionInfo versionInfo = null;

            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => VersionInfoValidation.EnsureValid(versionInfo));
        }
    }
}