using LowLevelDesign.Diagnostics.Castle.Config;
using LowLevelDesign.Diagnostics.Castle.Models;
using Microsoft.AspNet.Identity.Owin;
using Nancy;
using Nancy.ModelBinding;
using Nancy.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LowLevelDesign.Diagnostics.Castle.Modules
{
    public class UserAuthModule : NancyModule
    {
        public UserAuthModule()
        {
            Get["/auth/login"] = _ => {
                return View["Auth/Login.cshtml", new LoginViewModel()];
            };

            Post["/auth/login", true] = async (x, ct) => {
                var model = this.BindAndValidate<LoginViewModel>();
                if (this.ModelValidationResult.IsValid)
                {
                    var result = await SignInManager.PasswordSignInAsync(model.Login, model.Password, model.RememberMe, shouldLockout: false);
                    if (result == SignInStatus.Success)
                    {
                        // SUCCESS! FIXME
                    }
                    ModelValidationResult.Errors.Add("", "Invalid login attempt");
                }
                ViewBag.ValidationErrors = ModelValidationResult;
                return View["Auth/Login.cshtml", model];
            };
        }
        public ApplicationSignInManager SignInManager
        {
            get { return Context.GetFromOwinContext<ApplicationSignInManager>(); }
        }

        public ApplicationUserManager UserManager
        {
            get { return Context.GetFromOwinContext<ApplicationUserManager>(); }
        }

        private String[] GetErrorsListFromValidationResult(ModelValidationResult result)
        {
            var errors = new List<String>(10);
            foreach (var err in result.Errors) {
                errors.AddRange(err.Value.Select(suberr => suberr.ErrorMessage));
            }
            return errors.ToArray();
        }
    }
}