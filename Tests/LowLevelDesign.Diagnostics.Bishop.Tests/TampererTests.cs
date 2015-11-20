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
    class TampererTests
    {
        private readonly PluginSettings settings;

        public TampererTests()
        {
            settings = new PluginSettings() {
                UserDefinedTransformations = new [] {
                    new RequestTransformation {
                        RegexToMatchAgainstPathAndQuery = "^/sample-request",
                        DestinationHostHeader = "www.test2.com",
                        DestinationPathAndQuery = "/small"
                    },
                    new RequestTransformation {
                        RegexToMatchAgainstPathAndQuery = @"advert/(\d+)?+(.*)$",
                        DestinationPathAndQuery = "advert?id=$1&$2"
                    },
                }
            };
        }

        [Fact]
        public void TestRules()
        {
            var tamperingRules = new TamperingRulesContainer(settings);

            var req = CreateRequestDescriptorFromUri(new Uri(
                "http://wwww.test.com/sample-request?test=testval&test2=testval2"));

            var resultRule = tamperingRules.FindMatchingTamperParameters(req);
            Assert.Equal(resultRule.HostHeader, "www.test2.com", StringComparer.Ordinal);
            Assert.Equal(resultRule.PathAndQuery, "/small", StringComparer.Ordinal);

            req = CreateRequestDescriptorFromUri(new Uri(
                "http://wwww.test.com/advert/12345?test=testval"));

            resultRule = tamperingRules.FindMatchingTamperParameters(req);
            Assert.Null(resultRule.HostHeader);
            Assert.Equal(resultRule.PathAndQuery, "/advert?id=12345&test=testval", StringComparer.Ordinal);
        }

        private RequestDescriptor CreateRequestDescriptorFromUri(Uri uri)
        {
            var mreq = new Mock<RequestDescriptor>();
            mreq.SetupGet(req => req.FullUrl).Returns(uri.AbsoluteUri);
            mreq.SetupGet(req => req.Host).Returns(uri.Host);
            mreq.SetupGet(req => req.PathAndQuery).Returns(uri.PathAndQuery);
            return mreq.Object;
        }
    }
}
