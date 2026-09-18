# CLAUDE.md

.NET 10 class library (`PluginEngine`, NuGet `Zaiets.dotnet.plugin.engine`) that loads plugin assemblies into isolated collectible `AssemblyLoadContext`s with dependency resolution, versioning, hot reload and hot swap.

## Build

```bash
dotnet restore
dotnet build -c Release --no-restore          # or: make build
dotnet build -c Release /p:TreatWarningsAsErrors=true   # make lint
dotnet pack -c Release -o ./nupkg             # make pack
```

- Solution: `dotnet-plugin-engine.sln` (src + tests). `DotnetPluginEngine.slnx` contains only the library.
- CI (`.github/workflows/ci.yml`): ubuntu/windows/macos, .NET 10.0.x, `restore -> build -> test -> dotnet format --verify-no-changes`.
- Benchmarks: `dotnet run -c Release --project benchmarks/dotnet-plugin-engine.Benchmarks` (`make bench`).
- `examples/` are standalone `.cs` samples with no csproj; they are not built.

## Test

```bash
dotnet test -c Release --no-build --verbosity normal   # make test
dotnet test --filter "FullyQualifiedName~PluginEngineOptionsTests"
```

- xUnit 2.9 + FluentAssertions 8 + Moq. Test project: `tests/dotnet-plugin-engine.Tests/` (namespace `PluginEngine.Tests` / `dotnet_plugin_engine.Tests`).
- Many test files are excluded via `<Compile Remove>` in the test csproj because they do not compile against the current API. Do not re-include them blindly; fix one file at a time. Same pattern in `src/PluginEngine/PluginEngine.csproj` (Cli, Controllers, WebhookHandler, some Middleware/BackgroundServices are excluded from the build).

## Format / Lint

```bash
dotnet format                                  # make format
dotnet format --verify-no-changes --verbosity diagnostic   # make format-check (CI gate)
```

Rules in `.editorconfig`: 4-space indent, LF, final newline, braces required (warning), `var` when type is apparent.

## Layout

```
src/PluginEngine/                the library (only buildable source)
  PluginEngine.cs                facade over the five core services - entry point
  GlobalUsings.cs                global usings for all sub-namespaces
  Configuration/                 PluginEngineOptions, DependencyInjectionSetup (AddPluginEngine)
  Services/Abstractions/         I*Service interfaces
  Services/Implementations/      PluginLoaderService (ALC isolation), PluginManagerService,
                                 DependencyResolutionService, VersioningService, HotReload/HotSwapService
  Domain/Entities/               Plugin, PluginMetadata, PluginDependency, VersionInfo, ...
  Data/Repositories/             IPluginRepository (in-memory, swappable)
  Middleware/                    ASP.NET-style pipeline: Logging, Caching, ErrorHandling, RateLimit
  Events/                        in-process pub/sub (IPluginEvent, PluginEventPublisher)
  Exceptions/                    PluginException hierarchy + *Extensions
  Execution/ Formatters/ Marketplace/ Integration/ Results/ Utils/
tests/dotnet-plugin-engine.Tests/   xUnit tests, one file per type
benchmarks/                      BenchmarkDotNet
docs/                            ARCHITECTURE.md plus one .md per public type
```

Stray `.cs` files and oddly named files in the repo root (`Plugin.cs`, `}`, `});`, `string fullMessage = ...`) and in `src/Events`, `src/Exceptions`, `src/tests` are leftovers outside any csproj; they are not compiled. Do not add code there.

## Conventions

- Every source file starts with `#nullable enable` and the author header block (`Author: Vladyslav Zaiets | https://sarmkadan.com`). Keep existing headers.
- File-scoped namespaces (`namespace PluginEngine.X;`), `sealed` classes, `ArgumentNullException.ThrowIfNull` guards in constructors, XML docs on all public members (`GenerateDocumentationFile` is on).
- Interfaces `I*`, services `*Service`, one interface per service registered as singleton via DI.
- Helpers live beside the type as `*Extensions.cs`, `*JsonExtensions.cs`, `*Validation.cs`; entities stay free of serialization.
- Async everywhere: `*Async` suffix, take `CancellationToken`.
- Tests: `MethodName_Scenario_ExpectedResult`, Arrange/Act/Assert comments, `[Fact]`/`[Theory]`.
- Commits: conventional prefixes (`feat(Scope):`, `docs:`, `style:`, `chore:`).
