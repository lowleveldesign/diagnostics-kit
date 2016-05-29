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
        /// Returns a list of registered users ordered by username.
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<Tuple<User, IEnumerable<Claim>>>> GetRegisteredUsersWithClaimsAsync();
    }
}
