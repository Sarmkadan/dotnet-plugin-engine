[TestClass]
public class PluginDependencyExtensionsTests
{
    [Test]
    public void HappyPath_IsVersionSatisfied()
    {
        var dependency = new PluginDependency("1.0.0", "2.0.0");
        var version = "1.5.0";
        Assert.IsTrue(PluginDependencyExtensions.IsVersionSatisfied(dependency, version));
    }

    [Test]
    public void EdgeCase_IsVersionSatisfied_NullInput()
    {
        var dependency = new PluginDependency("1.0.0", "2.0.0");
        var version = "";
        Assert.Throws<ArgumentException>(() => PluginDependencyExtensions.IsVersionSatisfied(dependency, version));
    }

    [Test]
    public void ErrorPath_IsVersionSatisfied_ThrowsException()
    {
        var dependency = new PluginDependency("1.0.0", "2.0.0");
        var version = "invalid-version";
        Assert.Throws<ArgumentException>(() => PluginDependencyExtensions.IsVersionSatisfied(dependency, version));
    }

    [Test]
    public void HappyPath_OverlapsWith()
    {
        var first = new PluginDependency("1.0.0", "2.0.0");
        var second = new PluginDependency("1.5.0", "3.0.0");
        Assert.IsTrue(PluginDependencyExtensions.OverlapsWith(first, second));
    }

    [Test]
    public void EdgeCase_OverlapsWith_NullInput()
    {
        var first = new PluginDependency("1.0.0", "2.0.0");
        var second = new PluginDependency(null, null);
        Assert.Throws<ArgumentNullException>(() => PluginDependencyExtensions.OverlapsWith(first, second));
    }

    [Test]
    public void ErrorPath_OverlapsWith_ThrowsException()
    {
        var first = new PluginDependency("1.0.0", "2.0.0");
        var second = new PluginDependency("invalid-version", "3.0.0");
        Assert.Throws<FormatException>(() => PluginDependencyExtensions.OverlapsWith(first, second));
    }

    [Test]
    public void HappyPath_ToSummary()
    {
        var dependency = new PluginDependency("1.0.0", "2.0.0", "My Plugin", true);
        var expected = "MyPlugin [1.0.0 - 2.0.0] Optional - My Plugin";
        Assert.AreEqual(expected, PluginDependencyExtensions.ToSummary(dependency));
    }

    [Test]
    public void EdgeCase_ToSummary_NullInput()
    {
        var dependency = new PluginDependency(null, null, "My Plugin", true);
        Assert.Throws<ArgumentNullException>(() => PluginDependencyExtensions.ToSummary(dependency));
    }

    [Test]
    public void ErrorPath_ToSummary_ThrowsException()
    {
        var dependency = new PluginDependency("invalid-version", "2.0.0", "My Plugin", true);
        Assert.Throws<FormatException>(() => PluginDependencyExtensions.ToSummary(dependency));
    }
}