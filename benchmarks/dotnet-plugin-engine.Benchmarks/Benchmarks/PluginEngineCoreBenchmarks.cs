using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using PluginEngine.Domain.Entities;
using PluginEngine.Services.Abstractions;

namespace PluginEngine.Benchmarks;

/// <summary>
/// Benchmarks for core PluginEngine operations - the main façade operations.
/// Measures throughput and memory allocations for the main engine operations.
/// </summary>
[MemoryDiagnoser]
[HideColumns("Job", "RatioSD", "Alloc Ratio")]
[GroupBenchmarksBy(BenchmarkDotNet.Configs.BenchmarkLogicalGroupRule.ByCategory)]
public class PluginEngineCoreBenchmarks
{
    private PluginEngine _pluginEngine = null!;
    private IPluginLoaderService _pluginLoaderService = null!;
    private string _testPluginDirectory = string.Empty;
    private const int PluginCount = 20;

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

        // Setup the PluginEngine
        var services = new ServiceCollection();
        services.AddPluginEngineCore(options =>
        {
            options.PluginDirectory = _testPluginDirectory;
        });

        var serviceProvider = services.BuildServiceProvider();
        _pluginEngine = serviceProvider.GetRequiredService<PluginEngine>();
        _pluginLoaderService = serviceProvider.GetRequiredService<IPluginLoaderService>();

        // Initialize the engine
        await _pluginEngine.InitializeAsync();
    }

    [GlobalCleanup]
    public async Task GlobalCleanup()
    {
        // Clean up test files
        if (Directory.Exists(_testPluginDirectory))
        {
            Directory.Delete(_testPluginDirectory, true);
        }

        // Shutdown the engine
        await _pluginEngine.ShutdownAsync();
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

        // Save the assembly
        assemblyBuilder.Save(Path.GetFileName(outputPath));
    }

    /// <summary>
    /// Benchmark: Engine initialization
    /// Measures the time to initialize the PluginEngine
    /// </summary>
    [BenchmarkCategory("Engine Operations")]
    [Benchmark(Baseline = true)]
    public async Task Initialize_Engine()
    {
        var services = new ServiceCollection();
        services.AddPluginEngineCore();
        var serviceProvider = services.BuildServiceProvider();
        var engine = serviceProvider.GetRequiredService<PluginEngine>();

        await engine.InitializeAsync();
        await engine.ShutdownAsync();
    }

    /// <summary>
    /// Benchmark: Bulk plugin loading through PluginEngine façade
    /// Measures the time to load all plugins using the main PluginEngine.LoadAllPluginsAsync method
    /// </summary>
    [BenchmarkCategory("Engine Operations")]
    [Benchmark]
    public async Task LoadAllPlugins_ThroughEngine()
    {
        await _pluginEngine.LoadAllPluginsAsync();
    }

    /// <summary>
    /// Benchmark: Get engine health information
    /// Measures the time to retrieve comprehensive health statistics
    /// </summary>
    [BenchmarkCategory("Engine Operations")]
    [Benchmark]
    public async Task GetHealthInfo()
    {
        await _pluginEngine.GetHealthInfoAsync();
    }

    /// <summary>
    /// Benchmark: Get engine status
    /// Measures the time to retrieve the current engine status
    /// </summary>
    [BenchmarkCategory("Engine Operations")]
    [Benchmark]
    public async Task GetStatus()
    {
        await _pluginEngine.GetStatusAsync();
    }

    /// <summary>
    /// Benchmark: Bulk plugin unloading through PluginEngine façade
    /// Measures the time to unload all plugins using the main PluginEngine.UnloadAllPluginsAsync method
    /// </summary>
    [BenchmarkCategory("Engine Operations")]
    [Benchmark]
    public async Task UnloadAllPlugins_ThroughEngine()
    {
        await _pluginEngine.UnloadAllPluginsAsync();
    }

    /// <summary>
    /// Benchmark: Plugin lookup through PluginEngine façade
    /// Measures the time to check if a plugin is loaded using the main PluginEngine
    /// </summary>
    [BenchmarkCategory("Engine Operations")]
    [Benchmark]
    public async Task GetLoadedPlugin_ThroughEngine()
    {
        // Load some plugins first
        await _pluginEngine.LoadAllPluginsAsync();

        // Get a random loaded plugin
        var loadedPlugins = await _pluginLoaderService.GetAllLoadedPluginsAsync();
        var randomPlugin = loadedPlugins.FirstOrDefault();

        if (randomPlugin != null)
        {
            await _pluginEngine.PluginManager.GetPluginAsync(randomPlugin.Id);
        }
    }

    /// <summary>
    /// Benchmark: Engine with plugin execution enabled
    /// Measures the time to initialize engine with execution capabilities
    /// </summary>
    [BenchmarkCategory("Engine Operations - Advanced")]
    [Benchmark]
    public async Task Initialize_EngineWithExecution()
    {
        var services = new ServiceCollection();
        services.AddPluginEngineCore(options =>
        {
            options.PluginDirectory = _testPluginDirectory;
            options.EnableExecution = true;
        });
        var serviceProvider = services.BuildServiceProvider();
        var engine = serviceProvider.GetRequiredService<PluginEngine>();

        await engine.InitializeAsync();
        await engine.ShutdownAsync();
    }

    /// <summary>
    /// Benchmark: Engine health check with loaded plugins
    /// Measures the time to get health info with plugins loaded
    /// </summary>
    [BenchmarkCategory("Engine Operations - Health")]
    [Benchmark]
    public async Task GetHealthInfo_WithPlugins()
    {
        await _pluginEngine.LoadAllPluginsAsync();
        await _pluginEngine.GetHealthInfoAsync();
    }

    /// <summary>
    /// Benchmark: Sequential plugin operations
    /// Measures the time to perform multiple plugin operations sequentially
    /// </summary>
    [BenchmarkCategory("Engine Operations - Bulk")]
    [Benchmark]
    public async Task SequentialPluginOperations()
    {
        await _pluginEngine.LoadAllPluginsAsync();
        await _pluginEngine.GetHealthInfoAsync();
        await _pluginEngine.GetStatusAsync();
        await _pluginEngine.UnloadAllPluginsAsync();
    }
}
