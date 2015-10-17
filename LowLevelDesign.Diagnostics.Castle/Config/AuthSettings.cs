using LowLevelDesign.Diagnostics.LogStore.Commons.Auth;
using LowLevelDesign.Diagnostics.LogStore.Commons.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using System;
using System.Configuration;

namespace LowLevelDesign.Diagnostics.Castle.Config
{
    public static class AuthSettings
    {
        private const String UserMgrKey = "diag:usermgr";
        private static readonly Type userMgrType;

        private static readonly bool authenticationEnabled;

        static AuthSettings()
        {
            /* SECURITY */
            var userMgrTypeName = ConfigurationManager.AppSettings[UserMgrKey];
            if (userMgrTypeName == null) {
                userMgrType = AppSettings.FindSingleTypeInLowLevelDesignAssemblies(typeof(IAppUserManager), UserMgrKey);
            } else {
                userMgrType = Type.GetType(userMgrTypeName);
            }

            bool flag;
            Boolean.TryParse(ConfigurationManager.AppSettings["diag:authentication-enabled"], out flag);
            authenticationEnabled = flag;
        }

        public static bool AuthenticationEnabled
        {
            get { return authenticationEnabled; }
        }

        public static Type UserManagerType
        {
            get { return userMgrType; }
        }
    }

    public class ApplicationUserManager : UserManager<User, String>
    {
        public ApplicationUserManager(IUserStore<User, String> store) : base(store)
        {
        }

        public static ApplicationUserManager Create(IdentityFactoryOptions<ApplicationUserManager> options, IOwinContext context) 
        {
            var manager = new ApplicationUserManager(context.Get<IUserStore<User>>());
            // Configure validation logic for usernames
            manager.UserValidator = new UserValidator<User, String>(manager)
            {
                AllowOnlyAlphanumericUserNames = false,
                RequireUniqueEmail = true
            };
            // Configure validation logic for passwords
            manager.PasswordValidator = new PasswordValidator
            {
                RequiredLength = 6,
                RequireNonLetterOrDigit = true,
                RequireDigit = true,
                RequireLowercase = true,
                RequireUppercase = true,
            };
            var dataProtectionProvider = options.DataProtectionProvider;
            if (dataProtectionProvider != null)
            {
                manager.UserTokenProvider = new DataProtectorTokenProvider<User, String>(dataProtectionProvider.Create("ASP.NET Identity"));
            }
            return manager;
        }
    }
}