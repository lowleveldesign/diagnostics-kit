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
