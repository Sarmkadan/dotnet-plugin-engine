#nullable enable
using FluentAssertions;
using PluginEngine.Utils.Extensions;
using Xunit;

namespace PluginEngine.Tests;

/// <summary>
/// Provides unit tests for string extension methods in the PluginEngine.Utils.Extensions namespace.
/// Tests various string manipulation and validation utilities used for plugin path handling,
/// version validation, filename sanitization, and time/byte formatting.
/// </summary>
public sealed class StringExtensionsTests
{
    // ── NormalizePluginPath ─────────────────────────────────────────────────

    /// <summary>
    /// Tests that <see cref="StringExtensions.NormalizePluginPath"/> correctly handles null or whitespace input.
    /// </summary>
    [Fact]
    public void NormalizePluginPath_WithNullOrWhitespace_ReturnsEmpty()
    {
        "".NormalizePluginPath().Should().BeEmpty();
        " ".NormalizePluginPath().Should().BeEmpty();
    }

    /// <summary>
    /// Tests that <see cref="StringExtensions.NormalizePluginPath"/> converts forward slashes to OS-specific directory separators.
    /// </summary>
    [Fact]
    public void NormalizePluginPath_WithForwardSlashes_ConvertsToOsSeparator()
    {
        var path = "plugins/auth/MyPlugin.dll";

        var result = path.NormalizePluginPath();

        result.Should().Contain(Path.DirectorySeparatorChar.ToString());
    }

    /// <summary>
    /// Tests that <see cref="StringExtensions.NormalizePluginPath"/> removes trailing directory separators.
    /// </summary>
    [Fact]
    public void NormalizePluginPath_WithTrailingSeparator_RemovesTrailingSeparator()
    {
        var path = $"plugins{Path.DirectorySeparatorChar}auth{Path.DirectorySeparatorChar}";

        var result = path.NormalizePluginPath();

        result.Should().NotEndWith(Path.DirectorySeparatorChar.ToString());
    }

    // ── IsValidPluginId ─────────────────────────────────────────────────────

    /// <summary>
    /// Tests that <see cref="StringExtensions.IsValidPluginId"/> returns true for valid GUID strings.
    /// </summary>
    [Fact]
    public void IsValidPluginId_WithValidGuid_ReturnsTrue()
    {
        Guid.NewGuid().ToString().IsValidPluginId().Should().BeTrue();
    }

    /// <summary>
    /// Tests that <see cref="StringExtensions.IsValidPluginId"/> returns false for invalid strings that are not GUIDs.
    /// </summary>
    [Fact]
    public void IsValidPluginId_WithInvalidString_ReturnsFalse()
    {
        "not-a-guid".IsValidPluginId().Should().BeFalse();
    }

    /// <summary>
    /// Tests that <see cref="StringExtensions.IsValidPluginId"/> returns false for empty strings.
    /// </summary>
    [Fact]
    public void IsValidPluginId_WithEmptyString_ReturnsFalse()
    {
        "".IsValidPluginId().Should().BeFalse();
    }

    // ── IsValidVersion ──────────────────────────────────────────────────────

    /// <summary>
    /// Tests that <see cref="StringExtensions.IsValidVersion"/> returns true for valid semantic version strings.
    /// </summary>
    [Fact]
    public void IsValidVersion_WithSemanticVersion_ReturnsTrue()
    {
        "1.2.3".IsValidVersion().Should().BeTrue();
    }

    /// <summary>
    /// Tests that <see cref="StringExtensions.IsValidVersion"/> returns false for strings that are not valid version strings.
    /// </summary>
    [Fact]
    public void IsValidVersion_WithNonVersionString_ReturnsFalse()
    {
        "not-a-version".IsValidVersion().Should().BeFalse();
    }

    // ── SanitizeForFilename ─────────────────────────────────────────────────

    /// <summary>
    /// Tests that <see cref="StringExtensions.SanitizeForFilename"/> returns empty string when input is empty.
    /// </summary>
    [Fact]
    public void SanitizeForFilename_WithEmptyString_ReturnsEmpty()
    {
        "".SanitizeForFilename().Should().BeEmpty();
    }

    /// <summary>
    /// Tests that <see cref="StringExtensions.SanitizeForFilename"/> returns unchanged string when input contains only valid filename characters.
    /// </summary>
    [Fact]
    public void SanitizeForFilename_WithValidName_ReturnsUnchanged()
    {
        "MyPlugin".SanitizeForFilename().Should().Be("MyPlugin");
    }

    /// <summary>
    /// Tests that <see cref="StringExtensions.SanitizeForFilename"/> removes invalid filename characters from the input string.
    /// </summary>
    [Fact]
    public void SanitizeForFilename_WithInvalidFileChars_RemovesInvalidChars()
    {
        // Use chars that are definitely invalid on the current OS
        var invalidChar = Path.GetInvalidFileNameChars()[0];
        var input = $"Plugin{invalidChar}Name";

        var result = input.SanitizeForFilename();

        result.Should().Be("PluginName");
    }

    // ── GetAssemblyName ─────────────────────────────────────────────────────

    /// <summary>
    /// Tests that <see cref="StringExtensions.GetAssemblyName"/> extracts the assembly name without extension from a DLL path.
    /// </summary>
    [Fact]
    public void GetAssemblyName_WithDllPath_ReturnsNameWithoutExtension()
    {
        "/plugins/MyPlugin.dll".GetAssemblyName().Should().Be("MyPlugin");
    }

    /// <summary>
    /// Tests that <see cref="StringExtensions.GetAssemblyName"/> extracts only the filename without extension from nested paths.
    /// </summary>
    [Fact]
    public void GetAssemblyName_WithNestedPath_ReturnsOnlyFilenameWithoutExtension()
    {
        "/opt/plugins/subdir/Auth.Plugin.dll".GetAssemblyName().Should().Be("Auth.Plugin");
    }

    // ── IsAssemblyPath ──────────────────────────────────────────────────────

    /// <summary>
    /// Tests that <see cref="StringExtensions.IsAssemblyPath"/> returns true for paths with .dll extension.
    /// </summary>
    [Fact]
    public void IsAssemblyPath_WithDllExtension_ReturnsTrue()
    {
        "/plugins/MyPlugin.dll".IsAssemblyPath().Should().BeTrue();
    }

    /// <summary>
    /// Tests that <see cref="StringExtensions.IsAssemblyPath"/> returns true for paths with .exe extension.
    /// </summary>
    [Fact]
    public void IsAssemblyPath_WithExeExtension_ReturnsTrue()
    {
        "/plugins/MyPlugin.exe".IsAssemblyPath().Should().BeTrue();
    }

    /// <summary>
    /// Tests that <see cref="StringExtensions.IsAssemblyPath"/> returns false for paths with .txt extension.
    /// </summary>
    [Fact]
    public void IsAssemblyPath_WithTxtExtension_ReturnsFalse()
    {
        "/plugins/config.txt".IsAssemblyPath().Should().BeFalse();
    }

    /// <summary>
    /// Tests that <see cref="StringExtensions.IsAssemblyPath"/> returns true for paths with uppercase .DLL extension.
    /// </summary>
    [Fact]
    public void IsAssemblyPath_WithUppercaseDllExtension_ReturnsTrue()
    {
        "/plugins/MyPlugin.DLL".IsAssemblyPath().Should().BeTrue();
    }

    // ── TruncateWithEllipsis ────────────────────────────────────────────────

    /// <summary>
    /// Tests that <see cref="StringExtensions.TruncateWithEllipsis"/> returns the original string when it's shorter than the maximum length.
    /// </summary>
    /// <param name="maxLength">The maximum allowed length of the string.</param>
    [Fact]
    public void TruncateWithEllipsis_WhenShorterThanMax_ReturnsOriginal()
    {
        "Hello".TruncateWithEllipsis(10).Should().Be("Hello");
    }

    /// <summary>
    /// Tests that <see cref="StringExtensions.TruncateWithEllipsis"/> returns the original string when it's exactly at the maximum length.
    /// </summary>
    /// <param name="maxLength">The maximum allowed length of the string.</param>
    [Fact]
    public void TruncateWithEllipsis_WhenEqualToMax_ReturnsOriginal()
    {
        "Hello".TruncateWithEllipsis(5).Should().Be("Hello");
    }

    /// <summary>
    /// Tests that <see cref="StringExtensions.TruncateWithEllipsis"/> truncates strings longer than the maximum length and appends ellipsis.
    /// </summary>
    /// <param name="maxLength">The maximum allowed length of the string.</param>
    [Fact]
    public void TruncateWithEllipsis_WhenLongerThanMax_TruncatesWithEllipsis()
    {
        var result = "Hello World".TruncateWithEllipsis(8);

        result.Should().HaveLength(8)
            .And.EndWith("...");
    }

    // ── ToReadableTimeSpan ──────────────────────────────────────────────────

    /// <summary>
    /// Tests that <see cref="TimeSpanExtensions.ToReadableTimeSpan"/> returns "0s" for zero TimeSpan.
    /// </summary>
    [Fact]
    public void ToReadableTimeSpan_WithZeroTimeSpan_ReturnsZeroSeconds()
    {
        TimeSpan.Zero.ToReadableTimeSpan().Should().Be("0s");
    }

    /// <summary>
    /// Tests that <see cref="TimeSpanExtensions.ToReadableTimeSpan"/> formats TimeSpans with days, hours, minutes, and seconds.
    /// </summary>
    [Fact]
    public void ToReadableTimeSpan_WithDaysHoursMinutesSeconds_FormatsAllParts()
    {
        var span = new TimeSpan(1, 2, 30, 45);

        var result = span.ToReadableTimeSpan();

        result.Should().Contain("1d")
            .And.Contain("2h")
            .And.Contain("30m")
            .And.Contain("45s");
    }

    /// <summary>
    /// Tests that <see cref="TimeSpanExtensions.ToReadableTimeSpan"/> omits zero parts when formatting TimeSpans with only minutes.
    /// </summary>
    [Fact]
    public void ToReadableTimeSpan_WithOnlyMinutes_OmitsZeroParts()
    {
        var span = TimeSpan.FromMinutes(5);

        var result = span.ToReadableTimeSpan();

        result.Should().Be("5m");
    }

    // ── FormatBytes ─────────────────────────────────────────────────────────

    /// <summary>
    /// Tests that <see cref="NumericExtensions.FormatBytes"/> formats bytes correctly for values under 1024.
    /// </summary>
    [Fact]
    public void FormatBytes_WithBytesUnder1024_ReturnsBytesLabel()
    {
        512L.FormatBytes().Should().Contain("B").And.NotContain("KB");
    }

    /// <summary>
    /// Tests that <see cref="NumericExtensions.FormatBytes"/> formats exactly 1024 bytes as kilobytes.
    /// </summary>
    [Fact]
    public void FormatBytes_WithExactlyOneKilobyte_ReturnsKbLabel()
    {
        1024L.FormatBytes().Should().Contain("KB");
    }

    /// <summary>
    /// Tests that <see cref="NumericExtensions.FormatBytes"/> formats megabyte values correctly.
    /// </summary>
    [Fact]
    public void FormatBytes_WithMegabyteValue_ReturnsMbLabel()
    {
        (2 * 1024L * 1024L).FormatBytes().Should().Contain("MB");
    }

    /// <summary>
    /// Tests that <see cref="NumericExtensions.FormatBytes"/> formats gigabyte values correctly.
    /// </summary>
    [Fact]
    public void FormatBytes_WithGigabyteValue_ReturnsGbLabel()
    {
        (2L * 1024L * 1024L * 1024L).FormatBytes().Should().Contain("GB");
    }
}