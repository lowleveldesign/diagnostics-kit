using Dapper;
using LowLevelDesign.Diagnostics.Commons.Models;
using LowLevelDesign.Diagnostics.LogStore.Commons.Models;
using LowLevelDesign.Diagnostics.LogStore.Defaults;
using System.Configuration;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace LowLevelDesign.Diagnostics.Castle.Tests
{
    public class AppConfigurationManagerTests
    {
        private const string path = @"c:\TEMP\test\defaultstest12312312";
        private readonly DbProviderFactory dbProviderFactory;
        private readonly string dbConnString;

        public AppConfigurationManagerTests()
        {
            var configDbConnString = ConfigurationManager.ConnectionStrings["configdb"];
            dbProviderFactory = DbProviderFactories.GetFactory(configDbConnString.ProviderName ?? "System.Data.SqlClient");
            dbConnString = configDbConnString.ConnectionString;
        }

        [Fact]
        public async Task TestConfiguration()
        {
            var conf = new DefaultAppConfigurationManager();

            var expectedApp = new Application { Path = path, IsExcluded = true, IsHidden = false };

            await conf.AddOrUpdateAppAsync(expectedApp);
            var app = await conf.FindAppAsync(expectedApp.Path);

            Assert.NotNull(app);
            Assert.Equal(expectedApp.Path, app.Path);
            Assert.Equal(true, app.IsExcluded);
            Assert.Equal(false, app.IsHidden);
            Assert.Equal("defaultstest12312312", app.Name); // when no name is provided we will use the one based on a path

            expectedApp.IsExcluded = false;
            expectedApp.IsHidden = true;

            await conf.AddOrUpdateAppAsync(expectedApp);
            app = await conf.FindAppAsync(expectedApp.Path);

            Assert.Equal(true, app.IsExcluded);
            Assert.Equal(true, app.IsHidden);

            expectedApp.Name = "newappname";
            expectedApp.IsExcluded = false;

            await conf.AddOrUpdateAppAsync(expectedApp);
            app = await conf.FindAppAsync(expectedApp.Path);

            Assert.NotNull(app);
            Assert.Equal(expectedApp.Path, app.Path);
            Assert.Equal(expectedApp.IsExcluded, app.IsExcluded);
            Assert.Equal(expectedApp.Name, app.Name);

            app.IsExcluded = true;
            await conf.UpdateAppPropertiesAsync(app, new [] { "IsExcluded" });

            app = await conf.FindAppAsync(expectedApp.Path);

            Assert.True(app.IsExcluded);

            var appconf = new ApplicationServerConfig {
                AppPath = app.Path,
                Server = "TEST2",
                Bindings = new [] { "*:80:", "127.0.0.1:80:", ":80:www.test.com" },
                AppType = ApplicationServerConfig.WebAppType,
                ServiceName = "Test.Service",
                DisplayName = "Test Service Display"
            };
            await conf.AddOrUpdateAppServerConfigAsync(appconf);
            var dbconf = (await conf.GetAppConfigsAsync(new[] { app.Path })).FirstOrDefault();
            Assert.NotNull(dbconf);
            Assert.Equal(appconf.AppPath, dbconf.AppPath);
            Assert.Equal(appconf.AppPoolName, dbconf.AppPoolName);
            Assert.Equal(appconf.Server, dbconf.Server);
            Assert.Contains(appconf.Bindings[0], dbconf.Bindings);
            Assert.Contains(appconf.Bindings[1], dbconf.Bindings);
            Assert.Contains(appconf.Bindings[2], dbconf.Bindings);
            Assert.Equal(appconf.AppType, dbconf.AppType);
            Assert.Equal(appconf.ServiceName, dbconf.ServiceName);
            Assert.Equal(appconf.DisplayName, dbconf.DisplayName);

            Assert.True(app.IsExcluded);
        }

        public void Dispose()
        {
            using (var conn = dbProviderFactory.CreateConnection()) {
                conn.Open();

                conn.Execute("delete from Applications where Path = @path", new { path });
                conn.Execute("delete from ApplicationConfigs where Path = @path", new { path });
            }
        }
    }
}
