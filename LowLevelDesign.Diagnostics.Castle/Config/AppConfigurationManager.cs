using Dapper;
using LowLevelDesign.Diagnostics.Castle.Models;
using System;
using System.Collections.Concurrent;
using System.Configuration;
using System.Data.Common;
using System.IO;
using System.Runtime.Caching;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web.Configuration;

namespace LowLevelDesign.Diagnostics.Castle.Config
{
    public class AppConfigurationManager : IAppConfigurationManager
    {
        private const int appCacheExpirationInMinutes = 10;

        private static readonly ConcurrentDictionary<String, byte[]> applicationMd5Hashes = new ConcurrentDictionary<String, byte[]>();
        private static readonly MemoryCache cache;
        private static readonly DbProviderFactory dbProviderFactory;
        private static readonly String dbConnString;
        private readonly Random rand = new Random();

        static AppConfigurationManager() {
            var configDbConnString = WebConfigurationManager.ConnectionStrings["configdb"];
            if (configDbConnString == null) {
                throw new ConfigurationErrorsException("'configdb' connection string is missing. Please add it to the web.config file.");
            }
            dbProviderFactory = DbProviderFactories.GetFactory(configDbConnString.ProviderName ?? "System.Data.SqlClient");
            dbConnString = configDbConnString.ConnectionString;

            cache = new MemoryCache("configcache");
        }

        private DbConnection CreateConnection() {
            var conn = dbProviderFactory.CreateConnection();
            conn.ConnectionString = dbConnString;

            return conn;
        }

        private byte[] GetApplicationHash(String applicationPath)
        {
            byte[] apphash;
            if (!applicationMd5Hashes.TryGetValue(applicationPath, out apphash)) {
                using (var md5 = MD5.Create()) {
                    apphash = md5.ComputeHash(Encoding.UTF8.GetBytes(applicationPath));
                    applicationMd5Hashes.TryAdd(applicationPath, apphash);
                }
            }
            return apphash;
        }

        public async Task AddOrUpdateAppAsync(Application app) {
            if (app == null || app.Path == null) {
                throw new ArgumentException("app is null or app.Path is null");
            }
            app.Path = app.Path.ToLowerInvariant();
            var pathHash = GetApplicationHash(app.Path);
            if (String.IsNullOrEmpty(app.Name)) {
                // if name is not provided we need to assign a default one
                app.Name = Path.GetFileName(app.Path.TrimEnd(Path.DirectorySeparatorChar));
            }

            using (var conn = CreateConnection()) {
                await conn.OpenAsync();

                var tran = conn.BeginTransaction();
                try {
                    // try to update the record
                    var rec = await conn.ExecuteAsync("update Applications set Name = @Name, IsExcluded = @IsExcluded where PathHash = @PathHash", new {
                        app.Name, app.IsExcluded, PathHash = pathHash
                    }, tran);
                    if (rec == 0) {
                        // no application found - we need to insert it
                        await conn.ExecuteAsync("insert into Applications (Name, Path, PathHash, IsExcluded) values (@Name, @Path, @PathHash, @IsExcluded)", new {
                            app.Name, app.Path, app.IsExcluded, PathHash = pathHash
                        }, tran);
                    }
                    tran.Commit();
                } catch {
                    tran.Rollback();
                    throw;
                } finally {
                    tran.Dispose();
                }

                cache.Set(app.Path, app, new CacheItemPolicy {
                    AbsoluteExpiration = DateTimeOffset.UtcNow.AddMinutes(appCacheExpirationInMinutes)
                });
            }
        }

        public async Task<Application> FindAppAsync(String path) {
            if (path == null) {
                throw new ArgumentException("path is null");
            }
            path = path.ToLowerInvariant();
            var hash = GetApplicationHash(path);

            if (cache.Contains(path)) {
                return cache[path] as Application;
            }

            // we need to hit the database
            using (var conn = CreateConnection()) {
                await conn.OpenAsync();


                var apps = (await conn.QueryAsync<Application>("select * from Applications"));

                if (cache.Contains(path)) {
                    return cache[path] as Application;
                }
                Application result = null;
                lock (cache) {
                    if (cache.Contains(path)) {
                        return cache[path] as Application;
                    }

                    foreach (var app in apps) {
                        cache.Set(path, app, new CacheItemPolicy {
                            AbsoluteExpiration = DateTimeOffset.UtcNow.AddMinutes(appCacheExpirationInMinutes + rand.Next(10))
                        });
                        if (String.Equals(app.Path, path)) {
                            result = app;
                        }
                    }
                }
                return result;
            }
        }

        public async Task RemoveAppAsync(String path) {
            if (path == null) {
                throw new ArgumentException("path is null");
            }
            var pathHash = GetApplicationHash(path.ToLowerInvariant());

            using (var conn = CreateConnection()) {
                await conn.OpenAsync();

                await conn.ExecuteAsync("update Applications set IsExcluded = 1 where PathHash = @pathHash", new { pathHash });
            }

            // only if the app was cached, update it
            if (cache.Contains(path)) {
                var app = cache[path] as Application;
                app.IsExcluded = false;
                cache.Set(path, app, new CacheItemPolicy { 
                    AbsoluteExpiration = DateTimeOffset.UtcNow.AddMinutes(appCacheExpirationInMinutes + rand.Next(10))
                });
            }
        }
    }
}