using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using PluginEngine.Domain.Entities;
using PluginEngine.Services.Abstractions;

namespace PluginEngine.Benchmarks;

/// <summary>
/// Benchmarks for plugin execution operations - critical for actual plugin functionality.
/// Measures throughput and memory allocations for plugin lifecycle operations and command execution.
/// </summary>
[MemoryDiagnoser]
[HideColumns("Job", "RatioSD", "Alloc Ratio")]
[GroupBenchmarksBy(BenchmarkDotNet.Configs.BenchmarkLogicalGroupRule.ByCategory)]
public class PluginExecutionBenchmarks
{
    private IPluginLoaderService _pluginLoaderService = null!;
    private IPluginManagerService _pluginManagerService = null!;
    private PluginEngine _pluginEngine = null!;
    private string _testPluginDirectory = string.Empty;
    private Plugin _loadedPlugin = null!;
    private const int PluginCount = 15;

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
        _pluginManagerService = serviceProvider.GetRequiredService<IPluginManagerService>();

        // Initialize and load plugins
        await _pluginEngine.InitializeAsync();
        await _pluginLoaderService.LoadPluginsFromDirectoryAsync(_testPluginDirectory);

        // Get a loaded plugin for execution tests
        var loadedPlugins = await _pluginLoaderService.GetAllLoadedPluginsAsync();
        _loadedPlugin = loadedPlugins.FirstOrDefault() ?? throw new InvalidOperationException("No plugins loaded");
    }

    [GlobalCleanup]
    public async Task GlobalCleanup()
    {
        // Clean up
        if (Directory.Exists(_testPluginDirectory))
        {
            Directory.Delete(_testPluginDirectory, true);
        }

        await _pluginEngine.ShutdownAsync();
    }

    private static async Task CreateTestPluginAssembly(string outputPath)
    {
        // Create a minimal plugin assembly with execution capabilities
        var assembly = new System.Reflection.AssemblyName("TestPlugin");
        var assemblyBuilder = System.Runtime.Loader.AssemblyLoadContext.Default.DefineDynamicAssembly(
            assembly,
            System.Reflection.Emit.AssemblyBuilderAccess.Save);

        var moduleBuilder = assemblyBuilder.DefineDynamicModule("TestModule");

        // Define a plugin lifecycle class
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

        // Add plugin commands interface
        var commandsInterface = moduleBuilder.DefineType(
            "IPluginCommands",
            System.Reflection.TypeAttributes.Interface | System.Reflection.TypeAttributes.Abstract);
        var commandsType = commandsInterface.CreateType();

        // Define a command method
        var executeMethod = typeBuilder.DefineMethod(
            "ExecuteCommandAsync",
            System.Reflection.MethodAttributes.Public | System.Reflection.MethodAttributes.Virtual,
            typeof(Task<string>),
            new[] { typeof(string), typeof(string[]) });
        ilGenerator = executeMethod.GetILGenerator();
        ilGenerator.Emit(System.Reflection.Emit.OpCodes.Ldstr, "Command executed");
        ilGenerator.Emit(System.Reflection.Emit.OpCodes.Ret);

        var pluginType = typeBuilder.CreateType()!;

        // Save the assembly
        assemblyBuilder.Save(Path.GetFileName(outputPath));
    }

    /// <summary>
    /// Benchmark: Plugin lifecycle initialization
    /// Measures the time to initialize plugin lifecycle (OnBeforeLoadAsync)
    /// </summary>
    [BenchmarkCategory("Plugin Lifecycle")]
    [Benchmark(Baseline = true)]
    public async Task PluginLifecycle_Initialize()
    {
        await _pluginLoaderService.LoadPluginAsync(Path.Combine(_testPluginDirectory, "TestPlugin0.dll"));
    }

    /// <summary>
    /// Benchmark: Plugin lifecycle cleanup
    /// Measures the time to cleanup plugin lifecycle (OnAfterUnloadAsync)
    /// </summary>
    [BenchmarkCategory("Plugin Lifecycle")]
    [Benchmark]
    public async Task PluginLifecycle_Cleanup()
    {
        var plugin = await _pluginLoaderService.LoadPluginAsync(Path.Combine(_testPluginDirectory, "TestPlugin1.dll"));
        await _pluginLoaderService.UnloadPluginAsync(plugin.Id);
    }

    /// <summary>
    /// Benchmark: Plugin command execution (single command)
    /// Measures the time to execute plugin operations
    /// </summary>
    [BenchmarkCategory("Plugin Operations")]
    [Benchmark]
    public async Task ExecutePluginOperation_Single()
    {
        // Use plugin manager to get plugin details as a representative operation
        await _pluginManagerService.GetPluginDetailsAsync(_loadedPlugin.Id);
    }

    /// <summary>
    /// Benchmark: Plugin operations with multiple plugins
    /// Measures the time to perform operations on multiple plugins
    /// </summary>
    [BenchmarkCategory("Plugin Operations - Bulk")]
    [Benchmark]
    public async Task ExecuteOperations_Batch()
    {
        var plugins = await _pluginLoaderService.GetAllLoadedPluginsAsync();
        foreach (var plugin in plugins.Take(5))
        {
            await _pluginManagerService.GetPluginDetailsAsync(plugin.Id);
        }
    }

    /// <summary>
    /// Benchmark: Get plugin details
    /// Measures the time to retrieve plugin details
    /// </summary>
    [BenchmarkCategory("Plugin Metadata")]
    [Benchmark]
    public async Task GetPluginDetails()
    {
        await _pluginManagerService.GetPluginDetailsAsync(_loadedPlugin.Id);
    }

    /// <summary>
    /// Benchmark: Plugin health check
    /// Measures the time to perform plugin health operations
    /// </summary>
    [BenchmarkCategory("Plugin Health")]
    [Benchmark]
    public async Task CheckPluginHealth()
    {
        await _pluginManagerService.GetStatisticsAsync();
    }

    /// <summary>
    /// Benchmark: Multiple plugin lifecycle operations
    /// Measures the time to perform multiple lifecycle operations sequentially
    /// </summary>
    [BenchmarkCategory("Plugin Lifecycle - Bulk")]
    [Benchmark]
    public async Task MultipleLifecycleOperations()
    {
        for (var i = 0; i < 10; i++)
        {
            var pluginPath = Path.Combine(_testPluginDirectory, $"TestPlugin{i}.dll");
            var plugin = await _pluginLoaderService.LoadPluginAsync(pluginPath);
            await _pluginLoaderService.UnloadPluginAsync(plugin.Id);
        }
    }
}