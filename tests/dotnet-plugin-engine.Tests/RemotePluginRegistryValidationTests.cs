#nullable enable
using FluentAssertions;
using PluginEngine.Integration;
using Xunit;

namespace PluginEngine.Tests;

public sealed class RemotePluginRegistryValidationTests
{
    [Fact]
    public void PluginInfo_Validate_ValidInput_ReturnsEmptyList()
    {
        var pluginInfo = new PluginInfo
        {
            Id = Guid.NewGuid(),
            Name = "ValidPlugin",
            Version = "1.0.0",
            DownloadUrl = "https://example.com/plugin.zip"
        };

        var problems = pluginInfo.Validate();

        problems.Should().BeEmpty();
    }

    [Fact]
    public void PluginInfo_Validate_InvalidInput_ReturnsErrors()
    {
        var pluginInfo = new PluginInfo
        {
            Id = Guid.Empty,
            Name = "",
            Version = "invalid-version",
            DownloadUrl = "invalid-url"
        };

        var problems = pluginInfo.Validate();

        problems.Should().Contain([
            "PluginInfo.Name cannot be null, empty, or whitespace",
            "PluginInfo.Version must be a valid semantic version (e.g., 1.0.0)",
            "PluginInfo.Id cannot be an empty GUID",
            "PluginInfo.DownloadUrl must be a valid URL or null"
        ]);
    }

    [Fact]
    public void PluginInfo_IsValid_ReturnsCorrectResult()
    {
        var validPlugin = new PluginInfo { Id = Guid.NewGuid(), Name = "Name", Version = "1.0.0" };
        var invalidPlugin = new PluginInfo { Id = Guid.Empty, Name = "", Version = "" };

        validPlugin.IsValid().Should().BeTrue();
        invalidPlugin.IsValid().Should().BeFalse();
    }

    [Fact]
    public void PluginVersionInfo_Validate_InvalidInput_ReturnsErrors()
    {
        var versionInfo = new PluginVersionInfo
        {
            Version = "invalid-version",
            PublishedAtUtc = default,
            DownloadUrl = ""
        };

        var problems = versionInfo.Validate();

        problems.Should().Contain([
            "PluginVersionInfo.Version must be a valid semantic version (e.g., 1.0.0)",
            "PluginVersionInfo.PublishedAtUtc cannot be the default DateTime value",
            "PluginVersionInfo.DownloadUrl cannot be null, empty, or whitespace"
        ]);
    }

    [Fact]
    public void PluginPublishMetadata_Validate_InvalidInput_ReturnsErrors()
    {
        var metadata = new PluginPublishMetadata
        {
            PluginName = "",
            Version = "invalid",
            Description = "",
            Author = "",
            Tags = null!
        };

        var problems = metadata.Validate();

        problems.Should().Contain([
            "PluginPublishMetadata.PluginName cannot be null, empty, or whitespace",
            "PluginPublishMetadata.Version must be a valid semantic version (e.g., 1.0.0)",
            "PluginPublishMetadata.Description cannot be null, empty, or whitespace",
            "PluginPublishMetadata.Author cannot be null, empty, or whitespace",
            "PluginPublishMetadata.Tags cannot be null"
        ]);
    }

    [Fact]
    public void PluginInfo_EnsureValid_ThrowsExceptionOnInvalidInput()
    {
        var pluginInfo = new PluginInfo { Id = Guid.Empty, Name = "", Version = "" };

        var act = () => pluginInfo.EnsureValid();

        act.Should().Throw<ArgumentException>()
            .WithMessage("*PluginInfo is invalid*");
    }

    [Fact]
    public void PluginInfo_EnsureValid_DoesNotThrowOnValidInput()
    {
        var pluginInfo = new PluginInfo { Id = Guid.NewGuid(), Name = "Name", Version = "1.0.0" };

        var act = () => pluginInfo.EnsureValid();

        act.Should().NotThrow();
    }
}
