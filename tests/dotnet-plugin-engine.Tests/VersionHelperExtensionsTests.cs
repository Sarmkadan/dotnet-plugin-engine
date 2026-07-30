namespace dotnet_plugin_engine.Tests;

using System;
using PluginEngine.Utils.Helpers;
using Xunit;

public class VersionHelperExtensionsTests
{
    private readonly VersionHelper _helper = new();

    #region IsGreaterThan

    [Fact]
    public void IsGreaterThan_VersionIsHigher_ReturnsTrue()
    {
        // Arrange
        var higher = "2.0.0";
        var lower  = "1.5.0";

        // Act
        var result = _helper.IsGreaterThan(higher, lower);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsGreaterThan_VersionIsNotHigher_ReturnsFalse()
    {
        // Arrange
        var same   = "1.0.0";
        var lower  = "0.9.9";

        // Act
        var resultSame   = _helper.IsGreaterThan(same, same);
        var resultLower  = _helper.IsGreaterThan(lower, same);

        // Assert
        Assert.False(resultSame);
        Assert.False(resultLower);
    }

    [Fact]
    public void IsGreaterThan_NullHelper_ThrowsArgumentNullException()
    {
        // Arrange
        VersionHelper nullHelper = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => nullHelper!.IsGreaterThan("1.0.0", "0.9.0"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void IsGreaterThan_NullOrEmptyVersionString_ThrowsArgumentException(string versionString)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => _helper.IsGreaterThan(versionString, "1.0.0"));
    }

    #endregion

    #region IsLessThan

    [Fact]
    public void IsLessThan_VersionIsLower_ReturnsTrue()
    {
        // Arrange
        var lower = "1.0.0";
        var higher = "2.0.0";

        // Act
        var result = _helper.IsLessThan(lower, higher);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsLessThan_VersionIsNotLower_ThrowsArgumentExceptionOnInvalidInput()
    {
        // Arrange
        var version = "1.0.0";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => _helper.IsLessThan(version, null!));
    }

    #endregion

    #region IsEqualTo

    [Fact]
    public void IsEqualTo_VersionsEqual_ReturnsTrue()
    {
        // Arrange
        var v1 = "1.2.3";
        var v2 = "1.2.3";

        // Act
        var result = _helper.IsEqualTo(v1, v2);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsEqualTo_VersionsDifferent_ReturnsFalse()
    {
        // Arrange
        var v1 = "1.2.3";
        var v2 = "1.2.4";

        // Act
        var result = _helper.IsEqualTo(v1, v2);

        // Assert
        Assert.False(result);
    }

    #endregion

    #region GetVersionInfo

    [Fact]
    public void GetVersionInfo_ValidVersion_ReturnsParsedInfo()
    {
        // Arrange
        var version = "3.4.5-beta";

        // Act
        var info = _helper.GetVersionInfo(version);

        // Assert
        Assert.NotNull(info);
        // The exact shape of ParsedVersionInfo is not known; we verify at least the major component.
        // Assuming a property named Major exists.
        var majorProperty = info.GetType().GetProperty("Major");
        Assert.NotNull(majorProperty);
        Assert.Equal(3, majorProperty.GetValue(info));
    }

    [Fact]
    public void GetVersionInfo_NullOrEmptyVersion_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => _helper.GetVersionInfo(null!));
        Assert.Throws<ArgumentException>(() => _helper.GetVersionInfo(string.Empty));
    }

    #endregion

    #region IsValidSemanticVersion

    [Fact]
    public void IsValidSemanticVersion_ValidVersion_ReturnsTrue()
    {
        // Arrange
        var version = "1.0.0";

        // Act
        var result = _helper.IsValidSemanticVersion(version);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsValidSemanticVersion_InvalidVersion_ReturnsFalse()
    {
        // Arrange
        var version = "not-a-version";

        // Act
        var result = _helper.IsValidSemanticVersion(version);

        // Assert
        Assert.False(result);
    }

    #endregion
}
