using LowLevelDesign.Diagnostics.Bishop.Common;
using LowLevelDesign.Diagnostics.Bishop.Config;
using LowLevelDesign.Diagnostics.Bishop.Tampering;
using LowLevelDesign.Diagnostics.Commons.Models;
using Moq;
using System;
using System.Collections.Generic;
using Xunit;

namespace LowLevelDesign.Diagnostics.Bishop.Tests
{
    public class BishopTampererTests
    {
        const string selectedServer = "SRV1";
        private readonly PluginSettings settings;
        private readonly IEnumerable<ApplicationServerConfig> applicationConfigs;
        private readonly CustomTamperingRulesContainer tamperingRules;
        private readonly ServerRedirectionRulesContainer serverRedirectionRules;

        public BishopTampererTests()
        {
            settings = new PluginSettings() {
                UserDefinedTransformations = new [] {
                    new RequestTransformation {
                        RegexToMatchAgainstPathAndQuery = "^/sample-request",
                        DestinationHostHeader = "www.test2.com",
                        DestinationPathAndQuery = "/small"
                    },
                    new RequestTransformation {
                        RegexToMatchAgainstHost = "test.com",
                        RegexToMatchAgainstPathAndQuery = @"advert/(\d+)\??",
                        DestinationPathAndQuery = "advert?id=$1&"
                    },
                    new RequestTransformation {
                        RegexToMatchAgainstHost = "testip.com",
                        DestinationIpAddresses = new [] { "192.168.1.10" },
                        Protocol = "http"
                    },
                    new RequestTransformation {
                        RegexToMatchAgainstHost = "testport.com",
                        DestinationPorts = new ushort[] { 6060 }
                    },
                    new RequestTransformation {
                        RegexToMatchAgainstHost = "www.test.com",
                        RegexToMatchAgainstPathAndQuery = "/newver",
                        DestinationHostHeader = "www.test-newver.com",
                    }
                }
            };

            applicationConfigs = new ApplicationServerConfig[] {
                new ApplicationServerConfig {
                    AppPath = "path1",
                    Bindings = new [] { "http://192.168.1.10:80:", "https://192.168.1.10:443:" },
                    Server = "SRV1",
                    ServerFqdnOrIp = "srv1.test.com"
                },
                new ApplicationServerConfig {
                    AppPath = "path2",
                    Bindings = new [] { "http://*:80:www.testhost.com", "https://*:80:www.testhost.com" },
                    Server = "SRV1",
                    ServerFqdnOrIp = "srv1.test.com"
                },
                new ApplicationServerConfig {
                    AppPath = "path3",
                    Bindings = new [] { "http://*:6060:", "https://*:6061:" },
                    Server = "SRV1",
                    ServerFqdnOrIp = "srv1.test.com"
                },
                new ApplicationServerConfig {
                    AppPath = "path4",
                    Bindings = new [] { "http://*:80:www.test-newver.com" },
                    Server = "SRV1",
                    ServerFqdnOrIp = "srv1.test.com"
                },
            };

            tamperingRules = new CustomTamperingRulesContainer(settings);
            serverRedirectionRules = new ServerRedirectionRulesContainer(applicationConfigs);
        }

        [Fact]
        public void TestCustomTamperingRules()
        {
            TestHostHeaderAndPathQueryTampering();
            TestPathQueryAdvancedTampering();
            TestNoTampering();
        }

        private void TestHostHeaderAndPathQueryTampering()
        {
            var req = CreateRequestDescriptorFromUri(new Uri(
                "http://wwww.test.com/sample-request?test=testval&test2=testval2"));
            var tamperingContext = new TamperingContext();
            tamperingRules.ApplyMatchingTamperingRules(req, tamperingContext);
            Assert.Null(tamperingContext.ServerTcpAddressWithPort);
            Assert.Equal(tamperingContext.HostHeader, "www.test2.com", StringComparer.Ordinal);
            Assert.Equal("/small?test=testval&test2=testval2", tamperingContext.PathAndQuery, StringComparer.Ordinal);
        }

        private void TestPathQueryAdvancedTampering()
        {
            var req = CreateRequestDescriptorFromUri(new Uri(
                "http://wwww.test.com/prefix/advert/12345?test=testval"));
            var tamperingContext = new TamperingContext();
            tamperingRules.ApplyMatchingTamperingRules(req, tamperingContext);
            Assert.Null(tamperingContext.ServerTcpAddressWithPort);
            Assert.Null(tamperingContext.HostHeader);
            Assert.Equal("/prefix/advert?id=12345&test=testval", tamperingContext.PathAndQuery, StringComparer.Ordinal);
        }

        private void TestNoTampering()
        {
            var tamperingContext = new TamperingContext();
            var req = CreateRequestDescriptorFromUri(new Uri(
                "http://wwww.test.com/prefix/sample-request/12345?test=testval"));
            tamperingRules.ApplyMatchingTamperingRules(req, tamperingContext);
            Assert.Null(tamperingContext.ServerTcpAddressWithPort);
            Assert.Null(tamperingContext.HostHeader);
            Assert.Null(tamperingContext.PathAndQuery);
        }

        [Fact]
        public void TestServerRedirectionRules()
        {
            TestIpRedirect();
            TestIpHttpsRedirect();
            TestPortRedirect();
            TestHostRedirect();
            Test2StepHostRedirect();
        }


        private void TestIpRedirect()
        {
            var req = CreateRequestDescriptorFromUri(new Uri("http://www.testip.com/testurl?withpar1=v1&withpar2=v2"));
            var tamperingContext = new TamperingContext();
            tamperingRules.ApplyMatchingTamperingRules(req, tamperingContext);
            serverRedirectionRules.ApplyMatchingTamperingRules(req, tamperingContext, selectedServer);
            Assert.True(tamperingContext.ShouldTamperRequest);
            Assert.Null(tamperingContext.PathAndQuery);
            Assert.Null(tamperingContext.HostHeader);
            Assert.Equal("http", tamperingContext.Protocol);
            Assert.Equal("192.168.1.10:80", tamperingContext.ServerTcpAddressWithPort);
        }

        private void TestIpHttpsRedirect()
        {
            var req = CreateRequestDescriptorFromUri(new Uri("https://www.testip.com/testurl?withpar1=v1&withpar2=v2"));
            var tamperingContext = new TamperingContext();
            tamperingRules.ApplyMatchingTamperingRules(req, tamperingContext);
            serverRedirectionRules.ApplyMatchingTamperingRules(req, tamperingContext, selectedServer);
            Assert.True(tamperingContext.ShouldTamperRequest);
            Assert.Null(tamperingContext.PathAndQuery);
            Assert.Null(tamperingContext.HostHeader);
            Assert.Equal("https", tamperingContext.Protocol);
            Assert.Equal("192.168.1.10:443", tamperingContext.ServerTcpAddressWithPort);
        }

        private void TestPortRedirect()
        {
            var req = CreateRequestDescriptorFromUri(new Uri("http://www.testport.com/testurl?withpar1=v1&withpar2=v2"));
            var tamperingContext = new TamperingContext();
            tamperingRules.ApplyMatchingTamperingRules(req, tamperingContext);
            serverRedirectionRules.ApplyMatchingTamperingRules(req, tamperingContext, selectedServer);
            Assert.True(tamperingContext.ShouldTamperRequest);
            Assert.Null(tamperingContext.PathAndQuery);
            Assert.Null(tamperingContext.HostHeader);
            Assert.Equal("http", tamperingContext.Protocol);
            Assert.Equal("srv1.test.com:6060", tamperingContext.ServerTcpAddressWithPort);
        }

        private void TestHostRedirect()
        {
            var req = CreateRequestDescriptorFromUri(new Uri("http://www.testhost.com/testurl?withpar1=v1&withpar2=v2"));
            var tamperingContext = new TamperingContext();
            tamperingRules.ApplyMatchingTamperingRules(req, tamperingContext);
            serverRedirectionRules.ApplyMatchingTamperingRules(req, tamperingContext, selectedServer);
            Assert.True(tamperingContext.ShouldTamperRequest);
            Assert.Equal("http", tamperingContext.Protocol);
            Assert.Equal("srv1.test.com:80", tamperingContext.ServerTcpAddressWithPort);
            Assert.Equal("www.testhost.com", tamperingContext.HostHeader);
        }

        private void Test2StepHostRedirect()
        {
            var req = CreateRequestDescriptorFromUri(new Uri("http://www.test.com/newver?withpar1=v1&withpar2=v2"));
            var tamperingContext = new TamperingContext();
            tamperingRules.ApplyMatchingTamperingRules(req, tamperingContext);
            serverRedirectionRules.ApplyMatchingTamperingRules(req, tamperingContext, selectedServer);
            Assert.True(tamperingContext.ShouldTamperRequest);
            Assert.Equal("http", tamperingContext.Protocol);
            Assert.Equal("srv1.test.com:80", tamperingContext.ServerTcpAddressWithPort);
            Assert.Equal("www.test-newver.com", tamperingContext.HostHeader);
        }

        private IRequest CreateRequestDescriptorFromUri(Uri uri)
        {
            var mreq = new Mock<IRequest>();
            mreq.SetupGet(req => req.FullUrl).Returns(uri.AbsoluteUri);
            mreq.SetupGet(req => req.Host).Returns(uri.Host);
            mreq.SetupGet(req => req.PathAndQuery).Returns(uri.PathAndQuery);
            mreq.SetupGet(req => req.Protocol).Returns(uri.Scheme);
            return mreq.Object;
        }
    }
}
