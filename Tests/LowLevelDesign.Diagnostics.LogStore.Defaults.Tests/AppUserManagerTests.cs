using Dapper;
using LowLevelDesign.Diagnostics.Commons.Models;
using LowLevelDesign.Diagnostics.LogStore.Commons.Models;
using LowLevelDesign.Diagnostics.LogStore.Defaults;
using System;
using System.Configuration;
using System.Data.Common;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace LowLevelDesign.Diagnostics.Castle.Tests
{
    public class AppUserManagerTests
    {
        private readonly DbProviderFactory dbProviderFactory;
        private readonly string dbConnString;
        private readonly string userId;

        public AppUserManagerTests()
        {
            var configDbConnString = ConfigurationManager.ConnectionStrings["configdb"];
            dbProviderFactory = DbProviderFactories.GetFactory(configDbConnString.ProviderName ?? "System.Data.SqlClient");
            dbConnString = configDbConnString.ConnectionString;
            userId = Guid.NewGuid().ToString("N");
        }

        [Fact]
        public async Task TestAuth()
        {
            var users = new DefaultAppUserManager();

            var expectedUser = new User {
                Id = userId,
                UserName = "testuser",
                Email = "test@test.com",
                PasswordHash = "testhash",
                Enabled = true,
                RegistrationDateUtc = DateTime.UtcNow
            };

            await users.CreateAsync(expectedUser);

            var u = await users.FindByIdAsync(userId);
            Assert.NotNull(u);
            Assert.Equal(expectedUser.Id, u.Id);
            Assert.Equal(expectedUser.UserName, u.UserName);
            Assert.Equal(expectedUser.Email, u.Email);
            Assert.Equal(expectedUser.Enabled, u.Enabled);
            Assert.Equal(expectedUser.PasswordHash, u.PasswordHash);
            Assert.Equal(expectedUser.RegistrationDateUtc.ToString("yyyyMMdd HH:mm:ss"),
                u.RegistrationDateUtc.ToString("yyyyMMdd HH:mm:ss"));

            u = await users.FindByNameAsync(expectedUser.UserName);
            Assert.NotNull(u);
            Assert.Equal(expectedUser.Id, u.Id);
            Assert.Equal(expectedUser.UserName, u.UserName);
            Assert.Equal(expectedUser.Email, u.Email);
            Assert.Equal(expectedUser.Enabled, u.Enabled);
            Assert.Equal(expectedUser.PasswordHash, u.PasswordHash);
            Assert.Equal(expectedUser.RegistrationDateUtc.ToString("yyyyMMdd HH:mm:ss"),
                u.RegistrationDateUtc.ToString("yyyyMMdd HH:mm:ss"));

            expectedUser.UserName += "2";
            expectedUser.Email += "2";
            expectedUser.PasswordHash += "2";
            expectedUser.Enabled = false;
            await users.UpdateAsync(expectedUser);
            u = await users.FindByIdAsync(userId);
            Assert.NotNull(u);
            Assert.Equal(expectedUser.Id, u.Id);
            Assert.Equal(expectedUser.UserName, u.UserName);
            Assert.Equal(expectedUser.Email, u.Email);
            Assert.Equal(expectedUser.Enabled, u.Enabled);
            Assert.Equal(expectedUser.PasswordHash, u.PasswordHash);
            Assert.Equal(expectedUser.RegistrationDateUtc.ToString("yyyyMMdd HH:mm:ss"),
                u.RegistrationDateUtc.ToString("yyyyMMdd HH:mm:ss"));

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
