# Changelog

All notable changes to dotnet-plugin-engine are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [2.0.2] - 2026-05-20
### Fixed
- Fix assembly load context leak when plugin is unloaded and reloaded
- Added regression test for the fix

## [2.0.0] - 2026-05-18

### Breaking Changes
- Default port changed from 5000 to 8080
- Docker runtime base image switched from `dotnet/runtime` to `dotnet/aspnet`
- Removed deprecated `version` field from docker-compose.yml (Compose v2+ standard)
- `ASPNETCORE_URLS` now explicitly set to `http://+:8080`

### Added
- Multi-stage Dockerfile with .NET 10, build-time test execution, and non-root user
- HTTP-based HEALTHCHECK (`/health` endpoint) replacing `dotnet --version` probe
- docker-compose.yml with resource limits, structured logging, and plugin monitor service
- Migration guide: `docs/MIGRATION_v2.md`

### Changed
- Bumped package version to 2.0.0
- Docker labels updated to reflect v2.0.0
- Health check start period increased to 10s for more reliable container startup

### Fixed
- Health check now validates actual application state instead of runtime presence

---

## [1.0.0] - 2025-12-20

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

### Planned for 1.1.0
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

**Last Updated**: 2026-03-08
