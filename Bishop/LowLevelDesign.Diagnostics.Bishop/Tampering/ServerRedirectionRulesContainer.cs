/**
 *  Part of the Diagnostics Kit
 *
 *  Copyright (C) 2016  Sebastian Solnica
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.

 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 */

using LowLevelDesign.Diagnostics.Bishop.Common;
using LowLevelDesign.Diagnostics.Commons.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace LowLevelDesign.Diagnostics.Bishop.Tampering
{
    public sealed class ServerRedirectionRulesContainer
    {
        private class ApplicationBinding
        {
            public string Protocol { get; set; }

            public string HostnameOrIpAddress { get; set; }

            public ushort Port { get; set; }

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
                    serverAppsBindings.Add(appConfig.Server, bindings);
                }
                foreach (var bindingString in appConfig.Bindings) {
                    bindings.Add(ParseBindingString(appConfig.ServerFqdnOrIp, bindingString));
                }
            }
        }

        private static ApplicationBinding ParseBindingString(string serverFqdnOrIp, string bindingString)
        {
            var binding = new ApplicationBinding {
                Protocol = FindProtocol(bindingString)
            };
            if (binding.Protocol == null) {
                return binding;
            }

            var bindingTokens = bindingString.Remove(0, (binding.Protocol + "://").Length).Split(':');
            if (bindingTokens.Length > 0)
            {
                if (string.IsNullOrEmpty(bindingTokens[0]) || 
                    string.Equals(bindingTokens[0], "*", StringComparison.Ordinal)) {
                    binding.HostnameOrIpAddress = serverFqdnOrIp;
                } else {
                    binding.HostnameOrIpAddress = bindingTokens[0];
                }
                if (binding.Protocol != null)
                {
                    if (bindingTokens.Length > 1)
                    {
                        ushort port;
                        ushort.TryParse(bindingTokens[1], out port);
                        binding.Port = port;
                    }
                    if (bindingTokens.Length > 2) {
                        binding.HostForHeader = string.IsNullOrEmpty(bindingTokens[2]) ? null : bindingTokens[2];
                    }
                }
            }
            return binding;
        }

        private static string FindProtocol(string bindingString)
        {
            if (bindingString.StartsWith("http://", StringComparison.Ordinal)) {
                return "http";
            }
            if (bindingString.StartsWith("https://", StringComparison.Ordinal)) {
                return "https";
            }
            return null;
        }

        public void ApplyMatchingTamperingRules(IRequest request, TamperingContext context, string selectedServer)
        {
            List<ApplicationBinding> bindings;
            if (serverAppsBindings.TryGetValue(selectedServer, out bindings))
            {
                SortedList<float, ApplicationBinding> matchedBindings = new SortedList<float, ApplicationBinding>();
                foreach (var binding in bindings) {
                    float matchingPoints = 0;
                    if (context.IsIpAddressValidForRedirection(binding.HostnameOrIpAddress)) {
                        matchingPoints += 3;
                    }
                    if (context.IsPortValidForRedirection(binding.Port)) {
                        matchingPoints += 3;
                    }
                    if (IsHostMatchingBinding(binding, context.HostHeader ?? request.Host)) { 
                        matchingPoints += 2 + binding.HostForHeader.Length / 100.0f;
                    }
                    if (string.Equals(binding.Protocol, request.Protocol, StringComparison.OrdinalIgnoreCase)) {
                        matchingPoints += 1;
                    }
                    if (matchingPoints > 1 && !matchedBindings.ContainsKey(matchingPoints)) {
                        matchedBindings.Add(matchingPoints, binding);
                    }
                }

                if (matchedBindings.Any()) {
                    var bestFoundBinding = matchedBindings.Values.Last();
                    context.Protocol = context.Protocol ?? bestFoundBinding.Protocol;
                    context.ServerTcpAddressWithPort = string.Format("{0}:{1}", bestFoundBinding.HostnameOrIpAddress, 
                        bestFoundBinding.Port);
                    context.HostHeader = bestFoundBinding.HostForHeader;
                }
            }
        }

        private static bool IsHostMatchingBinding(ApplicationBinding binding, string host)
        {
            Debug.Assert(host != null);
            return !string.IsNullOrEmpty(binding.HostForHeader) &&
                host.IndexOf(binding.HostForHeader, StringComparison.InvariantCultureIgnoreCase) >= 0;
        }

        public IEnumerable<string> AvailableServers {
            get { return serverAppsBindings.Keys; }
        }
    }
}
