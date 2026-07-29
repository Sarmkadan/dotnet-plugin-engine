using System.Collections.Generic;
using System.Linq;
using Xunit;
using PluginEngine.Domain.Entities;

namespace PluginEngine.Tests
{
    public class AssemblyLoadContextInfoTests
    {
        [Fact]
        public void HappyPathTest()
        {
            // Arrange
            var assemblyLoadContextInfo = new AssemblyLoadContextInfo();
            // Act
            assemblyLoadContextInfo.AddLoadedAssembly("TestAssembly");
            // Assert
            Assert.True(assemblyLoadContextInfo.IsAssemblyLoaded("TestAssembly"));
        }

        [Fact]
        public void EdgeCaseTest_NullInput()
        {
            // Arrange
            var assemblyLoadContextInfo = new AssemblyLoadContextInfo();
            // Act and Assert
            Assert.Throws<ArgumentException>(() => assemblyLoadContextInfo.AddLoadedAssembly(null));
        }

        [Fact]
        public void EdgeCaseTest_EmptyInput()
        {
            // Arrange
            var assemblyLoadContextInfo = new AssemblyLoadContextInfo();
            // Act and Assert
            Assert.Throws<ArgumentException>(() => assemblyLoadContextInfo.AddLoadedAssembly(string.Empty));
        }

        [Fact]
        public void ErrorPathTest_NullInput()
        {
            // Arrange
            var assemblyLoadContextInfo = new AssemblyLoadContextInfo();
            // Act and Assert
            Assert.Throws<ArgumentException>(() => assemblyLoadContextInfo.AddLoadedAssembly(null));
        }
    }
}