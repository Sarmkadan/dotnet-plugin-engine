using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using Microsoft.Extensions.DependencyInjection;
using PluginEngine.Domain.Entities;
using PluginEngine.Services.Abstractions;

namespace PluginEngine.Benchmarks;

/// <summary>
/// Benchmarks for plugin loading operations - the most critical performance path in the engine.
/// Measures throughput and memory allocations for plugin loading/unloading scenarios.
/// </summary>
[MemoryDiagnoser]
[HideColumns("Job", "RatioSD", "Alloc Ratio")]
[GroupBenchmarksBy(BenchmarkDotNet.Configs.BenchmarkLogicalGroupRule.ByCategory)]
public class PluginLoadingBenchmarks
{
    private IPluginLoaderService _pluginLoaderService = null!;
    private string _testPluginPath = string.Empty;
    private string _testPluginDirectory = string.Empty;
    private const int PluginCount = 10;

    [GlobalSetup]
    public async Task GlobalSetup()
    {
        // Create a test plugin directory with sample plugins
        _testPluginDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testPluginDirectory);

        // Create sample plugin assemblies (simplified for benchmarking)
        // In a real scenario, these would be actual plugin DLLs
        for (var i = 0; i < PluginCount; i++)
        {
            var pluginName = $"TestPlugin{i}.dll";
            _testPluginPath = Path.Combine(_testPluginDirectory, pluginName);

            // Create a minimal plugin assembly
            await CreateTestPluginAssembly(_testPluginPath);
        }

        // Initialize the plugin loader service
        var serviceProvider = new ServiceCollection()
            .AddPluginEngineCore()
            .BuildServiceProvider();

        _pluginLoaderService = serviceProvider.GetRequiredService<IPluginLoaderService>();
    }

    [GlobalCleanup]
    public async Task GlobalCleanup()
    {
        // Clean up test files
        if (Directory.Exists(_testPluginDirectory))
        {
            Directory.Delete(_testPluginDirectory, true);
        }

        // Unload any loaded plugins
        var loadedPlugins = await _pluginLoaderService.GetAllLoadedPluginsAsync();
        foreach (var plugin in loadedPlugins)
        {
            await _pluginLoaderService.UnloadPluginAsync(plugin.Id);
        }
    }

    private static async Task CreateTestPluginAssembly(string outputPath)
    {
        // Create a minimal plugin assembly for benchmarking
        // This simulates a real plugin structure without requiring actual plugin files
        var assembly = new System.Reflection.AssemblyName("TestPlugin");
        var assemblyBuilder = System.Runtime.Loader.AssemblyLoadContext.Default.DefineDynamicAssembly(
            assembly,
            System.Reflection.Emit.AssemblyBuilderAccess.Save);

        var moduleBuilder = assemblyBuilder.DefineDynamicModule("TestModule");

        // Define a simple plugin lifecycle class
        var typeBuilder = moduleBuilder.DefineType(
            "TestPluginLifecycle",
            System.Reflection.TypeAttributes.Public,
            typeof(object),
            new[] { typeof(IPluginLifecycle) });

        // Implement IPluginLifecycle interface
        var onBeforeLoadMethod = typeBuilder.DefineMethod(
            "OnBeforeLoadAsync",
            System.Reflection.MethodAttributes.Public | System.Reflection.MethodAttributes.Virtual,
            typeof(Task),
            Type.EmptyTypes);

        var ilGenerator = onBeforeLoadMethod.GetILGenerator();
        ilGenerator.Emit(System.Reflection.Emit.OpCodes.Ret);

        var onAfterLoadMethod = typeBuilder.DefineMethod(
            "OnAfterLoadAsync",
            System.Reflection.MethodAttributes.Public | System.Reflection.MethodAttributes.Virtual,
            typeof(Task),
            Type.EmptyTypes);
        ilGenerator = onAfterLoadMethod.GetILGenerator();
        ilGenerator.Emit(System.Reflection.Emit.OpCodes.Ret);

        var onBeforeUnloadMethod = typeBuilder.DefineMethod(
            "OnBeforeUnloadAsync",
            System.Reflection.MethodAttributes.Public | System.Reflection.MethodAttributes.Virtual,
            typeof(Task),
            Type.EmptyTypes);
        ilGenerator = onBeforeUnloadMethod.GetILGenerator();
        ilGenerator.Emit(System.Reflection.Emit.OpCodes.Ret);

        var onAfterUnloadMethod = typeBuilder.DefineMethod(
            "OnAfterUnloadAsync",
            System.Reflection.MethodAttributes.Public | System.Reflection.MethodAttributes.Virtual,
            typeof(Task),
            Type.EmptyTypes);
        ilGenerator = onAfterUnloadMethod.GetILGenerator();
        ilGenerator.Emit(System.Reflection.Emit.OpCodes.Ret);

        var pluginType = typeBuilder.CreateType()!;

        // Save the assembly
        assemblyBuilder.Save(Path.GetFileName(outputPath));
    }

    /// <summary>
    /// Benchmark: Single plugin loading throughput
    /// Measures the time to load a single plugin assembly
    /// </summary>
    [BenchmarkCategory("Single Plugin Loading")]
    [Benchmark(Baseline = true)]
    public async Task Load_SinglePlugin()
    {
        await _pluginLoaderService.LoadPluginAsync(_testPluginPath);
    }

    /// <summary>
    /// Benchmark: Batch plugin loading throughput
    /// Measures the time to load multiple plugins from a directory
    /// </summary>
    [BenchmarkCategory("Batch Plugin Loading")]
    [Benchmark]
    public async Task Load_AllPluginsFromDirectory()
    {
        await _pluginLoaderService.LoadPluginsFromDirectoryAsync(_testPluginDirectory);
    }

    /// <summary>
    /// Benchmark: Plugin unloading throughput
    /// Measures the time to unload a loaded plugin
    /// </summary>
    [BenchmarkCategory("Plugin Unloading")]
    [Benchmark]
    public async Task Unload_Plugin()
    {
        // Load a plugin first
        var plugin = await _pluginLoaderService.LoadPluginAsync(_testPluginPath);

        // Then unload it
        await _pluginLoaderService.UnloadPluginAsync(plugin.Id);
    }

    /// <summary>
    /// Benchmark: Plugin reload throughput
    /// Measures the time to reload a plugin (unload + load)
    /// </summary>
    [BenchmarkCategory("Plugin Reloading")]
    [Benchmark]
    public async Task Reload_Plugin()
    {
        // Load a plugin first
        var plugin = await _pluginLoaderService.LoadPluginAsync(_testPluginPath);

        // Then reload it
        await _pluginLoaderService.ReloadPluginAsync(plugin.Id);
    }

    /// <summary>
    /// Benchmark: Plugin enumeration throughput
    /// Measures the time to get all loaded plugins
    /// </summary>
    [BenchmarkCategory("Plugin Management")]
    [Benchmark]
    public async Task GetAllLoadedPlugins()
    {
        await _pluginLoaderService.GetAllLoadedPluginsAsync();
    }

    /// <summary>
    /// Benchmark: Plugin lookup throughput
    /// Measures the time to check if a plugin is loaded
    /// </summary>
    [BenchmarkCategory("Plugin Management")]
    [Benchmark]
    public async Task IsPluginLoaded()
    {
        await _pluginLoaderService.IsPluginLoadedAsync(Guid.NewGuid());
    }

    /// <summary>
    /// Benchmark: Concurrent plugin loading
    /// Measures the time to load multiple plugins concurrently
    /// </summary>
    [BenchmarkCategory("Plugin Loading - Concurrency")]
    [Benchmark]
    public async Task Load_PluginsConcurrently()
    {
        var loadTasks = new List<Task>();
        for (var i = 0; i < 5; i++)
        {
            var pluginPath = Path.Combine(_testPluginDirectory, $"TestPlugin{i}.dll");
            loadTasks.Add(_pluginLoaderService.LoadPluginAsync(pluginPath));
        }
        await Task.WhenAll(loadTasks);
    }

    /// <summary>
    /// Benchmark: Plugin loading with dependency resolution
    /// Measures the time to load plugins with dependency resolution enabled
    /// </summary>
    [BenchmarkCategory("Plugin Loading - Advanced")]
    [Benchmark]
    public async Task Load_WithDependencyResolution()
    {
        await _pluginLoaderService.LoadPluginsFromDirectoryAsync(_testPluginDirectory);
    }

    /// <summary>
    /// Benchmark: Plugin loading with validation
    /// Measures the time to load plugins with file validation enabled
    /// </summary>
    [BenchmarkCategory("Plugin Loading - Validation")]
    [Benchmark]
    public async Task Load_WithValidation()
    {
        await _pluginLoaderService.LoadPluginsFromDirectoryAsync(_testPluginDirectory);
    }
}
