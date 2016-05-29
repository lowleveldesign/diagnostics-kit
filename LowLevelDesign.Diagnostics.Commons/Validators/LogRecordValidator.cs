/**
 *  Part of the Diagnostics Kit
 *
 *  Copyright (C) 2016  Sebastian Solnica
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.

 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 */

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
