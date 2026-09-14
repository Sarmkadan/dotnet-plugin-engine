using Xunit;
using System;
using System.Collections.Generic;
using PluginEngine.Exceptions;

    /// <summary>
    /// Tests for the <see cref="PluginException"/> class.
    /// </summary>
public class PluginExceptionTests
{
    /// <summary>Tests the constructor with no message, no error code, no entity ID, and no context.</summary>
    [Fact]
    public void Constructor_NoMessage_NoErrorCode_NoEntityId_NoContext()
    {
        // Act
        var exception = new PluginException();

        // Assert
        Assert.Null(exception.Message);
        Assert.Null(exception.ErrorCode);
        Assert.Null(exception.EntityId);
        Assert.Empty(exception.Context);
    }

    /// <summary>Tests the constructor with a message but no error code, no entity ID, and no context.</summary>
    [Fact]
    public void Constructor_Message_NoErrorCode_NoEntityId_NoContext()
    {
        // Act
        var exception = new PluginException("Test message");

        // Assert
        Assert.Equal("Test message", exception.Message);
        Assert.Null(exception.ErrorCode);
        Assert.Null(exception.EntityId);
        Assert.Empty(exception.Context);
    }

    /// <summary>Tests the constructor with a message and error code but no entity ID and no context.</summary>
    [Fact]
    public void Constructor_Message_WithErrorCode_NoEntityId_NoContext()
    {
        // Act
        var exception = new PluginException("Test message", "TEST_ERROR_CODE");

        // Assert
        Assert.Equal("Test message", exception.Message);
        Assert.Equal("TEST_ERROR_CODE", exception.ErrorCode);
        Assert.Null(exception.EntityId);
        Assert.Empty(exception.Context);
    }

    /// <summary>Tests the constructor with a message and inner exception but no error code, no entity ID, and no context.</summary>
    [Fact]
    public void Constructor_Message_WithInnerException_NoErrorCode_NoEntityId_NoContext()
    {
        // Act
        var innerException = new Exception("Inner exception message");
        var exception = new PluginException("Test message", innerException);

        // Assert
        Assert.Equal("Test message", exception.Message);
        Assert.Null(exception.ErrorCode);
        Assert.Null(exception.EntityId);
        Assert.Empty(exception.Context);
        Assert.Same(innerException, exception.InnerException);
    }

    /// <summary>Tests the ErrorCode getter property.</summary>
    [Fact]
    public void ErrorCode_Getter()
    {
        // Arrange
        var exception = new PluginException("Test message", "TEST_ERROR_CODE");

        // Act
        var errorCode = exception.ErrorCode;

        // Assert
        Assert.Equal("TEST_ERROR_CODE", errorCode);
    }

    /// <summary>Tests the EntityId getter property.</summary>
    [Fact]
    public void EntityId_Getter()
    {
        // Arrange
        var exception = new PluginException("Test message", "TEST_ERROR_CODE", Guid.NewGuid());

        // Act
        var entityId = exception.EntityId;

        // Assert
        Assert.NotNull(entityId);
    }

    /// <summary>Tests the Context getter property.</summary>
    [Fact]
    public void Context_Getter()
    {
        // Arrange
        var exception = new PluginException("Test message", "TEST_ERROR_CODE");
        exception.WithContext("key", "value");

        // Act
        var context = exception.Context;

        // Assert
        Assert.NotNull(context);
        Assert.Equal(1, context.Count);
        Assert.Equal("value", context["key"]);
    }

    /// <summary>Tests the ToString method with no entity ID and no context.</summary>
    [Fact]
    public void ToString_NoEntityId_NoContext()
    {
        // Act
        var exception = new PluginException("Test message", "TEST_ERROR_CODE");

        // Assert
        var toString = exception.ToString();
        Assert.Contains("TEST_ERROR_CODE", toString);
        Assert.Contains("Test message", toString);
    }

    /// <summary>Tests the ToString method with an entity ID but no context.</summary>
    [Fact]
    public void ToString_WithEntityId_NoContext()
    {
        // Act
        var exception = new PluginException("Test message", "TEST_ERROR_CODE", Guid.NewGuid());

        // Assert
        var toString = exception.ToString();
        Assert.Contains("TEST_ERROR_CODE", toString);
        Assert.Contains("Test message", toString);
        Assert.Contains("Entity: ", toString);
    }

    /// <summary>Tests the ToString method with context but no entity ID.</summary>
    [Fact]
    public void ToString_WithContext_NoEntityId()
    {
        // Act
        var exception = new PluginException("Test message", "TEST_ERROR_CODE");
        exception.WithContext("key", "value");

        // Assert
        var toString = exception.ToString();
        Assert.Contains("TEST_ERROR_CODE", toString);
        Assert.Contains("Test message", toString);
        Assert.Contains("Context: key=value", toString);
    }

    /// <summary>Tests the WithContext method adds context to the exception.</summary>
    [Fact]
    public void WithContext_AddsContext()
    {
        // Arrange
        var exception = new PluginException("Test message", "TEST_ERROR_CODE");

        // Act
        exception.WithContext("key", "value");

        // Assert
        Assert.NotNull(exception.Context);
        Assert.Equal(1, exception.Context.Count);
        Assert.Equal("value", exception.Context["key"]);
    }

    /// <summary>Tests the WithEntityId method sets the entity ID.</summary>
    [Fact]
    public void WithEntityId_SetsEntityId()
    {
        // Arrange
        var exception = new PluginException("Test message", "TEST_ERROR_CODE");

        // Act
        exception.WithEntityId(Guid.NewGuid());

        // Assert
        Assert.NotNull(exception.EntityId);
    }

    /// <summary>Tests that the constructor throws ArgumentNullException when message is null.</summary>
    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenMessageIsNull()
    {
        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => new PluginException(null));
    }

    /// <summary>Tests that the constructor throws ArgumentNullException when error code is null.</summary>
    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenErrorCodeIsNull()
    {
        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => new PluginException("Test message", null));
    }

    /// <summary>Tests that WithContext throws ArgumentNullException when key is null.</summary>
    [Fact]
    public void WithContext_ThrowsArgumentNullException_WhenKeyIsNull()
    {
        // Act and Assert
        var exception = new PluginException("Test message", "TEST_ERROR_CODE");
        Assert.Throws<ArgumentNullException>(() => exception.WithContext(null, "value"));
    }

    /// <summary>Tests that WithContext throws ArgumentNullException when value is null.</summary>
    [Fact]
    public void WithContext_ThrowsArgumentNullException_WhenValueIsNull()
    {
        // Act and Assert
        var exception = new PluginException("Test message", "TEST_ERROR_CODE");
        Assert.Throws<ArgumentNullException>(() => exception.WithContext("key", null));
    }
}
