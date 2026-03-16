# AssemblyLoadContext Isolation Guide

The dotnet-plugin-engine uses `AssemblyLoadContext` (ALC) to provide runtime isolation between plugins and the host application. This model enables features like hot-reloading and independent dependency resolution, but introduces specific challenges for plugin authors.

## The Isolation Boundary

When a plugin is loaded, it is placed in its own dedicated `AssemblyLoadContext`. This means:
- The plugin's assemblies and its bundled dependencies are isolated from the host and other plugins.
- Types loaded in one ALC are distinctly different from identically named types loaded in another ALC.

### Shared Abstractions

To communicate across the boundary, both the host and the plugin must reference the *same* abstraction assembly (e.g., interfaces or base classes) loaded in the *default* (host) ALC. The engine enforces this by configuring the plugin's ALC to delegate the loading of shared abstractions to the default ALC.

## Common Pitfalls and Troubleshooting

### 1. Cross-ALC Type Casting Failures (`InvalidCastException`)

**Symptom:** You receive an `InvalidCastException` when casting an object from a plugin to an interface, even though the types have the same name and namespace.

**Cause:** The interface assembly was loaded twice: once in the host's default ALC and once in the plugin's isolated ALC. The CLR considers these as completely different types.

**Solution:** 
- Ensure that the shared interface assemblies (abstractions) are **excluded** from the plugin's output directory.
- In your plugin's `.csproj`, reference the shared abstractions using `<Private>false</Private>` or `<ExcludeAssets>runtime</ExcludeAssets>`:
  ```xml
  <ProjectReference Include="..\SharedAbstractions\SharedAbstractions.csproj">
      <ExcludeAssets>runtime</ExcludeAssets>
  </ProjectReference>
  ```

### 2. Static Singletons

**Symptom:** Static state (e.g., caches, counters) is not shared between the host and the plugin, or memory leaks occur after hot-reloading.

**Cause:** Each ALC maintains its own separate copy of static variables. If a plugin sets a static variable, it only affects the plugin's isolated instance. Additionally, if the host holds a reference to a plugin's static variable, it will prevent the plugin's ALC from unloading.

**Solution:**
- Avoid using static singletons to share state across the plugin boundary.
- Instead, pass shared state via dependency injection or explicitly through shared interfaces.

### 3. Logger Factory Instances Not Propagating

**Symptom:** Plugins fail to log correctly, or use a different logging configuration than the host.

**Cause:** The `ILoggerFactory` or `ILogger` implementations are instantiated separately in the plugin, bypassing the host's configuration.

**Solution:**
- Provide the host's `ILoggerFactory` to the plugin during initialization.
- Ensure that the `Microsoft.Extensions.Logging.Abstractions` assembly is treated as a shared abstraction, so both the host and plugin use the exact same `ILogger` type.

### 4. Dependency Version Conflicts

**Symptom:** `FileNotFoundException` or `MissingMethodException` occurs when a plugin uses a transitive dependency that conflicts with the host or another plugin.

**Cause:** The dependency is being resolved from the wrong context, or the host is enforcing its own version.

**Solution:**
- Use the engine's built-in dependency resolution features.
- Ship necessary dependencies alongside your plugin.
- Avoid forcing the host to load a dependency version that conflicts with the plugin.

### 5. Memory Leaks During Hot-Reload

**Symptom:** The host application's memory usage grows each time a plugin is reloaded.

**Cause:** The old ALC cannot be garbage collected because the host application or a shared static event still holds a strong reference to an object from the unloaded plugin.

**Solution:**
- Ensure all event handlers attached to host events by the plugin are unregistered during the plugin's cleanup phase.
- Use the `IPluginLifecycle` hooks (if available) to clean up resources, dispose timers, and release references before the ALC unloads.
