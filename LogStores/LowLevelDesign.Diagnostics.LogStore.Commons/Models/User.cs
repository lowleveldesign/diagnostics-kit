using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace LowLevelDesign.Diagnostics.LogStore.Commons.Models
{
    public sealed class User : IUser
    {
        public String Id { get; set; }

        public String UserName { get; set; }

        public String Email { get; set; }

        public String PasswordHash { get; set; }

        public bool Enabled { get; set; }

        public DateTime RegistrationDateUtc { get; set; }
    }
}
