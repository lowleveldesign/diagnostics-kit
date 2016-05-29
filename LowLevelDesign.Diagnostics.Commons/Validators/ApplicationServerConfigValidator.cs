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

namespace LowLevelDesign.Diagnostics.Commons.Validators
{
    public sealed class ApplicationServerConfigValidator : AbstractValidator<ApplicationServerConfig>
    {
        public ApplicationServerConfigValidator()
        {
            RuleFor(c => c.AppPath).NotEmpty().Length(1, Constraints.MaxApplicationPathLength);
            RuleFor(c => c.AppType).NotEmpty().Must(t => string.Equals(t, ApplicationServerConfig.WebAppType, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(t, ApplicationServerConfig.WinSvcType, StringComparison.OrdinalIgnoreCase)).WithMessage("Invalid application type.");
            RuleFor(c => c.Server).NotEmpty().Length(1, Constraints.MaxServerNameLength);
            RuleFor(c => c.ServerFqdnOrIp).NotEmpty().Length(1, Constraints.MaxServerFqdnOrIpLength);
            RuleFor(c => c.AppPoolName).Length(0, Constraints.MaxAppPoolNameLength);
            When(c => c.Bindings != null, () => RuleFor(c => c.Bindings).Must(bindings => {
                    // number of chars is limited
                    int cnt = bindings.Length;
                    foreach (var b in bindings) {
                        cnt += b.Length;
                    }
                    return cnt <= Constraints.MaxBindingLength;
                }).WithMessage("Too many or too long bindings defined."));
            RuleFor(c => c.ServiceName).Length(0, Constraints.MaxServiceNameLength);
            RuleFor(c => c.DisplayName).Length(0, Constraints.MaxDisplayNameLength);
        }
    }
}
