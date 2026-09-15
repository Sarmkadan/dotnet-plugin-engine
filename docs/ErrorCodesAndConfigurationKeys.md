# ErrorCodes and ConfigurationKeys

Constant values used throughout the plugin engine.

## API

### ConfigurationKeys

### `public const string SectionName = ...;`

- **Value**: `PluginEngine`

- **Where used**:
  - None

---

### `public const string PluginDirectory = ...;`

- **Value**: `PluginDirectory`

- **Where used**:
  - src/PluginEngine/BackgroundServices/BackgroundPluginMonitor.cs:            if (!Directory.Exists(_options.PluginDirectory))
  - src/PluginEngine/BackgroundServices/BackgroundPluginMonitor.cs:                _logger.LogWarning("Plugin directory not found: {Path}", _options.PluginDirectory);
  - src/PluginEngine/BackgroundServices/BackgroundPluginMonitor.cs:            _watcher = new FileSystemWatcher(_options.PluginDirectory)
  - src/PluginEngine/BackgroundServices/BackgroundPluginMonitor.cs:            _logger.LogInformation("File system watcher initialized for: {Path}", _options.PluginDirectory);
  - src/PluginEngine/PluginEngine.cs:        if (!Directory.Exists(_options.PluginDirectory))
  - src/PluginEngine/PluginEngine.cs:            throw new DirectoryNotFoundException($"Plugin directory not found: {_options.PluginDirectory}");
  - src/PluginEngine/PluginEngine.cs:        var loadedPlugins = await _pluginLoaderService.LoadPluginsFromDirectoryAsync(_options.PluginDirectory, cancellationToken);
  - src/PluginEngine/Configuration/PluginEngineOptions.cs:    public string PluginDirectory { get; set; } = PluginEngineConstants.DefaultPluginDirectory;
  - src/PluginEngine/Configuration/PluginEngineOptions.cs:        if (string.IsNullOrWhiteSpace(PluginDirectory))
  - src/PluginEngine/Configuration/PluginEngineOptions.cs:        if (string.IsNullOrWhiteSpace(PluginDirectory))

---

### `public const string EnableHotReload = ...;`

- **Value**: `EnableHotReload`

- **Where used**:
  - src/PluginEngine/BackgroundServices/BackgroundPluginMonitor.cs:        if (!_options.EnableHotReload)
  - src/PluginEngine/Configuration/PluginEngineOptions.cs:    public bool EnableHotReload { get; set; } = true;

---

### `public const string HotReloadCheckInterval = ...;`

- **Value**: `HotReloadCheckInterval`

- **Where used**:
  - None

---

### `public const string EnableDependencyCaching = ...;`

- **Value**: `EnableDependencyCaching`

- **Where used**:
  - src/PluginEngine/Configuration/PluginEngineOptions.cs:    public bool EnableDependencyCaching { get; set; } = true;

---

### `public const string OperationTimeout = ...;`

- **Value**: `OperationTimeout`

- **Where used**:
  - None

---

### `public const string EnableLogging = ...;`

- **Value**: `EnableLogging`

- **Where used**:
  - src/PluginEngine/Configuration/PluginEngineOptions.cs:    public bool EnableLogging { get; set; } = true;

---

### `public const string LogLevel = ...;`

- **Value**: `LogLevel`

- **Where used**:
  - src/PluginEngine/Configuration/LoggingConfiguration.cs:    public LogLevel MinimumLogLevel { get; set; } = LogLevel.Information;
  - src/PluginEngine/Configuration/LoggingConfiguration.cs:        config.MinimumLogLevel = LogLevel.Debug;
  - src/PluginEngine/Configuration/LoggingConfiguration.cs:        config.MinimumLogLevel = LogLevel.Warning;
  - src/PluginEngine/Configuration/PluginEngineOptions.cs:    public LogLevel LogLevel { get; set; } = LogLevel.Information;
  - src/PluginEngine/Configuration/PluginEngineOptions.cs:public enum LogLevel
  - src/tests/dotnet-plugin-engine.Tests/VersionHelperTests.cs:                LogLevel.Warning,
  - src/tests/dotnet-plugin-engine.Tests/VersionHelperTests.cs:                LogLevel.Warning,

---

### `public const string MaxConcurrentPluginLoads = ...;`

- **Value**: `MaxConcurrentPluginLoads`

- **Where used**:
  - src/PluginEngine/Configuration/PluginEngineOptions.cs:    public int MaxConcurrentPluginLoads { get; set; } = DefaultMaxConcurrentPluginLoads;
  - src/PluginEngine/Configuration/PluginEngineOptions.cs:        if (MaxConcurrentPluginLoads <= 0)
  - src/PluginEngine/Configuration/PluginEngineOptions.cs:        if (MaxConcurrentPluginLoads <= 0)

---

### ErrorCodes

### `public const string GenericError = ...;`

- **Value**: `PLUGIN_ERROR`

- **Where used**:
  - None

---

### `public const string PluginNotFound = ...;`

- **Value**: `PLUGIN_NOT_FOUND`

- **Where used**:
  - None

---

### `public const string PluginAlreadyLoaded = ...;`

- **Value**: `PLUGIN_ALREADY_LOADED`

- **Where used**:
  - None

---

### `public const string PluginLoadFailed = ...;`

- **Value**: `PLUGIN_LOAD_FAILED`

- **Where used**:
  - None

---

### `public const string DependencyResolutionFailed = ...;`

- **Value**: `DEPENDENCY_RESOLUTION_FAILED`

- **Where used**:
  - src/Events/Events.cs:        public DependencyResolutionFailedEvent() : base(PluginEventType.DependencyResolutionFailed) { }

---

### `public const string VersionMismatch = ...;`

- **Value**: `VERSION_MISMATCH`

- **Where used**:
  - src/PluginEngine/Exceptions/DependencyResolutionException.cs:    VersionMismatch = 2,
  - src/PluginEngine/Exceptions/VersionMismatchExceptionExtensions.cs:        return $"VersionMismatch[{exception.ComponentType}:{exception.ComponentName}] Expected: {exception.ExpectedVersion}, Actual: {exception.ActualVersion}";

---

### `public const string InvalidConfiguration = ...;`

- **Value**: `INVALID_CONFIGURATION`

- **Where used**:
  - None

---

### `public const string HotReloadFailed = ...;`

- **Value**: `HOT_RELOAD_FAILED`

- **Where used**:
  - None

---

### `public const string CircularDependency = ...;`

- **Value**: `CIRCULAR_DEPENDENCY`

- **Where used**:
  - src/PluginEngine/Exceptions/DependencyResolutionException.cs:    CircularDependency = 3,
  - src/PluginEngine/Services/Implementations/PluginDependencyResolver.cs:                DependencyResolutionReason.CircularDependency);

---

### `public const string OperationTimeout = ...;`

- **Value**: `OperationTimeout`

- **Where used**:
  - None