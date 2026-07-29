#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using PluginEngine.Domain.Entities;
using Xunit;

namespace PluginEngine.Tests;

/// <summary>
/// Contains unit tests for the <see cref="PluginAssembly"/> entity.
/// Tests property behavior, validation, and state transition methods.
/// </summary>
public sealed class PluginAssemblyTests
{
    /// <summary>
    /// Creates a valid <see cref="PluginAssembly"/> instance for testing purposes.
    /// </summary>
    /// <param name="pluginId">The plugin ID. Defaults to a new GUID.</param>
    /// <returns>A new <see cref="PluginAssembly"/> instance with valid required properties.</returns>
    private static PluginAssembly CreateValidAssembly(Guid? pluginId = null)
    {
        return new PluginAssembly
        {
            PluginId = pluginId ?? Guid.NewGuid(),
            AssemblyName = "TestAssembly",
            AssemblyVersion = "1.0.0",
            FilePath = "/path/to/test.dll"
        };
    }

    // ── Property Tests ────────────────────────────────────────────────────────

    [Fact]
    public void Constructor_DefaultValues_AreAsExpected()
    {
        var assembly = new PluginAssembly();

        assembly.Id.Should().NotBe(Guid.Empty);
        assembly.PluginId.Should().Be(Guid.Empty);
        assembly.AssemblyName.Should().Be(string.Empty);
        assembly.AssemblyVersion.Should().Be(string.Empty);
        assembly.FilePath.Should().Be(string.Empty);
        assembly.FileSizeBytes.Should().Be(0L);
        assembly.FileHash.Should().Be(string.Empty);
        assembly.PublicKeyToken.Should().Be(string.Empty);
        assembly.IsMainAssembly.Should().BeFalse();
        assembly.LoadContextId.Should().Be(string.Empty);
        assembly.LastModifiedAt.Should().BeCloseTo(DateTime.UtcNow, precision: TimeSpan.FromSeconds(1));
        assembly.LoadedAt.Should().BeNull();
        assembly.Status.Should().Be(AssemblyLoadStatus.Unloaded);
        assembly.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void Properties_CanBeSetAndRetrieved()
    {
        var assembly = new PluginAssembly
        {
            Id = Guid.NewGuid(),
            PluginId = Guid.NewGuid(),
            AssemblyName = "TestAssembly",
            AssemblyVersion = "2.0.0",
            FilePath = "/custom/path.dll",
            FileSizeBytes = 1024L,
            FileHash = "abc123",
            PublicKeyToken = "abcd1234",
            IsMainAssembly = true,
            LoadContextId = "ctx-123",
            LastModifiedAt = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc),
            LoadedAt = new DateTime(2024, 1, 1, 12, 5, 0, DateTimeKind.Utc),
            Status = AssemblyLoadStatus.Loaded,
            ErrorMessage = "Some error"
        };

        assembly.Id.Should().Be(assembly.Id);
        assembly.PluginId.Should().Be(assembly.PluginId);
        assembly.AssemblyName.Should().Be("TestAssembly");
        assembly.AssemblyVersion.Should().Be("2.0.0");
        assembly.FilePath.Should().Be("/custom/path.dll");
        assembly.FileSizeBytes.Should().Be(1024L);
        assembly.FileHash.Should().Be("abc123");
        assembly.PublicKeyToken.Should().Be("abcd1234");
        assembly.IsMainAssembly.Should().BeTrue();
        assembly.LoadContextId.Should().Be("ctx-123");
        assembly.LastModifiedAt.Should().Be(new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc));
        assembly.LoadedAt.Should().Be(new DateTime(2024, 1, 1, 12, 5, 0, DateTimeKind.Utc));
        assembly.Status.Should().Be(AssemblyLoadStatus.Loaded);
        assembly.ErrorMessage.Should().Be("Some error");
    }

    // ── Validation Tests ──────────────────────────────────────────────────────

    [Fact]
    public void IsValid_ReturnsTrue_WhenAllRequiredFieldsSet()
    {
        var assembly = CreateValidAssembly();
        assembly.IsValid().Should().BeTrue();
    }

    [Fact]
    public void IsValid_ReturnsFalse_WhenPluginIdIsEmpty()
    {
        var assembly = CreateValidAssembly();
        assembly.PluginId = Guid.Empty;
        assembly.IsValid().Should().BeFalse();
    }

    [Fact]
    public void IsValid_ReturnsFalse_WhenAssemblyNameIsEmpty()
    {
        var assembly = CreateValidAssembly();
        assembly.AssemblyName = string.Empty;
        assembly.IsValid().Should().BeFalse();
    }

    [Fact]
    public void IsValid_ReturnsFalse_WhenAssemblyNameIsWhiteSpace()
    {
        var assembly = CreateValidAssembly();
        assembly.AssemblyName = "   ";
        assembly.IsValid().Should().BeFalse();
    }

    [Fact]
    public void IsValid_ReturnsFalse_WhenAssemblyVersionIsEmpty()
    {
        var assembly = CreateValidAssembly();
        assembly.AssemblyVersion = string.Empty;
        assembly.IsValid().Should().BeFalse();
    }

    [Fact]
    public void IsValid_ReturnsFalse_WhenAssemblyVersionIsWhiteSpace()
    {
        var assembly = CreateValidAssembly();
        assembly.AssemblyVersion = "   ";
        assembly.IsValid().Should().BeFalse();
    }

    [Fact]
    public void IsValid_ReturnsFalse_WhenFilePathIsEmpty()
    {
        var assembly = CreateValidAssembly();
        assembly.FilePath = string.Empty;
        assembly.IsValid().Should().BeFalse();
    }

    [Fact]
    public void IsValid_ReturnsFalse_WhenFilePathIsWhiteSpace()
    {
        var assembly = CreateValidAssembly();
        assembly.FilePath = "   ";
        assembly.IsValid().Should().BeFalse();
    }

    // ── Method Tests ────────────────────────────────────────────────────────

    [Fact]
    public void GetQualifiedName_ReturnsCorrectString_WhenPublicKeyTokenEmpty()
    {
        var assembly = CreateValidAssembly();
        assembly.PublicKeyToken = string.Empty;

        var qualifiedName = assembly.GetQualifiedName();

        qualifiedName.Should().Be("TestAssembly, Version=1.0.0");
    }

    [Fact]
    public void GetQualifiedName_ReturnsCorrectString_WhenPublicKeyTokenProvided()
    {
        var assembly = CreateValidAssembly();
        assembly.PublicKeyToken = "abcd1234";

        var qualifiedName = assembly.GetQualifiedName();

        qualifiedName.Should().Be("TestAssembly, Version=1.0.0, PublicKeyToken=abcd1234");
    }

    [Fact]
    public void UpdateFileInfo_UpdatesProperties_AndThrowsOnNullOrEmptyFilePath()
    {
        var assembly = CreateValidAssembly();
        const string newPath = "/new/path.dll";
        const long newSize = 2048L;
        const string newHash = "def456";

        // Act
        assembly.UpdateFileInfo(newPath, newSize, newHash);

        // Assert
        assembly.FilePath.Should().Be(newPath);
        assembly.FileSizeBytes.Should().Be(newSize);
        assembly.FileHash.Should().Be(newHash);
        assembly.LastModifiedAt.Should().BeCloseTo(DateTime.UtcNow, precision: TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void UpdateFileInfo_ThrowsArgumentException_WhenFilePathIsNull()
    {
        var assembly = CreateValidAssembly();

        var act = () => assembly.UpdateFileInfo(null!, 100L, "hash");

        act.Should().Throw<ArgumentException>()
            .WithParameterName("filePath");
    }

    [Fact]
    public void UpdateFileInfo_ThrowsArgumentException_WhenFilePathIsEmpty()
    {
        var assembly = CreateValidAssembly();

        var act = () => assembly.UpdateFileInfo(string.Empty, 100L, "hash");

        act.Should().Throw<ArgumentException>()
            .WithParameterName("filePath");
    }

    [Fact]
    public void UpdateFileInfo_ThrowsArgumentException_WhenFilePathIsWhiteSpace()
    {
        var assembly = CreateValidAssembly();

        var act = () => assembly.UpdateFileInfo("   ", 100L, "hash");

        act.Should().Throw<ArgumentException>()
            .WithParameterName("filePath");
    }

    [Fact]
    public void MarkAsLoaded_SetsStatusLoadedAndSetsLoadContextIdAndLoadedAt()
    {
        var assembly = CreateValidAssembly();
        const string contextId = "load-context-42";

        // Act
        assembly.MarkAsLoaded(contextId);

        // Assert
        assembly.Status.Should().Be(AssemblyLoadStatus.Loaded);
        assembly.LoadContextId.Should().Be(contextId);
        assembly.LoadedAt.Should().BeCloseTo(DateTime.UtcNow, precision: TimeSpan.FromSeconds(1));
        assembly.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void MarkAsFailedLoad_SetsStatusFailedAndSetsErrorMessageAndClearsLoadedAt()
    {
        var assembly = CreateValidAssembly();
        const string errorMsg = "Failed to load assembly";

        // Act
        assembly.MarkAsFailedLoad(errorMsg);

        // Assert
        assembly.Status.Should().Be(AssemblyLoadStatus.Failed);
        assembly.ErrorMessage.Should().Be(errorMsg);
        assembly.LoadedAt.Should().BeNull();
        // LoadContextId should remain unchanged (not cleared by MarkAsFailedLoad)
        assembly.LoadContextId.Should().Be(string.Empty);
    }

    [Fact]
    public void MarkAsFailedLoad_HandlesNullErrorMessage_UsesDefaultMessage()
    {
        var assembly = CreateValidAssembly();

        // Act
        assembly.MarkAsFailedLoad(null!);

        // Assert
        assembly.Status.Should().Be(AssemblyLoadStatus.Failed);
        assembly.ErrorMessage.Should().Be("Unknown error.");
        assembly.LoadedAt.Should().BeNull();
    }
}