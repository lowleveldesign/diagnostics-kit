using FluentValidation;
using LowLevelDesign.Diagnostics.Commons.Config;
using LowLevelDesign.Diagnostics.Commons.Models;
using LowLevelDesign.Diagnostics.Commons.Storage;
using Nancy;
using Nancy.ModelBinding;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LowLevelDesign.Diagnostics.Castle.Modules
{
    public class ApplicationConfModule : NancyModule
    {
        public ApplicationConfModule(IAppConfigurationManager appconf, ILogStore logStore, IValidator<Application> appvalidator,
            IValidator<ApplicationServerConfig> appconfvalidator)
        {
            Post["conf/appname", true] = async (x, ct) => {
                return await UpdateAppPropertyAsync(appconf, appvalidator, this.Bind<Application>(), "Name");
            };
            Post["conf/appmaintenance", true] = async (x, ct) => {
                return await UpdateAppPropertyAsync(appconf, appvalidator, this.Bind<Application>(), "DaysToKeepLogs");
            };
            Post["conf/appexclusion", true] = async (x, ct) => {
                return await UpdateAppPropertyAsync(appconf, appvalidator, this.Bind<Application>(), "IsExcluded");
            };
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
                    if (app != null && !app.IsExcluded) {
                        apppaths.Add(conf.AppPath);
                    }
                    await appconf.AddOrUpdateAppServerConfigAsync(conf);
                }
                return apppaths;
            };
            Get["conf/appsrvconfig/{apppath?}", true] = async (x, ct) => {
                IEnumerable<Application> apps;
                if (x.apppath != null) {
                    apps = new[] { await appconf.FindAppAsync(Application.GetPathFromBase64Key((String)x.apppath)) };
                } else {
                    // get all available applications
                    apps = await appconf.GetAppsAsync();
                }
                // and send back their configuration
                return await appconf.GetAppConfigsAsync(apps.Select(app => app.Path).ToArray());
            };
        }

        private static async Task<String> UpdateAppPropertyAsync(IAppConfigurationManager appconf,
            IValidator<Application> validator, Application app, String property)
        {
            var validationResult = validator.Validate(app, "Hash", property);
            if (!validationResult.IsValid) {
                return "ERR_INVALID";
            }
            await appconf.UpdateAppPropertiesAsync(app, new[] { property });

            return "OK";
        }
    }
}