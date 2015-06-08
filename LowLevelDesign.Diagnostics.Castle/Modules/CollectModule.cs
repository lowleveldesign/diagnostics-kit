using System;
using System.Collections.Generic;
using FluentValidation;
using LowLevelDesign.Diagnostics.Commons.Models;
using LowLevelDesign.Diagnostics.Commons.Storage;
using Nancy;
using Nancy.ModelBinding;
using Newtonsoft.Json;
using NLog;
using LowLevelDesign.Diagnostics.Castle.Models;
using LowLevelDesign.Diagnostics.Commons;
using LowLevelDesign.Diagnostics.Commons.Config;

namespace LowLevelDesign.Diagnostics.Castle.Modules
{
    public sealed class CollectModule : NancyModule
    {
        private static readonly JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings {
            NullValueHandling = NullValueHandling.Ignore
        };
        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        public CollectModule(IAppConfigurationManager config, ILogStore logstore, IValidator<LogRecord> logrecValidator)
        {
            Post["/collect", true] = async (x, ct) => {
                var logrec = this.Bind<LogRecord>(new BindingConfig { BodyOnly = true });
                var validationResult = logrecValidator.Validate(logrec);
                if (!validationResult.IsValid) {
                    return "VALIDATION ERROR";
                }

                // add new application to the configuration as excluded (it could be later renamed or unexcluded)
                if (await config.FindAppAsync(logrec.ApplicationPath) == null) {
                    await config.AddOrUpdateAppAsync(new Application { IsExcluded = true, Path = logrec.ApplicationPath });
                }

                // FIXME add task to the tasks queue, for now we do it synchronously
                await logstore.AddLogRecord(logrec);

                return "OK";
            };
            Post["/collectall", true] = async (x, ct) => {
                var logrecs = this.Bind<LogRecord[]>(new BindingConfig { BodyOnly = true });

                var logsToSave = new List<LogRecord>(logrecs.Length);
                foreach (var logrec in logrecs) {
                    var validationResult = logrecValidator.Validate(logrec);
                    if (validationResult.IsValid) {
                        // add new application to the configuration as excluded (it could be later renamed or unexcluded)
                        if (await config.FindAppAsync(logrec.ApplicationPath) == null) {
                            await config.AddOrUpdateAppAsync(new Application { IsExcluded = true, Path = logrec.ApplicationPath });
                        }

                        logsToSave.Add(logrec);
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

                return logsToSave.Count == logrecs.Length ? "OK" : "OK, BUT VALIDATION ERRORS OCCURED";
            };
        }
    }
}