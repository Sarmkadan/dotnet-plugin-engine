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

/// <summary>
/// Tests for the VersionHelper class.
/// </summary>
public sealed class VersionHelperTests
{
    private readonly Mock<ILogger<VersionHelper>> _mockLogger;
    private readonly VersionHelper _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="VersionHelperTests"/> class.
    /// </summary>
    public VersionHelperTests()
    {
        _mockLogger = new Mock<ILogger<VersionHelper>>();
        _sut = new VersionHelper(_mockLogger.Object);
    }

    [Fact]
    public void ParseVersion_WithVPrefix_StripsPrefixAndParsesCorrectly()
    {
        /// <summary>
        /// Tests that the ParseVersion method correctly strips the 'v' prefix and parses the version correctly.
        /// </summary>
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
        /// <summary>
        /// Tests that the ParseVersion method returns null when given an empty or whitespace string.
        /// </summary>
        /// <param name="input">The input string to test.</param>
        var result = _sut.ParseVersion(input);

        result.Should().BeNull();
    }

    [Fact]
    public void ParseVersion_WithInvalidString_LogsWarningAndReturnsNull()
    {
        /// <summary>
        /// Tests that the ParseVersion method logs a warning and returns null when given an invalid string.
        /// </summary>
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
        /// <summary>
        /// Tests that the ParseVersion method correctly parses the core version numbers when given a prerelease tag.
        /// </summary>
        var result = _sut.ParseVersion("3.0.0-beta.2");

        result.Should().NotBeNull();
        result!.Major.Should().Be(3);
        result.Minor.Should().Be(0);
    }

    [Fact]
    public void CompareVersions_WhenFirstVersionIsGreater_ReturnsPositive()
    {
        /// <summary>
        /// Tests that the CompareVersions method returns a positive value when the first version is greater.
        /// </summary>
        var result = _sut.CompareVersions("2.0.0", "1.9.9");

        result.Should().BePositive();
    }

    [Fact]
    public void CompareVersions_WhenVersionsAreEqual_ReturnsZero()
    {
        /// <summary>
        /// Tests that the CompareVersions method returns zero when the versions are equal.
        /// </summary>
        var result = _sut.CompareVersions("1.5.3", "1.5.3");

        result.Should().Be(0);
    }

    [Fact]
    public void SatisfiesConstraint_GreaterThanOrEqual_ReturnsTrueWhenVersionMeetsMinimum()
    {
        /// <summary>
        /// Tests that the SatisfiesConstraint method returns true when the version meets the minimum constraint.
        /// </summary>
        _sut.SatisfiesConstraint("1.2.0", ">=1.0.0").Should().BeTrue();
        _sut.SatisfiesConstraint("1.0.0", ">=1.0.0").Should().BeTrue();
    }

    [Fact]
    public void SatisfiesConstraint_GreaterThanOrEqual_ReturnsFalseWhenBelowMinimum()
    {
        /// <summary>
        /// Tests that the SatisfiesConstraint method returns false when the version is below the minimum constraint.
        /// </summary>
        _sut.SatisfiesConstraint("0.9.9", ">=1.0.0").Should().BeFalse();
    }

    [Fact]
    public void SatisfiesConstraint_CaretOperator_RejectsDifferentMajorVersion()
    {
        /// <summary>
        /// Tests that the SatisfiesConstraint method rejects different major versions when using the caret operator.
        /// </summary>
        _sut.SatisfiesConstraint("2.0.0", "^1.0.0").Should().BeFalse();
    }

    [Fact]
    public void SatisfiesConstraint_CaretOperator_AcceptsSameMajorHigherMinor()
    {
        /// <summary>
        /// Tests that the SatisfiesConstraint method accepts the same major version with higher minor version when using the caret operator.
        /// </summary>
        _sut.SatisfiesConstraint("1.5.0", "^1.0.0").Should().BeTrue();
    }

    [Fact]
    public void SatisfiesConstraint_TildeOperator_RejectsDifferentMinorVersion()
    {
        /// <summary>
        /// Tests that the SatisfiesConstraint method rejects different minor versions when using the tilde operator.
        /// </summary>
        _sut.SatisfiesConstraint("1.6.0", "~1.5.0").Should().BeFalse();
    }

    [Fact]
    public void GetLatestVersion_FromMixedVersionList_ReturnsHighestVersion()
    {
        /// <summary>
        /// Tests that the GetLatestVersion method returns the highest version from a mixed version list.
        /// </summary>
        var versions = new[] { "1.0.0", "3.0.0", "2.5.1", "0.9.9" };

        var result = _sut.GetLatestVersion(versions);

        result.Should().Be("3.0.0");
    }

    [Fact]
    public void GetVersionInfo_WithAlphaPrereleaseTag_SetsIsPrereleaseTrue()
    {
        /// <summary>
        /// Tests that the GetVersionInfo method sets IsPrerelease to true when given a version with an alpha prerelease tag.
        /// </summary>
        var info = _sut.GetVersionInfo("2.0.0-alpha");

        info.IsPrerelease.Should().BeTrue();
        info.IsStable.Should().BeFalse();
        info.Major.Should().Be(2);
    }

    [Fact]
    public void IsValidSemanticVersion_WithProperVersionString_ReturnsTrue()
    {
        /// <summary>
        /// Tests that the IsValidSemanticVersion method returns true when given a proper version string.
        /// </summary>
        _sut.IsValidSemanticVersion("1.2.3").Should().BeTrue();
    }

    [Fact]
    public void IsValidSemanticVersion_WithNonVersionString_ReturnsFalse()
    {
        /// <summary>
        /// Tests that the IsValidSemanticVersion method returns false when given a non-version string.
        /// </summary>
        _sut.IsValidSemanticVersion("hello-world").Should().BeFalse();
    }
}
