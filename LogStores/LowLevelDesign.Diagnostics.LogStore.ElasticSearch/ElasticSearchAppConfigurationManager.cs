using LowLevelDesign.Diagnostics.Commons.Models;
using LowLevelDesign.Diagnostics.LogStore.Commons.Config;
using LowLevelDesign.Diagnostics.LogStore.Commons.Models;
using LowLevelDesign.Diagnostics.LogStore.ElasticSearch.Models;
using Nest;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace LowLevelDesign.Diagnostics.LogStore.ElasticSearch
{
    public sealed class ElasticSearchAppConfigurationManager : IAppConfigurationManager
    {
        private const string AppConfIndexName = ElasticSearchClientConfiguration.MainConfigIndex;

        private readonly ElasticClient eclient;

        public ElasticSearchAppConfigurationManager()
        {
            eclient = ElasticSearchClientConfiguration.CreateClient(AppConfIndexName);
        }

        public async Task AddOrUpdateAppAsync(Application app)
        {
            if (app == null || app.Path == null)
            {
                throw new ArgumentException("app is null or app.Path is null");
            }
            if (string.IsNullOrEmpty(app.Name))
            {
                // if name is not provided we need to assign a default one
                app.Name = Path.GetFileName(app.Path.TrimEnd(Path.DirectorySeparatorChar));
            }

            var eapp = new ElasticApplication();
            Map(app, eapp);
            await eclient.IndexAsync<ElasticApplication>(eapp, ind => ind.Index(AppConfIndexName));
        }

        public async Task AddOrUpdateAppServerConfigAsync(ApplicationServerConfig config)
        {
            if (config == null || config.AppPath == null || config.Server == null)
            {
                throw new ArgumentException("AppPath and Server must be provided");
            }
            var econfig = new ElasticApplicationConfig();
            Map(config, econfig);
            await eclient.IndexAsync<ElasticApplicationConfig>(econfig, ind => ind.Index(AppConfIndexName));
        }

        public async Task<Application> FindAppAsync(string path)
        {
            if (path == null)
            {
                throw new ArgumentException("path is null");
            }
            var id = BitConverter.ToString(MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(path))).Replace("-", string.Empty);
            var qres = await eclient.GetAsync<ElasticApplication>(id, AppConfIndexName);

            if (!qres.Found)
            {
                return null;
            }

            var res = new Application();
            Map(qres.Source, res);
            return res;
        }

        public async Task<IEnumerable<ApplicationServerConfig>> GetAppConfigsAsync(string[] appPaths, string server = null)
        {
            var filter = Filter<ElasticApplicationConfig>.Terms(econf => econf.Path, appPaths);
            if (server != null)
            {
                filter = Filter<ElasticApplicationConfig>.And(f => filter, f => f.Term(econf => econf.Server, server));
            }
            return (await eclient.SearchAsync<ElasticApplicationConfig>(q => q.Filter(filter).Index(
                AppConfIndexName).Take(400))).Hits.Select(h => {
                    var conf = new ApplicationServerConfig();
                    Map(h.Source, conf);
                    return conf;
                });
        }

        public async Task<IEnumerable<Application>> GetAppsAsync()
        {
            return (await eclient.SearchAsync<ElasticApplication>(s => s.Index(AppConfIndexName).MatchAll().Take(400)
                    .SortAscending(app => app.Path))).Documents.Select(d => {
                var app = new Application();
                Map(d, app);
                return app;
            });
        }

        public async Task UpdateAppPropertiesAsync(Application app, string[] propertiesToUpdate)
        {
            if (app.Path == null)
            {
                throw new ArgumentException("path is null");
            }
            var id = BitConverter.ToString(MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(app.Path))).Replace("-", string.Empty);
            var eapp = (await eclient.GetAsync<ElasticApplication>(id, AppConfIndexName)).Source;
            Map(app, eapp, propertiesToUpdate);
            await eclient.UpdateAsync<ElasticApplication>(u => u.Index(AppConfIndexName).Doc(eapp));
        }

        private void Map(ElasticApplication from, Application to)
        {
            to.DaysToKeepLogs = from.DaysToKeepLogs;
            to.IsExcluded = from.IsExcluded;
            to.IsHidden = from.IsHidden;
            to.Name = from.Name;
            to.Path = from.Path;
        }

        private void Map(Application from, ElasticApplication to, string[] properties = null)
        {
            to.Id = BitConverter.ToString(MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(from.Path))).Replace("-", string.Empty);
            to.Path = from.Path;
            if (properties == null || properties.Contains("Name", StringComparer.OrdinalIgnoreCase)) {
                to.Name = from.Name;
            }
            if (properties == null || properties.Contains("DaysToKeepLogs", StringComparer.OrdinalIgnoreCase)) {
                to.DaysToKeepLogs = from.DaysToKeepLogs;
            }
            if (properties == null || properties.Contains("IsExcluded", StringComparer.OrdinalIgnoreCase)) {
                to.IsExcluded = from.IsExcluded;
            }
            if (properties == null || properties.Contains("IsHidden", StringComparer.OrdinalIgnoreCase)) {
                to.IsHidden = from.IsHidden;
            }
        }

        private void Map(ApplicationServerConfig from, ElasticApplicationConfig to)
        {
            to.Id = BitConverter.ToString(MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(from.AppPath + ":" +
                from.Server))).Replace("-", string.Empty);
            to.Path = from.AppPath;
            to.Server = from.Server;
            to.ServerFqdnOrIp = from.ServerFqdnOrIp;
            to.ServiceName = from.ServiceName;
            to.AppPoolName = from.AppPoolName;
            to.AppType = from.AppType;
            to.Binding = from.Bindings != null ? string.Join("|", from.Bindings) : null;
            to.DisplayName = from.DisplayName;
        }

        private void Map(ElasticApplicationConfig from, ApplicationServerConfig to)
        {
            to.AppPath = from.Path;
            to.Server = from.Server;
            to.ServerFqdnOrIp = from.ServerFqdnOrIp;
            to.ServiceName = from.ServiceName;
            to.AppPoolName = from.AppPoolName;
            to.AppType = from.AppType;
            to.Bindings = from.Binding != null ? from.Binding.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries) : null;
            to.DisplayName = from.DisplayName;
        }

        public async Task<string> GetGlobalSettingAsync(string key)
        {
            var resp = await eclient.GetAsync<ElasticGlobalSetting>(key, AppConfIndexName);
            return resp.Found ? resp.Source.ConfValue : null;
        }

        public string GetGlobalSetting(string key)
        {
            var resp = eclient.Get<ElasticGlobalSetting>(q => q.Index(AppConfIndexName).Id(key));
            return resp.Found ? resp.Source.ConfValue : null;
        }

        public async Task SetGlobalSettingAsync(string key, string value)
        {
            await eclient.IndexAsync<ElasticGlobalSetting>(new ElasticGlobalSetting {
                Id = key, ConfValue = value
            }, ind => ind.Index(AppConfIndexName));
        }
    }
}
