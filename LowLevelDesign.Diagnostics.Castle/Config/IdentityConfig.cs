using LowLevelDesign.Diagnostics.LogStore.Commons.Auth;
using LowLevelDesign.Diagnostics.LogStore.Commons.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace LowLevelDesign.Diagnostics.Castle.Config
{
    public class ApplicationUserManager : UserManager<User, String>
    {
        public ApplicationUserManager(IUserStore<User, String> store) : base(store)
        {
        }

        public static ApplicationUserManager Create(IdentityFactoryOptions<ApplicationUserManager> options, IOwinContext context)
        {
            var manager = new ApplicationUserManager(context.Get<IUserStore<User>>());
            // we won't lockout users with invalid credentials
            manager.UserLockoutEnabledByDefault = false;
            // Configure validation logic for usernames
            manager.UserValidator = new UserValidator<User, String>(manager) {
                AllowOnlyAlphanumericUserNames = false
            };
            // Configure validation logic for passwords
            manager.PasswordValidator = new PasswordValidator {
                RequiredLength = 6,
                RequireNonLetterOrDigit = false,
                RequireDigit = true,
                RequireLowercase = true,
                RequireUppercase = true
            };
            var dataProtectionProvider = options.DataProtectionProvider;
            if (dataProtectionProvider != null)
            {
                manager.UserTokenProvider = new DataProtectorTokenProvider<User, String>(dataProtectionProvider.Create("ASP.NET Identity"));
            }
            return manager;
        }
    }

    public class ApplicationSignInManager : SignInManager<User, string>
    {
        public ApplicationSignInManager(ApplicationUserManager userManager, IAuthenticationManager authenticationManager)
            : base(userManager, authenticationManager)
        {
        }

        public override Task<ClaimsIdentity> CreateUserIdentityAsync(User user)
        {
            return user.GenerateUserIdentityAsync((ApplicationUserManager)UserManager);
        }

        public static ApplicationSignInManager Create(IdentityFactoryOptions<ApplicationSignInManager> options, IOwinContext context)
        {
            return new ApplicationSignInManager(context.GetUserManager<ApplicationUserManager>(), context.Authentication);
        }
    }
    public class UserStoreWithLockoutSimulation : IUserStore<User>, IUserLockoutStore<User, string>, 
        IUserPasswordStore<User>, IUserClaimStore<User>, IUserTwoFactorStore<User, string>
    {
        private readonly IAppUserManager um;

        public UserStoreWithLockoutSimulation(IAppUserManager userManager)
        {
            this.um = userManager;
        }

        public Task AddClaimAsync(User user, Claim claim)
        {
            return um.AddClaimAsync(user, claim);
        }

        public void Dispose()
        {
            um.Dispose();
        }

        public Task<User> FindByIdAsync(string userId)
        {
            return um.FindByIdAsync(userId);
        }

        public Task<User> FindByNameAsync(string userName)
        {
            return um.FindByNameAsync(userName);
        }

        public Task<int> GetAccessFailedCountAsync(User user)
        {
            return Task.FromResult(0);
        }

        public Task<IList<Claim>> GetClaimsAsync(User user)
        {
            return um.GetClaimsAsync(user);
        }

        public Task<bool> GetLockoutEnabledAsync(User user)
        {
            return Task.FromResult(false);
        }

        public Task<DateTimeOffset> GetLockoutEndDateAsync(User user)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetPasswordHashAsync(User user)
        {
            return um.GetPasswordHashAsync(user);
        }

        public Task<bool> GetTwoFactorEnabledAsync(User user)
        {
            return Task.FromResult(false);
        }

        public Task<bool> HasPasswordAsync(User user)
        {
            return um.HasPasswordAsync(user);
        }

        public Task<int> IncrementAccessFailedCountAsync(User user)
        {
            return Task.FromResult(0);
        }

        public Task RemoveClaimAsync(User user, Claim claim)
        {
            return um.RemoveClaimAsync(user, claim);
        }

        public Task ResetAccessFailedCountAsync(User user)
        {
            return Task.FromResult(0);
        }

        public Task SetLockoutEnabledAsync(User user, bool enabled)
        {
            return Task.FromResult(0);
        }

        public Task SetLockoutEndDateAsync(User user, DateTimeOffset lockoutEnd)
        {
            return Task.FromResult(0);
        }

        public Task SetPasswordHashAsync(User user, string passwordHash)
        {
            return um.SetPasswordHashAsync(user, passwordHash);
        }

        public Task SetTwoFactorEnabledAsync(User user, bool enabled)
        {
            throw new NotImplementedException();
        }

        async Task IUserStore<User, string>.CreateAsync(User user)
        {
            await um.CreateAsync(user);
        }

        async Task IUserStore<User, string>.DeleteAsync(User user)
        {
            await um.DeleteAsync(user);
        }

        async Task IUserStore<User, string>.UpdateAsync(User user)
        {
            await um.UpdateAsync(user);
        }
    }

}