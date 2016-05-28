using FluentValidation;
using LowLevelDesign.Diagnostics.Commons.Models;
using System.Collections.Generic;

namespace LowLevelDesign.Diagnostics.Commons.Validators
{
    public class LogRecordValidator : AbstractValidator<LogRecord>
    {
        public class AdditionalFieldValidator : AbstractValidator<KeyValuePair<string, object>>
        {
            public AdditionalFieldValidator() {
                RuleFor(kvp => kvp.Key).NotEmpty().Length(1, Constraints.MaxAdditionalFieldKeyLength);
                RuleFor(kvp => kvp.Value).NotNull().Must(v => {
                    if (v is string) {
                        return ((string)v).Length < Constraints.MaxAdditionalFieldValueLength;
                    }
                    return true;
                });
            }
        }

        public class PerformanceDataValidator : AbstractValidator<KeyValuePair<string, float>>
        {
            public PerformanceDataValidator() {
                RuleFor(kvp => kvp.Key).NotEmpty().Length(1, Constraints.MaxPerformanceDataKeyLength);
            }
        }

        public LogRecordValidator() {
            RuleFor(r => r.LoggerName).NotEmpty().Length(1, Constraints.MaxLoggerNameLength);
            RuleFor(r => r.Message).Length(0, Constraints.MaxMessageLength);
            RuleFor(r => r.Server).NotEmpty().Length(1, Constraints.MaxServerNameLength);
            RuleFor(r => r.Identity).Length(0, Constraints.MaxIdentityLength    );
            RuleFor(r => r.TimeUtc).NotEmpty();
            RuleFor(r => r.ApplicationPath).NotEmpty().Length(0, Constraints.MaxApplicationPathLength);
            RuleFor(r => r.CorrelationId).Length(0, Constraints.MaxCorrelationIdLength);

            RuleFor(r => r.ExceptionType).Length(0, Constraints.MaxExceptionTypeLength);
            RuleFor(r => r.ExceptionMessage).Length(0, Constraints.MaxExceptionMessageLength);
            RuleFor(r => r.ExceptionAdditionalInfo).Length(0, Constraints.MaxExceptionAdditionalInfoLength);

            RuleForEach(r => r.AdditionalFields).SetValidator(new AdditionalFieldValidator());
            RuleForEach(r => r.PerformanceData).SetValidator(new PerformanceDataValidator());
        }
    }

}
