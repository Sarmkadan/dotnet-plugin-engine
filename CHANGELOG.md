# Changelog

All notable changes to dotnet-plugin-engine are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.2.0] - 2026-05-04

### Added
- Webhook support for plugin lifecycle events with retry logic
- Remote plugin registry integration for plugin discovery and download
- Plugin output formatters (JSON, XML, CSV)
- Rate limiting middleware for plugin execution
- Background health check service with configurable intervals
- Plugin statistics endpoint with performance metrics
- Extended plugin metadata support (tags, additional properties)
- Circuit breaker pattern for failed plugin protection
- Plugin execution context for parameter passing
- Support for plugin capability enumeration

### Changed
- Enhanced dependency resolution algorithm with better cycle detection
- Improved hot reload debouncing for high-frequency file changes
- Refactored middleware pipeline for better extensibility
- Updated logging to use structured logging patterns
- Improved error messages with actionable guidance
- Version constraint format extended to support more operators

### Fixed
- Fixed memory leak in hot reload file watchers
- Fixed race condition in concurrent plugin loading
- Fixed version comparison for pre-release versions
- Fixed dependency cache invalidation on plugin reload
- Fixed AssemblyLoadContext unloading for large plugins

### Security
- Added assembly signature verification support
- Enhanced input validation for plugin paths
- Added security headers for webhook requests
- Implemented API rate limiting

## [1.1.0] - 2026-03-15

### Added
- Event publishing/subscribing system for plugin lifecycle
- Caching layer for dependency resolution with configurable TTL
- CLI host for command-line plugin management
- Plugin discovery service with pattern matching
- PluginValidator utility for pre-load validation
- Support for pre-release versions (alpha, beta, rc)
- Build metadata in version info
- Extended plugin metadata (author, URL, license)
- Multiple output formatters for plugin information
- Plugin execution result with operation tracking

### Changed
- Refactored service layer for better separation of concerns
- Improved async/await usage throughout
- Enhanced test coverage to 85%
- Updated documentation with more examples
- Optimized reflection usage in plugin loading
- Improved logging with context information

### Fixed
- Fixed dependency ordering in resolution
- Fixed version constraint parsing edge cases
- Fixed hot reload callback memory leaks
- Fixed null reference exceptions in edge cases

### Deprecated
- Synchronous plugin loading methods (use async versions)

## [1.0.0] - 2026-01-20

### Added
- Core plugin engine with AssemblyLoadContext isolation
- Plugin loader service with automatic discovery
- Dependency resolution with version constraints
- Hot reload monitoring with FileSystemWatcher
- Semantic versioning support
- Version compatibility checking
- Circular dependency detection
- In-memory plugin repository
- Configurable operation timeouts
- Comprehensive exception hierarchy
- Dependency injection integration
- Full nullable reference types support
- Extensive API documentation
- Multiple usage examples

### Features
- 🔌 Plugin Isolation: AssemblyLoadContext per plugin
- 🔄 Hot Reload: Zero-downtime plugin updates
- 📦 Dependency Management: Transitive resolution and validation
- 🏗️ Clean Architecture: Domain-driven design
- 🔐 Type Safe: Full C# 13 support
- ⚡ Performance: Async throughout, intelligent caching
- 📊 Diagnostics: Health checks and statistics
- 🔧 Extensible: Interface-based design
- 🚀 Production Ready: Comprehensive error handling

### Documentation
- Complete README with architecture overview
- Getting Started guide
- API reference documentation
- Deployment guide for production environments
- FAQ with troubleshooting tips
- Multiple working examples
- Architecture deep dive

## [0.2.0] - 2025-12-10

### Added
- Beta version with core functionality
- Basic plugin loading
- Simple dependency tracking
- Manual plugin unloading
- Initial documentation

## [0.1.0] - 2025-11-15

### Added
- Initial project structure
- Core entity models
- Service interfaces
- Basic implementation skeleton
- License and documentation skeleton

---

## Upgrade Guides

### 1.1.0 → 1.2.0

The upgrade is backward compatible. New features are opt-in:

```csharp
// Enable webhook notifications (new in 1.2.0)
options.WebhookConfig = new WebhookConfiguration
{
    Enabled = true,
    BaseUrl = "https://myapp.example.com/webhooks",
    Events = new[] { "plugin.loaded", "plugin.failed" }
};

// Use new formatters
var formatter = serviceProvider.GetRequiredService<IPluginFormatter>();
var json = await formatter.FormatAsync(plugins);
```

### 1.0.0 → 1.1.0

Breaking changes are minimal:

```csharp
// Events are now available
subscriber.Subscribe<PluginLoadedEvent>(async @event =>
{
    Console.WriteLine($"Plugin loaded: {@event.Plugin.Name}");
});

// Repository is now in Data/Repositories folder
services.AddSingleton<IPluginRepository, MyCustomRepository>();
```

---

## Dependencies

### Runtime
- .NET Runtime 10.0.0 or later
- Microsoft.Extensions.DependencyInjection 10.0.0 or later
- Microsoft.Extensions.Logging 10.0.0 or later

### Build
- .NET SDK 10.0.0 or later
- C# 13 compiler or later

---

## Roadmap

### Planned for 1.3.0
- [ ] Plugin marketplace integration
- [ ] Distributed plugin cache
- [ ] Enhanced plugin analytics
- [ ] Plugin compatibility matrix
- [ ] Progressive rollout support

### Future Considerations
- [ ] AppDomain-based isolation (legacy .NET Framework)
- [ ] Plugin compilation/AOT support
- [ ] Cross-platform plugin signing
- [ ] Plugin update notifications
- [ ] Plugin telemetry dashboard

---

## Contributors

- **Vladyslav Zaiets** - Project Creator & Lead Architect
- Community contributors welcome! See [Contributing Guidelines](README.md#contributing)

---

## Support

For issues, questions, or suggestions:
- 🐛 [Report Issues](https://github.com/Sarmkadan/dotnet-plugin-engine/issues)
- 💬 [Discussions](https://github.com/Sarmkadan/dotnet-plugin-engine/discussions)
- 📧 Contact: vladyslav.zaiets@sarmkadan.com

---

**Last Updated**: 2026-05-04
