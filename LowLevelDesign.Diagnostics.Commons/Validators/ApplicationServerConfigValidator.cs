using FluentValidation;
using LowLevelDesign.Diagnostics.Commons.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LowLevelDesign.Diagnostics.Commons.Validators
{
    public sealed class ApplicationServerConfigValidator : AbstractValidator<ApplicationServerConfig>
    {
        public ApplicationServerConfigValidator()
        {
            RuleFor(c => c.AppPath).NotEmpty().Length(1, 2000);
            RuleFor(c => c.AppType).NotEmpty().Must(t => String.Equals(t, ApplicationServerConfig.WebAppType, StringComparison.OrdinalIgnoreCase) ||
                String.Equals(t, ApplicationServerConfig.WinSvcType, StringComparison.OrdinalIgnoreCase)).WithMessage("Invalid application type.");
            RuleFor(c => c.Server).NotEmpty().Length(1, 200);
            RuleFor(c => c.AppPoolName).Length(0, 500);
            When(c => c.Bindings != null, () => RuleFor(c => c.Bindings).Must(bindings => {
                    // number of chars is limited
                    int cnt = bindings.Length;
                    foreach (var b in bindings) {
                        cnt += b.Length;
                    }
                    return cnt <= 3000;
                }).WithMessage("Too many or too long bindings defined."));
            RuleFor(c => c.ServiceName).Length(0, 300);
            RuleFor(c => c.DisplayName).Length(0, 500);
        }
    }
}
