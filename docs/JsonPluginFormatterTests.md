# JsonPluginFormatterTests
The `JsonPluginFormatterTests` class is a test suite designed to verify the functionality of the `JsonPluginFormatter` class in the `dotnet-plugin-engine` project. This class contains a set of test methods that cover various scenarios to ensure the formatter correctly produces JSON output for plugins, plugin collections, and health reports.

## API
The `JsonPluginFormatterTests` class includes the following public test methods:
* `FormatType_IsJson`: Verifies that the formatter produces JSON output.
* `FormatPluginAsync_ProducesValidJson`: Tests if the formatter produces valid JSON for a plugin.
* `FormatPluginAsync_ContainsPluginName`: Checks if the formatted JSON contains the plugin name.
* `FormatPluginAsync_ContainsVersion`: Verifies if the formatted JSON includes the plugin version.
* `FormatPluginAsync_ContainsDependencyCount`: Tests if the formatted JSON contains the dependency count.
* `FormatPluginsAsync_ProducesValidJson`: Verifies that the formatter produces valid JSON for a collection of plugins.
* `FormatPluginsAsync_CountMatchesPluginCount`: Checks if the formatted JSON count matches the number of plugins.
* `FormatPluginsAsync_WithEmptyCollection_ReturnsZeroCount`: Tests the formatter's behavior with an empty plugin collection.
* `FormatDetailedReportAsync_ProducesValidJson`: Verifies that the formatter produces valid JSON for a detailed report.
* `FormatDetailedReportAsync_ContainsMetadataWhenSet`: Checks if the formatted JSON includes metadata when set.
* `FormatDetailedReportAsync_MetadataIsNullWhenNotSet`: Verifies that the metadata is null in the formatted JSON when not set.
* `FormatHealthReportAsync_ProducesValidJson`: Tests if the formatter produces valid JSON for a health report.
* `FormatHealthReportAsync_IsHealthyReflectsInput`: Verifies that the formatted JSON reflects the input health status.

## Usage
Here are two examples of using the `JsonPluginFormatterTests` class:
```csharp
// Example 1: Testing plugin formatting
[TestMethod]
public async Task TestPluginFormatting()
{
    var formatter = new JsonPluginFormatter();
    var plugin = new Plugin { Name = "TestPlugin", Version = "1.0" };
    var json = await formatter.FormatPluginAsync(plugin);
    Assert.IsTrue(json.Contains("TestPlugin"));
    Assert.IsTrue(json.Contains("1.0"));
}

// Example 2: Testing plugin collection formatting
[TestMethod]
public async Task TestPluginCollectionFormatting()
{
    var formatter = new JsonPluginFormatter();
    var plugins = new List<Plugin> { new Plugin { Name = "TestPlugin1", Version = "1.0" }, new Plugin { Name = "TestPlugin2", Version = "2.0" } };
    var json = await formatter.FormatPluginsAsync(plugins);
    Assert.IsTrue(json.Contains("TestPlugin1"));
    Assert.IsTrue(json.Contains("TestPlugin2"));
    Assert.IsTrue(json.Contains("2"));
}
```

## Notes
The `JsonPluginFormatterTests` class is designed to be thread-safe, as it does not maintain any internal state between test methods. However, the test methods may throw exceptions if the formatter being tested is not properly configured or if the input data is invalid. Additionally, the test methods may have dependencies on other classes or libraries, which should be carefully managed to avoid conflicts or versioning issues. It is also important to note that the `JsonPluginFormatter` class is not tested for null or empty input, which may cause `NullReferenceException` or other exceptions.
