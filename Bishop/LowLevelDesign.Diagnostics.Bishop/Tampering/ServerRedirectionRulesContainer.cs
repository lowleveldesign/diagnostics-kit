using LowLevelDesign.Diagnostics.Bishop.Common;
using LowLevelDesign.Diagnostics.Commons.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LowLevelDesign.Diagnostics.Bishop.Tampering
{
    public sealed class ServerRedirectionRulesContainer
    {
        private class ApplicationBinding
        {
            public string Protocol { get; set; }

            public string HostnameOrIpAddress { get; set; }

            public int Port { get; set; }

            public string HostForHeader { get; set; }
        }

        private readonly Dictionary<string, List<ApplicationBinding>> serverAppsBindings;

        public ServerRedirectionRulesContainer(IEnumerable<ApplicationServerConfig> applicationsConfigs)
        {
            serverAppsBindings = new Dictionary<string, List<ApplicationBinding>>(StringComparer.Ordinal);

            foreach (var appConfig in applicationsConfigs)
            {
                List<ApplicationBinding> bindings;
                if (!serverAppsBindings.TryGetValue(appConfig.Server, out bindings)) {
                    bindings = new List<ApplicationBinding>();
                }
                foreach (var bindingString in appConfig.Bindings) {
                    bindings.Add(ParseBindingString(appConfig.ServerFqdnOrIp, bindingString));
                }
            }
        }

        private static ApplicationBinding ParseBindingString(string serverFqdnOrIp, string bindingString)
        {
            var bindingTokens = bindingString.Split(':');
            var binding = new ApplicationBinding();
            if (bindingTokens.Length > 0)
            {
                if (bindingTokens[0].StartsWith("http://", StringComparison.Ordinal)) {
                    binding.Protocol = "http";
                    binding.HostnameOrIpAddress = bindingTokens[0].Remove(0, "http://".Length);
                } else if (bindingTokens[0].StartsWith("https://", StringComparison.Ordinal)) {
                    binding.Protocol = "https";
                    binding.HostnameOrIpAddress = bindingTokens[0].Remove(0, "https://".Length);
                }
                if (string.Equals(binding.HostnameOrIpAddress, "*", StringComparison.Ordinal)) {
                    binding.HostnameOrIpAddress = serverFqdnOrIp;
                }
                if (binding.Protocol != null)
                {
                    if (bindingTokens.Length > 1)
                    {
                        int port;
                        int.TryParse(bindingTokens[1], out port);
                        binding.Port = port;
                    }
                    if (bindingTokens.Length > 2) {
                        binding.HostForHeader = bindingTokens[2];
                    }
                }
            }
            return binding;
        }

        public void ApplyMatchingTamperingRules(IRequest request, TamperingContext context, string selectedServer)
        {
            List<ApplicationBinding> bindings;
            if (serverAppsBindings.TryGetValue(selectedServer, out bindings))
            {
                SortedList<int, ApplicationBinding> matchedBindings = new SortedList<int, ApplicationBinding>();
                foreach (var binding in bindings) {
                    int matchingPoints = 0;
                    if (context.CustomServerIpAddresses.Contains(binding.HostnameOrIpAddress)) {
                        matchingPoints += 2;
                    }
                    if (request.Host.IndexOf(binding.HostForHeader, StringComparison.InvariantCultureIgnoreCase) >= 0) {
                        matchingPoints += 1;
                    }
                    if (matchingPoints > 0) {
                        matchedBindings.Add(matchingPoints, binding);
                    }
                }

                if (bindings.Any()) {
                    var bestFoundBinding = bindings.Last();
                    context.ServerTcpAddressWithPort = string.Format("{0}:{1}", bestFoundBinding.HostnameOrIpAddress, 
                        bestFoundBinding.Port);
                    context.HostHeader = bestFoundBinding.HostForHeader;
                }
                //FIXME what about https ??
            }
        }

        public IEnumerable<string> AvailableServers {
            get { return serverAppsBindings.Keys; }
        }
    }
}
