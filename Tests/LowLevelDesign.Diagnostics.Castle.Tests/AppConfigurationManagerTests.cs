using Dapper;
using LowLevelDesign.Diagnostics.Castle.Config;
using LowLevelDesign.Diagnostics.Castle.Models;
using LowLevelDesign.Diagnostics.Commons.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace LowLevelDesign.Diagnostics.Castle.Tests
{
    public class AppConfigurationManagerTests
    {
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

            var expectedApp = new Application { Path = @"c:\TEMP\test\testapp\", IsExcluded = true };
            await conf.AddOrUpdateAppAsync(expectedApp);

            var app = await conf.FindAppAsync(expectedApp.Path);

            Assert.NotNull(app);
            Assert.Equal(expectedApp.Path.ToLowerInvariant(), app.Path);
            Assert.Equal(expectedApp.IsExcluded, app.IsExcluded);
            Assert.Equal("testapp", app.Name); // when no name is provided we will use the one based on a path

            expectedApp.Name = "newappname";
            expectedApp.IsExcluded = false;

            await conf.AddOrUpdateAppAsync(expectedApp);
            app = await conf.FindAppAsync(expectedApp.Path);

            Assert.NotNull(app);
            Assert.Equal(expectedApp.Path.ToLowerInvariant(), app.Path);
            Assert.Equal(expectedApp.IsExcluded, app.IsExcluded);
            Assert.Equal(expectedApp.Name, app.Name);

            app.IsExcluded = true;
            await conf.UpdateAppPropertiesAsync(app, new [] { "IsExcluded" });

            app = await conf.FindAppAsync(expectedApp.Path);

            Assert.False(app.IsExcluded);
        }
    }
}
