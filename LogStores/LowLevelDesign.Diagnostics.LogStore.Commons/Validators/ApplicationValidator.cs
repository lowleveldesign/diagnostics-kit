using FluentValidation;
using LowLevelDesign.Diagnostics.LogStore.Commons.Models;

namespace LowLevelDesign.Diagnostics.LogStore.Commons.Validators
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
