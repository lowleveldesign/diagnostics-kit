using LowLevelDesign.Diagnostics.Bishop.Common;
using LowLevelDesign.Diagnostics.Bishop.Config;
using LowLevelDesign.Diagnostics.Bishop.Tampering;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace LowLevelDesign.Diagnostics.Bishop.Tests
{
    public class BishopTampererTests
    {
        private readonly PluginSettings settings;

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
                        RegexToMatchAgainstPathAndQuery = @"advert/(\d+)\??",
                        DestinationPathAndQuery = "advert?id=$1&"
                    },
                }
            };
        }

        [Fact]
        public void TestRules()
        {
            var tamperingRules = new TamperingRulesContainer(settings);
            TamperParameters tamperParameters;

            var req = CreateRequestDescriptorFromUri(new Uri(
                "http://wwww.test.com/sample-request?test=testval&test2=testval2"));

            tamperParameters = new TamperParameters();
            tamperingRules.ApplyMatchingTamperParameters(req, tamperParameters);
            Assert.Null(tamperParameters.ServerTcpAddressWithPort);
            Assert.Equal(tamperParameters.HostHeader, "www.test2.com", StringComparer.Ordinal);
            Assert.Equal("/small?test=testval&test2=testval2", tamperParameters.PathAndQuery, StringComparer.Ordinal);

            req = CreateRequestDescriptorFromUri(new Uri(
                "http://wwww.test.com/prefix/advert/12345?test=testval"));

            tamperParameters = new TamperParameters();
            tamperingRules.ApplyMatchingTamperParameters(req, tamperParameters);
            Assert.Null(tamperParameters.ServerTcpAddressWithPort);
            Assert.Null(tamperParameters.HostHeader);
            Assert.Equal("/prefix/advert?id=12345&test=testval", tamperParameters.PathAndQuery, StringComparer.Ordinal);


            tamperParameters = new TamperParameters();
            req = CreateRequestDescriptorFromUri(new Uri(
                "http://wwww.test.com/prefix/sample-request/12345?test=testval"));
            tamperingRules.ApplyMatchingTamperParameters(req, tamperParameters);
            Assert.Null(tamperParameters.ServerTcpAddressWithPort);
            Assert.Null(tamperParameters.HostHeader);
            Assert.Null(tamperParameters.PathAndQuery);
        }

        private IRequest CreateRequestDescriptorFromUri(Uri uri)
        {
            var mreq = new Mock<IRequest>();
            mreq.SetupGet(req => req.FullUrl).Returns(uri.AbsoluteUri);
            mreq.SetupGet(req => req.Host).Returns(uri.Host);
            mreq.SetupGet(req => req.PathAndQuery).Returns(uri.PathAndQuery);
            return mreq.Object;
        }
    }
}
