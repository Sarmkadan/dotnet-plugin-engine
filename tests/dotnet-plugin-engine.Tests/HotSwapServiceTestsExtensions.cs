#nullable enable

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using PluginEngine.Domain.Entities;
using PluginEngine.Results;
using PluginEngine.Services.Abstractions;
using PluginEngine.Services.Implementations;
using Xunit;

namespace PluginEngine.Tests;

/// <summary>
/// Extension methods for <see cref="HotSwapServiceTests"/> to provide additional test utilities and assertions.
/// </summary>
public static class HotSwapServiceTestsExtensions
{
    /// <summary>
    /// Creates a new instance of <see cref="HotSwapServiceTests"/> with customizable setup.
    /// </summary>
    /// <param name="mockLoader">Optional custom mock for IPluginLoaderService</param>
    /// <param name="mockLogger">Optional custom mock for ILogger&lt;HotSwapService&gt;</param>
    /// <param name="tempDir">Optional custom temp directory path</param>
    /// <returns>New instance of HotSwapServiceTests</returns>
    public static HotSwapServiceTests CreateTestInstance(
        this HotSwapServiceTests _,
        Mock<IPluginLoaderService>? mockLoader = null,
        Mock<ILogger<HotSwapService>>? mockLogger = null,
        string? tempDir = null)
    {
        var loader = mockLoader ?? new Mock<IPluginLoaderService>();
        var logger = mockLogger ?? new Mock<ILogger<HotSwapService>>();
        var directory = tempDir ?? Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        var instance = new HotSwapServiceTests(loader, logger, directory);
        return instance;
    }

    /// <summary>
    /// Creates a test plugin with customizable properties.
    /// </summary>
    /// <param name="status">Plugin status</param>
    /// <param name="assemblyPath">Custom assembly path</param>
    /// <param name="name">Plugin name</param>
    /// <param name="version">Plugin version</param>
    /// <returns>Configured Plugin instance</returns>
    public static Plugin CreatePlugin(
        this HotSwapServiceTests _,
        PluginStatus status = PluginStatus.Active,
        string? assemblyPath = null,
        string? name = null,
        string? version = null)
    {
        return new Plugin
        {
            Id = Guid.NewGuid(),
            Name = name ?? "TestPlugin",
            Version = version ?? "1.0.0",
            AssemblyPath = assemblyPath ?? CreateTempDll(_),
            Status = status
        };
    }

    /// <summary>
    /// Creates a minimal DLL file for testing.
    /// </summary>
    /// <param name="name">DLL filename</param>
    /// <param name="tempDir">Optional custom temp directory</param>
    /// <returns>Full path to created DLL</returns>
    public static string CreateTempDll(this HotSwapServiceTests _, string name = "plugin.dll", string? tempDir = null)
    {
        var directory = tempDir ?? Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var path = Path.Combine(directory, name);

        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllBytes(path, [0x4D, 0x5A]); // minimal PE header stub
        return path;
    }

    /// <summary>
    /// Creates a <see cref="HotSwapService"/> instance using the test's mocks.
    /// </summary>
    /// <param name="tests">Test instance</param>
    /// <returns>Configured HotSwapService</returns>
    public static HotSwapService CreateService(this HotSwapServiceTests tests)
    {
        return new HotSwapService(tests._mockLoader.Object, tests._mockLogger.Object);
    }

    /// <summary>
    /// Asserts that a swap operation was successful and validates the result.
    /// </summary>
    /// <param name="result">Swap result to validate</param>
    /// <param name="expectedNewPath">Expected new assembly path</param>
    public static void ShouldBeSuccessfulSwap(
        this HotSwapServiceTests _,
        Result<Plugin> result,
        string expectedNewPath)
    {
        result.Success.Should().BeTrue();
        result.ErrorCode.Should().Be(0);
        result.Data.Should().NotBeNull();
        result.Data!.AssemblyPath.Should().Be(expectedNewPath);
    }

    /// <summary>
    /// Asserts that a swap operation failed with expected error code.
    /// </summary>
    /// <param name="result">Swap result to validate</param>
    /// <param name="expectedErrorCode">Expected error code</param>
    public static void ShouldBeFailedSwap(
        this HotSwapServiceTests _,
        Result<Plugin> result,
        int expectedErrorCode)
    {
        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be(expectedErrorCode);
    }

    /// <summary>
    /// Asserts that swap history contains exactly one entry with expected properties.
    /// </summary>
    /// <param name="history">Swap history result</param>
    /// <param name="expectedNewPath">Expected new assembly path</param>
    /// <param name="expectedSuccess">Expected success status</param>
    public static void ShouldHaveSingleHistoryEntry(
        this HotSwapServiceTests _,
        Result<IReadOnlyList<PluginSwapHistory>> history,
        string expectedNewPath,
        bool expectedSuccess = true)
    {
        history.Success.Should().BeTrue();
        history.Data.Should().NotBeNull();
        history.Data.Should().HaveCount(1);

        var entry = history.Data![0];
        entry.NewAssemblyPath.Should().Be(expectedNewPath);
        entry.Success.Should().Be(expectedSuccess);
    }

    /// <summary>
    /// Asserts that the plugin callback was invoked with the expected plugin.
    /// </summary>
    /// <param name="callbackArg">Callback argument to validate</param>
    /// <param name="expectedAssemblyPath">Expected assembly path</param>
    public static void CallbackShouldReceivePlugin(
        this HotSwapServiceTests _,
        Plugin? callbackArg,
        string expectedAssemblyPath)
    {
        callbackArg.Should().NotBeNull();
        callbackArg!.AssemblyPath.Should().Be(expectedAssemblyPath);
    }

    /// <summary>
    /// Creates a plugin with a specific status and validates CanSwap behavior.
    /// </summary>
    /// <param name="tests">Test instance</param>
    /// <param name="status">Plugin status to test</param>
    /// <returns>Configured plugin</returns>
    public static Plugin CreateAndTestCanSwap(
        this HotSwapServiceTests tests,
        PluginStatus status)
    {
        var plugin = tests.CreatePlugin(status);
        var service = tests.CreateService();

        var canSwap = service.CanSwap(plugin);

        if (status == PluginStatus.Active || status == PluginStatus.Loaded)
        {
            canSwap.Should().BeTrue();
        }
        else
        {
            canSwap.Should().BeFalse();
        }

        return plugin;
    }
}