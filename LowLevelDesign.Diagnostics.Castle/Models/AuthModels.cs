using LowLevelDesign.Diagnostics.LogStore.Commons.Models;
using Nancy.Security;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;

namespace LowLevelDesign.Diagnostics.Castle.Models
{
    public sealed class AuthenticatedUser : IUserIdentity
    {
        public AuthenticatedUser(string username, IEnumerable<string> claims)
        {
            this.UserName = username;
            this.Claims = claims;
        }

        public AuthenticatedUser(ClaimsPrincipal principal)
        {
            UserName = principal.Identity.Name;
            Claims = principal.Claims.Select(c => c.Type + ":" + c.Value);
        }

        public string UserName { get; private set; }

        public IEnumerable<string> Claims { get; private set; }
    }

    public sealed class LoginViewModel
    {
        [Required]
        public string Login { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public bool RememberMe { get; set; }
    }

    public sealed class RegisterViewModel
    {
        [Required]
        public string Login { get; set; }

        [Required]
        public bool IsAdmin { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        public string Password { get; set; }

        [Required]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public sealed class AdminResetPasswordViewModel
    {
        [Required]
        public String Id { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        public string Password { get; set; }

        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public sealed class ResetPasswordViewModel
    {
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        public string OldPassword { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        public string Password { get; set; }

        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public sealed class UserWithClaims
    {
        public const string AdminRole = "admin";

        public UserWithClaims(User u, IEnumerable<Claim> claims)
        {
            Id = u.Id;
            UserName = u.UserName;
            Claims = claims;
        }

        public string Id { get; private set; }

        public string UserName { get; private set; }

        public IEnumerable<Claim> Claims { get; private set; }

        public bool IsAdmin
        {
            get
            {
                return Claims != null && Claims.FirstOrDefault(c =>
                    string.Equals(c.Type, ClaimTypes.Role, StringComparison.Ordinal) &&
                    string.Equals(c.Value, AdminRole, StringComparison.Ordinal)) != null;
            }
        }
    }
}