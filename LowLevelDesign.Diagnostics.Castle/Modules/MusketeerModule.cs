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

using FluentValidation;
using LowLevelDesign.Diagnostics.Commons.Models;
using LowLevelDesign.Diagnostics.LogStore.Commons.Config;
using LowLevelDesign.Diagnostics.LogStore.Commons.Models;
using LowLevelDesign.Diagnostics.LogStore.Commons.Storage;
using Nancy;
using Nancy.ModelBinding;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace LowLevelDesign.Diagnostics.Castle.Modules
{
    public class MusketeerModule : NancyModule
    {
        private static readonly TraceSource logger = new TraceSource("LowLevelDesign.Diagnostics.Castle");

        public MusketeerModule(IAppConfigurationManager appconf, ILogStore logStore,
            IValidator<Application> appvalidator, IValidator<ApplicationServerConfig> appconfvalidator)
        {
            Post["conf/appsrvconfig", true] = async (x, ct) => {
                var configs = this.Bind<ApplicationServerConfig[]>();
                var apppaths = new List<String>();
                foreach (var conf in configs) {
                    var validationResult = appconfvalidator.Validate(conf);
                    if (!validationResult.IsValid) {
                        logger.TraceEvent(TraceEventType.Error, 0, "Validation failed for config {@0}, errors: {1}", conf, validationResult.Errors);
                        continue;
                    }
                    var app = await appconf.FindAppAsync(conf.AppPath);
                    if (app == null) {
                        app = new Application {
                            IsExcluded = true,
                            Path = conf.AppPath
                        }; 
                        await appconf.AddOrUpdateAppAsync(app);
                    } else if (app != null && !app.IsExcluded) {
                        apppaths.Add(conf.AppPath);
                    }
                    await appconf.AddOrUpdateAppServerConfigAsync(conf);
                }
                return Response.AsJson(apppaths);
            };
        }
    }
}
