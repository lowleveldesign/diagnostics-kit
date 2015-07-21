﻿using Dapper;
using LowLevelDesign.Diagnostics.Commons.Models;
using LowLevelDesign.Diagnostics.LogStore.MySql;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace LowLevelDesign.Diagnostics.LogStore.Tests
{
    public class MySqlConfigurationManagerTests : IDisposable
    {
        private const string path = @"c:\TEMP\test\blablabla";
        private readonly string dbConnString;

        public MySqlConfigurationManagerTests()
        {
            var configDbConnString = ConfigurationManager.ConnectionStrings["mysqlconn"];
            dbConnString = configDbConnString.ConnectionString;
        }

        [Fact]
        public async Task TestConfiguration()
        {
            var conf = new MySqlAppConfigurationManager();

            var expectedApp = new Application { Path = path, IsExcluded = true };
            await conf.AddOrUpdateAppAsync(expectedApp);

            var app = await conf.FindAppAsync(expectedApp.Path);

            Assert.NotNull(app);
            Assert.Equal(expectedApp.Path.ToLowerInvariant(), app.Path);
            Assert.Equal(expectedApp.IsExcluded, app.IsExcluded);
            Assert.Equal("blablabla", app.Name); // when no name is provided we will use the one based on a path

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
            Assert.True(app.IsExcluded);

            var appconf = new ApplicationServerConfig {
                AppPath = app.Path,
                Server = "TEST2",
                Bindings = new [] { "*:80:", "127.0.0.1:80:", ":80:www.test.com" }
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

            Assert.True(app.IsExcluded);
        }

        public void Dispose()
        {
            using (var conn = new MySqlConnection(dbConnString)) {
                conn.Open();

                conn.Execute("delete from Applications where Path = @path", new { path });
                conn.Execute("delete from ApplicationConfigs where Path = @path", new { path });
            }
        }
    }
}