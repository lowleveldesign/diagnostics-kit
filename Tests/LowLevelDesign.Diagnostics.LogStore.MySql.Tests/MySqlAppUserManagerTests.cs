using Dapper;
using LowLevelDesign.Diagnostics.LogStore.Commons.Models;
using LowLevelDesign.Diagnostics.LogStore.MySql;
using MySql.Data.MySqlClient;
using System;
using System.Configuration;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace LowLevelDesign.Diagnostics.Castle.Tests
{
    public class MySqlAppUserManagerTests
    {
        private readonly string dbConnString;
        private readonly string userId;

        public MySqlAppUserManagerTests()
        {
            var configDbConnString = ConfigurationManager.ConnectionStrings["mysqlconn"];
            dbConnString = configDbConnString.ConnectionString;
            userId = Guid.NewGuid().ToString("N");
        }

        [Fact]
        public async Task TestAuth()
        {
            var users = new MySqlAppUserManager();

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
            using (var conn = new MySqlConnection(dbConnString)) {
                conn.Open();

                conn.Execute("delete from Users where Id = @userId", new { userId });
                conn.Execute("delete from UserClaims where UserId = @userId", new { userId });
            }
        }
    }
}
