using LowLevelDesign.Diagnostics.Castle.Config;
using LowLevelDesign.Diagnostics.LogStore.Commons.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Extensions;
using Microsoft.Owin.Security.Cookies;
using Nancy.Owin;
using Owin;
using System;

namespace LowLevelDesign.Diagnostics.Castle
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            if (AuthSettings.AuthenticationEnabled) {
                ConfigureAuth(app);
            }
            app.UseNancy();
            app.UseStageMarker(PipelineStage.MapHandler);
        }

        private void ConfigureAuth(IAppBuilder app)
        {
            // only when enabled we will show additional options
            app.CreatePerOwinContext(() => {
                return (IUserStore<User>)Activator.CreateInstance(AuthSettings.UserManagerType);
            });

            // Configure the db context and user manager to use a single instance per request
            app.CreatePerOwinContext<ApplicationUserManager>(ApplicationUserManager.Create);

            // Enable the application to use a cookie to store information for the signed in user
            // and to use a cookie to temporarily store information about a user logging in with a third party login provider
            // Configure the sign in cookie
            app.UseCookieAuthentication(new CookieAuthenticationOptions {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/auth/login"),
                Provider = new CookieAuthenticationProvider {
                    OnValidateIdentity = SecurityStampValidator.OnValidateIdentity<ApplicationUserManager, User, String>(
                        TimeSpan.FromMinutes(30),
                        async (manager, user) => {
                            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
                            var userIdentity = await manager.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie);
                            // Add custom user claims here
                            return userIdentity;
                        },
                        null)
                }
            });

            //app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);
        }
    }
}