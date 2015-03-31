using Dapper;
using LowLevelDesign.Diagnostics.Castle.Models;
using System;
using System.Configuration;
using System.Data.Common;
using System.Runtime.Caching;
using System.Threading.Tasks;
using System.Web.Configuration;

namespace LowLevelDesign.Diagnostics.Castle.Config
{
    public class AppConfigurationManager : IAppConfigurationManager
    {
        private const int appCacheExpirationInMinutes = 20;

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

        public async Task AddOrUpdateApp(Application app) {
            if (app == null || app.Path == null) {
                throw new ArgumentException("app is null or app.Path is null");
            }
            app.Path = app.Path.ToLowerInvariant();

            using (var conn = CreateConnection()) {
                await conn.OpenAsync();

                var tran = conn.BeginTransaction();
                try {
                    // try to update the record
                    var rec = await conn.ExecuteAsync("update Applications set Name = @Name, IsExcluded = @IsExcluded, TabKey = @TabKey where Path = @Path", app, tran);
                    if (rec == 0) {
                        // no application found - we need to insert it
                        await conn.ExecuteAsync("insert into Applications (Name, Path, IsExcluded, TabKey) values (@Name, @Path, @IsExcluded, @TabKey)", app, tran);
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

        public async Task<Application> FindApp(String path) {
            if (path == null) {
                throw new ArgumentException("path is null");
            }
            path = path.ToLowerInvariant();

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

        public async Task RemoveApp(String path) {
            if (path == null) {
                throw new ArgumentException("path is null");
            }
            path = path.ToLowerInvariant();

            using (var conn = CreateConnection()) {
                await conn.OpenAsync();

                await conn.ExecuteAsync("update Applications set IsExcluded = 1 where Path = @path", new { path });
            }

            cache.Set(path, new Application { Path = path, IsExcluded = true }, new CacheItemPolicy {
                AbsoluteExpiration = DateTimeOffset.UtcNow.AddMinutes(appCacheExpirationInMinutes + rand.Next(10))
            });
        }
    }
}