using LowLevelDesign.Diagnostics.Bishop.UI;
using Moq;
using Xunit;

namespace LowLevelDesign.Diagnostics.Bishop.Tests
{
    public class BishopMenuTests
    {
        [Fact]
        public void CastleConfigurationTest()
        {
            var pluginMock = new Mock<IBishop>();
            pluginMock.Setup(fp => fp.TurnOffServerRedirection());
            pluginMock.Setup(fp => fp.IsDiagnosticsCastleConfigured()).Returns(false);
            pluginMock.SetupGet(fp => fp.AvailableServers).Returns(new[] { "SRV1", "SRV2" });

            var menu = new PluginMenu(pluginMock.Object);
            Assert.True(menu.BishopMenu.MenuItems.ContainsKey("miConfigureCastle"));
            Assert.True(menu.BishopMenu.MenuItems.ContainsKey("miNoServer"));
            Assert.True(menu.BishopMenu.MenuItems["miNoServer"].Checked);
            Assert.False(menu.BishopMenu.MenuItems.ContainsKey("miServerSRV1"));
            Assert.False(menu.BishopMenu.MenuItems.ContainsKey("miServerSRV2"));

            pluginMock.Setup(fp => fp.IsDiagnosticsCastleConfigured()).Returns(true);
            menu.PrepareServerMenu();
            Assert.False(menu.BishopMenu.MenuItems.ContainsKey("miConfigureCastle"));
            Assert.True(menu.BishopMenu.MenuItems.ContainsKey("miNoServer"));
            Assert.True(menu.BishopMenu.MenuItems.ContainsKey("miServerSRV1"));
            Assert.True(menu.BishopMenu.MenuItems.ContainsKey("miServerSRV2"));

            menu.BishopMenu.MenuItems["miNoServer"].Checked = false;
            menu.BishopMenu.MenuItems["miServerSRV1"].Checked = true;

            pluginMock.Setup(fp => fp.IsDiagnosticsCastleConfigured()).Returns(false);
            menu.PrepareServerMenu();
            Assert.True(menu.BishopMenu.MenuItems.ContainsKey("miConfigureCastle"));
            Assert.True(menu.BishopMenu.MenuItems.ContainsKey("miNoServer"));
            Assert.True(menu.BishopMenu.MenuItems["miNoServer"].Checked);
            Assert.False(menu.BishopMenu.MenuItems.ContainsKey("miServerSRV1"));
            Assert.False(menu.BishopMenu.MenuItems.ContainsKey("miServerSRV2"));
        }

        [Fact]
        public void ServersChangedTest()
        {
            var pluginMock = new Mock<IBishop>();
            pluginMock.Setup(fp => fp.TurnOffServerRedirection());
            pluginMock.Setup(fp => fp.IsDiagnosticsCastleConfigured()).Returns(true);
            pluginMock.SetupGet(fp => fp.AvailableServers).Returns(new[] { "SRV1", "SRV2", "SRV3" });

            var menu = new PluginMenu(pluginMock.Object);
            Assert.True(menu.BishopMenu.MenuItems.ContainsKey("miServerSRV1"));
            Assert.Equal(1, menu.BishopMenu.MenuItems["miServerSRV1"].Index);
            Assert.True(menu.BishopMenu.MenuItems.ContainsKey("miServerSRV2"));
            Assert.Equal(2, menu.BishopMenu.MenuItems["miServerSRV2"].Index);
            Assert.True(menu.BishopMenu.MenuItems.ContainsKey("miServerSRV3"));
            Assert.Equal(3, menu.BishopMenu.MenuItems["miServerSRV3"].Index);

            pluginMock.SetupGet(fp => fp.AvailableServers).Returns(new[] { "SRV1", "SRV3" });
            menu.PrepareServerMenu();
            Assert.True(menu.BishopMenu.MenuItems.ContainsKey("miServerSRV1"));
            Assert.Equal(1, menu.BishopMenu.MenuItems["miServerSRV1"].Index);
            Assert.False(menu.BishopMenu.MenuItems.ContainsKey("miServerSRV2"));
            Assert.True(menu.BishopMenu.MenuItems.ContainsKey("miServerSRV3"));
            Assert.Equal(2, menu.BishopMenu.MenuItems["miServerSRV3"].Index);

            pluginMock.SetupGet(fp => fp.AvailableServers).Returns(new[] { "SRV3" });
            menu.PrepareServerMenu();
            Assert.False(menu.BishopMenu.MenuItems.ContainsKey("miServerSRV1"));
            Assert.False(menu.BishopMenu.MenuItems.ContainsKey("miServerSRV2"));
            Assert.True(menu.BishopMenu.MenuItems.ContainsKey("miServerSRV3"));
            Assert.Equal(1, menu.BishopMenu.MenuItems["miServerSRV3"].Index);
        }
    }
}
