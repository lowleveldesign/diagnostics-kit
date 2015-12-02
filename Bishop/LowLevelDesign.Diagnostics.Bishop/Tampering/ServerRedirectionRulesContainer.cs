using LowLevelDesign.Diagnostics.Commons.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LowLevelDesign.Diagnostics.Bishop.Tampering
{
    public sealed class ServerRedirectionRulesContainer
    {
        private class ApplicationBinding
        {
            public string Protocol { get; set; }

            public string IpAddress { get; set; }

            public int Port { get; set; }

            public string Host { get; set; }
        }

        private readonly Dictionary<string, List<ApplicationBinding>> serverAppsBindings;

        public ServerRedirectionRulesContainer(IEnumerable<ApplicationServerConfig> applicationsConfigs)
        {
            serverAppsBindings = new Dictionary<string, List<ApplicationBinding>>();

        }

        public IEnumerable<string> AvailableServers {
            get { return serverAppsBindings.Keys; }
        }
    }
}
