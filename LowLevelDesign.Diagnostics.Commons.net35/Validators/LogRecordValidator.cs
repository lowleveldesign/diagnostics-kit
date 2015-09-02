using FluentValidation;
using LowLevelDesign.Diagnostics.Commons.Models;
using System;
using System.Collections.Generic;

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
                RuleFor(kvp => kvp.Key).NotEmpty().Length(1, 100);
            }
        }

        public LogRecordValidator() {
            RuleFor(r => r.LoggerName).NotEmpty().Length(1, 200);
            RuleFor(r => r.Message).Length(0, 7000);
            RuleFor(r => r.Server).NotEmpty().Length(1, 200);
            RuleFor(r => r.Identity).Length(0, 200);
            RuleFor(r => r.TimeUtc).NotEmpty();
            RuleFor(r => r.ApplicationPath).NotEmpty().Length(0, 2000);
            RuleFor(r => r.CorrelationId).Length(0, 1024);

            RuleFor(r => r.ExceptionType).Length(0, 100);
            RuleFor(r => r.ExceptionMessage).Length(0, 2000);
            RuleFor(r => r.ExceptionAdditionalInfo).Length(0, 5000);

            RuleForEach(r => r.AdditionalFields).SetValidator(new AdditionalFieldValidator());
            RuleForEach(r => r.PerformanceData).SetValidator(new PerformanceDataValidator());
        }
    }

}
