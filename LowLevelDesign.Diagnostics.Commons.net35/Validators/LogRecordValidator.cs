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
using System;
using System.Collections.Generic;

namespace LowLevelDesign.Diagnostics.Commons.Validators
{
    public class LogRecordValidator : AbstractValidator<LogRecord>
    {
        public class AdditionalFieldValidator : AbstractValidator<KeyValuePair<String, Object>>
        {
            public AdditionalFieldValidator()
            {
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
            public PerformanceDataValidator()
            {
                RuleFor(kvp => kvp.Key).NotEmpty().Length(1, 100);
            }
        }

        public LogRecordValidator()
        {
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

            RuleFor(r => r.AdditionalFields).Must(f => {
                var vld = new AdditionalFieldValidator();
                foreach (var k in f) {
                    if (!vld.Validate(k).IsValid) {
                        return false;
                    }
                }
                return true;
            });
            RuleFor(r => r.PerformanceData).Must(f => {
                var vld = new PerformanceDataValidator();
                foreach (var k in f) {
                    if (!vld.Validate(k).IsValid) {
                        return false;
                    }
                }
                return true;
            });
        }
    }

}
