using Dapper;
using LowLevelDesign.Diagnostics.Commons.Models;
using LowLevelDesign.Diagnostics.LogStore.Commons.Config;
using LowLevelDesign.Diagnostics.LogStore.Commons.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace LowLevelDesign.Diagnostics.LogStore.Defaults
{
    /// <summary>
    /// Use only in single instance mode. Should be a singleton.
    /// </summary>
    public class DefaultAppConfigurationManager : IAppConfigurationManager
    {
        protected static readonly MemoryCache cache = new MemoryCache("configcache");
        private static readonly Object lck = new Object();
        private static readonly Task completedTask = Task.FromResult(false);

        protected const int appCacheExpirationInMinutes = 10;
        protected readonly String dbConnStringName;
        protected readonly String dbConnString;
        protected readonly ConcurrentDictionary<String, byte[]> applicationMd5Hashes = new ConcurrentDictionary<String, byte[]>();

        private static DbProviderFactory dbProviderFactory;

        private readonly Random rand = new Random();

        public DefaultAppConfigurationManager(String connstrName = "configdb")
        {
            var configDbConnString = ConfigurationManager.ConnectionStrings[connstrName];
            if (configDbConnString == null) {
                throw new ConfigurationErrorsException("'" + connstrName + "' connection string is missing. Please add it to the web.config file.");
            }
            dbConnStringName = connstrName;
            dbConnString = configDbConnString.ConnectionString;
        }

        protected virtual DbConnection CreateConnection()
        {
            if (dbProviderFactory == null) {
                lock (lck) {
                    if (dbProviderFactory == null) {
                        var configDbConnString = ConfigurationManager.ConnectionStrings[dbConnStringName];
                        dbProviderFactory = DbProviderFactories.GetFactory(configDbConnString.ProviderName ?? "System.Data.SqlClient");
                    }
                }
            }
            var conn = dbProviderFactory.CreateConnection();
            conn.ConnectionString = dbConnString;

            return conn;
        }

        protected byte[] GetApplicationHash(String applicationPath)
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

        public virtual async Task AddOrUpdateAppAsync(Application app)
        {
            if (app == null || app.Path == null) {
                throw new ArgumentException("app is null or app.Path is null");
            }
            var pathHash = GetApplicationHash(app.Path);
            if (String.IsNullOrEmpty(app.Name)) {
                // if name is not provided we need to assign a default one
                app.Name = Path.GetFileName(app.Path.TrimEnd(Path.DirectorySeparatorChar));
            }

            using (var conn = CreateConnection()) {
                await conn.OpenAsync();

                lock (lck) {
                    // try to update the record
                    var rec = conn.Execute("update Applications set Name = @Name, IsExcluded = @IsExcluded, IsHidden = @IsHidden where PathHash = @PathHash", new {
                        app.Name,
                        app.IsExcluded,
                        PathHash = pathHash,
                        app.IsHidden
                    });
                    if (rec == 0) {
                        // no application found - we need to insert it
                        conn.Execute("insert into Applications (Name, Path, PathHash, IsExcluded, IsHidden) values (@Name, @Path, @PathHash, @IsExcluded, @IsHidden)", new {
                            app.Name,
                            app.Path,
                            app.IsExcluded,
                            app.IsHidden,
                            PathHash = pathHash
                        });
                    }

                    cache.Set(app.Path, app, new CacheItemPolicy {
                        AbsoluteExpiration = DateTimeOffset.UtcNow.AddMinutes(appCacheExpirationInMinutes)
                    });
                }
            }
        }

        public virtual async Task<Application> FindAppAsync(String path)
        {
            if (path == null) {
                throw new ArgumentException("path is null");
            }
            var hash = GetApplicationHash(path);

            if (cache.Contains(path)) {
                return cache[path] as Application;
            }

            // we need to hit the database
            using (var conn = CreateConnection()) {
                await conn.OpenAsync();

                var apps = (await conn.QueryAsync<Application>("select * from Applications"));
                Application result = null;
                foreach (var app in apps) {
                    cache.Set(app.Path, app, new CacheItemPolicy {
                        AbsoluteExpiration = DateTimeOffset.UtcNow.AddMinutes(appCacheExpirationInMinutes + rand.Next(10))
                    });
                    if (String.Equals(app.Path, path)) {
                        result = app;
                    }
                }
                return result;
            }
        }

        public virtual async Task<IEnumerable<Application>> GetAppsAsync()
        {
            using (var conn = CreateConnection()) {
                await conn.OpenAsync();

                return await conn.QueryAsync<Application>("select * from Applications order by Path");
            }
        }


        public virtual async Task UpdateAppPropertiesAsync(Application app, string[] propertiesToUpdate)
        {
            var path = app.Path;
            if (path == null) {
                throw new ArgumentException("path is null");
            }
            var pathHash = GetApplicationHash(path);

            using (var conn = CreateConnection()) {
                await conn.OpenAsync();

                var variables = String.Join(",", propertiesToUpdate.Select(prop => prop + " = @" + prop));

                await conn.ExecuteAsync("update Applications set " + variables + " where PathHash = @pathHash", new {
                    app.Name,
                    app.Path,
                    app.IsExcluded,
                    app.IsHidden,
                    PathHash = pathHash,
                    app.DaysToKeepLogs
                });
            }

            // invalidate cache
            cache.Remove(path);
        }

        protected class AppConfig
        {
            public byte[] PathHash { get; set; }

            public string Path { get; set; }

            public string Server { get; set; }

            public string ServerFqdnOrIp { get; set; }

            public string Binding { get; set; }

            public string AppPoolName { get; set; }

            public string ServiceName { get; set; }

            public string DisplayName { get; set; }

            public string AppType { get; set; }
        }


        public virtual async Task AddOrUpdateAppServerConfigAsync(ApplicationServerConfig config)
        {
            if (config == null || config.AppPath == null || config.Server == null) {
                throw new ArgumentException("AppPath and Server must be provided");
            }
            var c = new AppConfig {
                PathHash = GetApplicationHash(config.AppPath),
                Path = config.AppPath,
                Server = config.Server,
                ServerFqdnOrIp = config.ServerFqdnOrIp,
                Binding = string.Join("|", config.Bindings),
                AppPoolName = config.AppPoolName,
                AppType = config.AppType,
                ServiceName = config.ServiceName,
                DisplayName = config.DisplayName
            };

            using (var conn = CreateConnection()) {
                await conn.OpenAsync();

                lock (lck) {
                    // try to update the record or insert it
                    var rec = conn.Execute("update ApplicationConfigs set Path = @Path, Binding = @Binding, AppPoolName = @AppPoolName, " +
                        "AppType = @AppType, ServiceName = @ServiceName, DisplayName = @DisplayName where PathHash = @PathHash and Server = @Server", c);
                    if (rec == 0) {
                        // no application found - we need to insert it
                        conn.Execute("insert into ApplicationConfigs (PathHash, Path, Server, ServerFqdnOrIp, Binding, AppPoolName, AppType, ServiceName, DisplayName) values " +
                                        "(@PathHash, @Path, @Server, @ServerFqdnOrIp, @Binding, @AppPoolName, @AppType, @ServiceName, @DisplayName)", c);
                    }
                }
            }
        }

        public virtual async Task<IEnumerable<ApplicationServerConfig>> GetAppConfigsAsync(string[] appPaths, string server = null)
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

            using (var conn = CreateConnection()) {
                await conn.OpenAsync();

                return (await conn.QueryAsync<AppConfig>(sql.ToString(), new { hashes, server })).Select(
                    c => new ApplicationServerConfig {
                        AppPath = c.Path,
                        AppPoolName = c.AppPoolName,
                        Server = c.Server,
                        ServerFqdnOrIp = c.ServerFqdnOrIp,
                        Bindings = c.Binding.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries),
                        AppType = c.AppType,
                        ServiceName = c.ServiceName,
                        DisplayName = c.DisplayName
                    });
            }
        }

        public virtual async Task SetGlobalSettingAsync(String key, String value)
        {
            if (String.IsNullOrEmpty(key)) {
                throw new ArgumentException("Invalid configuration key - can't be empty.");
            }

            using (var conn = CreateConnection()) {
                await conn.OpenAsync();

                if (String.IsNullOrEmpty(value)) {
                    await conn.ExecuteAsync("delete from Globals where ConfKey = @key", new { key });
                } else {
                    lock (lck) {
                        var cnt = conn.Execute("update Globals set ConfValue = @value where ConfKey = @key", new { key, value });
                        if (cnt == 0) {
                            conn.Execute("insert into Globals (ConfKey, ConfValue) values (@key, @value)", new { key, value });
                        }
                    }
                }
            }
        }

        public virtual async Task<String> GetGlobalSettingAsync(String key)
        {
            if (String.IsNullOrEmpty(key)) {
                throw new ArgumentException("Invalid configuration key - can't be empty.");
            }

            using (var conn = CreateConnection()) {
                await conn.OpenAsync();

                return (await conn.QueryAsync<String>("select ConfValue from Globals where ConfKey = @key", new { key })).SingleOrDefault();
            }
        }

        public string GetGlobalSetting(string key)
        {
            if (String.IsNullOrEmpty(key)) {
                throw new ArgumentException("Invalid configuration key - can't be empty.");
            }

            using (var conn = CreateConnection()) {
                conn.Open();

                return conn.Query<String>("select ConfValue from Globals where ConfKey = @key", new { key }).SingleOrDefault();
            }
        }
    }
}