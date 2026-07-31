using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Running;
using PluginEngine;
using PluginEngine.Configuration;
using PluginEngine.Services.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

[MemoryDiagnoser]
public class PluginEngineBenchmarks
{
    private PluginEngine _pluginEngine;
    private PluginEngineOptions _options;
    private IPluginManagerService _pluginManagerService;
    private IPluginLoaderService _pluginLoaderService;
    private IDependencyResolutionService _dependencyResolutionService;
    private IVersioningService _versioningService;
    private IHotReloadService _hotReloadService;

    [GlobalSetup]
    public void Setup()
    {
        _options = new PluginEngineOptions { PluginDirectory = Path.GetTempPath() };
        _pluginManagerService = new PluginManagerService();
        _pluginLoaderService = new PluginLoaderService();
        _dependencyResolutionService = new DependencyResolutionService();
        _versioningService = new VersioningService();
        _hotReloadService = new HotReloadService();
        _pluginEngine = new PluginEngine(_pluginManagerService, _pluginLoaderService, _dependencyResolutionService, _versioningService, _hotReloadService, _options);
    }

    [Benchmark]
    [Params(10, 100, 1000)]
    public async Task LoadAllPluginsAsync(int count)
    {
        // Create test plugins
        for (int i = 0; i < count; i++)
        {
            var pluginPath = Path.Combine(_options.PluginDirectory, $"plugin{i}.dll");
            using var stream = new FileStream(pluginPath, FileMode.Create);
            using var writer = new StreamWriter(stream);
            writer.Write("Test plugin");
        }

        await _pluginEngine.LoadAllPluginsAsync();
    }

    [Benchmark]
    public async Task InitializeAsync()
    {
        await _pluginEngine.InitializeAsync();
    }

    [Benchmark]
    public async Task GetStatusAsync()
    {
        await _pluginEngine.GetStatusAsync();
    }

    [Benchmark]
    public async Task GetHealthInfoAsync()
    {
        await _pluginEngine.GetHealthInfoAsync();
    }

    [Benchmark]
    public async Task UnloadAllPluginsAsync()
    {
        await _pluginEngine.UnloadAllPluginsAsync();
    }
}
