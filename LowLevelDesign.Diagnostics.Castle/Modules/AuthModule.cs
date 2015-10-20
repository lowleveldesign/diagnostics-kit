using LowLevelDesign.Diagnostics.Castle.Config;
using LowLevelDesign.Diagnostics.Castle.Models;
using LowLevelDesign.Diagnostics.LogStore.Commons.Auth;
using LowLevelDesign.Diagnostics.LogStore.Commons.Models;
using Microsoft.AspNet.Identity.Owin;
using Nancy;
using Nancy.ModelBinding;
using Nancy.Validation;
using System;
using System.Linq;
using System.Security.Claims;

namespace LowLevelDesign.Diagnostics.Castle.Modules
{
    public sealed class AuthModule : NancyModule
    {
        public AuthModule(GlobalConfig globals, IAppUserManager appUserManager)
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

            Get["/auth/register"] = _ => {
                return View["Auth/Register.cshtml", new RegisterViewModel()];
            };

            Post["/auth/register", true] = async (x, ct) => {
                // FIXME only admin or if authentication is disabled everyone
                var model = this.BindAndValidate<RegisterViewModel>();
                // HACK: the compare attribute is not working in Nancyvalidation
                if (!String.Equals(model.Password, model.ConfirmPassword, StringComparison.Ordinal))
                {
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

                        // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                        // Send an email with this link
                        // string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                        // var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                        // await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");

                        return Response.AsRedirect("~/auth/users");
                    }
                    ModelValidationResult.Errors.Add("", result.Errors.Select(err => new ModelValidationError(
                        "", err)).ToList());
                }
                ViewBag.ValidationErrors = ModelValidationResult;
                // If we got this far, something failed, redisplay form
                return View["Auth/Register.cshtml", model];
            };

            Get["/auth/users", true] = async (x, ct) => {

                // FIXME admin only

                ViewBag.AuthEnabled = await globals.IsAuthenticationEnabled();
                var ucs = await appUserManager.GetRegisteredUsersWithClaimsAsync();

                return View["Auth/Users.cshtml", ucs.Select(uc => new UserWithClaims(uc.Item1, uc.Item2))];
            };

            Post["/auth/remove", true] = async (x, ct) => {
                // FIXME admin only - authentication

                var user = this.Bind<User>();
                // only id is required
                await appUserManager.DeleteAsync(user);
                return Response.AsRedirect("~/auth/users");
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
    }
}