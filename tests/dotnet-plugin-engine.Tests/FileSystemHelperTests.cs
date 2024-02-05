#nullable enable
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using PluginEngine.Utils.Helpers;
using System.Runtime.InteropServices;
using Xunit;

namespace PluginEngine.Tests;

public sealed class FileSystemHelperTests
{
    private readonly Mock<ILogger<FileSystemHelper>> _mockLogger;
    private readonly FileSystemHelper _sut;
    private readonly string _testDirectory;

    public FileSystemHelperTests()
    {
        _mockLogger = new Mock<ILogger<FileSystemHelper>>();
        _sut = new FileSystemHelper(_mockLogger.Object);
        _testDirectory = Path.Combine(Path.GetTempPath(), $"plugin-test-{Guid.NewGuid()}");
    }

    [Fact]
    public void EnsureDirectoryExists_WithNonExistentDirectory_CreatesDirectory()
    {
        var testDir = Path.Combine(Path.GetTempPath(), $"test-dir-{Guid.NewGuid()}");
        Directory.Exists(testDir).Should().BeFalse();

        var result = _sut.EnsureDirectoryExists(testDir);

        try
        {
            result.Should().BeTrue();
            Directory.Exists(testDir).Should().BeTrue();
        }
        finally
        {
            if (Directory.Exists(testDir))
                Directory.Delete(testDir);
        }
    }

    [Fact]
    public void EnsureDirectoryExists_WithExistingDirectory_ReturnsTrue()
    {
        var testDir = Path.Combine(Path.GetTempPath(), $"existing-dir-{Guid.NewGuid()}");
        Directory.CreateDirectory(testDir);

        try
        {
            var result = _sut.EnsureDirectoryExists(testDir);

            result.Should().BeTrue();
        }
        finally
        {
            if (Directory.Exists(testDir))
                Directory.Delete(testDir);
        }
    }

    [Fact]
    public void EnsureDirectoryExists_WithInvalidPath_ReturnsFalse()
    {
        var invalidPath = "\0invalid";

        var result = _sut.EnsureDirectoryExists(invalidPath);

        result.Should().BeFalse();
    }

    [Fact]
    public void DiscoverPlugins_WithNonExistentDirectory_ReturnsEmptyList()
    {
        var nonExistentDir = Path.Combine(Path.GetTempPath(), $"nonexistent-{Guid.NewGuid()}");

        var result = _sut.DiscoverPlugins(nonExistentDir);

        result.Should().BeEmpty();
    }

    [Fact]
    public void DiscoverPlugins_WithEmptyDirectory_ReturnsEmptyList()
    {
        var testDir = Path.Combine(Path.GetTempPath(), $"empty-dir-{Guid.NewGuid()}");
        Directory.CreateDirectory(testDir);

        try
        {
            var result = _sut.DiscoverPlugins(testDir);

            result.Should().BeEmpty();
        }
        finally
        {
            Directory.Delete(testDir);
        }
    }

    [Fact]
    public void DiscoverPlugins_WithDllFiles_ReturnsAllDllFiles()
    {
        var testDir = Path.Combine(Path.GetTempPath(), $"plugins-{Guid.NewGuid()}");
        Directory.CreateDirectory(testDir);

        try
        {
            File.Create(Path.Combine(testDir, "Plugin1.dll")).Dispose();
            File.Create(Path.Combine(testDir, "Plugin2.dll")).Dispose();
            File.Create(Path.Combine(testDir, "OtherFile.txt")).Dispose();

            var result = _sut.DiscoverPlugins(testDir).ToList();

            result.Should().HaveCount(2)
                .And.AllSatisfy(f => f.Should().EndWith(".dll"));
        }
        finally
        {
            Directory.Delete(testDir, true);
        }
    }

    [Fact]
    public void DiscoverPlugins_ExcludesFilesStartingWithUnderscore()
    {
        var testDir = Path.Combine(Path.GetTempPath(), $"plugins-underscore-{Guid.NewGuid()}");
        Directory.CreateDirectory(testDir);

        try
        {
            File.Create(Path.Combine(testDir, "Plugin.dll")).Dispose();
            File.Create(Path.Combine(testDir, "_Internal.dll")).Dispose();

            var result = _sut.DiscoverPlugins(testDir).ToList();

            result.Should().HaveCount(1)
                .And.Contain(f => f.Contains("Plugin.dll"));
        }
        finally
        {
            Directory.Delete(testDir, true);
        }
    }


    [Fact]
    public void GetFileInfo_WithExistingFile_ReturnsFileSizeAndModifiedTime()
    {
        var testFile = Path.Combine(Path.GetTempPath(), $"test-{Guid.NewGuid()}.txt");
        var testContent = "test data";
        File.WriteAllText(testFile, testContent);

        try
        {
            var result = _sut.GetFileInfo(testFile);

            result.Should().NotBeNull();
            result!.Value.Size.Should().Be(testContent.Length);
            result.Value.Modified.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        }
        finally
        {
            File.Delete(testFile);
        }
    }

    [Fact]
    public void GetFileInfo_WithNonExistentFile_ReturnsNull()
    {
        var nonExistentFile = Path.Combine(Path.GetTempPath(), $"nonexistent-{Guid.NewGuid()}.txt");

        var result = _sut.GetFileInfo(nonExistentFile);

        result.Should().BeNull();
    }

    [Fact]
    public void GetFileInfo_WithLargeFile_ReturnsCorrectSize()
    {
        var testFile = Path.Combine(Path.GetTempPath(), $"large-{Guid.NewGuid()}.bin");
        var largeContent = new byte[10240];
        new Random().NextBytes(largeContent);
        File.WriteAllBytes(testFile, largeContent);

        try
        {
            var result = _sut.GetFileInfo(testFile);

            result.Should().NotBeNull();
            result!.Value.Size.Should().Be(10240);
        }
        finally
        {
            File.Delete(testFile);
        }
    }

    [Fact]
    public void DeleteDirectoryRecursive_WithExistingDirectory_DeletesDirectory()
    {
        var testDir = Path.Combine(Path.GetTempPath(), $"to-delete-{Guid.NewGuid()}");
        Directory.CreateDirectory(testDir);
        File.Create(Path.Combine(testDir, "file.txt")).Dispose();

        var result = _sut.DeleteDirectoryRecursive(testDir);

        result.Should().BeTrue();
        Directory.Exists(testDir).Should().BeFalse();
    }

    [Fact]
    public void DeleteDirectoryRecursive_WithNonExistentDirectory_ReturnsTrue()
    {
        var nonExistentDir = Path.Combine(Path.GetTempPath(), $"nonexistent-{Guid.NewGuid()}");

        var result = _sut.DeleteDirectoryRecursive(nonExistentDir);

        result.Should().BeTrue();
    }

    [Fact]
    public void SafeCopyFile_WithValidSourceAndDestination_CopiesFile()
    {
        var sourceFile = Path.Combine(Path.GetTempPath(), $"source-{Guid.NewGuid()}.txt");
        var destFile = Path.Combine(Path.GetTempPath(), $"dest-{Guid.NewGuid()}.txt");
        var testContent = "copy test";
        File.WriteAllText(sourceFile, testContent);

        try
        {
            var result = _sut.SafeCopyFile(sourceFile, destFile);

            result.Should().BeTrue();
            File.Exists(destFile).Should().BeTrue();
            File.ReadAllText(destFile).Should().Be(testContent);
        }
        finally
        {
            if (File.Exists(sourceFile)) File.Delete(sourceFile);
            if (File.Exists(destFile)) File.Delete(destFile);
        }
    }

    [Fact]
    public void SafeCopyFile_WithNonExistentSource_ReturnsFalse()
    {
        var nonExistentSource = Path.Combine(Path.GetTempPath(), $"nonexistent-{Guid.NewGuid()}.txt");
        var destFile = Path.Combine(Path.GetTempPath(), $"dest-{Guid.NewGuid()}.txt");

        var result = _sut.SafeCopyFile(nonExistentSource, destFile);

        result.Should().BeFalse();
    }

    [Fact]
    public void SafeCopyFile_WhenDestinationExistsAndOverwriteDisabled_ReturnsFalse()
    {
        var sourceFile = Path.Combine(Path.GetTempPath(), $"source-{Guid.NewGuid()}.txt");
        var destFile = Path.Combine(Path.GetTempPath(), $"dest-{Guid.NewGuid()}.txt");
        File.WriteAllText(sourceFile, "source");
        File.WriteAllText(destFile, "destination");

        try
        {
            var result = _sut.SafeCopyFile(sourceFile, destFile, overwrite: false);

            result.Should().BeFalse();
            File.ReadAllText(destFile).Should().Be("destination");
        }
        finally
        {
            File.Delete(sourceFile);
            File.Delete(destFile);
        }
    }

    [Fact]
    public void SafeCopyFile_WhenDestinationExistsAndOverwriteEnabled_CopiesFile()
    {
        var sourceFile = Path.Combine(Path.GetTempPath(), $"source-{Guid.NewGuid()}.txt");
        var destFile = Path.Combine(Path.GetTempPath(), $"dest-{Guid.NewGuid()}.txt");
        File.WriteAllText(sourceFile, "source content");
        File.WriteAllText(destFile, "old content");

        try
        {
            var result = _sut.SafeCopyFile(sourceFile, destFile, overwrite: true);

            result.Should().BeTrue();
            File.ReadAllText(destFile).Should().Be("source content");
        }
        finally
        {
            File.Delete(sourceFile);
            File.Delete(destFile);
        }
    }

    [Fact]
    public void GetDirectorySize_WithEmptyDirectory_ReturnsZero()
    {
        var testDir = Path.Combine(Path.GetTempPath(), $"empty-{Guid.NewGuid()}");
        Directory.CreateDirectory(testDir);

        try
        {
            var size = _sut.GetDirectorySize(testDir);

            size.Should().Be(0);
        }
        finally
        {
            Directory.Delete(testDir);
        }
    }

    [Fact]
    public void GetDirectorySize_WithFiles_ReturnsTotalSize()
    {
        var testDir = Path.Combine(Path.GetTempPath(), $"sized-{Guid.NewGuid()}");
        Directory.CreateDirectory(testDir);

        try
        {
            File.WriteAllText(Path.Combine(testDir, "file1.txt"), "hello");
            File.WriteAllText(Path.Combine(testDir, "file2.txt"), "world");

            var size = _sut.GetDirectorySize(testDir);

            size.Should().Be(10);
        }
        finally
        {
            Directory.Delete(testDir, true);
        }
    }

    [Fact]
    public void GetDirectorySize_WithNonExistentDirectory_ReturnsZero()
    {
        var nonExistentDir = Path.Combine(Path.GetTempPath(), $"nonexistent-{Guid.NewGuid()}");

        var size = _sut.GetDirectorySize(nonExistentDir);

        size.Should().Be(0);
    }

    [Fact]
    public void CreateBackup_WithExistingFile_CreatesBackupWithTimestamp()
    {
        var testFile = Path.Combine(Path.GetTempPath(), $"backup-test-{Guid.NewGuid()}.txt");
        var testContent = "backup me";
        File.WriteAllText(testFile, testContent);

        string? backupPath = null;
        try
        {
            backupPath = _sut.CreateBackup(testFile);

            backupPath.Should().NotBeNullOrEmpty();
            File.Exists(backupPath).Should().BeTrue();
            File.ReadAllText(backupPath).Should().Be(testContent);
            backupPath!.Should().Contain(".backup.");
        }
        finally
        {
            if (File.Exists(testFile))
                File.Delete(testFile);
            if (backupPath != null && File.Exists(backupPath))
                File.Delete(backupPath);
        }
    }

    [Fact]
    public void CreateBackup_WithNonExistentFile_ReturnsNull()
    {
        var nonExistentFile = Path.Combine(Path.GetTempPath(), $"nonexistent-{Guid.NewGuid()}.txt");

        var backupPath = _sut.CreateBackup(nonExistentFile);

        backupPath.Should().BeNull();
    }

    [Fact]
    public void IsDirectoryWritable_WithWritableDirectory_ReturnsTrue()
    {
        var testDir = Path.Combine(Path.GetTempPath(), $"writable-{Guid.NewGuid()}");
        Directory.CreateDirectory(testDir);

        try
        {
            var result = _sut.IsDirectoryWritable(testDir);

            result.Should().BeTrue();
        }
        finally
        {
            Directory.Delete(testDir);
        }
    }

    [Fact]
    public void IsDirectoryWritable_WithNonExistentDirectory_ReturnsFalse()
    {
        var nonExistentDir = Path.Combine(Path.GetTempPath(), $"nonexistent-{Guid.NewGuid()}");

        var result = _sut.IsDirectoryWritable(nonExistentDir);

        result.Should().BeFalse();
    }
}
