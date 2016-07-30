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

using Dapper;
using LowLevelDesign.Diagnostics.LogStore.Commons.Models;
using LowLevelDesign.Diagnostics.LogStore.SqlServer.Standard;
using System;
using System.Configuration;
using System.Data.Common;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace LowLevelDesign.Diagnostics.Castle.Tests
{
    public class SqlServerAppUserManagerTests
    {
        private readonly DbProviderFactory dbProviderFactory;
        private readonly string dbConnString;
        private readonly string userId;

        public SqlServerAppUserManagerTests()
        {
            var configDbConnString = ConfigurationManager.ConnectionStrings["sqlserverconn"];
            dbProviderFactory = DbProviderFactories.GetFactory(configDbConnString.ProviderName ?? "System.Data.SqlClient");
            dbConnString = configDbConnString.ConnectionString;
            userId = Guid.NewGuid().ToString("N");
        }

        [Fact]
        public async Task TestAuth()
        {
            var users = new SqlServerAppUserManager();

            var expectedUser = new User {
                Id = userId,
                UserName = "testuser",
                PasswordHash = "testhash",
                RegistrationDateUtc = DateTime.UtcNow
            };

            await users.CreateAsync(expectedUser);

            var u = await users.FindByIdAsync(userId);
            Assert.NotNull(u);
            Assert.Equal(expectedUser.Id, u.Id);
            Assert.Equal(expectedUser.UserName, u.UserName);
            Assert.Equal(expectedUser.PasswordHash, u.PasswordHash);
            Assert.True(Math.Abs(expectedUser.RegistrationDateUtc.Subtract(
                u.RegistrationDateUtc).TotalSeconds) < 10);

            u = await users.FindByNameAsync(expectedUser.UserName);
            Assert.NotNull(u);
            Assert.Equal(expectedUser.Id, u.Id);
            Assert.Equal(expectedUser.UserName, u.UserName);
            Assert.Equal(expectedUser.PasswordHash, u.PasswordHash);
            Assert.True(Math.Abs(expectedUser.RegistrationDateUtc.Subtract(
                u.RegistrationDateUtc).TotalSeconds) < 10);

            expectedUser.UserName += "2";
            expectedUser.PasswordHash += "2";
            await users.UpdateAsync(expectedUser);
            u = await users.FindByIdAsync(userId);
            Assert.NotNull(u);
            Assert.Equal(expectedUser.Id, u.Id);
            Assert.Equal(expectedUser.UserName, u.UserName);
            Assert.Equal(expectedUser.PasswordHash, u.PasswordHash);
            Assert.True(Math.Abs(expectedUser.RegistrationDateUtc.Subtract(
                u.RegistrationDateUtc).TotalSeconds) < 10);

            Assert.True(await users.HasPasswordAsync(u));
            Assert.Equal(expectedUser.PasswordHash, await users.GetPasswordHashAsync(u));

            // claims
            var claims = await users.GetClaimsAsync(expectedUser);
            Assert.NotNull(claims);
            Assert.Empty(claims);

            var claim = new Claim(ClaimTypes.Role, "admin");
            await users.AddClaimAsync(expectedUser, claim);

            claims = await users.GetClaimsAsync(expectedUser);
            Assert.NotNull(claims);
            Assert.Contains(claims, c => String.Equals(c.Type, claim.Type, StringComparison.Ordinal) &&
                                String.Equals(c.Value, claim.Value));

            await users.RemoveClaimAsync(expectedUser, claim);

            claims = await users.GetClaimsAsync(expectedUser);
            Assert.NotNull(claims);
            Assert.Empty(claims);

            // delete tests
            await users.DeleteAsync(expectedUser);
            u = await users.FindByIdAsync(userId);
            Assert.Null(u);
        }

        public void Dispose()
        {
            using (var conn = dbProviderFactory.CreateConnection()) {
                conn.Open();

                conn.Execute("delete from Users where Id = @userId", new { userId });
                conn.Execute("delete from UserClaims where UserId = @userId", new { userId });
            }
        }
    }
}
