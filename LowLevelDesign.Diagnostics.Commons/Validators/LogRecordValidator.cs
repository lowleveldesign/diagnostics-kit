using FluentValidation;
using LowLevelDesign.Diagnostics.Commons.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LowLevelDesign.Diagnostics.Commons.Validators
{
    public class LogRecordValidator : AbstractValidator<LogRecord>
    {
        public class AdditionalFieldValidator : AbstractValidator<KeyValuePair<String, Object>>
        {
            public AdditionalFieldValidator() {
                RuleFor(kvp => kvp.Key).NotEmpty().Length(1, 256);
                RuleFor(kvp => kvp.Value).NotNull().Must(v => {
                    if (v is String) {
                        return ((String)v).Length < 5000;
                    }
                    return true;
                });
            }
        }

        public class PerformanceDataValidator : AbstractValidator<KeyValuePair<String, float>>
        {
            public PerformanceDataValidator() {
                RuleFor(kvp => kvp.Key).NotEmpty().Length(1, 256);
            }
        }

        public LogRecordValidator() {
            RuleFor(r => r.LoggerName).NotEmpty().Length(1, 1024);
            RuleFor(r => r.LogLevel).NotEmpty().Must(lvl => new[] { "TRACE", "DEBUG", "INFO", "WARN", "ERROR", "CRITICAL" }.Contains(lvl,
                StringComparer.OrdinalIgnoreCase));
            RuleFor(r => r.Message).Length(0, 5000);
            RuleFor(r => r.Server).NotEmpty().Length(1, 1024);
            RuleFor(r => r.ThreadIdentity).Length(0, 1024);
            RuleFor(r => r.TimeUtc).NotEmpty();
            RuleFor(r => r.ApplicationPath).Length(0, 2048);
            RuleFor(r => r.CorrelationId).Length(0, 1024);

            RuleForEach(r => r.AdditionalFields).SetValidator(new AdditionalFieldValidator());
            RuleForEach(r => r.PerformanceData).SetValidator(new PerformanceDataValidator());
        }
    }

}
