# Getting Started with dotnet-plugin-engine

This guide walks you through setting up and using the dotnet-plugin-engine in your project.

## Installation

### Via NuGet Package Manager

```bash
dotnet add package DotnetPluginEngine
```

### Via .csproj

```xml
<ItemGroup>
    <PackageReference Include="DotnetPluginEngine" Version="1.0.0" />
</ItemGroup>
```

### Build from Source

```bash
git clone https://github.com/Sarmkadan/dotnet-plugin-engine.git
cd dotnet-plugin-engine
dotnet build -c Release
dotnet pack -c Release --output ./nupkg
# Reference the local .nupkg file in your project
```

## Prerequisites

- **.NET 10** or later
- **C# 13** language features enabled in your project
- **Microsoft.Extensions.DependencyInjection** 10.0.0 or later

Verify your project file targets the correct framework:

```xml
<Project Sdk="Microsoft.NET.Sdk">
    <TargetFramework>net10.0</TargetFramework>
    <LangVersion>latest</LangVersion>
    <Nullable>enable</Nullable>
</Project>
```

## Basic Setup (5 Minutes)

### Step 1: Install the Package

```bash
dotnet add package DotnetPluginEngine
```

### Step 2: Configure Dependency Injection

In your `Program.cs` or startup configuration:

```csharp
using Microsoft.Extensions.DependencyInjection;
using PluginEngine.Configuration;

// Create service collection
var services = new ServiceCollection();

// Add plugin engine with default options
services.AddPluginEngine(options =>
{
    options.PluginDirectory = Path.Combine(AppContext.BaseDirectory, "plugins");
    options.EnableHotReload = true;
    options.EnableLogging = true;
});

// Build service provider
var serviceProvider = services.BuildServiceProvider();
```

### Step 3: Initialize and Load Plugins

```csharp
// Get the main engine instance
var engine = serviceProvider.GetRequiredService<PluginEngine.PluginEngine>();

// Initialize (one-time setup)
await engine.InitializeAsync();

// Load all plugins from the configured directory
var loadedCount = await engine.LoadAllPluginsAsync();
Console.WriteLine($"Successfully loaded {loadedCount} plugins");
```

### Step 4: Use Your Plugins

```csharp
// Get the manager service
var manager = serviceProvider.GetRequiredService<IPluginManagerService>();

// Retrieve all loaded plugins
var plugins = await manager.GetAllLoadedPluginsAsync();

foreach (var plugin in plugins)
{
    Console.WriteLine($"Name: {plugin.Name}");
    Console.WriteLine($"Version: {plugin.Version}");
    Console.WriteLine($"Description: {plugin.Metadata?.Description}");
    Console.WriteLine();
}
```

## Creating Your First Plugin

### Plugin Structure

A plugin is a .NET class library that targets the same framework as your host application:

```xml
<Project Sdk="Microsoft.NET.Sdk">
    <TargetFramework>net10.0</TargetFramework>
    <LangVersion>latest</LangVersion>
    <Nullable>enable</Nullable>
    
    <ItemGroup>
        <!-- Reference the plugin engine if you want to implement interfaces -->
        <ProjectReference Include="../PluginEngine/PluginEngine.csproj" />
    </ItemGroup>
</Project>
```

### Plugin Assembly Metadata

Add assembly metadata to your plugin's `.csproj`:

```xml
<PropertyGroup>
    <AssemblyName>MyAwesomePlugin</AssemblyName>
    <Authors>Your Name</Authors>
    <Description>A description of what your plugin does</Description>
    <Version>1.0.0</Version>
</PropertyGroup>
```

### Plugin Code

```csharp
using System;

public class MyAwesomePlugin
{
    public string Name => "My Awesome Plugin";
    public string Version => "1.0.0";
    public string? Description => "This plugin does something awesome";
    
    public async Task<string> ExecuteAsync(string input)
    {
        await Task.Delay(100); // Simulate async work
        return $"Processed: {input}";
    }
}
```

### Build and Deploy

```bash
cd src/MyAwesomePlugin
dotnet build -c Release
# Copy the output DLL to your host's plugins directory
cp bin/Release/net10.0/MyAwesomePlugin.dll ../HostApp/plugins/
```

## Configuration Options

The `PluginEngineOptions` class controls all engine behavior:

```csharp
services.AddPluginEngine(options =>
{
    // ===== Discovery & Loading =====
    // Directory where plugin DLLs are located
    options.PluginDirectory = "./plugins";
    
    // ===== Hot Reload =====
    // Enable automatic file monitoring and hot reload
    options.EnableHotReload = true;
    
    // How often to check for plugin file changes (milliseconds)
    options.HotReloadCheckIntervalMs = 5000;
    
    // ===== Caching =====
    // Cache dependency resolution results
    options.EnableDependencyCaching = true;
    
    // How long to keep cache entries (milliseconds)
    options.DependencyCacheTTLMs = 300000; // 5 minutes
    
    // ===== Performance =====
    // Maximum time for any plugin operation (milliseconds)
    options.OperationTimeoutMs = 30000;
    
    // Maximum plugins to load concurrently
    options.MaxConcurrentPluginLoads = Environment.ProcessorCount;
    
    // ===== Validation =====
    // Enforce strict semantic versioning
    options.StrictVersionChecking = true;
    
    // Detect and prevent circular dependencies
    options.EnableCircularDependencyDetection = true;
    
    // Maximum attempts to resolve dependencies
    options.MaxDependencyResolutionAttempts = 10;
    
    // ===== Diagnostics =====
    // Enable detailed logging
    options.EnableLogging = true;
    
    // Minimum log level
    options.MinimumLogLevel = LogLevel.Information;
});
```

## Advanced Configuration

### With Custom Logging

```csharp
services
    .AddLogging(builder =>
    {
        builder
            .AddConsole()
            .AddDebug()
            .SetMinimumLevel(LogLevel.Debug);
    })
    .AddPluginEngine(options =>
    {
        options.EnableLogging = true;
        options.MinimumLogLevel = LogLevel.Debug;
        options.PluginDirectory = "./plugins";
    });
```

### With Webhook Integration

```csharp
services.AddPluginEngine(options =>
{
    options.PluginDirectory = "./plugins";
    
    options.WebhookConfig = new WebhookConfiguration
    {
        Enabled = true,
        BaseUrl = "https://myapp.example.com",
        Events = new[]
        {
            "plugin.loaded",
            "plugin.failed",
            "plugin.reloaded"
        },
        RetryAttempts = 3,
        RetryDelayMs = 1000
    };
});
```

### With Custom Dependency Injection

```csharp
services
    .AddPluginEngine(options =>
    {
        options.PluginDirectory = "./plugins";
    })
    .AddScoped<IMyCustomService, MyCustomService>()
    .AddSingleton<IMyRepository, MyRepository>();
    
var serviceProvider = services.BuildServiceProvider();
```

## Common Tasks

### Load a Single Plugin

```csharp
var loader = serviceProvider.GetRequiredService<IPluginLoaderService>();

try
{
    var plugin = await loader.LoadPluginAsync("./plugins/MyPlugin.dll");
    Console.WriteLine($"Loaded: {plugin.Name} v{plugin.Version}");
}
catch (PluginLoadException ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}
```

### Check Plugin Dependencies

```csharp
var resolver = serviceProvider.GetRequiredService<IDependencyResolutionService>();
var manager = serviceProvider.GetRequiredService<IPluginManagerService>();

var plugin = await manager.GetPluginAsync("my-plugin");
var dependencies = await resolver.ResolveDependenciesAsync(plugin);

Console.WriteLine($"Total dependencies: {dependencies.Count}");
foreach (var dep in dependencies)
{
    Console.WriteLine($"  - {dep.Name}: {dep.VersionConstraint}");
}
```

### Monitor Plugin Changes

```csharp
var reloader = serviceProvider.GetRequiredService<IHotReloadService>();

await reloader.RegisterHotReloadCallback("my-plugin", async plugin =>
{
    Console.WriteLine($"Plugin reloaded: {plugin.Name}");
    // Re-initialize plugin state here if needed
});

await reloader.StartHotReloadMonitoringAsync();
```

### Get Engine Health Status

```csharp
var engine = serviceProvider.GetRequiredService<PluginEngine.PluginEngine>();

var health = await engine.GetHealthInfoAsync();
Console.WriteLine($"Total loaded: {health.LoadedPluginsCount}");
Console.WriteLine($"Failed: {health.FailedPluginsCount}");
Console.WriteLine($"Average load time: {health.AveragePluginLoadTimeMs}ms");

foreach (var status in health.PluginHealthStatus)
{
    Console.WriteLine($"{status.Name}: {status.Status}");
    if (!string.IsNullOrEmpty(status.ErrorMessage))
    {
        Console.WriteLine($"  Error: {status.ErrorMessage}");
    }
}
```

## Troubleshooting

### Plugins Not Found

Check that your plugin directory exists and contains `.dll` files:

```bash
# On Windows
dir plugins\

# On Linux/macOS
ls -la plugins/
```

Verify the path in your configuration is correct:

```csharp
var path = options.PluginDirectory;
Console.WriteLine($"Looking for plugins in: {Path.GetFullPath(path)}");
```

### Plugin Load Fails

Enable debug logging to see detailed error information:

```csharp
options.EnableLogging = true;
options.MinimumLogLevel = LogLevel.Debug;
```

Check that plugins target the correct framework:

```bash
# Check what framework a DLL targets
dotnet --info  # Shows your .NET version
```

### Hot Reload Not Working

Ensure hot reload is enabled:

```csharp
options.EnableHotReload = true;
```

Verify the plugin directory has read permissions and is being monitored:

```csharp
var reloader = serviceProvider.GetRequiredService<IHotReloadService>();
var stats = await reloader.GetStatisticsAsync();
Console.WriteLine($"Monitoring active: {stats.IsMonitoring}");
```

## Next Steps

- Review the [Architecture Guide](./architecture.md) for deep dives into core concepts
- Check [API Reference](./api-reference.md) for complete service documentation
- Explore the [examples/](../examples/) directory for complete working applications
- Read [Deployment Guide](./deployment.md) for production setup
- Check [FAQ](./faq.md) for common questions
