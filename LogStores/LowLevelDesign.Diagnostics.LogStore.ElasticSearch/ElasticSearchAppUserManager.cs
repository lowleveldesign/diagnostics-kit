using LowLevelDesign.Diagnostics.LogStore.Commons.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using Nest;
using User = LowLevelDesign.Diagnostics.LogStore.Commons.Models.User;
using LowLevelDesign.Diagnostics.LogStore.ElasticSearch.Models;

namespace LowLevelDesign.Diagnostics.LogStore.ElasticSearch
{
    public class ElasticSearchAppUserManager : IAppUserManager
    {
        internal const string AppUsersIndexName = ElasticSearchClientConfiguration.MainConfigIndex;
        private static readonly IList<Claim> EmptyClaimList = new List<Claim>(2);

        private readonly ElasticClient eclient;

        public ElasticSearchAppUserManager()
        {
            eclient = ElasticSearchClientConfiguration.CreateClient(AppUsersIndexName);
        }

        public async Task CreateAsync(User user)
        {
            if (await FindByNameAsync(user.UserName) != null)
            {
                throw new ArgumentException(string.Format("Username: '{0}' is already taken.", user.UserName));
            }
            var euser = new ElasticUser();
            Map(user, euser);
            await eclient.IndexAsync(euser, ind => ind.Index(AppUsersIndexName));
        }
        public async Task UpdateAsync(User user)
        {
            var qres = await eclient.GetAsync(DocumentPath<ElasticUser>.Id(user.Id).Index(AppUsersIndexName));
            if (!qres.Found)
            {
                throw new ArgumentException(string.Format("User with id: '{0}' does not exist.", user.Id));
            }
            var euser = qres.Source;
            Map(user, euser);
            await eclient.IndexAsync(euser, ind => ind.Index(AppUsersIndexName));
        }


        public async Task DeleteAsync(User user)
        {
            await eclient.DeleteAsync(DocumentPath<ElasticUser>.Id(user.Id).Index(AppUsersIndexName));
        }

        public async Task<User> FindByIdAsync(string userId)
        {
            var qres = await eclient.GetAsync(DocumentPath<ElasticUser>.Id(userId).Index(AppUsersIndexName));
            if (!qres.Found)
            {
                return null;
            }
            var user = new User();
            Map(qres.Source, user);
            return user;
        }

        public async Task<User> FindByNameAsync(string userName)
        {
            var res = (await eclient.SearchAsync<ElasticUser>(s => s.Query(q => q.Term(eu => eu.UserName,
                userName)).Size(2))).Documents.ToList();
            if (res.Count > 1)
            {
                // should never happen
                throw new Exception(string.Format("More than one user with the username '{0}' found.", userName));
            }
            if (res.Count == 0)
            {
                return null;
            }
            var u = new User();
            Map(res[0], u);
            return u;
        }

        public async Task<IList<Claim>> GetClaimsAsync(User user)
        {
            var qres = await eclient.GetAsync(DocumentPath<ElasticUser>.Id(user.Id).Index(AppUsersIndexName));
            if (!qres.Found)
            {
                return EmptyClaimList;
            }
            return qres.Source.Claims != null ? qres.Source.Claims.Select(c => new Claim(c.Key, 
                c.Value)).ToList() : EmptyClaimList;
        }

        public async Task AddClaimAsync(User user, Claim claim)
        {
            var qres = await eclient.GetAsync(DocumentPath<ElasticUser>.Id(user.Id).Index(AppUsersIndexName));
            if (!qres.Found)
            {
                throw new ArgumentException(string.Format("User '{0}' not found.", user.Id));
            }
            var euser = qres.Source;
            if (euser.Claims == null)
            {
                euser.Claims = new List<KeyValuePair<string, string>>();
            }
            euser.Claims.Add(new KeyValuePair<string, string>(claim.Type, claim.Value));
            await eclient.IndexAsync(euser, ind => ind.Index(AppUsersIndexName));
        }

        public async Task RemoveClaimAsync(User user, Claim claim)
        {
            var qres = await eclient.GetAsync(DocumentPath<ElasticUser>.Id(user.Id).Index(AppUsersIndexName));
            if (!qres.Found)
            {
                throw new ArgumentException(string.Format("User '{0}' not found.", user.Id));
            }
            var euser = qres.Source;
            if (euser.Claims == null)
            {
                throw new ArgumentException(string.Format("No claims found for user: '{0}'", user.Id));
            }
            euser.Claims.Remove(new KeyValuePair<string, string>(claim.Type, claim.Value));
            await eclient.IndexAsync(euser, ind => ind.Index(AppUsersIndexName));
        }

        public async Task<IEnumerable<Tuple<User, IEnumerable<Claim>>>> GetRegisteredUsersWithClaimsAsync()
        {
            var eusers = (await eclient.SearchAsync<ElasticUser>(s => s.Index(AppUsersIndexName).MatchAll().Sort(
                sort => sort.Ascending(u => u.UserName)).Take(200))).Documents;
            var res = new List<Tuple<User, IEnumerable<Claim>>>();
            foreach (var eu in eusers)
            {
                var u = new User();
                Map(eu, u);
                res.Add(new Tuple<User, IEnumerable<Claim>>(u, eu.Claims == null ? 
                    EmptyClaimList : eu.Claims.Select(c => new Claim(c.Key, c.Value))));
            }
            return res;
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

        private void Map(User user, ElasticUser euser)
        {
            euser.Id = user.Id;
            euser.UserName = user.UserName;
            euser.PasswordHash = user.PasswordHash;
            euser.RegistrationDateUtc = user.RegistrationDateUtc;
        }

        private void Map(ElasticUser euser, User user)
        {
            user.Id = euser.Id;
            user.UserName = euser.UserName;
            user.PasswordHash = euser.PasswordHash;
            user.RegistrationDateUtc = euser.RegistrationDateUtc;
        }
        public void Dispose()
        {
        }
    }
}
