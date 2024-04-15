using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using PluginEngine.Domain.Entities;
using PluginEngine.Services.Abstractions;

namespace PluginEngine.Benchmarks;

/// <summary>
/// Benchmarks for plugin discovery operations - critical for finding plugins in directories.
/// Measures throughput and memory allocations for plugin discovery and metadata extraction.
/// </summary>
[MemoryDiagnoser]
[HideColumns("Job", "RatioSD", "Alloc Ratio")]
[GroupBenchmarksBy(BenchmarkDotNet.Configs.BenchmarkLogicalGroupRule.ByCategory)]
public class PluginDiscoveryBenchmarks
{
    private IPluginLoaderService _pluginLoaderService = null!;
    private string _testPluginDirectory = string.Empty;
    private const int PluginCount = 50;
    private const int LargePluginCount = 200;

    [GlobalSetup]
    public async Task GlobalSetup()
    {
        // Create a test plugin directory with sample plugins
        _testPluginDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testPluginDirectory);

        // Create sample plugin assemblies
        for (var i = 0; i < PluginCount; i++)
        {
            var pluginName = $"TestPlugin{i}.dll";
            var pluginPath = Path.Combine(_testPluginDirectory, pluginName);
            await CreateTestPluginAssembly(pluginPath);
        }

        // Setup the plugin loader service
        var serviceProvider = new ServiceCollection()
            .AddPluginEngineCore()
            .BuildServiceProvider();

        _pluginLoaderService = serviceProvider.GetRequiredService<IPluginLoaderService>();
    }

    [GlobalCleanup]
    public void GlobalCleanup()
    {
        // Clean up test files
        if (Directory.Exists(_testPluginDirectory))
        {
            Directory.Delete(_testPluginDirectory, true);
        }
    }

    private static async Task CreateTestPluginAssembly(string outputPath)
    {
        // Create a minimal plugin assembly for benchmarking
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

        // Add plugin metadata
        var pluginInfoType = moduleBuilder.DefineType(
            "PluginMetadata",
            System.Reflection.TypeAttributes.Public);

        var pluginInfoConstructor = pluginInfoType.DefineConstructor(
            System.Reflection.MethodAttributes.Public,
            System.Reflection.CallingConventions.Standard,
            new[] { typeof(string), typeof(string), typeof(string) });
        ilGenerator = pluginInfoConstructor.GetILGenerator();
        ilGenerator.Emit(System.Reflection.Emit.OpCodes.Ret);

        var pluginInfo = pluginInfoType.CreateType()!;

        // Save the assembly
        assemblyBuilder.Save(Path.GetFileName(outputPath));
    }

    /// <summary>
    /// Benchmark: Discover plugins in empty directory
    /// Measures baseline performance with no plugins
    /// </summary>
    [BenchmarkCategory("Plugin Discovery")]
    [Benchmark(Baseline = true)]
    public async Task Discover_EmptyDirectory()
    {
        await _pluginLoaderService.LoadPluginsFromDirectoryAsync(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()));
    }

    /// <summary>
    /// Benchmark: Discover plugins in directory with 50 plugins
    /// Measures performance with typical plugin count
    /// </summary>
    [BenchmarkCategory("Plugin Discovery")]
    [Benchmark]
    public async Task Discover_50Plugins()
    {
        await _pluginLoaderService.LoadPluginsFromDirectoryAsync(_testPluginDirectory);
    }

    /// <summary>
    /// Benchmark: Discover plugins in directory with 200 plugins
    /// Measures scalability with large plugin count
    /// </summary>
    [BenchmarkCategory("Plugin Discovery - Scalability")]
    [Benchmark]
    public async Task Discover_200Plugins()
    {
        // Create a large plugin directory
        var largeDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(largeDirectory);

        try
        {
            // Create 200 sample plugin assemblies
            for (var i = 0; i < LargePluginCount; i++)
            {
                var pluginName = $"LargeTestPlugin{i}.dll";
                var pluginPath = Path.Combine(largeDirectory, pluginName);
                await CreateTestPluginAssembly(pluginPath);
            }

            await _pluginLoaderService.LoadPluginsFromDirectoryAsync(largeDirectory);
        }
        finally
        {
            // Clean up
            if (Directory.Exists(largeDirectory))
            {
                Directory.Delete(largeDirectory, true);
            }
        }
    }

    /// <summary>
    /// Benchmark: Get plugin metadata for single plugin
    /// Measures performance of metadata extraction
    /// </summary>
    [BenchmarkCategory("Plugin Metadata")]
    [Benchmark]
    public async Task GetPluginMetadata_Single()
    {
        var plugins = await _pluginLoaderService.GetAllLoadedPluginsAsync();
        _ = plugins.Count();
    }

    /// <summary>
    /// Benchmark: Validate plugin files
    /// Measures performance of plugin file validation
    /// </summary>
    [BenchmarkCategory("Plugin Validation")]
    [Benchmark]
    public async Task ValidatePluginFiles()
    {
        // Plugin file validation is handled during load
        await _pluginLoaderService.LoadPluginsFromDirectoryAsync(_testPluginDirectory);
    }

    /// <summary>
    /// Benchmark: Filter valid plugins
    /// Measures performance of plugin filtering
    /// </summary>
    [BenchmarkCategory("Plugin Filtering")]
    [Benchmark]
    public async Task FilterValidPlugins()
    {
        var allPlugins = await _pluginLoaderService.GetAllLoadedPluginsAsync();
        var validPlugins = allPlugins.Where(p => p.Status != PluginStatus.Failed).ToList();
        _ = validPlugins.Count;
    }
}