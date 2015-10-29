using FluentValidation;
using LowLevelDesign.Diagnostics.Commons.Models;
using LowLevelDesign.Diagnostics.LogStore.Commons.Config;
using LowLevelDesign.Diagnostics.LogStore.Commons.Models;
using LowLevelDesign.Diagnostics.LogStore.Commons.Storage;
using Nancy;
using Nancy.ModelBinding;
using Newtonsoft.Json;
using Serilog;
using Serilog.Events;
using System.Collections.Generic;

namespace LowLevelDesign.Diagnostics.Castle.Modules
{
    public sealed class CollectModule : NancyModule
    {
        private static readonly JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings {
            NullValueHandling = NullValueHandling.Ignore
        };

        public CollectModule(IAppConfigurationManager config, IValidator<LogRecord> logRecordValidator, ILogStore logstore)
        {
            Post["/collect", true] = async (x, ct) => {
                var logrec = this.Bind<LogRecord>(new BindingConfig { BodyOnly = true });
                var validationResult = logRecordValidator.Validate(logrec);
                if (!validationResult.IsValid) {
                    return "VALIDATION ERROR";
                }

                var app = await config.FindAppAsync(logrec.ApplicationPath);
                if (app == null) {
                    app = new Application {
                        IsExcluded = true,
                        Path = logrec.ApplicationPath
                    };
                    await config.AddOrUpdateAppAsync(app);
                }

                if (!app.IsExcluded) {
                    await logstore.AddLogRecord(logrec);
                }
                return "OK";
            };
            Post["/collectall", true] = async (x, ct) => {
                var logrecs = this.Bind<LogRecord[]>(new BindingConfig { BodyOnly = true });

                var logsToSave = new List<LogRecord>(logrecs.Length);
                foreach (var logrec in logrecs) {
                    var validationResult = logRecordValidator.Validate(logrec);
                    if (validationResult.IsValid) {
                        // add new application to the configuration as excluded (it could be later renamed or unexcluded)
                        var app = await config.FindAppAsync(logrec.ApplicationPath);
                        if (app == null) {
                            app = new Application {
                                IsExcluded = true,
                                Path = logrec.ApplicationPath
                            };
                            await config.AddOrUpdateAppAsync(app);
                        }

                        // we should collect logs only for applications which are not excluded
                        if (!app.IsExcluded) {
                            logsToSave.Add(logrec);
                        } else {
                            Log.Debug("Log record for the application '{0}' was not stored as the application is excluded.");
                        }
                    } else {
                        if (Log.IsEnabled(LogEventLevel.Warning)) {
                            Log.Warning("Validation error(s) occured when saving a logrecord: {0}, errors: {1}",
                                logrec, validationResult.Errors);
                        }
                    }
                }
                await logstore.AddLogRecords(logsToSave);

                return "OK";
            };
        }
    }
}