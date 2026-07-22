using System;
using System.Collections.Generic;
using PluginEngine.Exceptions;
using Xunit;

namespace PluginEngine.Tests
{
    public class PluginExceptionExtensionsTests
    {
        #region WithErrorCode

        [Fact]
        public void WithErrorCode_ReturnsNewException_WithUpdatedErrorCode_AndCopiesAllOtherProperties()
        {
            // Arrange
            var original = new PluginException(
                message: "Original message",
                errorCode: "E001",
                innerException: new InvalidOperationException("inner"));

            var guid = Guid.NewGuid();
            original.EntityId = guid;
            original.Context["key"] = "value";

            // Act
            var result = original.WithErrorCode("E002");

            // Assert
            Assert.NotSame(original, result);
            Assert.Equal(original.Message, result.Message);
            Assert.Equal("E002", result.ErrorCode);
            Assert.Equal(original.InnerException, result.InnerException);
            Assert.Equal(guid, result.EntityId);
            Assert.Equal(original.Context, result.Context);
        }

        [Fact]
        public void WithErrorCode_ThrowsArgumentNullException_WhenExceptionIsNull()
        {
            PluginException? ex = null;
            Assert.Throws<ArgumentNullException>(() => ex!.WithErrorCode("E123"));
        }

        [Fact]
        public void WithErrorCode_ThrowsArgumentNullException_WhenNewErrorCodeIsNull()
        {
            var ex = new PluginException("msg", "E001", null);
            Assert.Throws<ArgumentNullException>(() => ex.WithErrorCode(null!));
        }

        #endregion

        #region WithContext

        [Fact]
        public void WithContext_AddsMultipleEntries_ToExceptionContext()
        {
            // Arrange
            var ex = new PluginException("msg", "E001", null);
            ex.Context.Clear();

            // Act
            ex.WithContext(
                ("first", 1),
                ("second", "two"));

            // Assert
            Assert.Equal(2, ex.Context.Count);
            Assert.Equal(1, ex.Context["first"]);
            Assert.Equal("two", ex.Context["second"]);
        }

        [Fact]
        public void WithContext_ThrowsArgumentNullException_WhenExceptionIsNull()
        {
            PluginException? ex = null;
            Assert.Throws<ArgumentNullException>(() => ex!.WithContext(("k", "v")));
        }

        [Fact]
        public void WithContext_ThrowsArgumentNullException_WhenKeyValuePairsIsNull()
        {
            var ex = new PluginException("msg", "E001", null);
            Assert.Throws<ArgumentNullException>(() => ex.WithContext(null!));
        }

        #endregion

        #region ToDiagnosticReport

        [Fact]
        public void ToDiagnosticReport_IncludesAllRelevantSections()
        {
            // Arrange
            var guid = Guid.NewGuid();
            var inner = new ArgumentException("inner message");
            var ex = new PluginException("Top level error", "E999", inner)
            {
                EntityId = guid
            };
            ex.Context["alpha"] = 42;
            ex.Context["beta"] = "value";

            // Act
            var report = ex.ToDiagnosticReport(includeStackTrace: true);

            // Assert
            Assert.Contains("=== PLUGIN EXCEPTION DIAGNOSTIC REPORT ===", report);
            Assert.Contains($"Error Code: {ex.ErrorCode}", report);
            Assert.Contains($"Message: {ex.Message}", report);
            Assert.Contains($"Entity ID: {guid}", report);
            Assert.Contains("Context:", report);
            Assert.Contains("- alpha: 42", report);
            Assert.Contains("- beta: value", report);
            Assert.Contains($"Inner Exception: {inner.GetType().Name}", report);
            Assert.Contains($"Message: {inner.Message}", report);
            Assert.Contains("=== STACK TRACE ===", report);
            Assert.Contains("=== END REPORT ===", report);
        }

        [Fact]
        public void ToDiagnosticReport_ThrowsArgumentNullException_WhenExceptionIsNull()
        {
            PluginException? ex = null;
            Assert.Throws<ArgumentNullException>(() => ex!.ToDiagnosticReport());
        }

        #endregion

        #region ToUserFriendlyMessage

        [Fact]
        public void ToUserFriendlyMessage_RemovesBracketedPrefix()
        {
            var ex = new PluginException("[PLUGIN_ERROR] Something went wrong", "E001", null);
            var friendly = ex.ToUserFriendlyMessage();
            Assert.Equal("Something went wrong", friendly);
        }

        [Fact]
        public void ToUserFriendlyMessage_ReturnsOriginalMessage_WhenNoBracketPrefix()
        {
            var ex = new PluginException("Plain message", "E001", null);
            var friendly = ex.ToUserFriendlyMessage();
            Assert.Equal("Plain message", friendly);
        }

        [Fact]
        public void ToUserFriendlyMessage_ThrowsArgumentNullException_WhenExceptionIsNull()
        {
            PluginException? ex = null;
            Assert.Throws<ArgumentNullException>(() => ex!.ToUserFriendlyMessage());
        }

        #endregion

        #region IsErrorCode

        [Fact]
        public void IsErrorCode_ReturnsTrue_WhenCodesMatch()
        {
            var ex = new PluginException("msg", "E123", null);
            Assert.True(ex.IsErrorCode("E123"));
        }

        [Fact]
        public void IsErrorCode_ReturnsFalse_WhenCodesDoNotMatch()
        {
            var ex = new PluginException("msg", "E123", null);
            Assert.False(ex.IsErrorCode("E999"));
        }

        [Fact]
        public void IsErrorCode_ThrowsArgumentNullException_WhenExceptionIsNull()
        {
            PluginException? ex = null;
            Assert.Throws<ArgumentNullException>(() => ex!.IsErrorCode("E123"));
        }

        [Fact]
        public void IsErrorCode_ThrowsArgumentNullException_WhenErrorCodeIsNull()
        {
            var ex = new PluginException("msg", "E123", null);
            Assert.Throws<ArgumentNullException>(() => ex.IsErrorCode(null!));
        }

        #endregion
    }
}
