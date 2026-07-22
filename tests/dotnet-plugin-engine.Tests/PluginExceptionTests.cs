using Xunit;
using System;
using System.Collections.Generic;
using PluginEngine.Exceptions;

public class PluginExceptionTests
{
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

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenMessageIsNull()
    {
        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => new PluginException(null));
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenErrorCodeIsNull()
    {
        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => new PluginException("Test message", null));
    }

    [Fact]
    public void WithContext_ThrowsArgumentNullException_WhenKeyIsNull()
    {
        // Act and Assert
        var exception = new PluginException("Test message", "TEST_ERROR_CODE");
        Assert.Throws<ArgumentNullException>(() => exception.WithContext(null, "value"));
    }

    [Fact]
    public void WithContext_ThrowsArgumentNullException_WhenValueIsNull()
    {
        // Act and Assert
        var exception = new PluginException("Test message", "TEST_ERROR_CODE");
        Assert.Throws<ArgumentNullException>(() => exception.WithContext("key", null));
    }
}
