using Xunit;

namespace PluginEngine.Tests.Middleware;

public class CachingMiddlewareValidationTests
{
    [Fact]
    public void Validate_EmptyMiddleware_ReturnsEmptyList()
    {
        // Arrange
        var middleware = new CachingMiddleware();

        // Act
        var result = CachingMiddlewareValidation.Validate(middleware);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void Validate_NullMiddleware_ThrowsArgumentNullException()
    {
        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => CachingMiddlewareValidation.Validate(null));
    }

    [Fact]
    public void Validate_MiddlewareWithInvalidCacheDuration_ReturnsError()
    {
        // Arrange
        var middleware = new CachingMiddleware { CacheDuration = TimeSpan.Zero };

        // Act
        var result = CachingMiddlewareValidation.Validate(middleware);

        // Assert
        Assert.Single(result);
        Assert.Equal("Cache duration must be a positive time span.", result[0]);
    }

    [Fact]
    public void Validate_MiddlewareWithEmptyCachableOperations_ReturnsError()
    {
        // Arrange
        var middleware = new CachingMiddleware { CachableOperations = new List<string>() };

        // Act
        var result = CachingMiddlewareValidation.Validate(middleware);

        // Assert
        Assert.Single(result);
        Assert.Equal("Cachable operations collection cannot be null or empty.", result[0]);
    }

    [Fact]
    public void IsValid_ValidMiddleware_ReturnsTrue()
    {
        // Arrange
        var middleware = new CachingMiddleware { CacheDuration = TimeSpan.FromDays(1), CachableOperations = new List<string>() };

        // Act
        var result = CachingMiddlewareValidation.IsValid(middleware);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsValid_InvalidMiddleware_ReturnsFalse()
    {
        // Arrange
        var middleware = new CachingMiddleware { CacheDuration = TimeSpan.Zero, CachableOperations = new List<string>() };

        // Act
        var result = CachingMiddlewareValidation.IsValid(middleware);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void EnsureValid_ValidMiddleware_DoesNotThrow()
    {
        // Arrange
        var middleware = new CachingMiddleware { CacheDuration = TimeSpan.FromDays(1), CachableOperations = new List<string>() };

        // Act and Assert
        CachingMiddlewareValidation.EnsureValid(middleware);
    }

    [Fact]
    public void EnsureValid_InvalidMiddleware_ThrowsArgumentException()
    {
        // Arrange
        var middleware = new CachingMiddleware { CacheDuration = TimeSpan.Zero, CachableOperations = new List<string>() };

        // Act and Assert
        Assert.Throws<ArgumentException>(() => CachingMiddlewareValidation.EnsureValid(middleware));
    }
}
