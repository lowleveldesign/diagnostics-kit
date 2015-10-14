using Dapper;
using LowLevelDesign.Diagnostics.LogStore.Commons.Auth;
using LowLevelDesign.Diagnostics.LogStore.Commons.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace LowLevelDesign.Diagnostics.LogStore.Defaults
{
    public class DefaultAppUserManager : IAppUserManager
    {
        protected sealed class UserClaim
        {
            public String UserId { get; set; }

            public String ClaimType { get; set; }

            public String ClaimValue { get; set; }
        }

        private static readonly Object lck = new Object();
        private static DbProviderFactory dbProviderFactory;

        protected readonly String dbConnStringName;
        protected readonly String dbConnString;

        public DefaultAppUserManager(String connstrName = "configdb")
        {
            var configDbConnString = ConfigurationManager.ConnectionStrings[connstrName];
            if (configDbConnString == null)
            {
                throw new ConfigurationErrorsException("'" + connstrName + "' connection string is missing. Please add it to the web.config file.");
            }
            dbConnStringName = connstrName;
            dbConnString = configDbConnString.ConnectionString;
        }

        protected virtual DbConnection CreateConnection()
        {
            if (dbProviderFactory == null)
            {
                lock (lck)
                {
                    if (dbProviderFactory == null)
                    {
                        var configDbConnString = ConfigurationManager.ConnectionStrings[dbConnStringName];
                        dbProviderFactory = DbProviderFactories.GetFactory(configDbConnString.ProviderName ?? "System.Data.SqlClient");
                    }
                }
            }
            var conn = dbProviderFactory.CreateConnection();
            conn.ConnectionString = dbConnString;

            return conn;
        }

        public virtual async Task<IEnumerable<Tuple<User, IEnumerable<Claim>>>> GetRegisteredUsersWithClaimsAsync()
        {
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                var users = await conn.QueryAsync<User>("select * from Users");
                var claims = await conn.QueryAsync<UserClaim>("select * from UserClaims");

                var res = new List<Tuple<User, IEnumerable<Claim>>>();
                foreach (var u in users)
                {
                    res.Add(new Tuple<User, IEnumerable<Claim>>(u, claims.Where(c => c.UserId.Equals(u.Id)).Select(
                        c => new Claim(c.ClaimType, c.ClaimValue))));
                }
                return res;
            }
        }

        public Task SetPasswordHashAsync(User user, string passwordHash)
        {
            user.PasswordHash = passwordHash;
            return Task.FromResult(0);
        }

        public Task<string> GetPasswordHashAsync(User user)
        {
            return Task.FromResult(user.PasswordHash);
        }

        public Task<bool> HasPasswordAsync(User user)
        {
            return Task.FromResult(user.PasswordHash != null);
        }

        public async Task CreateAsync(User user)
        {
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                await conn.ExecuteAsync("insert into Users (Id, UserName, Email, PasswordHash, Enabled, RegistrationDateUtc) values (" +
                    "@Id, @UserName, @Email, @PasswordHash, @Enabled, @RegistrationDateUtc)", user);
            }
        }

        public async Task UpdateAsync(User user)
        {

            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                await conn.ExecuteAsync("update Users set UserName = @UserName, Email = @Email, PasswordHash = @PasswordHash, Enabled = @Enabled, " + 
                    "RegistrationDateUtc = @RegistrationDateUtc where Id = @Id", user);
            }
        }

        public async Task DeleteAsync(User user)
        {
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                await conn.ExecuteAsync("delete from Users where Id = @Id", user);
            }
        }

        public async Task<User> FindByIdAsync(string userId)
        {
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                return (await conn.QueryAsync<User>("select * from Users where Id = @userId", new { userId })).FirstOrDefault();
            }
        }

        public async Task<User> FindByNameAsync(string userName)
        {
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                return (await conn.QueryAsync<User>("select * from Users where UserName = @userName", new { userName })).FirstOrDefault();
            }
        }

        public async Task<IList<Claim>> GetClaimsAsync(User user)
        {
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                return (await conn.QueryAsync<UserClaim>("select * from UserClaims where UserId = @Id", user)).Select(
                    c => new Claim(c.ClaimType, c.ClaimValue)).ToList();
            }
        }

        public async Task AddClaimAsync(User user, Claim claim)
        {
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                await conn.ExecuteAsync("insert into UserClaims (UserId, ClaimType, ClaimValue) values (@UserId, @ClaimType, @ClaimValue)",
                    new UserClaim { UserId = user.Id, ClaimType = claim.Type, ClaimValue = claim.Value });
            }
        }

        public async Task RemoveClaimAsync(User user, Claim claim)
        {
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                await conn.ExecuteAsync("delete from UserClaims where UserId = @UserId and ClaimType = @ClaimType",
                    new { UserId = user.Id, ClaimType = claim.Type });
            }
        }
        public void Dispose()
        {
        }

    }
}
