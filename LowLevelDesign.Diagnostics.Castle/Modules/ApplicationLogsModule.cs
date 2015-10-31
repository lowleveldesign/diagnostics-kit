using LowLevelDesign.Diagnostics.Castle.Config;
using LowLevelDesign.Diagnostics.Castle.Models;
using LowLevelDesign.Diagnostics.Castle.Search;
using LowLevelDesign.Diagnostics.Commons.Models;
using LowLevelDesign.Diagnostics.LogStore.Commons.Config;
using LowLevelDesign.Diagnostics.LogStore.Commons.Models;
using LowLevelDesign.Diagnostics.LogStore.Commons.Storage;
using Nancy;
using Nancy.ModelBinding;
using Nancy.Security;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace LowLevelDesign.Diagnostics.Castle.Modules
{
    public sealed class ApplicationLogsModule : NancyModule
    {
        private const int MaxLogsCount = 35;

        public ApplicationLogsModule(GlobalConfig globals, ILogStore logStore, IAppConfigurationManager config)
        {
            if (globals.IsAuthenticationEnabled())
            {
                this.RequiresAuthentication();
            }

            Get["/logs/{apppath}/{server?}", true] = async (x, ct) => {
                var app = await config.FindAppAsync(Application.GetPathFromBase64Key((String)x.apppath));
                var model = this.Bind<ApplicationLogFilterModel>();

                model.dfrom = model.dfrom.HasValue ? model.dfrom.Value : DateTime.Now.Subtract(TimeSpan.FromHours(10));
                model.dto = model.dto.HasValue ? model.dto.Value : DateTime.Now;
                model.apppath = app.Path;
                model.appname = app.Name;
                model.lfrom = model.lfrom.HasValue ? model.lfrom.Value : (short)LogRecord.ELogLevel.Info;
                model.lto = model.lto.HasValue ? model.lto.Value : (short)LogRecord.ELogLevel.Critical;
                model.server = x.server;

                ViewBag.Logs = await FilterLogsAsync(logStore, model, model.off);

                return View["ApplicationLogs.cshtml", model];
            };
        }

        private static async Task<ApplicationLogSearchResults> FilterLogsAsync(ILogStore logStore, ApplicationLogFilterModel filter, int offset)
        {
            Debug.Assert(filter.dfrom.HasValue);
            Debug.Assert(filter.dto.HasValue);

            var levels = new List<LogRecord.ELogLevel>();
            for (short lvl = filter.lfrom ?? 0; lvl <= filter.lto; lvl++) {
                levels.Add((LogRecord.ELogLevel)lvl);
            }

            var searchResults = await logStore.FilterLogsAsync(new LogSearchCriteria {
                ApplicationPath = filter.apppath,
                Server = filter.server,
                FromUtc = filter.dfrom.Value.ToUniversalTime(),
                ToUtc = filter.dto.Value.ToUniversalTime(),
                Logger = filter.logger,
                Keywords = KeywordsParser.Parse(filter.keywords),
                Levels = levels.ToArray(),
                Limit = MaxLogsCount + 1,
                Offset = offset
            });

            var foundItems = searchResults.FoundItems.ToArray();
            LogRecord[] finalResults;
            if (foundItems.Length < MaxLogsCount + 1) {
                finalResults = foundItems;
            } else {
                finalResults = new LogRecord[foundItems.Length - 1];
                Array.Copy(foundItems, finalResults, finalResults.Length);
            }

            return new ApplicationLogSearchResults {
                FoundItems = finalResults,
                Limit = MaxLogsCount,
                Offset = offset,
                IsLastPage = foundItems.Length < MaxLogsCount + 1
            };
        }
    }
}