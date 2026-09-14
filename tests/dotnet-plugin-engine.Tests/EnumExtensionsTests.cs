namespace dotnet_plugin_engine.Tests;

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;
using Xunit;
using PluginEngine.Utils.Extensions;

/// <summary>
/// Tests for the EnumExtensions class.
/// </summary>
public class EnumExtensionsTests
{
    [Fact]
    /// <summary>
    /// Verifies that GetDescription returns the correct description for a valid enum value.
    /// </summary>
    public void GetDescription_HappyPath_ReturnsDescription()
    {
        // Arrange
        var enumValue = Enum.Parse(typeof(PluginStatus), "Loaded");

        // Act
        var description = EnumExtensions.GetDescription(enumValue);

        // Assert
        Assert.Equal("Loaded", description);
    }

    [Fact]
    /// <summary>
    /// Verifies that GetDescription throws ArgumentNullException when input is null.
    /// </summary>
    public void GetDescription_NullInput_ThrowsArgumentNullException()
    {
        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => EnumExtensions.GetDescription(null));
    }

    [Fact]
    /// <summary>
    /// Verifies that GetDisplayName returns the correct display name for a valid enum value.
    /// </summary>
    public void GetDisplayName_HappyPath_ReturnsDisplayName()
    {
        // Arrange
        var enumValue = Enum.Parse(typeof(PluginStatus), "Loaded");

        // Act
        var displayName = EnumExtensions.GetDisplayName(enumValue);

        // Assert
        Assert.Equal("Loaded", displayName);
    }

    [Fact]
    /// <summary>
    /// Verifies that GetDisplayName throws ArgumentNullException when input is null.
    /// </summary>
    public void GetDisplayName_NullInput_ThrowsArgumentNullException()
    {
        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => EnumExtensions.GetDisplayName(null));
    }

    [Fact]
    /// <summary>
    /// Verifies that ToUserFriendlyString returns the correct user-friendly string for a valid enum value.
    /// </summary>
    public void ToUserFriendlyString_HappyPath_ReturnsUserFriendlyString()
    {
        // Arrange
        var enumValue = Enum.Parse(typeof(PluginStatus), "Loaded");

        // Act
        var userFriendlyString = EnumExtensions.ToUserFriendlyString(enumValue);

        // Assert
        Assert.Equal("Loaded", userFriendlyString);
    }

    [Fact]
    /// <summary>
    /// Verifies that ToUserFriendlyString throws ArgumentNullException when input is null.
    /// </summary>
    public void ToUserFriendlyString_NullInput_ThrowsArgumentNullException()
    {
        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => EnumExtensions.ToUserFriendlyString(null));
    }

    [Fact]
    /// <summary>
    /// Verifies that ToCssClass returns the correct CSS class for a valid enum value.
    /// </summary>
    public void ToCssClass_HappyPath_ReturnsCssClass()
    {
        // Arrange
        var enumValue = Enum.Parse(typeof(PluginStatus), "Loaded");

        // Act
        var cssClass = EnumExtensions.ToCssClass(enumValue);

        // Assert
        Assert.Equal("status-loaded", cssClass);
    }

    [Fact]
    /// <summary>
    /// Verifies that ToCssClass throws ArgumentNullException when input is null.
    /// </summary>
    public void ToCssClass_NullInput_ThrowsArgumentNullException()
    {
        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => EnumExtensions.ToCssClass(null));
    }

    [Fact]
    /// <summary>
    /// Verifies that IsHealthy returns true for the Healthy enum value.
    /// </summary>
    public void IsHealthy_HappyPath_ReturnsTrue()
    {
        // Arrange
        var enumValue = Enum.Parse(typeof(PluginStatus), "Loaded");

        // Act
        var isHealthy = EnumExtensions.IsHealthy(enumValue);

        // Assert
        Assert.True(isHealthy);
    }

    [Fact]
    /// <summary>
    /// Verifies that IsHealthy throws ArgumentNullException when input is null.
    /// </summary>
    public void IsHealthy_NullInput_ThrowsArgumentNullException()
    {
        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => EnumExtensions.IsHealthy(null));
    }

    [Fact]
    /// <summary>
    /// Verifies that IsTransient returns true for the Transient enum value.
    /// </summary>
    public void IsTransient_HappyPath_ReturnsTrue()
    {
        // Arrange
        var enumValue = Enum.Parse(typeof(PluginStatus), "Loading");

        // Act
        var isTransient = EnumExtensions.IsTransient(enumValue);

        // Assert
        Assert.True(isTransient);
    }

    [Fact]
    /// <summary>
    /// Verifies that IsTransient throws ArgumentNullException when input is null.
    /// </summary>
    public void IsTransient_NullInput_ThrowsArgumentNullException()
    {
        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => EnumExtensions.IsTransient(null));
    }

    [Fact]
    /// <summary>
    /// Verifies that GetAllValues returns all enum values.
    /// </summary>
    public void GetAllValues_HappyPath_ReturnsAllValues()
    {
        // Arrange
        var enumType = typeof(PluginStatus);

        // Act
        var allValues = EnumExtensions.GetAllValues(enumType);

        // Assert
        Assert.Equal(Enum.GetNames(enumType), allValues);
    }

    [Fact]
    /// <summary>
    /// Verifies that TryParse returns the parsed enum value for a valid string.
    /// </summary>
    public void TryParse_HappyPath_ReturnsParsedValue()
    {
        // Arrange
        var enumType = typeof(PluginStatus);
        var value = "Loaded";

        // Act
        var parsedValue = EnumExtensions.TryParse(enumType, value);

        // Assert
        Assert.Equal(Enum.Parse(enumType, value), parsedValue);
    }

    [Fact]
    /// <summary>
    /// Verifies that TryParse returns null when input string is null.
    /// </summary>
    public void TryParse_NullInput_ReturnsNull()
    {
        // Act and Assert
        Assert.Null(EnumExtensions.TryParse(typeof(PluginStatus), null));
    }

    [Fact]
    /// <summary>
    /// Verifies that TryParse returns null for an invalid enum string.
    /// </summary>
    public void TryParse_InvalidInput_ReturnsNull()
    {
        // Act and Assert
        Assert.Null(EnumExtensions.TryParse(typeof(PluginStatus), "Invalid"));
    }

    [Fact]
    /// <summary>
    /// Verifies that GetIntValue returns the correct integer value for a valid enum value.
    /// </summary>
    public void GetIntValue_HappyPath_ReturnsIntValue()
    {
        // Arrange
        var enumValue = Enum.Parse(typeof(PluginStatus), "Loaded");

        // Act
        var intValue = EnumExtensions.GetIntValue(enumValue);

        // Assert
        Assert.Equal(1, intValue);
    }

    [Fact]
    /// <summary>
    /// Verifies that GetIntValue throws ArgumentNullException when input is null.
    /// </summary>
    public void GetIntValue_NullInput_ThrowsArgumentNullException()
    {
        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => EnumExtensions.GetIntValue(null));
    }

    [Fact]
    /// <summary>
    /// Verifies that GetValueDescriptions returns the correct descriptions for all enum values.
    /// </summary>
    public void GetValueDescriptions_HappyPath_ReturnsValueDescriptions()
    {
        // Arrange
        var enumType = typeof(PluginStatus);

        // Act
        var valueDescriptions = EnumExtensions.GetValueDescriptions(enumType);

        // Assert
        Assert.Equal(Enum.GetNames(enumType), valueDescriptions.Select(x => x.Value));
    }

    [Fact]
    /// <summary>
    /// Verifies that ToColorHex returns the correct color hex for a valid enum value.
    /// </summary>
    public void ToColorHex_HappyPath_ReturnsColorHex()
    {
        // Arrange
        var enumValue = Enum.Parse(typeof(ExecutionState), "Running");

        // Act
        var colorHex = EnumExtensions.ToColorHex(enumValue);

        // Assert
        Assert.Equal("#0066CC", colorHex);
    }

    [Fact]
    /// <summary>
    /// Verifies that ToColorHex throws ArgumentNullException when input is null.
    /// </summary>
    public void ToColorHex_NullInput_ThrowsArgumentNullException()
    {
        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => EnumExtensions.ToColorHex(null));
    }

    [Fact]
    /// <summary>
    /// Verifies that IsTerminal returns true for the Terminal enum value.
    /// </summary>
    public void IsTerminal_HappyPath_ReturnsTrue()
    {
        // Arrange
        var enumValue = Enum.Parse(typeof(ExecutionState), "Completed");

        // Act
        var isTerminal = EnumExtensions.IsTerminal(enumValue);

        // Assert
        Assert.True(isTerminal);
    }

    [Fact]
    /// <summary>
    /// Verifies that IsTerminal throws ArgumentNullException when input is null.
    /// </summary>
    public void IsTerminal_NullInput_ThrowsArgumentNullException()
    {
        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => EnumExtensions.IsTerminal(null));
    }
}