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
using System;
using System.Collections.Generic;

namespace LowLevelDesign.Diagnostics.Castle.Modules
{
    public sealed class CollectModule : NancyModule
    {
        private static readonly JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings {
            NullValueHandling = NullValueHandling.Ignore
        };

        public CollectModule(IAppConfigurationManager config, IValidator<LogRecord> logRecordValidator, ILogStore logStore)
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
                    await logStore.AddLogRecordAsync(logrec);
                    await logStore.UpdateApplicationStatusAsync(
                        CreateApplicationStatusFromLogRecord(logrec));
                }
                return "OK";
            };
            Post["/collectall", true] = async (x, ct) => {
                var logrecs = this.Bind<LogRecord[]>(new BindingConfig { BodyOnly = true });

                var logsToSave = new List<LogRecord>(logrecs.Length);
                var appStatuses = new List<LastApplicationStatus>(logrecs.Length);
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
                            appStatuses.Add(CreateApplicationStatusFromLogRecord(logrec));
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

                if (logsToSave.Count > 0) {
                    await logStore.AddLogRecordsAsync(logsToSave);
                    await logStore.UpdateApplicationStatusesAsync(appStatuses);
                }

                return "OK";
            };
        }

        private LastApplicationStatus CreateApplicationStatusFromLogRecord(LogRecord logrec)
        {
            var appStatus = new LastApplicationStatus {
                ApplicationPath = logrec.ApplicationPath,
                Server = logrec.Server,
                LastUpdateTimeUtc = DateTime.UtcNow
            };

            if (logrec.LogLevel >= LogRecord.ELogLevel.Error) {
                appStatus.LastErrorTimeUtc = logrec.TimeUtc;
                appStatus.LastErrorType = logrec.ExceptionType;
            }

            if (logrec.PerformanceData != null && logrec.PerformanceData.Count > 0) {
                appStatus.LastPerformanceDataUpdateTimeUtc = DateTime.UtcNow;
                float v;
                if (logrec.PerformanceData.TryGetValue("CPU", out v)) {
                    appStatus.Cpu = v;
                }
                if (logrec.PerformanceData.TryGetValue("Memory", out v)) {
                    appStatus.Memory = v;
                }
            }

            return appStatus;
        }
    }
}