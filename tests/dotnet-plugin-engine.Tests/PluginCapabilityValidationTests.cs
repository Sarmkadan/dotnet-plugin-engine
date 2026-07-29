using Xunit;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Collections;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.Security.Permissions;
using System.Security.Principal;
using System.Security.AccessControl;
using System.Security.Authentication;
using System.Security.Authorization;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebControl;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Xml.XPath;
using System.Xml.Xsl;

namespace dotnet_plugin_engine.Tests
{
    public class PluginCapabilityValidationTests
    {
        [Fact]
        public void Validate_Happy_PATH()
        {
            // Arrange
            var pluginCapabilityValidation = new PluginCapabilityValidation();
            // Act
            var result = pluginCapabilityValidation.Validate();
            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void IsValid_HAPPY_PATH()
        {
            // Arrange
            var pluginCapabilityValidation = new PluginCapabilityValidation();
            // Act
            var result = pluginCapabilityValidation.IsValid();
            // Assert
            Assert.True(result);
        }

        [Fact]
        public void EnsureValid_HAPPY_PATH()
        {
            // Arrange
            var pluginCapabilityValidation = new PluginCapabilityValidation();
            // Act
            pluginCapabilityValidation.EnsureValid();
            // Assert
            Assert.DoesNotThrow(() => pluginCapabilityValidation.EnsureValid());
        }

        [Fact]
        public void Validate_NULL_INPUT()
        {
            // Arrange
            var pluginCapabilityValidation = new PluginCapabilityValidation();
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => pluginCapabilityValidation.Validate());
        }

        [Fact]
        public void IsValid_NULL_INPUT()
        {
            // Arrange
            var pluginCapabilityValidation = new PluginCapabilityValidation();
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => pluginCapabilityValidation.IsValid());
        }

        [Fact]
        public void EnsureValid_NULL_INPUT()
        {
            // Arrange
            var pluginCapabilityValidation = new PluginCapabilityValidation();
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => pluginCapabilityValidation.EnsureValid());
        }
    }
}
