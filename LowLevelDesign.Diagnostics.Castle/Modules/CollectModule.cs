using FluentValidation;
using LowLevelDesign.Diagnostics.Commons.Models;
using LowLevelDesign.Diagnostics.Commons.Storage;
using Nancy;
using Nancy.ModelBinding;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LowLevelDesign.Diagnostics.Castle.Modules
{
    public class CollectModule : NancyModule
    {
        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        public CollectModule(ILogStore logstore, IValidator<LogRecord> logrecValidator) {
            Post["/collect"] = _ => {
                var logrec = this.Bind<LogRecord>(new BindingConfig { BodyOnly = true });
                var validationResult = logrecValidator.Validate(logrec);
                if (!validationResult.IsValid) {
                    // FIXME log warning and errors that were detected
                    return "FAIL";
                }

                // FIXME add task to the tasks queue, for now we do it synchronously
                logstore.AddLogRecord(logrec);

                return "OK";
            };
        }
    }
}