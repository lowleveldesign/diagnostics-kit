using FluentValidation;
using LowLevelDesign.Diagnostics.LogStore.Commons.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LowLevelDesign.Diagnostics.LogStore.Commons.Validators
{
    public sealed class UserValidator : AbstractValidator<User>
    {
        public UserValidator()
        {
            RuleFor(u => u.Id).NotNull().Length(1, 32);
            RuleFor(u => u.UserName).NotNull().Length(1, 100);
        }
    }
}
