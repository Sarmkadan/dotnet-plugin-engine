# `PluginEngineCliHost`

`PluginEngineCliHost` is a sealed command-line host defined in
`src/PluginEngine/Cli/PluginEngineCliHost.cs`. It parses an argument array, dispatches the
selected command to a service, writes results to `Console.Out` or `Console.Error`, and returns
an integer exit code.

## Build status

The class is currently **excluded from compilation**. `src/PluginEngine/PluginEngine.csproj`
contains a `Compile Remove` entry for `Cli\PluginEngineCliHost.cs`. The same project file also
excludes `Cli\CommandParser.cs`, which defines the parser and `CommandType` used by the host.
Consequently, `PluginEngineCliHost` is not part of the compiled `PluginEngine` assembly and
cannot currently be referenced by consumers. The API and example below describe the checked-in
source, not an available type in the current build output.

## Public API

| Member | Signature | Behavior |
| --- | --- | --- |
| Constructor | `public PluginEngineCliHost(IPluginManagerService pluginManager, IPluginLoaderService pluginLoader, IVersioningService versioningService, CommandParser commandParser, IPluginMarketplaceService marketplace, IHotSwapService hotSwap, IPluginDependencyResolver dependencyResolver)` | Stores all seven supplied dependencies. The source does not perform null checks. `_versioningService` is stored but is not subsequently used by this class. |
| Run | `public async Task<int> RunAsync(string[] args)` | Displays help for an empty argument array or an exact first argument of `help`, `--help`, or `-h`; otherwise parses and dispatches the command. Returns `0` on the host's successful paths and `1` on failures. Any exception escaping parsing or dispatch is written as `Fatal error: <message>` and produces exit code `1`. |

## Command flow

`RunAsync` checks its help forms before invoking `CommandParser.Parse`. The parser lowercases the
command name, maps it to a `CommandType`, and collects dash-prefixed options into a
case-insensitive dictionary. An option followed by another option or by the end of the array is
stored with the value `"true"`; non-option tokens that are not consumed as values are ignored.
Although `CommandParser` has a `ValidateArguments` method, `PluginEngineCliHost` does not call it.
Required arguments are checked by the individual handlers instead.

The parsed command then follows this dispatch table:

| Command input | Handler flow | Success output/result | Failure behavior |
| --- | --- | --- | --- |
| `load --path <path>` | Calls `IPluginLoaderService.LoadPluginAsync(path)`. | Prints the loaded plugin name and version; returns `0`. | A missing `path` or a caught exception is written to standard error; returns `1`. |
| `unload --id <guid>` | Parses the value with `Guid.Parse`, then calls `IPluginManagerService.UnloadPluginAsync`. | Prints the supplied ID; returns `0`. | A missing ID is rejected. Parsing and service exceptions are caught by the handler; returns `1`. |
| `list` | Calls `IPluginManagerService.GetAllPluginsAsync`. | Prints `No plugins loaded.` for an empty collection, or prints each plugin's name, version, and status; returns `0`. | A caught exception is written to standard error; returns `1`. Parsed options are unused. |
| `status` | Calls `IPluginManagerService.GetHealthAsync`. | Prints status, loaded-plugin count, and maximum plugin capacity; returns `0`. | A caught exception is written to standard error; returns `1`. Parsed options are unused. |
| `version`, `--version`, or `-v` | Does not call `IVersioningService`. | Prints the fixed text `Plugin Engine CLI v1.0.0`; returns `0`. | No handler-specific failure path exists. |
| `marketplace` | Selects an action, then uses `IPluginMarketplaceService`; see below. | Depends on the action. | Invalid actions, unsuccessful service results, validation failures, and caught exceptions return `1`. |
| `swap --id <guid> --path <assembly-path>` | Validates the ID with `Guid.TryParse`, then calls `IHotSwapService.SwapPluginAsync`. | Prints the result message when `Success` is true; returns `0`. | Missing/invalid arguments, an unsuccessful result, or a caught exception return `1`. |
| `resolve --id <guid>` | Validates the ID with `Guid.TryParse`, then calls `IPluginDependencyResolver.BuildResolutionPlanAsync`. | Prints whether the plan is executable, step and conflict counts, every ordered step, and any conflict descriptions; returns `0`. | Missing/invalid IDs, an unsuccessful result, or a caught exception return `1`. |
| Any other command | Routes `CommandType.Unknown` to `HandleUnknownCommand`. | None. | Prints the unknown command and a help hint; returns `1`. |

### Marketplace actions

The marketplace handler reads `--action`. If it is absent, the handler chooses `search` when a
`query` key exists and otherwise chooses `trending`. Action matching is case-insensitive.

| Action | Behavior |
| --- | --- |
| `search` | Reads optional `query` and `limit` values, calls `SearchAsync`, and prints returned entries. `limit` defaults to `20`; if parsing fails, `int.TryParse` leaves it as `0`, and `Math.Clamp` changes it to `1`. Parsed values are clamped to the range 1 through 100. |
| `info` | Requires a valid GUID in `id`, calls `GetEntryAsync`, and prints entry details, including available versions when present. |
| `install` | Requires a valid GUID in `id`, defaults `version` to `latest` and `target` to `./plugins`, then calls `InstallAsync`. |

Only these three cases are implemented. Despite the help text describing the default action as
`trending` and the source comment naming `browse`, `trending`, and `featured`, those actions have
no handler cases. Therefore `marketplace` without `--action` or `--query` selects `trending`,
prints it as an unknown marketplace action, and returns `1`.

## Exit codes and error handling

The host consistently uses `0` for its implemented successful paths and `1` for errors. Each
service-backed handler catches `Exception` and writes a command-specific message to standard
error. `RunAsync` adds an outer catch for exceptions not handled inside a command, including an
exception from the parser. Cancellation exceptions are not treated specially; where caught,
they follow the same error path as other exceptions.

`RunAsync` accesses `args.Length` without checking `args` for null. Passing null therefore causes
a `NullReferenceException` before execution enters the method's `try` block.

## Example

Because the host and parser are excluded from the current build, this example is illustrative and
only compiles in a build where those source files are included. Given implementations of the
seven constructor dependencies, it runs the status command and returns the host's exit code:

```csharp
using PluginEngine.Cli;

var host = new PluginEngineCliHost(
    pluginManager,
    pluginLoader,
    versioningService,
    new CommandParser(),
    marketplace,
    hotSwap,
    dependencyResolver);

int exitCode = await host.RunAsync(["status"]);
Console.WriteLine($"CLI exited with code {exitCode}");
```

All user interaction performed by the class goes through the process-wide `Console` streams;
the source does not provide injectable input or output abstractions.
