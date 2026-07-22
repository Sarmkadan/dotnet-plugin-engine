using Xunit;
using System;
using System.Collections.Generic;
using PluginEngine.Exceptions;

namespace PluginEngine.Tests;

public class DependencyResolutionExceptionValidationTests
{
    [Fact]
    public void Validate_ValidException_ReturnsEmptyList()
    {
        // Arrange
        var exception = new DependencyResolutionException
        {
            Reason = DependencyResolutionReason.DependencyNotFound
        };
        exception.AddUnresolvedDependency("PluginA");
        exception.AddUnresolvedDependency("PluginB");

        // Act
        var problems = exception.Validate();

        // Assert
        Assert.Empty(problems);
    }

    [Fact]
    public void Validate_NullException_ThrowsArgumentNullException()
    {
        // Arrange
        DependencyResolutionException? exception = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => exception!.Validate());
    }

    [Fact]
    public void Validate_InvalidVersionConstraint_ReturnsProblems()
    {
        // Arrange
        var exception = new DependencyResolutionException
        {
            VersionConstraint = new string('a', 257) // Exceeds max length
        };

        // Act
        var problems = exception.Validate();

        // Assert
        Assert.Single(problems);
        Assert.Contains("VersionConstraint exceeds maximum length of 256 characters", problems[0]);
    }

    [Fact]
    public void Validate_VersionConstraintWithControlChars_ReturnsProblems()
    {
        // Arrange
        var exception = new DependencyResolutionException
        {
            VersionConstraint = "1.0.0\r\n"
        };

        // Act
        var problems = exception.Validate();

        // Assert
        Assert.Single(problems);
        Assert.Contains("VersionConstraint contains control characters", problems[0]);
    }

    [Fact]
    public void Validate_InvalidReason_ReturnsProblem()
    {
        // Arrange
        var exception = new DependencyResolutionException
        {
            Reason = (DependencyResolutionReason)999 // Invalid enum value
        };

        // Act
        var problems = exception.Validate();

        // Assert
        Assert.Single(problems);
        Assert.Contains("Reason has an invalid enum value", problems[0]);
    }

    [Fact]
    public void Validate_NullUnresolvedDependencies_ReturnsProblem()
    {
        // Arrange
        var exception = new DependencyResolutionException
        {
            UnresolvedDependencies = null!
        };

        // Act
        var problems = exception.Validate();

        // Assert
        Assert.Single(problems);
        Assert.Contains("UnresolvedDependencies collection is null", problems[0]);
    }

    [Fact]
    public void Validate_UnresolvedDependenciesExceedsMaxCount_ReturnsProblem()
    {
        // Arrange
        var exception = new DependencyResolutionException
        {
            Reason = DependencyResolutionReason.DependencyNotFound
        };

        // Add 1001 dependencies (exceeds max of 1000)
        for (int i = 0; i < 1001; i++)
        {
            exception.AddUnresolvedDependency($"Plugin{i}");
        }

        // Act
        var problems = exception.Validate();

        // Assert
        Assert.Single(problems);
        Assert.Contains("UnresolvedDependencies collection exceeds maximum size of 1000 items", problems[0]);
    }

    [Fact]
    public void Validate_UnresolvedDependenciesWithDuplicates_ReturnsProblem()
    {
        // Arrange
        var exception = new DependencyResolutionException
        {
            Reason = DependencyResolutionReason.DependencyNotFound
        };
        exception.AddUnresolvedDependency("PluginA");
        exception.AddUnresolvedDependency("PluginB");
        exception.AddUnresolvedDependency("PluginA"); // Duplicate
        exception.AddUnresolvedDependency("PluginC");
        exception.AddUnresolvedDependency("PluginA"); // Another duplicate

        // Act
        var problems = exception.Validate();

        // Assert
        Assert.Single(problems);
        Assert.Contains("UnresolvedDependencies contains 3 duplicate entries", problems[0]);
        Assert.Contains("PluginA", problems[0]);
    }

    [Fact]
    public void Validate_UnresolvedDependenciesWithWhitespaceEntry_ReturnsProblem()
    {
        // Arrange
        var exception = new DependencyResolutionException
        {
            Reason = DependencyResolutionReason.DependencyNotFound
        };
        exception.AddUnresolvedDependency("PluginA");
        exception.AddUnresolvedDependency(" "); // Whitespace only
        exception.AddUnresolvedDependency("PluginB");

        // Act
        var problems = exception.Validate();

        // Assert
        Assert.Single(problems);
        Assert.Contains("UnresolvedDependencies contains null, empty, or whitespace-only entries", problems[0]);
    }

    [Fact]
    public void Validate_UnresolvedDependenciesWithLongEntry_ReturnsProblem()
    {
        // Arrange
        var exception = new DependencyResolutionException
        {
            Reason = DependencyResolutionReason.DependencyNotFound
        };
        exception.AddUnresolvedDependency("PluginA");
        exception.AddUnresolvedDependency(new string('b', 257)); // Exceeds max length
        exception.AddUnresolvedDependency("PluginB");

        // Act
        var problems = exception.Validate();

        // Assert
        Assert.Single(problems);
        Assert.Contains("UnresolvedDependencies contains entries exceeding maximum length of 256 characters", problems[0]);
    }

    [Fact]
    public void Validate_UnresolvedDependenciesWithControlChars_ReturnsProblem()
    {
        // Arrange
        var exception = new DependencyResolutionException
        {
            Reason = DependencyResolutionReason.DependencyNotFound
        };
        exception.AddUnresolvedDependency("PluginA");
        exception.AddUnresolvedDependency("Plugin\r\nB"); // Contains control chars
        exception.AddUnresolvedDependency("PluginC");

        // Act
        var problems = exception.Validate();

        // Assert
        Assert.Single(problems);
        Assert.Contains("UnresolvedDependencies contains entries with control characters", problems[0]);
    }

    [Fact]
    public void IsValid_ValidException_ReturnsTrue()
    {
        // Arrange
        var exception = new DependencyResolutionException
        {
            Reason = DependencyResolutionReason.DependencyNotFound
        };
        exception.AddUnresolvedDependency("PluginA");

        // Act
        var isValid = exception.IsValid();

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void IsValid_InvalidException_ReturnsFalse()
    {
        // Arrange
        var exception = new DependencyResolutionException
        {
            Reason = (DependencyResolutionReason)999 // Invalid enum value
        };

        // Act
        var isValid = exception.IsValid();

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void IsValid_NullException_ThrowsArgumentNullException()
    {
        // Arrange
        DependencyResolutionException? exception = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => exception!.IsValid());
    }

    [Fact]
    public void EnsureValid_ValidException_DoesNotThrow()
    {
        // Arrange
        var exception = new DependencyResolutionException
        {
            Reason = DependencyResolutionReason.DependencyNotFound
        };
        exception.AddUnresolvedDependency("PluginA");

        // Act
        var exceptionResult = Record.Exception(() => exception.EnsureValid());

        // Assert
        Assert.Null(exceptionResult);
    }

    [Fact]
    public void EnsureValid_InvalidException_ThrowsArgumentException()
    {
        // Arrange
        var exception = new DependencyResolutionException
        {
            Reason = (DependencyResolutionReason)999 // Invalid enum value
        };

        // Act
        var ex = Assert.Throws<ArgumentException>(() => exception.EnsureValid());

        // Assert
        Assert.Contains("DependencyResolutionException is invalid", ex.Message);
        Assert.Contains("Reason has an invalid enum value", ex.Message);
    }

    [Fact]
    public void EnsureValid_NullException_ThrowsArgumentNullException()
    {
        // Arrange
        DependencyResolutionException? exception = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => exception!.EnsureValid());
    }

    [Fact]
    public void Validate_MultipleProblems_ReturnsAllProblems()
    {
        // Arrange
        var exception = new DependencyResolutionException
        {
            VersionConstraint = new string('a', 257), // Too long
            Reason = (DependencyResolutionReason)999 // Invalid
        };
        exception.AddUnresolvedDependency("PluginA");
        exception.AddUnresolvedDependency(" "); // Whitespace

        // Act
        var problems = exception.Validate();

        // Assert
        Assert.Equal(3, problems.Count);
    }
}