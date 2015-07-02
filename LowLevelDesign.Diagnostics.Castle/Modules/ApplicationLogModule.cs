using LowLevelDesign.Diagnostics.Castle.Models;
using LowLevelDesign.Diagnostics.Commons.Config;
using LowLevelDesign.Diagnostics.Commons.Models;
using LowLevelDesign.Diagnostics.Commons.Storage;
using Nancy;
using Nancy.ModelBinding;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LowLevelDesign.Diagnostics.Castle.Modules
{
    public sealed class ApplicationLogModule : NancyModule
    {
        private const int MaxLogsCount = 100;

        public ApplicationLogModule(ILogStore logStore, IAppConfigurationManager config)
        {
            Get["/logs/{apppath}/{server?}", true] = async (x, ct) => {
                var app = await config.FindAppAsync(Application.GetPathFromBase64Key((String)x.apppath));
                DateTime dateFrom = DateTime.Now.Subtract(TimeSpan.FromHours(10));
                DateTime dateTo = DateTime.Now;

                var model = new ApplicationLogFilterModel {
                    ApplicationPath = app.Path,
                    ApplicationName = app.Name,
                    LevelFrom = (short)LogRecord.ELogLevel.Info,
                    LevelTo = (short)LogRecord.ELogLevel.Critical,
                    DateFrom = dateFrom,
                    DateTo = dateTo,
                    Server = x.server
                };

                ViewBag.Logs = await FilterLogsAsync(logStore, model, 0);

                return View["ApplicationLog.cshtml", model];
            };
            Post["/logs/{apppath}/{server?}", true] = async (x, ct) => {
                // FIXME validation
                var model = this.Bind<ApplicationLogFilterModel>();



                return View["ApplicationLog.cshtml", model];
            };
        }

        private static async Task<ApplicationLogSearchResults> FilterLogsAsync(ILogStore logStore, ApplicationLogFilterModel filter, int offset)
        {
            var levels = new List<LogRecord.ELogLevel>();
            for (short lvl = filter.LevelFrom; lvl <= filter.LevelTo; lvl++) {
                levels.Add((LogRecord.ELogLevel)lvl);
            }

            var searchResults = await logStore.FilterLogs(new LogSearchCriteria {
                ApplicationPath = filter.ApplicationPath,
                Server = filter.Server,
                FromUtc = filter.DateFrom.ToUniversalTime(),
                ToUtc = filter.DateTo.ToUniversalTime(),
                Logger = filter.Logger,
                Keywords = filter.Keywords,
                Levels = levels.ToArray(),
                Limit = MaxLogsCount,
                Offset = offset
            });

            return new ApplicationLogSearchResults {
                FoundItems = searchResults.FoundItems.ToArray(),
                Limit = MaxLogsCount,
                Offset = offset
            };
        }
    }
}