using FluentValidation;
using LowLevelDesign.Diagnostics.Castle.Logs;
using LowLevelDesign.Diagnostics.Commons.Config;
using LowLevelDesign.Diagnostics.Commons.Models;
using LowLevelDesign.Diagnostics.Commons.Storage;
using Nancy;
using Nancy.ModelBinding;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;

namespace LowLevelDesign.Diagnostics.Castle.Modules
{
    public sealed class CollectModule : NancyModule
    {
        private static readonly JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings {
            NullValueHandling = NullValueHandling.Ignore
        };
        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        public CollectModule(IAppConfigurationManager config, IValidator<LogRecord> logrecValidator, ILogStore logstore)
        {
            Post["/collect", true] = async (x, ct) => {
                var logrec = this.Bind<LogRecord>(new BindingConfig { BodyOnly = true });
                var validationResult = logrecValidator.Validate(logrec);
                if (!validationResult.IsValid) {
                    return "VALIDATION ERROR";
                }

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
                    // FIXME add task to the tasks queue, for now we do it synchronously
                    await logstore.AddLogRecord(logrec);
                } else {
                    logger.Debug("Log record for the application '{0}' was not stored as the application is excluded.");
                }

                return "OK";
            };
            Post["/collectall", true] = async (x, ct) => {
                var logrecs = this.Bind<LogRecord[]>(new BindingConfig { BodyOnly = true });

                var logsToSave = new List<LogRecord>(logrecs.Length);
                foreach (var logrec in logrecs) {
                    var validationResult = logrecValidator.Validate(logrec);
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
                            logger.Debug("Log record for the application '{0}' was not stored as the application is excluded.");
                        }
                    } else {
                        if (logger.IsWarnEnabled) {
                            logger.Warn("Validation error(s) occured when saving a logrecord: {0}, errors: {1}",
                                JsonConvert.SerializeObject(logrec, Formatting.Indented, jsonSerializerSettings), String.Join(";",
                                validationResult.Errors));
                        }
                    }
                }

                // FIXME add task to the tasks queue, for now we do it synchronously
                await logstore.AddLogRecords(logsToSave);

                return "OK";
            };
        }
    }
}