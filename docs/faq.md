# Frequently Asked Questions

## Installation & Setup

### Q: What .NET versions are supported?
**A:** dotnet-plugin-engine requires .NET 10 or later. It uses C# 13 language features extensively. We recommend staying on the latest .NET 10 patch version for security updates.

### Q: How do I add the package to my project?
**A:** Via NuGet: `dotnet add package DotnetPluginEngine`, or edit your .csproj:
```xml
<ItemGroup>
    <PackageReference Include="DotnetPluginEngine" Version="1.0.0" />
</ItemGroup>
```

### Q: Can I build from source instead of using NuGet?
**A:** Yes. Clone the repository, build with `dotnet build -c Release`, then reference the project directly in your .csproj or pack with `dotnet pack`.

### Q: Do I need any additional dependencies?
**A:** Only `Microsoft.Extensions.DependencyInjection` 10.0.0+. All other dependencies are included in the package.

## Plugin Development

### Q: How do I create a plugin?
**A:** Plugins are standard .NET class libraries targeting net10.0. Add assembly metadata via .csproj properties:
```xml
<Authors>Your Name</Authors>
<Description>Plugin description</Description>
<Version>1.0.0</Version>
```

### Q: What interfaces must my plugin implement?
**A:** None required. The engine uses assembly metadata and naming conventions. You can optionally implement interfaces if your host application defines plugin contracts.

### Q: Can plugins have dependencies on other plugins?
**A:** Yes. Declare dependencies in your plugin class or assembly metadata. The engine's dependency resolver will validate and resolve them automatically.

### Q: How do I version my plugins correctly?
**A:** Use semantic versioning (major.minor.patch). Set it in your plugin's .csproj:
```xml
<Version>1.2.3</Version>
```

### Q: Can a plugin depend on multiple versions of the same dependency?
**A:** No. The engine ensures only one version of each dependency is loaded. You must declare compatible version constraints.

## Plugin Loading & Execution

### Q: How are plugins isolated from each other?
**A:** Each plugin loads into its own `AssemblyLoadContext`. This provides namespace isolation, version isolation, and independent unloading.

### Q: Can I unload a plugin without stopping my application?
**A:** Yes. Call `unloadPluginAsync()` on the loader service. The plugin's AssemblyLoadContext is unloaded and can be garbage collected.

### Q: What happens if a plugin throws an exception?
**A:** The exception bubbles up to your handler. Use try/catch around plugin execution. Enable detailed logging to diagnose plugin errors.

### Q: How do I call methods on plugins?
**A:** Plugins are standard .NET assemblies. Use reflection or interfaces to invoke plugin types:
```csharp
var assembly = Assembly.LoadFrom(pluginPath);
var type = assembly.GetType("MyPlugin.PluginClass");
var instance = Activator.CreateInstance(type);
```

### Q: Can plugins access resources from the host application?
**A:** Yes, but carefully. Avoid sharing mutable objects. Use dependency injection to provide stable contracts.

## Hot Reload

### Q: Does hot reload require application restart?
**A:** No. Hot reload monitors plugin files and reloads them on change without stopping your application.

### Q: What happens during hot reload?
**A:** The old plugin version is unloaded (ALC disposed), the new version is loaded, callbacks are invoked, and the update is broadcast as an event.

### Q: Can I disable hot reload?
**A:** Yes. Set `EnableHotReload: false` in options. Useful for read-only production environments.

### Q: How often does hot reload check for changes?
**A:** By default every 5 seconds. Adjust with `HotReloadCheckIntervalMs` option. Longer intervals reduce I/O and CPU usage.

### Q: What if a plugin file is being modified when the check happens?
**A:** The engine uses debouncing to wait for writes to complete. File locks are handled gracefully.

## Dependency Resolution

### Q: How does dependency resolution work?
**A:** Uses breadth-first search to resolve transitive dependencies. Validates version constraints and detects circular dependencies.

### Q: What are version constraint formats?
**A:** 
- Exact: `1.2.3`
- Range: `>=1.0.0,<2.0.0`
- Loose: `~1.2.0` (>= 1.2.0, < 1.3.0)
- Caret: `^1.2.0` (>= 1.2.0, < 2.0.0)
- Wildcard: `*` (any version)

### Q: How can I detect circular dependencies?
**A:** Call `resolver.HasCircularDependenciesAsync(plugin)`. Or enable `EnableCircularDependencyDetection: true` to auto-detect and report during loading.

### Q: Can I cache dependency resolution results?
**A:** Yes. Enable `EnableDependencyCaching: true` and set TTL with `DependencyCacheTTLMs`. Default is 5 minutes.

### Q: What's the maximum dependency depth supported?
**A:** Theoretically unlimited, but very deep graphs (>20 levels) may impact performance. Set `MaxDependencyResolutionAttempts` to limit resolution attempts.

## Configuration & Optimization

### Q: How do I configure the plugin directory?
**A:** Set `PluginDirectory` in options:
```csharp
options.PluginDirectory = "/app/plugins";
```

### Q: How many plugins can I load?
**A:** Limited only by available memory. Typical: 50-100 plugins per GB RAM. Memory usage varies by plugin complexity.

### Q: How long do plugin operations timeout?
**A:** Default 30 seconds. Change with `OperationTimeoutMs`. Increase for large plugins or slow systems.

### Q: How many plugins can load concurrently?
**A:** Default is CPU core count. Adjust with `MaxConcurrentPluginLoads`. Higher values = more parallelism but higher memory/CPU.

### Q: Can I disable logging?
**A:** Yes, set `EnableLogging: false`. But it's recommended to keep logging enabled for diagnostics.

## Error Handling

### Q: What exceptions can be thrown?
**A:** 
- `PluginLoadException`: Plugin load failed
- `DependencyResolutionException`: Dependencies can't be resolved
- `VersionMismatchException`: Version constraint violated
- `PluginException`: Generic plugin engine error

### Q: How do I get detailed error information?
**A:** Enable debug logging and check exception properties:
```csharp
try { await loader.LoadPluginAsync(path); }
catch (PluginLoadException ex)
{
    Console.WriteLine($"Stage: {ex.LoadStage}");
    Console.WriteLine($"Message: {ex.Message}");
    Console.WriteLine($"Inner: {ex.InnerException}");
}
```

### Q: What does "unsatisfied dependencies" error mean?
**A:** A required plugin is not loaded or its version doesn't match constraints. Load the dependency first or adjust version constraints.

### Q: How do I recover from plugin load failures?
**A:** Check the error details, fix the root cause (missing dependency, version conflict, etc.), and retry loading.

## Performance & Scaling

### Q: What's the plugin load performance?
**A:** Typical: 50-200ms per plugin depending on size and dependencies. Enable caching to improve repeated operations.

### Q: How much memory do plugins use?
**A:** Varies greatly (5-100+ MB). Each plugin in its own ALC shares framework assemblies but has isolated metadata.

### Q: Can I profile plugin performance?
**A:** Yes. Use dotnet tools like dotnet-dump, dotnet-trace, or PerfView. Monitor your logging output for performance metrics.

### Q: How do I optimize for high-concurrency?
**A:** 
- Increase `MaxConcurrentPluginLoads`
- Enable dependency caching
- Use SSD for plugin directory
- Allocate sufficient RAM
- Consider multiple instances with load balancing

## Integration & Extension

### Q: Can I use the engine in ASP.NET Core?
**A:** Yes. Add to `Program.cs` during service registration. Use plugins in controllers via DI.

### Q: Can I implement custom plugin repositories?
**A:** Yes. Implement `IPluginRepository` and register in DI:
```csharp
services.AddSingleton<IPluginRepository, MyDatabaseRepository>();
```

### Q: How do I add custom middleware?
**A:** Implement `IPluginMiddleware` and register it:
```csharp
services.AddSingleton<IPluginMiddleware, MyCustomMiddleware>();
```

### Q: Can I format plugin info in custom formats?
**A:** Yes. Implement `IPluginFormatter`:
```csharp
services.AddSingleton<IPluginFormatter, MyCustomFormatter>();
```

## Webhooks & Events

### Q: What events are available?
**A:** 
- `PluginLoadedEvent`
- `PluginUnloadedEvent`
- `PluginFailedEvent`
- `PluginReloadedEvent`

Subscribe using `PluginEventSubscriber`.

### Q: How do I enable webhook notifications?
**A:** Configure `WebhookConfiguration`:
```csharp
options.WebhookConfig = new WebhookConfiguration
{
    Enabled = true,
    BaseUrl = "https://myapp.example.com/webhooks",
    Events = new[] { "plugin.loaded", "plugin.failed" }
};
```

### Q: Can I filter which events trigger webhooks?
**A:** Yes. Specify events in the `Events` array in `WebhookConfiguration`.

## Production Deployment

### Q: How do I deploy to production?
**A:** See [Deployment Guide](./deployment.md) for comprehensive instructions including Docker, configuration, monitoring, and troubleshooting.

### Q: What are the minimum server requirements?
**A:** .NET 10 runtime, 256 MB RAM (base) + 50 MB per concurrent load, 100 MB disk space.

### Q: How do I implement health checks?
**A:** Call `engine.GetHealthInfoAsync()` periodically:
```csharp
app.MapGet("/health", async (PluginEngine engine) =>
{
    var health = await engine.GetHealthInfoAsync();
    return health.IsHealthy ? Results.Ok() : Results.StatusCode(503);
});
```

### Q: What's the recommended plugin directory size?
**A:** 100 plugins @ average 2 MB = 200 MB. Account for growth and temporary files.

### Q: How do I backup plugins?
**A:** Tar/zip the plugin directory regularly. Store securely. Test restore procedures.

## Troubleshooting

### Q: Plugin loads but I can't find types in it?
**A:** Load the assembly from the plugin's AssemblyLoadContext:
```csharp
var assembly = alc.Assemblies.FirstOrDefault(a => a.GetName().Name == "MyPlugin");
var type = assembly.GetType("MyPlugin.MyType");
```

### Q: "Plugin not found" error when loading?
**A:** Check plugin directory path, verify file exists, ensure .NET framework version matches.

### Q: Memory usage keeps increasing after hot reloads?
**A:** Ensure old ALC is being garbage collected. Check for managed references to unloaded plugins. Enable verbose GC logging.

### Q: Dependencies not resolving even though plugins exist?
**A:** Check exact ID/name matching (case-sensitive), verify version constraints, enable debug logging.

### Q: Hot reload isn't working?
**A:** Verify `EnableHotReload: true`, check file permissions, ensure plugin directory is monitored, review logs for errors.

## Advanced Topics

### Q: How do I implement plugin signing and verification?
**A:** Use `X509Certificate` to verify plugin signatures before loading. Implement in custom loader service.

### Q: Can I run plugins in a sandbox?
**A:** AssemblyLoadContext provides some isolation. For stronger sandbox (permissions, code access), consider AppDomain alternatives or separate processes.

### Q: How do I handle plugin breaking changes?
**A:** Use semantic versioning strictly. Require major version increment for breaking changes. Update version constraints in dependencies.

### Q: Can plugins be compiled at runtime?
**A:** Yes, using Roslyn. Generate code, compile to assembly, load with engine. See examples for implementation.

### Q: How do I implement plugin licensing?
**A:** Verify license files during load, check license validity against clock/token, implement in custom loader.

## Support & Contributing

### Q: Where do I report bugs?
**A:** [GitHub Issues](https://github.com/Sarmkadan/dotnet-plugin-engine/issues)

### Q: How do I contribute?
**A:** Fork, create feature branch, make changes, add tests, submit PR. See [Contributing Guide](../README.md#contributing).

### Q: Is there a Discord/Slack community?
**A:** Not currently. Use GitHub Issues for discussions and questions.

### Q: What's the roadmap?
**A:** See GitHub Projects for planned features and improvements.
