namespace tests.dotnet-plugin-engine.Tests;

public class RateLimitMiddlewareJsonExtensionsTests
{
    [Fact]
    public void ToJson_HappyPath_ReturnsJsonString()
    {
        // Arrange
        var rateLimitMiddleware = new RateLimitMiddleware(10, 100);
        var expectedJson = "{\"Limit\":10,\"Period\":100}";

        // Act
        var actualJson = RateLimitMiddlewareJsonExtensions.ToJson(rateLimitMiddleware);

        // Assert
        Assert.Equal(expectedJson, actualJson);
    }

    [Fact]
    public void ToJson_NullInput_ThrowsArgumentNullException()
    {
        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => RateLimitMiddlewareJsonExtensions.ToJson(null));
    }

    [Fact]
    public void FromJson_HappyPath_ReturnsRateLimitMiddleware()
    {
        // Arrange
        var json = "{\"Limit\":10,\"Period\":100}";
        var expectedRateLimitMiddleware = new RateLimitMiddleware(10, 100);

        // Act
        var actualRateLimitMiddleware = RateLimitMiddlewareJsonExtensions.FromJson(json);

        // Assert
        Assert.Equal(expectedRateLimitMiddleware, actualRateLimitMiddleware);
    }

    [Fact]
    public void FromJson_NullInput_ThrowsArgumentNullException()
    {
        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => RateLimitMiddlewareJsonExtensions.FromJson(null));
    }

    [Fact]
    public void FromJson_EmptyJson_ReturnsNull()
    {
        // Act
        var actualRateLimitMiddleware = RateLimitMiddlewareJsonExtensions.FromJson("");

        // Assert
        Assert.Null(actualRateLimitMiddleware);
    }

    [Fact]
    public void TryFromJson_HappyPath_ReturnsTrue()
    {
        // Arrange
        var json = "{\"Limit\":10,\"Period\":100}";

        // Act
        var result = RateLimitMiddlewareJsonExtensions.TryFromJson(json, out var rateLimitMiddleware);

        // Assert
        Assert.True(result);
        Assert.Equal(new RateLimitMiddleware(10, 100), rateLimitMiddleware);
    }

    [Fact]
    public void TryFromJson_NullInput_ReturnsFalse()
    {
        // Act
        var result = RateLimitMiddlewareJsonExtensions.TryFromJson(null, out var rateLimitMiddleware);

        // Assert
        Assert.False(result);
        Assert.Null(rateLimitMiddleware);
    }

    [Fact]
    public void TryFromJson_EmptyJson_ReturnsFalse()
    {
        // Act
        var result = RateLimitMiddlewareJsonExtensions.TryFromJson("", out var rateLimitMiddleware);

        // Assert
        Assert.False(result);
        Assert.Null(rateLimitMiddleware);
    }
}
