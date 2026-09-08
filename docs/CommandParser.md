# CommandParser

`CommandParser` converts command-line tokens into a `CommandType` and a case-insensitive dictionary of option values. It can also verify that the parsed dictionary contains the keys required by a command.

## Public Signatures

| Member | Signature | Behavior |
|--------|-----------|----------|
| `Parse` | `(CommandType, Dictionary<string, string>) Parse(string[] args)` | Identifies the first token as a command and parses subsequent option tokens into a dictionary. |
| `ValidateArguments` | `void ValidateArguments(CommandType commandType, Dictionary<string, string> args)` | Throws an `ArgumentException` when a required option key is absent. |

## Supported Commands

| Accepted first token | `CommandType` | Required option keys |
|----------------------|---------------|----------------------|
| `load` | `Load` | `path` |
| `unload` | `Unload` | `id` |
| `list` | `List` | None |
| `status` | `Status` | None |
| `version`, `--version`, or `-v` | `Version` | None |
| `marketplace` | `Marketplace` | None |
| `swap` | `HotSwap` | `id`, `path` |
| `resolve` | `Resolve` | `id` |

`CommandType.Unknown` is returned when the argument array is empty or its first token is not in the command map. Command matching is case-insensitive because the first token is converted with `ToLowerInvariant()`.

## Argument Syntax

`Parse` examines tokens after the command token:

- An option starts with either `--` or `-`. The leading prefix is removed to form the dictionary key.
- If the following token exists and does not start with `-`, that token becomes the option value and is consumed.
- Otherwise, the option receives the string value `"true"`.
- Tokens that do not start with `-` and are not consumed as values are ignored.
- Option keys are case-insensitive. Repeating a key replaces its earlier value.

For example, `load --path ./plugins -verbose` produces `CommandType.Load` with `path` set to `./plugins` and `verbose` set to `"true"`.

Values beginning with `-` cannot be consumed as option values: they are treated as separate options on the next loop iteration. The parser does not reject empty option names, unknown options, or extra positional tokens.

## Validation Rules

`ValidateArguments` obtains the required keys shown in the supported commands table and checks each one with `Dictionary.ContainsKey`.

- A missing required key causes `ArgumentException` with the message `Missing required argument: --{key}`.
- Validation tests only whether a key exists. It does not inspect whether the associated value is empty, `"true"`, or otherwise suitable.
- `Unknown` and any command not explicitly assigned required keys have no requirements.
- Key comparison follows the comparer of the dictionary passed to `ValidateArguments`. Dictionaries returned by `Parse` use `StringComparer.OrdinalIgnoreCase`.

## Example

```csharp
using PluginEngine.Cli;

var parser = new CommandParser();
var (command, options) = parser.Parse(
    ["swap", "--id", "sample-plugin", "--path", "./plugins/sample"]);

parser.ValidateArguments(command, options);

Console.WriteLine(command);         // HotSwap
Console.WriteLine(options["id"]);   // sample-plugin
Console.WriteLine(options["path"]); // ./plugins/sample
```

## Build Status

`CommandParser.cs` is currently excluded from compilation by the `Compile Remove="Cli\CommandParser.cs"` entry in `src/PluginEngine/PluginEngine.csproj`. Consequently, the types documented here are not part of the built `PluginEngine` assembly while that exclusion remains in place.
