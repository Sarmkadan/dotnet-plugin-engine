using Xunit;
using System.Collections.Generic;
using System.Threading.Tasks;
using PluginEngine.Utils.Helpers;
using PluginEngine.Data.Models;
using Moq;

namespace dotnet_plugin_engine.Tests
{
    public class DependencyGraphAnalyzerExtensionsTests
    {
        [Fact]
        public void GetCriticalIssues_NullAnalyzer_ThrowsArgumentNullException()
        {
            // Arrange
            DependencyGraphAnalyzer analyzer = null;
            DependencyAnalysisReport report = new DependencyAnalysisReport();

            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => analyzer.GetCriticalIssues(report));
        }

        [Fact]
        public void GetCriticalIssues_NullReport_ThrowsArgumentNullException()
        {
            // Arrange
            DependencyGraphAnalyzer analyzer = new DependencyGraphAnalyzer();
            DependencyAnalysisReport report = null;

            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => analyzer.GetCriticalIssues(report));
        }

        [Fact]
        public void GetCriticalIssues_EmptyReport_ReturnsEmptyList()
        {
            // Arrange
            DependencyGraphAnalyzer analyzer = new DependencyGraphAnalyzer();
            DependencyAnalysisReport report = new DependencyAnalysisReport();

            // Act
            var result = analyzer.GetCriticalIssues(report);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void GetCriticalIssues_ReportWithCriticalIssues_ReturnsCriticalIssues()
        {
            // Arrange
            DependencyGraphAnalyzer analyzer = new DependencyGraphAnalyzer();
            DependencyAnalysisReport report = new DependencyAnalysisReport
            {
                Issues = new List<string> { "Critical issue 1", "Critical issue 2" }
            };

            // Act
            var result = analyzer.GetCriticalIssues(report);

            // Assert
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public void HasHealthyDependencies_NullAnalyzer_ThrowsArgumentNullException()
        {
            // Arrange
            DependencyGraphAnalyzer analyzer = null;
            DependencyAnalysisReport report = new DependencyAnalysisReport();

            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => analyzer.HasHealthyDependencies(report));
        }

        [Fact]
        public void HasHealthyDependencies_NullReport_ThrowsArgumentNullException()
        {
            // Arrange
            DependencyGraphAnalyzer analyzer = new DependencyGraphAnalyzer();
            DependencyAnalysisReport report = null;

            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => analyzer.HasHealthyDependencies(report));
        }

        [Fact]
        public void HasHealthyDependencies_ReportWithCriticalIssues_ReturnsFalse()
        {
            // Arrange
            DependencyGraphAnalyzer analyzer = new DependencyGraphAnalyzer();
            DependencyAnalysisReport report = new DependencyAnalysisReport
            {
                Issues = new List<string> { "Critical issue 1", "Critical issue 2" }
            };

            // Act
            var result = analyzer.HasHealthyDependencies(report);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void GetDependencyHealthSummary_NullAnalyzer_ThrowsArgumentNullException()
        {
            // Arrange
            DependencyGraphAnalyzer analyzer = null;
            DependencyAnalysisReport report = new DependencyAnalysisReport();

            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => analyzer.GetDependencyHealthSummary(report));
        }

        [Fact]
        public void GetDependencyHealthSummary_NullReport_ThrowsArgumentNullException()
        {
            // Arrange
            DependencyGraphAnalyzer analyzer = new DependencyGraphAnalyzer();
            DependencyAnalysisReport report = null;

            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => analyzer.GetDependencyHealthSummary(report));
        }

        [Fact]
        public void GetDependencyHealthSummary_ReportWithCriticalIssues_ReturnsCriticalIssuesSummary()
        {
            // Arrange
            DependencyGraphAnalyzer analyzer = new DependencyGraphAnalyzer();
            DependencyAnalysisReport report = new DependencyAnalysisReport
            {
                Issues = new List<string> { "Critical issue 1", "Critical issue 2" }
            };

            // Act
            var result = analyzer.GetDependencyHealthSummary(report);

            // Assert
            Assert.Equal("CRITICAL: Critical issue 1, Critical issue 2", result);
        }

        [Fact]
        public async Task GenerateDependencyGraphAsync_NullAnalyzer_ThrowsArgumentNullException()
        {
            // Arrange
            DependencyGraphAnalyzer analyzer = null;
            Plugin rootPlugin = new Plugin();
            int maxDepth = 3;

            // Act and Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => analyzer.GenerateDependencyGraphAsync(rootPlugin, maxDepth));
        }

        [Fact]
        public async Task GenerateDependencyGraphAsync_NullRootPlugin_ThrowsArgumentNullException()
        {
            // Arrange
            DependencyGraphAnalyzer analyzer = new DependencyGraphAnalyzer();
            Plugin rootPlugin = null;
            int maxDepth = 3;

            // Act and Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => analyzer.GenerateDependencyGraphAsync(rootPlugin, maxDepth));
        }

        [Fact]
        public async Task GenerateDependencyGraphAsync_MaxDepthLessThanZero_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            DependencyGraphAnalyzer analyzer = new DependencyGraphAnalyzer();
            Plugin rootPlugin = new Plugin();
            int maxDepth = -1;

            // Act and Assert
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => analyzer.GenerateDependencyGraphAsync(rootPlugin, maxDepth));
        }
    }
}
