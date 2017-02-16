/**
 *  Part of the Diagnostics Kit
 *
 *  Copyright (C) 2016  Sebastian Solnica
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.

 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 */

using LowLevelDesign.Diagnostics.Commons.Models;
using LowLevelDesign.Diagnostics.LogStore.Commons.Models;
using LowLevelDesign.Diagnostics.LogStore.ElasticSearch.Models;
using Nest;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace LowLevelDesign.Diagnostics.LogStore.ElasticSearch.Tests
{
    public class ElasticConfigurationManagerTests : IDisposable
    {
        private const string confkey = "test:test:1234";
        private const string path = @"c:\TEMP\test\blablabla";

        public ElasticConfigurationManagerTests()
        {
        }

        [Fact]
        public async Task TestConfiguration()
        {
            var conf = new ElasticSearchAppConfigurationManager();

            var expectedApp = new Application { Path = path, IsExcluded = true, IsHidden = false };
            await conf.AddOrUpdateAppAsync(expectedApp);
            // give it 1s to swallow
            await Task.Delay(1000);

            var app = await conf.FindAppAsync(expectedApp.Path);

            Assert.NotNull(app);
            Assert.Equal(expectedApp.Path, app.Path);
            Assert.Equal(expectedApp.IsExcluded, app.IsExcluded);
            Assert.Equal("blablabla", app.Name); // when no name is provided we will use the one based on a path
            Assert.Equal(true, app.IsExcluded);
            Assert.Equal(false, app.IsHidden);

            expectedApp.IsExcluded = false;
            expectedApp.IsHidden = true;

            await conf.AddOrUpdateAppAsync(expectedApp);
            // give it 1s to swallow
            await Task.Delay(1000);

            var apps = await conf.GetAppsAsync();
            Assert.Contains(apps, ap => expectedApp.Path.Equals(ap.Path, StringComparison.OrdinalIgnoreCase));

            app = await conf.FindAppAsync(expectedApp.Path);

            Assert.Equal(true, app.IsExcluded);
            Assert.Equal(true, app.IsHidden);

            expectedApp.Name = "newappname";
            expectedApp.IsExcluded = false;

            await conf.AddOrUpdateAppAsync(expectedApp);
            // give it 1s to swallow
            await Task.Delay(1000);

            app = await conf.FindAppAsync(expectedApp.Path);

            Assert.NotNull(app);
            Assert.Equal(expectedApp.Path, app.Path);
            Assert.Equal(expectedApp.IsExcluded, app.IsExcluded);
            Assert.Equal(expectedApp.Name, app.Name);

            app.IsExcluded = true;
            await conf.UpdateAppPropertiesAsync(app, new[] { "IsExcluded" });
            // give it 1s to swallow
            await Task.Delay(1000);

            app = await conf.FindAppAsync(expectedApp.Path);
            Assert.True(app.IsExcluded);

            var appconf = new ApplicationServerConfig {
                AppPath = app.Path,
                Server = "TEST2",
                ServerFqdnOrIp = "test2.ad.com",
                Bindings = new[] { "*:80:", "127.0.0.1:80:", ":80:www.test.com" },
                AppType = ApplicationServerConfig.WinSvcType,
                ServiceName = "Test.Service",
                DisplayName = "Test Service Display"
            };
            await conf.AddOrUpdateAppServerConfigAsync(appconf);

            // give it 1s to swallow
            await Task.Delay(2000);

            var dbconf = (await conf.GetAppConfigsAsync(new[] { app.Path })).FirstOrDefault();
            Assert.NotNull(dbconf);
            Assert.Equal(appconf.AppPath, dbconf.AppPath);
            Assert.Equal(appconf.AppPoolName, dbconf.AppPoolName);
            Assert.Equal(appconf.Server, dbconf.Server);
            Assert.Equal(appconf.ServerFqdnOrIp, dbconf.ServerFqdnOrIp);
            Assert.Contains(appconf.Bindings[0], dbconf.Bindings);
            Assert.Contains(appconf.Bindings[1], dbconf.Bindings);
            Assert.Contains(appconf.Bindings[2], dbconf.Bindings);
            Assert.Equal(appconf.AppType, dbconf.AppType);
            Assert.Equal(appconf.ServiceName, dbconf.ServiceName);
            Assert.Equal(appconf.DisplayName, dbconf.DisplayName);

            Assert.True(app.IsExcluded);
        }

        [Fact]
        public async Task TestGlobals()
        {
            var conf = new ElasticSearchAppConfigurationManager();

            var v = await conf.GetGlobalSettingAsync(confkey);
            Assert.Null(v);

            v = "testvalue";
            await conf.SetGlobalSettingAsync(confkey, v);
            var v2 = await conf.GetGlobalSettingAsync(confkey);
            Assert.Equal(v, v2);
            v2 = conf.GetGlobalSetting(confkey);
            Assert.Equal(v, v2);
            await conf.SetGlobalSettingAsync(confkey, null);
            v2 = await conf.GetGlobalSettingAsync(confkey);
            Assert.Null(v2);
        }

        public void Dispose()
        {
            var client = ElasticSearchClientConfiguration.CreateClient("lldconf");
            client.DeleteByQuery<ElasticApplicationConfig>(
                d => d.Query(q => q.Term(t => t.Field(conf => conf.Path).Value(path))));
            client.DeleteByQuery<ElasticApplication>(
                d => d.Query(q => q.Term(t => t.Field(conf => conf.Path).Value(path))));
        }
    }
}
