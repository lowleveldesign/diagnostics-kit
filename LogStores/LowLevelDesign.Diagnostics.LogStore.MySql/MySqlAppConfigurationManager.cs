using Dapper;
using LowLevelDesign.Diagnostics.Commons.Models;
using LowLevelDesign.Diagnostics.LogStore.Commons.Models;
using LowLevelDesign.Diagnostics.LogStore.Defaults;
using MySql.Data.MySqlClient;
using System;
using System.IO;
using System.Runtime.Caching;
using System.Threading.Tasks;

namespace LowLevelDesign.Diagnostics.LogStore.MySql
{
    /// <summary>
    /// Should be a singleton in the application.
    /// </summary>
    public class MySqlAppConfigurationManager : DefaultAppConfigurationManager
    {
        public MySqlAppConfigurationManager()
            : base(MySqlLogStoreConfiguration.ConnectionStringName)
        {
            using (var conn = new MySqlConnection(dbConnString)) {
                conn.Open();

                conn.Execute("create table if not exists Applications (PathHash binary(16) primary key," +
                        "Path varchar(2000) not null, Name varchar(500) not null, IsExcluded bit not null, IsHidden bit not null, DaysToKeepLogs tinyint unsigned)");

                conn.Execute("create table if not exists ApplicationConfigs (PathHash binary(16) not null, Path varchar(2000) not null, " +
                                "Server varchar(200) not null, ServerFqdnOrIp varchar(255) not null, Binding varchar(3000) not null, AppPoolName varchar(500), " +
                                "AppType char(3), ServiceName varchar(300), DisplayName varchar(500), primary key (PathHash, Server))");

                conn.Execute("create table if not exists Globals (ConfKey varchar(100) not null primary key, ConfValue varchar(1000))");
            }
        }

        protected override System.Data.Common.DbConnection CreateConnection()
        {
            return new MySqlConnection(dbConnString);
        }

        public override async Task AddOrUpdateAppAsync(Application app)
        {
            if (app == null || app.Path == null) {
                throw new ArgumentException("app is null or app.Path is null");
            }
            var pathHash = GetApplicationHash(app.Path);
            if (String.IsNullOrEmpty(app.Name)) {
                // if name is not provided we need to assign a default one
                app.Name = Path.GetFileName(app.Path.TrimEnd(Path.DirectorySeparatorChar));
            }

            using (var conn = new MySqlConnection(dbConnString)) {
                await conn.OpenAsync();

                // try to update the record or insert it
                await conn.ExecuteAsync("replace into Applications (Name, Path, PathHash, IsExcluded, IsHidden, DaysToKeepLogs) values " +
                    "(@Name, @Path, @PathHash, @IsExcluded, @IsHidden, @DaysToKeepLogs)", new {
                        app.Name,
                        app.Path,
                        app.IsExcluded,
                        app.IsHidden,
                        PathHash = pathHash,
                        app.DaysToKeepLogs
                    });

                cache.Set(app.Path, app, new CacheItemPolicy {
                    AbsoluteExpiration = DateTimeOffset.UtcNow.AddMinutes(appCacheExpirationInMinutes)
                });
            }
        }

        public override async Task AddOrUpdateAppServerConfigAsync(ApplicationServerConfig config)
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

            using (var conn = new MySqlConnection(dbConnString)) {
                await conn.OpenAsync();

                // try to update the record or insert it
                await conn.ExecuteAsync("replace into ApplicationConfigs (PathHash, Path, Server, ServerFqdnOrIp, Binding, AppPoolName, AppType, ServiceName, DisplayName) values " +
                    "(@PathHash, @Path, @Server, @ServerFqdnOrIp, @Binding, @AppPoolName, @AppType, @ServiceName, @DisplayName)", c);
            }
        }

        public override async Task SetGlobalSettingAsync(string key, string value)
        {
            if (String.IsNullOrEmpty(key)) {
                throw new ArgumentException("Invalid configuration key - can't be empty.");
            }

            using (var conn = new MySqlConnection(dbConnString)) {
                await conn.OpenAsync();

                if (String.IsNullOrEmpty(value)) {
                    await conn.ExecuteAsync("delete from Globals where ConfKey = @key", new { key });
                } else {
                    // try to update the record or insert it
                    await conn.ExecuteAsync("replace into Globals (ConfKey, ConfValue) values (@key, @value)", new { key, value });
                }
            }
        }
    }
}