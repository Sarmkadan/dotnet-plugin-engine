using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using PluginEngine.Cli;

public class CommandParserTests
{
    [Fact]
    public void CommandWithFlagsAndValues()
    {
        // Arrange
        var parser = new CommandParser();
        var args = new[] { "load", "--path", "path/to/plugin", "--other-flag", "other-value" };

        // Act
        var (commandType, arguments) = parser.Parse(args);

        // Assert
        Assert.Equal(CommandType.Load, commandType);
        Assert.Equal(2, arguments.Count);
        Assert.Equal("path/to/plugin", arguments["path"]);
        Assert.Equal("other-value", arguments["other-flag"]);
    }

    [Fact]
    public void QuotedArgumentsContainingSpaces()
    {
        // Arrange
        var parser = new CommandParser();
        var args = new[] { "load", "--path", "\"path to plugin\"", "--other-flag", "other-value" };

        // Act
        var (commandType, arguments) = parser.Parse(args);

        // Assert
        Assert.Equal(CommandType.Load, commandType);
        Assert.Equal(2, arguments.Count);
        Assert.Equal("path to plugin", arguments["path"]);
        Assert.Equal("other-value", arguments["other-flag"]);
    }

    [Fact]
    public void UnknownCommandHandling()
    {
        // Arrange
        var parser = new CommandParser();
        var args = new[] { "unknown-command" };

        // Act
        var (commandType, arguments) = parser.Parse(args);

        // Assert
        Assert.Equal(CommandType.Unknown, commandType);
        Assert.Empty(arguments);
    }

    [Fact]
    public void EmptyInput()
    {
        // Arrange
        var parser = new CommandParser();
        var args = new string[0];

        // Act
        var (commandType, arguments) = parser.Parse(args);

        // Assert
        Assert.Equal(CommandType.Unknown, commandType);
        Assert.Empty(arguments);
    }

    [Fact]
    public void CaseSensitivityOfCommandNames()
    {
        // Arrange
        var parser = new CommandParser();
        var args = new[] { "LOAD" };

        // Act
        var (commandType, arguments) = parser.Parse(args);

        // Assert
        Assert.Equal(CommandType.Unknown, commandType);
        Assert.Empty(arguments);
    }
}
