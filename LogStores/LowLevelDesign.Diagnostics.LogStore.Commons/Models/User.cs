using Microsoft.AspNet.Identity;
using System;

namespace LowLevelDesign.Diagnostics.LogStore.Commons.Models
{
    public sealed class User : IUser
    {
        public String Id { get; set; }

        public String UserName { get; set; }

        public String PasswordHash { get; set; }

        public DateTime RegistrationDateUtc { get; set; }
    }
}
