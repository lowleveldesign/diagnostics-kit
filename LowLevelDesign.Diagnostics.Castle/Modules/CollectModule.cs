using System;
using System.Collections.Generic;
using FluentValidation;
using LowLevelDesign.Diagnostics.Commons.Models;
using LowLevelDesign.Diagnostics.Commons.Storage;
using Nancy;
using Nancy.ModelBinding;
using Newtonsoft.Json;
using NLog;

namespace LowLevelDesign.Diagnostics.Castle.Modules
{
    public sealed class CollectModule : NancyModule
    {
        private static readonly JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore
        };
        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        public CollectModule(ILogStore logstore, IValidator<LogRecord> logrecValidator) {
            Post["/collect"] = _ => {
                var logrec = this.Bind<LogRecord>(new BindingConfig { BodyOnly = true });
                var validationResult = logrecValidator.Validate(logrec);
                if (!validationResult.IsValid) {
                    return "VALIDATION ERROR";
                }

                // FIXME add task to the tasks queue, for now we do it synchronously
                logstore.AddLogRecord(logrec);

                return "OK";
            };
            Post["/collectall"] = _ =>
            {
                var logrecs = this.Bind<LogRecord[]>(new BindingConfig { BodyOnly = true });

                var logsToSave = new List<LogRecord>(logrecs.Length);
                foreach (var logrec in logrecs)
                {
                    var validationResult = logrecValidator.Validate(logrec);
                    if (validationResult.IsValid)
                    {
                        logsToSave.Add(logrec);
                    }
                    else
                    {
                        if (logger.IsWarnEnabled)
                        {
                            logger.Warn("Validation error(s) occured when saving a logrecord: {0}, errors: {1}",
                                JsonConvert.SerializeObject(logrec, Formatting.Indented, jsonSerializerSettings), String.Join(";", 
                                validationResult.Errors));
                        }
                    }
                }

                logstore.AddLogRecords(logsToSave);

                return logsToSave.Count == logrecs.Length ? "OK" : "OK, BUT VALIDATION ERRORS OCCURED";
            };
        }
    }
}