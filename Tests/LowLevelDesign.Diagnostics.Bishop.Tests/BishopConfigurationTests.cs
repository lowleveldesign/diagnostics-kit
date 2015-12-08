using LowLevelDesign.Diagnostics.Bishop.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace LowLevelDesign.Diagnostics.Bishop.Tests
{
    public class BishopConfigurationTests : IDisposable
    {
        private class RequestTransformationEqualityComparer : IEqualityComparer<RequestTransformation>
        {
            public bool Equals(RequestTransformation x, RequestTransformation y)
            {
                return string.Equals(x.DestinationPathAndQuery, y.DestinationPathAndQuery) &&
                    string.Equals(x.DestinationHostHeader, y.DestinationHostHeader) &&
                    string.Equals(x.RegexToMatchAgainstPathAndQuery, y.RegexToMatchAgainstPathAndQuery);
            }

            public int GetHashCode(RequestTransformation obj)
            {
                return (obj.DestinationPathAndQuery ?? string.Empty).GetHashCode();
            }
        }

        private class HttpsRedirectEqualityComparer : IEqualityComparer<HttpsLocalRedirect>
        {
            public bool Equals(HttpsLocalRedirect x, HttpsLocalRedirect y)
            {
                return x.LocalHttpPort == y.LocalHttpPort && x.RemoteHttpsPort == y.RemoteHttpsPort;
            }

            public int GetHashCode(HttpsLocalRedirect obj)
            {
                return (obj.LocalHttpPort + obj.RemoteHttpsPort).GetHashCode();
            }
        }

        private readonly ITestOutputHelper output;
        private readonly string configFilePath;

        public BishopConfigurationTests(ITestOutputHelper output)
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
            Assert.NotNull(expectedSettings.HttpsRedirects);
            Assert.Equal(0, expectedSettings.UserDefinedTransformations.Count());
            Assert.Equal(0, expectedSettings.HttpsRedirects.Count());

            expectedSettings.DiagnosticsUrl = new Uri("http://diagnostics.test.com/test");
            expectedSettings.UserName = "testuser";
            var expectedPassword = "testpassword";
            expectedSettings.SetPassword(expectedPassword);
            expectedSettings.UserDefinedTransformations = new RequestTransformation[] {
                new RequestTransformation {
                    DestinationHostHeader = "testheader1",
                    DestinationPathAndQuery = "http://testurl1",
                    RegexToMatchAgainstPathAndQuery = ".*"
                },
                new RequestTransformation {
                    DestinationHostHeader = "testheader2",
                    DestinationPathAndQuery = "http://testurl2",
                    RegexToMatchAgainstPathAndQuery = ".*"
                },
            };
            expectedSettings.HttpsRedirects = new[] {
                new HttpsLocalRedirect {
                    LocalHttpPort = 2000,
                    RemoteHttpsPort = 444
                },
                new HttpsLocalRedirect {
                    LocalHttpPort = 2001,
                    RemoteHttpsPort = 445
                }
            };
            expectedSettings.Save(configFilePath);

            output.WriteLine(File.ReadAllText(configFilePath));

            var actualSettings = PluginSettings.Load(configFilePath);
            Assert.Equal(expectedSettings.DiagnosticsUrl, actualSettings.DiagnosticsUrl);
            Assert.Equal(expectedSettings.UserName, actualSettings.UserName);
            Assert.Equal(expectedPassword, actualSettings.GetPassword());
            Assert.Equal(expectedSettings.UserDefinedTransformations, actualSettings.UserDefinedTransformations,
                new RequestTransformationEqualityComparer());
            Assert.Equal(expectedSettings.HttpsRedirects, actualSettings.HttpsRedirects,
                new HttpsRedirectEqualityComparer());
        }

        public void Dispose()
        {
            if (File.Exists(configFilePath)) {
                File.Delete(configFilePath);
            }
        }
    }
}
