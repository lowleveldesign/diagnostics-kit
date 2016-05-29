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

using LowLevelDesign.Diagnostics.Castle.Config;
using LowLevelDesign.Diagnostics.Castle.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Nancy;
using Nancy.ModelBinding;
using Nancy.Validation;
using System;
using System.Collections.Generic;
using System.Linq;

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
                        return Response.AsRedirect(Request.Query.ReturnUrl != null ? (String)Request.Query.ReturnUrl : "~/");
                    }
                    ModelValidationResult.Errors.Add("", "Invalid login attempt");
                }
                ViewBag.ValidationErrors = ModelValidationResult;
                return View["Auth/Login.cshtml", model];
            };

            Get["/auth/logout"] = _ => {
                SignInManager.AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
                return Response.AsRedirect("~/");
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
