using LowLevelDesign.Diagnostics.Castle.Config;
using LowLevelDesign.Diagnostics.Castle.Models;
using LowLevelDesign.Diagnostics.LogStore.Commons.Auth;
using LowLevelDesign.Diagnostics.LogStore.Commons.Models;
using Nancy;
using Nancy.ModelBinding;
using Nancy.Security;
using Nancy.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace LowLevelDesign.Diagnostics.Castle.Modules
{
    public sealed class UserAdminModule : NancyModule
    {
        public UserAdminModule(GlobalConfig globals, IAppUserManager appUserManager)
        {
            if (globals.IsAuthenticationEnabled())
            {
                this.RequiresAuthentication();
                this.RequiresClaims(GlobalConfig.AdminRoleClaim);
            }

            Post["/auth/register", true] = async (x, ct) => {
                var model = this.BindAndValidate<RegisterViewModel>();
                // HACK: the compare attribute is not working in Nancyvalidation
                if (!String.Equals(model.Password, model.ConfirmPassword, StringComparison.Ordinal)) {
                    ModelValidationResult.Errors.Add("", "The password and confirmation password do not match.");
                }
                if (ModelValidationResult.IsValid) {
                    var user = new User {
                        Id = Guid.NewGuid().ToString("N"),
                        UserName = model.Login,
                        RegistrationDateUtc = DateTime.UtcNow
                    };
                    var result = await UserManager.CreateAsync(user, model.Password);
                    if (result.Succeeded && model.IsAdmin) {
                        result = await UserManager.AddClaimAsync(user.Id, new Claim(ClaimTypes.Role, UserWithClaims.AdminRole));
                    }
                    if (result.Succeeded) {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return Response.AsJson(new JsonFormValidationResult {
                            IsSuccess = true,
                            Result = user.Id
                        });
                    }
                    ModelValidationResult.Errors.Add("", result.Errors.Select(err => new ModelValidationError(
                        "", err)).ToList());
                }
                ViewBag.ValidationErrors = ModelValidationResult;
                // If we got this far, something failed, redisplay form
                return Response.AsJson(new JsonFormValidationResult {
                    IsSuccess = false,
                    Errors = GetErrorsListFromValidationResult(ModelValidationResult)
                });
            };

            Get["/auth/users", true] = async (x, ct) => {
                ViewBag.AuthEnabled = globals.IsAuthenticationEnabled();
                var ucs = await appUserManager.GetRegisteredUsersWithClaimsAsync();

                return View["Auth/Users.cshtml", ucs.Select(uc => new UserWithClaims(uc.Item1, uc.Item2))];
            };

            Post["/auth/remove", true] = async (x, ct) => {
                var user = this.Bind<User>();
                // only id is required
                await appUserManager.DeleteAsync(user);
                return Response.AsRedirect("~/auth/users");
            };

            Post["/auth/enable", true] = async (x, ct) => {
                if (Request.Form.Enabled == null) {
                    throw new ArgumentException();
                }
                await globals.ToggleAuthentication((bool)Request.Form.Enabled);
                return "OK";
            };

            Post["/auth/adminresetpasswd", true] = async (x, ct) => {
                var model = this.BindAndValidate<AdminResetPasswordViewModel>();
                if (!String.Equals(model.Password, model.ConfirmPassword, StringComparison.Ordinal)) {
                    ModelValidationResult.Errors.Add("", "The password and confirmation password do not match.");
                }
                if (ModelValidationResult.IsValid) {
                    var user = await UserManager.FindByIdAsync(model.Id);
                    if (user == null) {
                        ModelValidationResult.Errors.Add("", "User not found.");
                    } else {
                        // here we don't do the usual password reset procedure but a mock
                        var token = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                        var result = await UserManager.ResetPasswordAsync(user.Id, token, model.Password);
                        if (result.Succeeded) {
                            return Response.AsJson(new JsonFormValidationResult { IsSuccess = true });
                        }
                        ModelValidationResult.Errors.Add("", result.Errors.Select(
                            err => new ModelValidationError("", err)).ToList());
                    }
                }
                return Response.AsJson(new JsonFormValidationResult {
                    IsSuccess = false,
                    Errors = GetErrorsListFromValidationResult(ModelValidationResult)
                });
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