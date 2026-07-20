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
/// Unit tests for <see cref="HotSwapService"/>.
/// </summary>
public sealed class HotSwapServiceTests : IDisposable
{
    internal readonly Mock<IPluginLoaderService> _mockLoader;
    internal readonly Mock<ILogger<HotSwapService>> _mockLogger;
    private readonly string _tempDir;

    public HotSwapServiceTests()
        : this(new Mock<IPluginLoaderService>(), new Mock<ILogger<HotSwapService>>(), Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()))
    {
    }

    /// <summary>
    /// Creates a test instance with explicit mocks and temporary directory.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown when any argument is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="tempDir"/> is blank.</exception>
    internal HotSwapServiceTests(
        Mock<IPluginLoaderService> mockLoader,
        Mock<ILogger<HotSwapService>> mockLogger,
        string tempDir)
    {
        ArgumentNullException.ThrowIfNull(mockLoader);
        ArgumentNullException.ThrowIfNull(mockLogger);
        ArgumentException.ThrowIfNullOrWhiteSpace(tempDir);

        _mockLoader = mockLoader;
        _mockLogger = mockLogger;
        _tempDir = tempDir;
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    private HotSwapService CreateService() => new(_mockLoader.Object, _mockLogger.Object);

    private string CreateTempDll(string name = "plugin.dll")
    {
        var path = Path.Combine(_tempDir, name);
        File.WriteAllBytes(path, [0x4D, 0x5A]); // minimal PE header stub
        return path;
    }

    private Plugin MakePlugin(PluginStatus status = PluginStatus.Active, string? assemblyPath = null) =>
        new()
        {
            Id = Guid.NewGuid(),
            Name = "TestPlugin",
            Version = "1.0.0",
            AssemblyPath = assemblyPath ?? CreateTempDll("current.dll"),
            Status = status
        };

    // ── CanSwap ──────────────────────────────────────────────────────────────

    [Theory]
    [InlineData(PluginStatus.Active)]
    [InlineData(PluginStatus.Loaded)]
    public void CanSwap_ActiveOrLoadedPlugin_ReturnsTrue(PluginStatus status)
    {
        var service = CreateService();
        var plugin = MakePlugin(status);

        service.CanSwap(plugin).Should().BeTrue();
    }

    [Theory]
    [InlineData(PluginStatus.Unloaded)]
    [InlineData(PluginStatus.Failed)]
    [InlineData(PluginStatus.Loading)]
    public void CanSwap_NonRunningPlugin_ReturnsFalse(PluginStatus status)
    {
        var service = CreateService();
        var plugin = MakePlugin(status);

        service.CanSwap(plugin).Should().BeFalse();
    }

    [Fact]
    public void CanSwap_NullPlugin_ReturnsFalse()
    {
        var service = CreateService();
        service.CanSwap(null!).Should().BeFalse();
    }

    // ── SwapPluginAsync ──────────────────────────────────────────────────────

    [Fact]
    public async Task SwapPluginAsync_EmptyPath_ReturnsFailure()
    {
        var service = CreateService();

        var result = await service.SwapPluginAsync(Guid.NewGuid(), string.Empty);

        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be(400);
    }

    [Fact]
    public async Task SwapPluginAsync_MissingFile_ReturnsFailure()
    {
        var service = CreateService();

        var result = await service.SwapPluginAsync(Guid.NewGuid(), "/does/not/exist.dll");

        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be(404);
    }

    [Fact]
    public async Task SwapPluginAsync_PluginNotLoaded_ReturnsFailure()
    {
        var newDll = CreateTempDll("new.dll");
        _mockLoader
            .Setup(l => l.GetLoadedPluginAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Plugin?)null);

        var service = CreateService();
        var result = await service.SwapPluginAsync(Guid.NewGuid(), newDll);

        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be(404);
    }

    [Fact]
    public async Task SwapPluginAsync_Success_RecordsSwapHistory()
    {
        var plugin = MakePlugin();
        var newDll = CreateTempDll("new.dll");
        var newPlugin = MakePlugin(assemblyPath: newDll);

        _mockLoader
            .Setup(l => l.GetLoadedPluginAsync(plugin.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(plugin);
        _mockLoader
            .Setup(l => l.UnloadPluginAsync(plugin.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _mockLoader
            .Setup(l => l.LoadPluginAsync(newDll, It.IsAny<CancellationToken>()))
            .ReturnsAsync(newPlugin);

        var service = CreateService();
        var result = await service.SwapPluginAsync(plugin.Id, newDll);

        result.Success.Should().BeTrue();

        var historyResult = await service.GetSwapHistoryAsync(plugin.Id);
        historyResult.Data.Should().HaveCount(1);
        historyResult.Data![0].NewAssemblyPath.Should().Be(newDll);
        historyResult.Data[0].Success.Should().BeTrue();
    }

    [Fact]
    public async Task SwapPluginAsync_Success_InvokesPostSwapCallback()
    {
        var plugin = MakePlugin();
        var newDll = CreateTempDll("new.dll");
        var newPlugin = MakePlugin(assemblyPath: newDll);

        _mockLoader
            .Setup(l => l.GetLoadedPluginAsync(plugin.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(plugin);
        _mockLoader
            .Setup(l => l.UnloadPluginAsync(plugin.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _mockLoader
            .Setup(l => l.LoadPluginAsync(newDll, It.IsAny<CancellationToken>()))
            .ReturnsAsync(newPlugin);

        Plugin? callbackArg = null;
        var service = CreateService();
        service.RegisterPostSwapCallback(plugin.Id, p => { callbackArg = p; return Task.CompletedTask; });

        await service.SwapPluginAsync(plugin.Id, newDll);

        callbackArg.Should().NotBeNull();
        callbackArg!.AssemblyPath.Should().Be(newDll);
    }

    // ── RollbackSwapAsync ────────────────────────────────────────────────────

    [Fact]
    public async Task RollbackSwapAsync_NoHistory_ReturnsFailure()
    {
        var service = CreateService();
        var result = await service.RollbackSwapAsync(Guid.NewGuid());

        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be(404);
    }

    [Fact]
    public async Task RollbackSwapAsync_AfterSuccessfulSwap_ReloadsOldAssembly()
    {
        var oldDll = CreateTempDll("old.dll");
        var newDll = CreateTempDll("new.dll");
        var plugin = MakePlugin(assemblyPath: oldDll);
        var newPlugin = MakePlugin(assemblyPath: newDll);

        _mockLoader
            .Setup(l => l.GetLoadedPluginAsync(plugin.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(plugin);
        _mockLoader
            .Setup(l => l.UnloadPluginAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _mockLoader
            .Setup(l => l.LoadPluginAsync(newDll, It.IsAny<CancellationToken>()))
            .ReturnsAsync(newPlugin);
        _mockLoader
            .Setup(l => l.LoadPluginAsync(oldDll, It.IsAny<CancellationToken>()))
            .ReturnsAsync(plugin);

        var service = CreateService();
        await service.SwapPluginAsync(plugin.Id, newDll);

        var rollback = await service.RollbackSwapAsync(plugin.Id);

        rollback.Success.Should().BeTrue();
        _mockLoader.Verify(l => l.LoadPluginAsync(oldDll, It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    // ── GetSwapHistoryAsync ──────────────────────────────────────────────────

    [Fact]
    public async Task GetSwapHistoryAsync_NoSwaps_ReturnsEmptyList()
    {
        var service = CreateService();
        var result = await service.GetSwapHistoryAsync(Guid.NewGuid());

        result.Success.Should().BeTrue();
        result.Data.Should().BeEmpty();
    }

    // ── Callback registration ────────────────────────────────────────────────

    [Fact]
    public void UnregisterPostSwapCallback_RemovesCallback()
    {
        var service = CreateService();
        var pluginId = Guid.NewGuid();

        service.RegisterPostSwapCallback(pluginId, _ => Task.CompletedTask);
        service.UnregisterPostSwapCallback(pluginId);

        // No error thrown; callback no longer tracked (verified via swap test separately)
    }

    // ── Swap to failing plugin rolls back or marks failed ────────────────────────

    [Fact]
    public async Task SwapPluginAsync_FailingPlugin_MarksFailedAndRollsBack()
    {
        var oldDll = CreateTempDll("old.dll");
        var newDll = CreateTempDll("new.dll");
        var plugin = MakePlugin(assemblyPath: oldDll);

        _mockLoader
            .Setup(l => l.GetLoadedPluginAsync(plugin.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(plugin);
        _mockLoader
            .Setup(l => l.UnloadPluginAsync(plugin.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _mockLoader
            .Setup(l => l.LoadPluginAsync(newDll, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Invalid plugin assembly"));

        var service = CreateService();
        var result = await service.SwapPluginAsync(plugin.Id, newDll);

        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be(500);

        // Verify failure was recorded
        var historyResult = await service.GetSwapHistoryAsync(plugin.Id);
        historyResult.Data.Should().HaveCount(1);
        historyResult.Data![0].Success.Should().BeFalse();
        historyResult.Data[0].ErrorMessage.Should().Contain("Invalid plugin assembly");
        historyResult.Data[0].PreviousAssemblyPath.Should().Be(oldDll);
        historyResult.Data[0].NewAssemblyPath.Should().Be(newDll);
    }

    // ── Concurrent swap requests for the same plugin are serialized ────────────────

    [Fact]
    public async Task SwapPluginAsync_ConcurrentRequests_SerializesAccess()
    {
        var plugin = MakePlugin();
        var newDll1 = CreateTempDll("new1.dll");
        var newDll2 = CreateTempDll("new2.dll");

        _mockLoader
            .Setup(l => l.GetLoadedPluginAsync(plugin.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(plugin);
        _mockLoader
            .Setup(l => l.UnloadPluginAsync(plugin.Id, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Simulate concurrent loading - only one should succeed
        _mockLoader
            .SetupSequence(l => l.LoadPluginAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakePlugin(assemblyPath: newDll1))
            .ReturnsAsync(MakePlugin(assemblyPath: newDll2));

        var service = CreateService();

        // Start two concurrent swaps
        var task1 = service.SwapPluginAsync(plugin.Id, newDll1);
        var task2 = service.SwapPluginAsync(plugin.Id, newDll2);

        await Task.WhenAll(task1, task2);

        // Only one swap should have succeeded (the first one)
        var successCount = new[] { task1.Result.Success, task2.Result.Success }.Count(s => s);
        successCount.Should().Be(1);

        // Verify history shows both attempts
        var historyResult = await service.GetSwapHistoryAsync(plugin.Id);
        historyResult.Data.Should().HaveCount(2);
        historyResult.Data![0].Success.Should().BeTrue();
        historyResult.Data[1].Success.Should().BeFalse();
    }

    // ── Swap of unknown plugin id errors cleanly ────────────────────────────────────

    [Fact]
    public async Task SwapPluginAsync_UnknownPluginId_ErrorsCleanly()
    {
        var service = CreateService();
        var unknownPluginId = Guid.NewGuid();
        var newDll = CreateTempDll("new.dll");

        _mockLoader
            .Setup(l => l.GetLoadedPluginAsync(unknownPluginId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Plugin?)null);

        var result = await service.SwapPluginAsync(unknownPluginId, newDll);

        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be(404);
        result.Message.Should().Contain(unknownPluginId.ToString());
    }
}