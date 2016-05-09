using LowLevelDesign.Diagnostics.LogStore.ElasticSearch.Models;
using Nest;
using System;
using System.Security.Claims;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace LowLevelDesign.Diagnostics.LogStore.ElasticSearch.Tests
{
    public class ElasticUserManagerTests : IDisposable
    {
        private const string path = @"c:\TEMP\test\blablabla";
        private readonly ElasticClient client;
        private readonly string userId;

        public ElasticUserManagerTests()
        {
            client = ElasticSearchClientConfiguration.CreateClient(null);
            userId = Guid.NewGuid().ToString("N");
        }

        [Fact]
        public async Task TestConfiguration()
        {
            var users = new ElasticSearchAppUserManager();

            var expectedUser = new Commons.Models.User {
                Id = userId,
                UserName = "testuser",
                PasswordHash = "testhash",
                RegistrationDateUtc = DateTime.UtcNow
            };

            await users.CreateAsync(expectedUser);

            await Task.Delay(2000);

            var u = await users.FindByIdAsync(userId);
            Assert.NotNull(u);
            Assert.Equal(expectedUser.Id, u.Id);
            Assert.Equal(expectedUser.UserName, u.UserName);
            Assert.Equal(expectedUser.PasswordHash, u.PasswordHash);
            Assert.Equal(expectedUser.RegistrationDateUtc.ToString("yyyyMMdd HH:mm:ss"),
                u.RegistrationDateUtc.ToString("yyyyMMdd HH:mm:ss"));

            u = await users.FindByNameAsync(expectedUser.UserName);
            Assert.NotNull(u);
            Assert.Equal(expectedUser.Id, u.Id);
            Assert.Equal(expectedUser.UserName, u.UserName);
            Assert.Equal(expectedUser.PasswordHash, u.PasswordHash);
            Assert.Equal(expectedUser.RegistrationDateUtc.ToString("yyyyMMdd HH:mm:ss"),
                u.RegistrationDateUtc.ToString("yyyyMMdd HH:mm:ss"));

            expectedUser.UserName += "2";
            expectedUser.PasswordHash += "2";
            await users.UpdateAsync(expectedUser);
            u = await users.FindByIdAsync(userId);
            Assert.NotNull(u);
            Assert.Equal(expectedUser.Id, u.Id);
            Assert.Equal(expectedUser.UserName, u.UserName);
            Assert.Equal(expectedUser.PasswordHash, u.PasswordHash);
            Assert.Equal(expectedUser.RegistrationDateUtc.ToString("yyyyMMdd HH:mm:ss"),
                u.RegistrationDateUtc.ToString("yyyyMMdd HH:mm:ss"));

            Assert.True(await users.HasPasswordAsync(u));
            Assert.Equal(expectedUser.PasswordHash, await users.GetPasswordHashAsync(u));

            // claims
            var claims = await users.GetClaimsAsync(expectedUser);
            Assert.NotNull(claims);
            Assert.Empty(claims);

            var expectedClaim = new Claim(ClaimTypes.Role, "admin");
            await users.AddClaimAsync(expectedUser, expectedClaim);

            claims = await users.GetClaimsAsync(expectedUser);
            Assert.NotNull(claims);
            Assert.Contains(claims, c => string.Equals(c.Type, expectedClaim.Type, StringComparison.Ordinal) &&
                                string.Equals(c.Value, expectedClaim.Value));

            // get ES a moment to propagate
            await Task.Delay(2000);
            // users with claims
            var us = await users.GetRegisteredUsersWithClaimsAsync();
            var t = us.SingleOrDefault(_ => _.Item1.Id == expectedUser.Id);
            Assert.NotNull(t);
            Assert.Equal(expectedUser.UserName, t.Item1.UserName);

            Assert.Contains(t.Item2, c => string.Equals(c.Type, expectedClaim.Type, StringComparison.Ordinal) && 
                                string.Equals(c.Value, expectedClaim.Value, StringComparison.Ordinal));

            await users.RemoveClaimAsync(expectedUser, expectedClaim);

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
            client.Delete(new DocumentPath<ElasticUser>(userId).Index(ElasticSearchAppUserManager.AppUsersIndexName));
        }
    }
}
