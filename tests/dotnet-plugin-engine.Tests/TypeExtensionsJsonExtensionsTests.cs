namespace dotnet_plugin_engine.Tests;

using System;
using System.Text.Json;
using PluginEngine.Utils.Extensions;
using Xunit;

public class TypeExtensionsJsonExtensionsTests
{
    [Fact]
    public void ToJson_ValidType_ReturnsJsonString()
    {
        var type = typeof(string);
        var json = type.ToJson();

        Assert.Contains("System.String", json);
    }

    [Fact]
    public void ToJson_Indented_ReturnsFormattedJson()
    {
        var type = typeof(int);
        var json = type.ToJson(indented: true);

        Assert.Contains(Environment.NewLine, json);
    }

    [Fact]
    public void ToJson_NullType_ThrowsArgumentNullException()
    {
        Type type = null!;
        Assert.Throws<ArgumentNullException>(() => type.ToJson());
    }

    [Fact]
    public void FromJson_ValidJson_ReturnsType()
    {
        var type = typeof(string);
        var json = JsonSerializer.Serialize(type.AssemblyQualifiedName);
        
        var result = TypeExtensionsJsonExtensions.FromJson(json);

        Assert.Equal(type, result);
    }

    [Fact]
    public void FromJson_NullOrEmptyJson_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => TypeExtensionsJsonExtensions.FromJson(null!));
        Assert.Throws<ArgumentException>(() => TypeExtensionsJsonExtensions.FromJson(string.Empty));
    }

    [Fact]
    public void TryFromJson_ValidJson_ReturnsTrueAndType()
    {
        var type = typeof(string);
        var json = JsonSerializer.Serialize(type.AssemblyQualifiedName);
        
        var success = TypeExtensionsJsonExtensions.TryFromJson(json, out var result);

        Assert.True(success);
        Assert.Equal(type, result);
    }

    [Fact]
    public void TryFromJson_InvalidJson_ReturnsFalse()
    {
        var json = "invalid-json";
        
        var success = TypeExtensionsJsonExtensions.TryFromJson(json, out var result);

        Assert.False(success);
        Assert.Null(result);
    }

    [Fact]
    public void TryFromJson_NullOrEmptyJson_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => TypeExtensionsJsonExtensions.TryFromJson(null!, out _));
        Assert.Throws<ArgumentException>(() => TypeExtensionsJsonExtensions.TryFromJson(string.Empty, out _));
    }
}
