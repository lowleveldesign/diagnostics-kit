using FluentValidation;
using LowLevelDesign.Diagnostics.Commons.Models;
using LowLevelDesign.Diagnostics.LogStore.Commons.Config;
using LowLevelDesign.Diagnostics.LogStore.Commons.Models;
using LowLevelDesign.Diagnostics.LogStore.Commons.Storage;
using Nancy;
using Nancy.ModelBinding;
using Serilog;
using System;
using System.Collections.Generic;

namespace LowLevelDesign.Diagnostics.Castle.Modules
{
    public class MusketeerModule : NancyModule
    {
        public MusketeerModule(IAppConfigurationManager appconf, ILogStore logStore,
            IValidator<Application> appvalidator, IValidator<ApplicationServerConfig> appconfvalidator)
        {
            Post["conf/appsrvconfig", true] = async (x, ct) => {
                var configs = this.Bind<ApplicationServerConfig[]>();
                var apppaths = new List<String>();
                foreach (var conf in configs) {
                    var validationResult = appconfvalidator.Validate(conf);
                    if (!validationResult.IsValid) {
                        Log.Error("Validation failed for config {@0}, errors: {1}", conf, validationResult.Errors);
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