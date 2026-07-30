namespace dotnet_plugin_engine.Tests;

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;
using Xunit;
using PluginEngine.Utils.Extensions;

public class EnumExtensionsTests
{
    [Fact]
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
    public void GetDescription_NullInput_ThrowsArgumentNullException()
    {
        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => EnumExtensions.GetDescription(null));
    }

    [Fact]
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
    public void GetDisplayName_NullInput_ThrowsArgumentNullException()
    {
        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => EnumExtensions.GetDisplayName(null));
    }

    [Fact]
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
    public void ToUserFriendlyString_NullInput_ThrowsArgumentNullException()
    {
        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => EnumExtensions.ToUserFriendlyString(null));
    }

    [Fact]
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
    public void ToCssClass_NullInput_ThrowsArgumentNullException()
    {
        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => EnumExtensions.ToCssClass(null));
    }

    [Fact]
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
    public void IsHealthy_NullInput_ThrowsArgumentNullException()
    {
        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => EnumExtensions.IsHealthy(null));
    }

    [Fact]
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
    public void IsTransient_NullInput_ThrowsArgumentNullException()
    {
        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => EnumExtensions.IsTransient(null));
    }

    [Fact]
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
    public void TryParse_NullInput_ReturnsNull()
    {
        // Act and Assert
        Assert.Null(EnumExtensions.TryParse(typeof(PluginStatus), null));
    }

    [Fact]
    public void TryParse_InvalidInput_ReturnsNull()
    {
        // Act and Assert
        Assert.Null(EnumExtensions.TryParse(typeof(PluginStatus), "Invalid"));
    }

    [Fact]
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
    public void GetIntValue_NullInput_ThrowsArgumentNullException()
    {
        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => EnumExtensions.GetIntValue(null));
    }

    [Fact]
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
    public void ToColorHex_NullInput_ThrowsArgumentNullException()
    {
        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => EnumExtensions.ToColorHex(null));
    }

    [Fact]
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
    public void IsTerminal_NullInput_ThrowsArgumentNullException()
    {
        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => EnumExtensions.IsTerminal(null));
    }
}
