using LowLevelDesign.Diagnostics.Castle.Config;
using LowLevelDesign.Diagnostics.Castle.Models;
using Microsoft.AspNet.Identity.Owin;
using Nancy;
using Nancy.ModelBinding;

namespace LowLevelDesign.Diagnostics.Castle.Modules
{
    public sealed class AuthModule : NancyModule
    {
        public AuthModule(GlobalConfig globals)
        {
            // FIXME some general message here to show info if authentication is not enabled

            Get["/auth/login"] = _ => {
                return View["Auth/Login.cshtml", new LoginViewModel()];
            };

            Post["/auth/login", true] = async (x, ct) => {
                var model = this.BindAndValidate<LoginViewModel>();
                if (this.ModelValidationResult.IsValid) {
                    var result = await SignInManager.PasswordSignInAsync(model.Login, model.Password, model.RememberMe, shouldLockout: false);
                    if (result == SignInStatus.Success) {
                        // SUCCESS! FIXME
                    }
                    ModelValidationResult.Errors.Add("", "Invalid login attempt");
                }
                ViewBag.ValidationErrors = ModelValidationResult;
                return View["Auth/Login.cshtml", model];
            };

            // FIXME
            Get["/auth/users", true] = async (x, ct) => {
                bool isenabled = await globals.IsAuthenticationEnabled();

                return View["Auth/Users.cshtml", isenabled.ToString()];
            };
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return Context.GetFromOwinContext<ApplicationSignInManager>();
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return Context.GetFromOwinContext<ApplicationUserManager>();
            }
        }
    }
}