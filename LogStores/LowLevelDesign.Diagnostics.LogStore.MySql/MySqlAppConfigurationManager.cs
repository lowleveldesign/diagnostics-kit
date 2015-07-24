using Dapper;
using LowLevelDesign.Diagnostics.Commons.Models;
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
        private const int appCacheExpirationInMinutes = 10;

        public MySqlAppConfigurationManager()
            : base(MySqlLogStoreConfiguration.ConnectionStringName)
        {
            using (var conn = new MySqlConnection(dbConnString)) {
                conn.Open();

                conn.Execute("create table if not exists Applications (PathHash binary(16) primary key," +
                        "Path varchar(2000) not null, Name varchar(500) not null, IsExcluded bit not null, DaysToKeepLogs tinyint unsigned)");

                conn.Execute("create table if not exists ApplicationConfigs (PathHash binary(16) not null, Path varchar(2000) not null, " +
                                "Server varchar(200) not null, Binding varchar(3000) not null, AppPoolName varchar(500), AppType char(3), " + 
                                "ServiceName varchar(300), DisplayName varchar(500), primary key (PathHash, Server))");
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
            app.Path = app.Path.ToLowerInvariant();
            var pathHash = GetApplicationHash(app.Path);
            if (String.IsNullOrEmpty(app.Name)) {
                // if name is not provided we need to assign a default one
                app.Name = Path.GetFileName(app.Path.TrimEnd(Path.DirectorySeparatorChar));
            }

            using (var conn = new MySqlConnection(dbConnString)) {
                await conn.OpenAsync();

                // try to update the record or insert it
                await conn.ExecuteAsync("replace into Applications (Name, Path, PathHash, IsExcluded, DaysToKeepLogs) values " +
                    "(@Name, @Path, @PathHash, @IsExcluded, @DaysToKeepLogs)", new {
                        app.Name,
                        app.Path,
                        app.IsExcluded,
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
                PathHash = GetApplicationHash(config.AppPath.ToLower()),
                Path = config.AppPath,
                Server = config.Server,
                Binding = String.Join("|", config.Bindings),
                AppPoolName = config.AppPoolName,
                AppType = config.AppType,
                ServiceName = config.ServiceName,
                DisplayName = config.DisplayName
            };

            using (var conn = new MySqlConnection(dbConnString)) {
                await conn.OpenAsync();

                // try to update the record or insert it
                await conn.ExecuteAsync("replace into ApplicationConfigs (PathHash, Path, Server, Binding, AppPoolName, AppType, ServiceName, DisplayName) values " +
                    "(@PathHash, @Path, @Server, @Binding, @AppPoolName, @AppType, @ServiceName, @DisplayName)", c);
            }
        }

    }
}