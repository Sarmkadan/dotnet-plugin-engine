#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace PluginEngine.Cli;

/// <summary>
/// Parses command-line arguments and extracts command type and options.
/// Handles validation and error reporting for malformed commands.
/// </summary>
public sealed class CommandParser
{
    private static readonly Dictionary<string, CommandType> CommandMap = new()
    {
        ["load"] = CommandType.Load,
        ["unload"] = CommandType.Unload,
        ["list"] = CommandType.List,
        ["status"] = CommandType.Status,
        ["version"] = CommandType.Version,
        ["--version"] = CommandType.Version,
        ["-v"] = CommandType.Version,
    };

    /// <summary>
    /// Parses command-line arguments into command type and argument dictionary.
    /// Validates argument format and provides detailed error messages.
    /// </summary>
    public (CommandType, Dictionary<string, string>) Parse(string[] args)
    {
        if (args.Length == 0)
            return (CommandType.Unknown, []);

        var commandName = args[0].ToLowerInvariant();

        if (!CommandMap.TryGetValue(commandName, out var commandType))
            return (CommandType.Unknown, []);

        var arguments = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        // Parse key-value pairs starting from index 1
        for (int i = 1; i < args.Length; i++)
        {
            var arg = args[i];

            if (!arg.StartsWith("--") && !arg.StartsWith("-"))
                continue;

            var key = arg.StartsWith("--") ? arg[2..] : arg[1..];

            if (i + 1 < args.Length && !args[i + 1].StartsWith("-"))
            {
                arguments[key] = args[i + 1];
                i++;
            }
            else
            {
                arguments[key] = "true";
            }
        }

        return (commandType, arguments);
    }

    /// <summary>
    /// Validates that all required arguments for a command are present.
    /// Throws ArgumentException if validation fails.
    /// </summary>
    public void ValidateArguments(CommandType commandType, Dictionary<string, string> args)
    {
        var required = GetRequiredArguments(commandType);

        foreach (var requiredArg in required)
        {
            if (!args.ContainsKey(requiredArg))
                throw new ArgumentException($"Missing required argument: --{requiredArg}");
        }
    }

    private static string[] GetRequiredArguments(CommandType commandType) => commandType switch
    {
        CommandType.Load => ["path"],
        CommandType.Unload => ["id"],
        CommandType.List => [],
        CommandType.Status => [],
        CommandType.Version => [],
        _ => []
    };
}

/// <summary>
/// Enumeration of all supported CLI commands.
/// </summary>
public enum CommandType
{
    Unknown,
    Load,
    Unload,
    List,
    Status,
    Version,
}
