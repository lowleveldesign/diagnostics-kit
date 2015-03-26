using FluentValidation;
using LowLevelDesign.Diagnostics.Commons.Models;
using LowLevelDesign.Diagnostics.Commons.Storage;
using Nancy;
using Nancy.ModelBinding;
using NLog;

namespace LowLevelDesign.Diagnostics.Castle.Modules
{
    public sealed class CollectModule : NancyModule
    {
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
        }
    }
}