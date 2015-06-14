using FluentValidation;
using LowLevelDesign.Diagnostics.Commons.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LowLevelDesign.Diagnostics.Commons.Validators
{
    public class ApplicationValidator : AbstractValidator<Application>
    {
        public ApplicationValidator()
        {
            RuleFor(app => app.Name).NotNull().Length(1, 500);
            RuleFor(app => app.Path).NotNull().Length(1, 2000);
        }
    }
}
