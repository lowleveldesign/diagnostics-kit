using LowLevelDesign.Diagnostics.Bishop.Config;
using LowLevelDesign.Diagnostics.Bishop.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace LowLevelDesign.Diagnostics.Bishop.Tests
{
    public class ConfigurationTests : IDisposable
    {
        private class RequestTransformationEqualityComparer : IEqualityComparer<RequestTransformation>
        {
            public bool Equals(RequestTransformation x, RequestTransformation y)
            {
                return string.Equals(x.DestinationUrl, y.DestinationUrl) &&
                    string.Equals(x.DestinationHostHeader, y.DestinationHostHeader) &&
                    string.Equals(x.RegexToMatch, y.RegexToMatch);
            }

            public int GetHashCode(RequestTransformation obj)
            {
                return (obj.DestinationUrl ?? string.Empty).GetHashCode();
            }
        }

        private readonly ITestOutputHelper output;
        private readonly string configFilePath;

        public ConfigurationTests(ITestOutputHelper output)
        {
            configFilePath = Path.GetTempFileName();
            this.output = output;

        }

        [Fact]
        public void PluginSettingsTest()
        {
            var expectedSettings = PluginSettings.Load(configFilePath);
            Assert.NotNull(expectedSettings);
            Assert.Null(expectedSettings.DiagnosticsUrl);
            Assert.NotNull(expectedSettings.UserDefinedTransformations);
            Assert.Equal(0, expectedSettings.UserDefinedTransformations.Count());

            expectedSettings.DiagnosticsUrl = new Uri("http://diagnostics.test.com/test");
            expectedSettings.IsAuthenticationRequired = true;
            expectedSettings.UserName = "testuser";
            var expectedPassword = "testpassword";
            expectedSettings.SetPassword(expectedPassword);
            expectedSettings.UserDefinedTransformations = new RequestTransformation[] {
                new RequestTransformation {
                    DestinationHostHeader = "testheader1",
                    DestinationUrl = "http://testurl1",
                    RegexToMatch = ".*"
                },
                new RequestTransformation {
                    DestinationHostHeader = "testheader2",
                    DestinationUrl = "http://testurl2",
                    RegexToMatch = ".*"
                },
            };
            expectedSettings.Save(configFilePath);

            output.WriteLine(File.ReadAllText(configFilePath));

            var actualSettings = PluginSettings.Load(configFilePath);
            Assert.Equal(expectedSettings.DiagnosticsUrl, actualSettings.DiagnosticsUrl);
            Assert.Equal(expectedSettings.IsAuthenticationRequired, actualSettings.IsAuthenticationRequired);
            Assert.Equal(expectedSettings.UserName, actualSettings.UserName);
            Assert.Equal(expectedPassword, actualSettings.GetPassword());
            Assert.Equal(expectedSettings.UserDefinedTransformations, actualSettings.UserDefinedTransformations,
                new RequestTransformationEqualityComparer());
        }

        public void Dispose()
        {
            if (File.Exists(configFilePath)) {
                File.Delete(configFilePath);
            }
        }
    }
}
