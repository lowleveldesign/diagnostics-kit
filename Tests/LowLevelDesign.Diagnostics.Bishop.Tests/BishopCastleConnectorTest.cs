using LowLevelDesign.Diagnostics.Bishop.Config;
using System;
using Xunit;

namespace LowLevelDesign.Diagnostics.Bishop.Tests
{
    public sealed class BishopCastleConnectorTest
    {
        [Fact(Skip = "Run only when application is started with noauth settings")]
        public void TestConnectWithoutAuth()
        {
            var connector = new BishopHttpCastleConnector(new PluginSettings {
                DiagnosticsUrl = new Uri("http://localhost:50890/test/"),
            });
            var configs = connector.ReadApplicationConfigs();
            Assert.NotEmpty(configs);
        }

        [Fact(Skip = "Run only when application is started with auth settings")]
        public void TestConnectWithAuth()
        {
            var settings = new PluginSettings {
                DiagnosticsUrl = new Uri("http://localhost:50890/test/"),
                UserName = "fiddler"
            };
            settings.SetPassword("Fiddler");

            var connector = new BishopHttpCastleConnector(settings);
            Assert.False(connector.AreSettingsValid());
            Assert.Throws<Exception>(() => connector.ReadApplicationConfigs());

            settings.SetPassword("Fiddler1");
            Assert.True(connector.AreSettingsValid());
            var configs = connector.ReadApplicationConfigs();
            Assert.NotEmpty(configs);
        }
    }
}
