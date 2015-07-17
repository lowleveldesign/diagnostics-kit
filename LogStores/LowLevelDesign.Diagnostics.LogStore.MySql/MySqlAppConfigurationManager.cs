using Dapper;
using FluentValidation.Internal;
using LowLevelDesign.Diagnostics.Commons.Config;
using LowLevelDesign.Diagnostics.Commons.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace LowLevelDesign.Diagnostics.LogStore.MySql
{
    public class MySqlAppConfigurationManager : IAppConfigurationManager
    {
        private const int appCacheExpirationInMinutes = 10;

        private static readonly ConcurrentDictionary<String, byte[]> applicationMd5Hashes = new ConcurrentDictionary<String, byte[]>();
        private static readonly MemoryCache cache;
        private static readonly String dbConnString;
        private readonly Random rand = new Random();

        static MySqlAppConfigurationManager()
        {
            dbConnString = MySqlLogStoreConfiguration.ConnectionString;

            cache = new MemoryCache("configcache");

            using (var conn = new MySqlConnection(dbConnString)) {
                conn.Open();

                conn.Execute("create table if not exists Applications (PathHash binary(16) primary key," +
                        "Path varchar(2000) not null, Name varchar(500) not null, IsExcluded bit not null, DaysToKeepLogs tinyint unsigned)");

                conn.Execute("create table if not exists ApplicationConfigs (PathHash binary(16) not null, Path varchar(2000) not null, " + 
                                "Server varchar(200) not null, Binding varchar(3000) not null, AppPoolName varchar(500), primary key (PathHash, Server))");
            }
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

        public async Task AddOrUpdateAppAsync(Application app)
        {
            if (app == null || app.Path == null) {
                throw new ArgumentException("app is null or app.Path is null");
            }
            app.Path = app.Path.ToLowerInvariant();
            var pathHash = GetApplicationHash(app.Path);
            if (String.IsNullOrEmpty(app.Name)) {
                // if name is not provided we need to assign a default one
                app.Name = Path.GetFileName(app.Path.TrimEnd(Path.DirectorySeparatorChar));
            }

            using (var conn = new MySqlConnection(dbConnString)) {
                await conn.OpenAsync();

                var tran = conn.BeginTransaction();
                try {
                    // try to update the record or insert it
                    await conn.ExecuteAsync("replace into Applications (Name, Path, PathHash, IsExcluded, DaysToKeepLogs) values " +
                        "(@Name, @Path, @PathHash, @IsExcluded, @DaysToKeepLogs)", new {
                            app.Name,
                            app.Path,
                            app.IsExcluded,
                            PathHash = pathHash,
                            app.DaysToKeepLogs
                        }, tran);

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

        public async Task<Application> FindAppAsync(String path)
        {
            if (path == null) {
                throw new ArgumentException("path is null");
            }
            path = path.ToLowerInvariant();
            var hash = GetApplicationHash(path);

            if (cache.Contains(path)) {
                return cache[path] as Application;
            }

            // we need to hit the database
            using (var conn = new MySqlConnection(dbConnString)) {
                await conn.OpenAsync();

                var apps = (await conn.QueryAsync<Application>("select * from Applications"));
                Application result = null;
                lock (cache) {
                    foreach (var app in apps) {
                        cache.Set(app.Path, app, new CacheItemPolicy {
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

        public async Task<IEnumerable<Application>> GetAppsAsync()
        {
            using (var conn = new MySqlConnection(dbConnString)) {
                await conn.OpenAsync();

                return await conn.QueryAsync<Application>("select * from Applications order by Path");
            }
        }

        public async Task UpdateAppPropertiesAsync(Application app, string[] propertiesToUpdate)
        {
            var path = app.Path;
            if (path == null) {
                throw new ArgumentException("path is null");
            }
            var pathHash = GetApplicationHash(path.ToLowerInvariant());

            using (var conn = new MySqlConnection(dbConnString)) {
                await conn.OpenAsync();

                var variables = String.Join(",", propertiesToUpdate.Select(prop => prop + " = @" + prop));

                await conn.ExecuteAsync("update Applications set " + variables + " where PathHash = @pathHash", new {
                    app.Name,
                    app.Path,
                    app.IsExcluded,
                    PathHash = pathHash,
                    app.DaysToKeepLogs
                });
            }

            // invalidate cache
            cache.Remove(path);
        }

        private class AppConfig
        {
            public byte[] PathHash { get; set; }

            public String Path { get; set; }

            public String Server { get; set; }

            public String Binding { get; set; }

            public String AppPoolName { get; set; }
        }

        public async Task AddOrUpdateAppServerConfigAsync(ApplicationServerConfig config)
        {
            if (config == null || config.AppPath == null || config.Server == null) {
                throw new ArgumentException("AppPath and Server must be provided");
            }
            var c = new AppConfig {
                PathHash = GetApplicationHash(config.AppPath.ToLower()),
                Path = config.AppPath,
                Server = config.Server,
                Binding = String.Join("|", config.Bindings),
                AppPoolName = config.AppPoolName
            };

            using (var conn = new MySqlConnection(dbConnString)) {
                await conn.OpenAsync();

                var tran = conn.BeginTransaction();
                try {
                    // try to update the record or insert it
                    await conn.ExecuteAsync("replace into ApplicationConfigs (PathHash, Path, Server, Binding, AppPoolName) values " +
                        "(@PathHash, @Path, @Server, @Binding, @AppPoolName)", c, tran);

                    tran.Commit();
                } catch {
                    tran.Rollback();
                    throw;
                } finally {
                    tran.Dispose();
                }
            }
        }

        public async Task<IEnumerable<ApplicationServerConfig>> GetAppConfigsAsync(string[] appPaths, string server = null)
        {
            if (appPaths == null || appPaths.Length == 0) {
                throw new ArgumentException("At least one application path must be provided.");
            }

            var sql = new StringBuilder("select * from ApplicationConfigs where PathHash in @hashes");
            if (server != null) {
                sql.Append(" and Server = @server");
            }
            var hashes = new byte[appPaths.Length][];
            for (int i = 0; i < hashes.Length; i++) {
                hashes[i] = GetApplicationHash(appPaths[i]);
            }

            using (var conn = new MySqlConnection(dbConnString)) {
                await conn.OpenAsync();

                return (await conn.QueryAsync<AppConfig>(sql.ToString(), new { hashes, server })).Select(
                    c => new ApplicationServerConfig {
                        AppPath = c.Path,
                        AppPoolName = c.AppPoolName,
                        Server = c.Server,
                        Bindings = c.Binding.Split(new [] { '|' }, StringSplitOptions.RemoveEmptyEntries)
                    });
            }
        }
    }
}