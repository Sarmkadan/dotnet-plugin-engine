#nullable enable
using FluentAssertions;
using PluginEngine.Utils.Extensions;
using Xunit;

namespace PluginEngine.Tests;

public sealed class StringExtensionsTests
{
    // ── NormalizePluginPath ─────────────────────────────────────────────────

    [Fact]
    public void NormalizePluginPath_WithNullOrWhitespace_ReturnsEmpty()
    {
        "".NormalizePluginPath().Should().BeEmpty();
        "   ".NormalizePluginPath().Should().BeEmpty();
    }

    [Fact]
    public void NormalizePluginPath_WithForwardSlashes_ConvertsToOsSeparator()
    {
        var path = "plugins/auth/MyPlugin.dll";

        var result = path.NormalizePluginPath();

        result.Should().Contain(Path.DirectorySeparatorChar.ToString());
    }

    [Fact]
    public void NormalizePluginPath_WithTrailingSeparator_RemovesTrailingSeparator()
    {
        var path = $"plugins{Path.DirectorySeparatorChar}auth{Path.DirectorySeparatorChar}";

        var result = path.NormalizePluginPath();

        result.Should().NotEndWith(Path.DirectorySeparatorChar.ToString());
    }

    // ── IsValidPluginId ─────────────────────────────────────────────────────

    [Fact]
    public void IsValidPluginId_WithValidGuid_ReturnsTrue()
    {
        Guid.NewGuid().ToString().IsValidPluginId().Should().BeTrue();
    }

    [Fact]
    public void IsValidPluginId_WithInvalidString_ReturnsFalse()
    {
        "not-a-guid".IsValidPluginId().Should().BeFalse();
    }

    [Fact]
    public void IsValidPluginId_WithEmptyString_ReturnsFalse()
    {
        "".IsValidPluginId().Should().BeFalse();
    }

    // ── IsValidVersion ──────────────────────────────────────────────────────

    [Fact]
    public void IsValidVersion_WithSemanticVersion_ReturnsTrue()
    {
        "1.2.3".IsValidVersion().Should().BeTrue();
    }

    [Fact]
    public void IsValidVersion_WithNonVersionString_ReturnsFalse()
    {
        "not-a-version".IsValidVersion().Should().BeFalse();
    }

    // ── SanitizeForFilename ─────────────────────────────────────────────────

    [Fact]
    public void SanitizeForFilename_WithEmptyString_ReturnsEmpty()
    {
        "".SanitizeForFilename().Should().BeEmpty();
    }

    [Fact]
    public void SanitizeForFilename_WithValidName_ReturnsUnchanged()
    {
        "MyPlugin".SanitizeForFilename().Should().Be("MyPlugin");
    }

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

    [Fact]
    public void GetAssemblyName_WithDllPath_ReturnsNameWithoutExtension()
    {
        "/plugins/MyPlugin.dll".GetAssemblyName().Should().Be("MyPlugin");
    }

    [Fact]
    public void GetAssemblyName_WithNestedPath_ReturnsOnlyFilenameWithoutExtension()
    {
        "/opt/plugins/subdir/Auth.Plugin.dll".GetAssemblyName().Should().Be("Auth.Plugin");
    }

    // ── IsAssemblyPath ──────────────────────────────────────────────────────

    [Fact]
    public void IsAssemblyPath_WithDllExtension_ReturnsTrue()
    {
        "/plugins/MyPlugin.dll".IsAssemblyPath().Should().BeTrue();
    }

    [Fact]
    public void IsAssemblyPath_WithExeExtension_ReturnsTrue()
    {
        "/plugins/MyPlugin.exe".IsAssemblyPath().Should().BeTrue();
    }

    [Fact]
    public void IsAssemblyPath_WithTxtExtension_ReturnsFalse()
    {
        "/plugins/config.txt".IsAssemblyPath().Should().BeFalse();
    }

    [Fact]
    public void IsAssemblyPath_WithUppercaseDllExtension_ReturnsTrue()
    {
        "/plugins/MyPlugin.DLL".IsAssemblyPath().Should().BeTrue();
    }

    // ── TruncateWithEllipsis ────────────────────────────────────────────────

    [Fact]
    public void TruncateWithEllipsis_WhenShorterThanMax_ReturnsOriginal()
    {
        "Hello".TruncateWithEllipsis(10).Should().Be("Hello");
    }

    [Fact]
    public void TruncateWithEllipsis_WhenEqualToMax_ReturnsOriginal()
    {
        "Hello".TruncateWithEllipsis(5).Should().Be("Hello");
    }

    [Fact]
    public void TruncateWithEllipsis_WhenLongerThanMax_TruncatesWithEllipsis()
    {
        var result = "Hello World".TruncateWithEllipsis(8);

        result.Should().HaveLength(8)
            .And.EndWith("...");
    }

    // ── ToReadableTimeSpan ──────────────────────────────────────────────────

    [Fact]
    public void ToReadableTimeSpan_WithZeroTimeSpan_ReturnsZeroSeconds()
    {
        TimeSpan.Zero.ToReadableTimeSpan().Should().Be("0s");
    }

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

    [Fact]
    public void ToReadableTimeSpan_WithOnlyMinutes_OmitsZeroParts()
    {
        var span = TimeSpan.FromMinutes(5);

        var result = span.ToReadableTimeSpan();

        result.Should().Be("5m");
    }

    // ── FormatBytes ─────────────────────────────────────────────────────────

    [Fact]
    public void FormatBytes_WithBytesUnder1024_ReturnsBytesLabel()
    {
        512L.FormatBytes().Should().Contain("B").And.NotContain("KB");
    }

    [Fact]
    public void FormatBytes_WithExactlyOneKilobyte_ReturnsKbLabel()
    {
        1024L.FormatBytes().Should().Contain("KB");
    }

    [Fact]
    public void FormatBytes_WithMegabyteValue_ReturnsMbLabel()
    {
        (2 * 1024L * 1024L).FormatBytes().Should().Contain("MB");
    }

    [Fact]
    public void FormatBytes_WithGigabyteValue_ReturnsGbLabel()
    {
        (2L * 1024L * 1024L * 1024L).FormatBytes().Should().Contain("GB");
    }
}
