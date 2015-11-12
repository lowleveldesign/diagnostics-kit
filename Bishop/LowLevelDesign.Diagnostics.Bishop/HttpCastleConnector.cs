using LowLevelDesign.Diagnostics.Bishop.Config;
using LowLevelDesign.Diagnostics.Commons.Connectors;
using LowLevelDesign.Diagnostics.Commons.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LowLevelDesign.Diagnostics.Bishop
{
    public sealed class BishopHttpCastleConnector
    {

        public BishopHttpCastleConnector(PluginSettings settings)
        {
            // FIXME authentication etc.
        }

        public IEnumerable<ApplicationServerConfig> ReadApplicationConfigs()
        {
            //return JsonConvert.DeserializeObject<ApplicationServerConfig[]>(MakeGetRequest(
            //    string.Format("{0}/conf/appsrvconfig", diagnosticsAddress),
            //    JsonConvert.SerializeObject(configs, Formatting.None, JsonSettings)), JsonSettings);
            throw new NotImplementedException();
        }
    }
}
