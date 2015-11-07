using FluentValidation;
using LowLevelDesign.Diagnostics.Commons.Models;
using LowLevelDesign.Diagnostics.LogStore.Commons.Config;
using LowLevelDesign.Diagnostics.LogStore.Commons.Models;
using LowLevelDesign.Diagnostics.LogStore.Commons.Storage;
using Nancy;
using Nancy.ModelBinding;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace LowLevelDesign.Diagnostics.Castle.Modules
{
    public sealed class CollectModule : NancyModule
    {
        private static readonly TraceSource logger = new TraceSource("LowLevelDesign.Diagnostics.Castle");

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
                logrec.ApplicationPath = logrec.ApplicationPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
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
                        CreateApplicationStatus(logrec));
                }
                return "OK";
            };
            Post["/collectall", true] = async (x, ct) => {
                var logrecs = this.Bind<LogRecord[]>(new BindingConfig { BodyOnly = true });

                var logsToSave = new List<LogRecord>(logrecs.Length);
                foreach (var logrec in logrecs) {
                    var validationResult = logRecordValidator.Validate(logrec);
                    if (validationResult.IsValid) {
                        logrec.ApplicationPath = logrec.ApplicationPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                        // add new application to the configuration as excluded (it could be later renamed or unexcluded)
                        var app = await config.FindAppAsync(logrec.ApplicationPath);
                        if (app == null) {
                            app = new Application {
                                IsExcluded = true,
                                Path = logrec.ApplicationPath
                            };
                            await config.AddOrUpdateAppAsync(app);
                        }
                        if (!app.IsExcluded) {
                            logsToSave.Add(logrec);
                        } else {
                            logger.TraceEvent(TraceEventType.Verbose, 0, "Log record for the application '{0}' was not stored as the application is excluded.");
                        }
                    } else {
                        if (logger.Switch.ShouldTrace(TraceEventType.Warning)) {
                            logger.TraceEvent(TraceEventType.Warning, 0, "Validation error(s) occured when saving a logrecord: {0}, errors: {1}",
                                logrec, validationResult.Errors);
                        }
                    }
                }

                if (logsToSave.Count > 0) {
                    await logStore.AddLogRecordsAsync(logsToSave);
                    await logStore.UpdateApplicationStatusesAsync(CreateApplicationStatusesList(logsToSave));
                }

                return "OK";
            };
        }

        private IEnumerable<LastApplicationStatus> CreateApplicationStatusesList(IEnumerable<LogRecord> logrecs)
        {
            var appStatuses = new Dictionary<string, LastApplicationStatus>();
            foreach (var logrec in logrecs) {
                LastApplicationStatus alreadyProcessedStatus;
                if (appStatuses.TryGetValue(logrec.ApplicationPath, out alreadyProcessedStatus)) {
                    if (logrec.TimeUtc > alreadyProcessedStatus.LastUpdateTimeUtc) {
                        appStatuses[logrec.ApplicationPath] = CreateApplicationStatus(logrec);
                    }
                }
                else {
                    appStatuses.Add(logrec.ApplicationPath, CreateApplicationStatus(logrec));
                }
            }
            return appStatuses.Values;
        }

        private LastApplicationStatus CreateApplicationStatus(LogRecord logrec)
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