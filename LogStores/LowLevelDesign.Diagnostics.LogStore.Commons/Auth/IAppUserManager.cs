using LowLevelDesign.Diagnostics.LogStore.Commons.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace LowLevelDesign.Diagnostics.LogStore.Commons.Auth
{
    public interface IAppUserManager : IUserStore<User>, IUserPasswordStore<User>, IUserClaimStore<User>
    {

        /// <summary>
        /// Returns a list of registered users
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<Tuple<User, IEnumerable<Claim>>>> GetRegisteredUsersWithClaimsAsync();
    }
}
