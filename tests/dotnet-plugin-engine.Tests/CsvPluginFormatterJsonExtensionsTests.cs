namespace dotnet_plugin_engine.Tests;

using System;
using System.Text.Json;
using PluginEngine.Formatters;
using Xunit;

public class CsvPluginFormatterJsonExtensionsTests
{
    [Fact]
    public void ToJson_HappyPath_ReturnsJsonString()
    {
        // Arrange
        var formatter = new CsvPluginFormatter();

        // Act
        var json = CsvPluginFormatterJsonExtensions.ToJson(formatter);

        // Assert
        Assert.NotEmpty(json);
    }

    [Fact]
    public void FromJson_HappyPath_ReturnsCsvPluginFormatter()
    {
        // Arrange
        var formatter = new CsvPluginFormatter();
        var json = CsvPluginFormatterJsonExtensions.ToJson(formatter);

        // Act
        var result = CsvPluginFormatterJsonExtensions.FromJson(json);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public void FromJson_NullInput_ReturnsNull()
    {
        // Act
        var result = CsvPluginFormatterJsonExtensions.FromJson(null);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void TryFromJson_HappyPath_ReturnsTrueAndCsvPluginFormatter()
    {
        // Arrange
        var formatter = new CsvPluginFormatter();
        var json = CsvPluginFormatterJsonExtensions.ToJson(formatter);

        // Act
        var success = CsvPluginFormatterJsonExtensions.TryFromJson(json, out var result);

        // Assert
        Assert.True(success);
        Assert.NotNull(result);
    }

    [Fact]
    public void TryFromJson_NullInput_ReturnsFalseAndNull()
    {
        // Act
        var success = CsvPluginFormatterJsonExtensions.TryFromJson(null, out var result);

        // Assert
        Assert.False(success);
        Assert.Null(result);
    }

    [Fact]
    public void ToJson_ThrowsArgumentNullException_ForNullInput()
    {
        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => CsvPluginFormatterJsonExtensions.ToJson(null));
    }

    [Fact]
    public void FromJson_ThrowsArgumentException_ForEmptyInput()
    {
        // Act and Assert
        Assert.Throws<ArgumentException>(() => CsvPluginFormatterJsonExtensions.FromJson(string.Empty));
    }
}
