#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using PluginEngine.Utils.Helpers;
using Xunit;

namespace PluginEngine.Tests;

public sealed class VersionHelperTests
{
    private readonly Mock<ILogger<VersionHelper>> _mockLogger;
    private readonly VersionHelper _sut;

    public VersionHelperTests()
    {
        _mockLogger = new Mock<ILogger<VersionHelper>>();
        _sut = new VersionHelper(_mockLogger.Object);
    }

    [Fact]
    public void ParseVersion_WithVPrefix_StripsPrefixAndParsesCorrectly()
    {
        var result = _sut.ParseVersion("v2.3.1");

        result.Should().NotBeNull();
        result!.Major.Should().Be(2);
        result.Minor.Should().Be(3);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void ParseVersion_WithEmptyOrWhitespace_ReturnsNull(string input)
    {
        var result = _sut.ParseVersion(input);

        result.Should().BeNull();
    }

    [Fact]
    public void ParseVersion_WithInvalidString_LogsWarningAndReturnsNull()
    {
        var result = _sut.ParseVersion("not-a-version-at-all");

        result.Should().BeNull();
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => true),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void ParseVersion_WithPrereleaseTag_ParsesCoreVersionNumbers()
    {
        var result = _sut.ParseVersion("3.0.0-beta.2");

        result.Should().NotBeNull();
        result!.Major.Should().Be(3);
        result.Minor.Should().Be(0);
    }

    [Fact]
    public void CompareVersions_WhenFirstVersionIsGreater_ReturnsPositive()
    {
        var result = _sut.CompareVersions("2.0.0", "1.9.9");

        result.Should().BePositive();
    }

    [Fact]
    public void CompareVersions_WhenVersionsAreEqual_ReturnsZero()
    {
        var result = _sut.CompareVersions("1.5.3", "1.5.3");

        result.Should().Be(0);
    }

    [Fact]
    public void SatisfiesConstraint_GreaterThanOrEqual_ReturnsTrueWhenVersionMeetsMinimum()
    {
        _sut.SatisfiesConstraint("1.2.0", ">=1.0.0").Should().BeTrue();
        _sut.SatisfiesConstraint("1.0.0", ">=1.0.0").Should().BeTrue();
    }

    [Fact]
    public void SatisfiesConstraint_GreaterThanOrEqual_ReturnsFalseWhenBelowMinimum()
    {
        _sut.SatisfiesConstraint("0.9.9", ">=1.0.0").Should().BeFalse();
    }

    [Fact]
    public void SatisfiesConstraint_CaretOperator_RejectsDifferentMajorVersion()
    {
        _sut.SatisfiesConstraint("2.0.0", "^1.0.0").Should().BeFalse();
    }

    [Fact]
    public void SatisfiesConstraint_CaretOperator_AcceptsSameMajorHigherMinor()
    {
        _sut.SatisfiesConstraint("1.5.0", "^1.0.0").Should().BeTrue();
    }

    [Fact]
    public void SatisfiesConstraint_TildeOperator_RejectsDifferentMinorVersion()
    {
        _sut.SatisfiesConstraint("1.6.0", "~1.5.0").Should().BeFalse();
    }

    [Fact]
    public void GetLatestVersion_FromMixedVersionList_ReturnsHighestVersion()
    {
        var versions = new[] { "1.0.0", "3.0.0", "2.5.1", "0.9.9" };

        var result = _sut.GetLatestVersion(versions);

        result.Should().Be("3.0.0");
    }

    [Fact]
    public void GetVersionInfo_WithAlphaPrereleaseTag_SetsIsPrereleaseTrue()
    {
        var info = _sut.GetVersionInfo("2.0.0-alpha");

        info.IsPrerelease.Should().BeTrue();
        info.IsStable.Should().BeFalse();
        info.Major.Should().Be(2);
    }

    [Fact]
    public void IsValidSemanticVersion_WithProperVersionString_ReturnsTrue()
    {
        _sut.IsValidSemanticVersion("1.2.3").Should().BeTrue();
    }

    [Fact]
    public void IsValidSemanticVersion_WithNonVersionString_ReturnsFalse()
    {
        _sut.IsValidSemanticVersion("hello-world").Should().BeFalse();
    }
}
