using LowLevelDesign.Diagnostics.Bishop.Common;
using LowLevelDesign.Diagnostics.Bishop.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace LowLevelDesign.Diagnostics.Bishop.Tampering
{
    public sealed class CustomTamperingRulesContainer
    {
        private sealed class RequestTransformationWithCompiledRegex
        {
            public Regex RegexToMatchAgainsHost { get; set; }

            public Regex RegexToMatchAgainstPathAndQuery { get; set; }

            public string DestinationPathAndQuery { get; set; }

            public string DestinationHostHeader { get; set; }

            public string[] DestinationIpAddresses { get; set; }

            public ushort[] DestinationPorts { get; set; }
        }

        private static readonly Regex RegexMatchingEverything = new Regex(".*", RegexOptions.Compiled);
        private readonly IEnumerable<RequestTransformationWithCompiledRegex> transformations;

        public CustomTamperingRulesContainer(PluginSettings settings)
        {
            transformations = new List<RequestTransformationWithCompiledRegex>(
                settings.UserDefinedTransformations.Select(s => new RequestTransformationWithCompiledRegex {
                    RegexToMatchAgainsHost = CreateCompiledRegex(s.RegexToMatchAgainstHost),
                    RegexToMatchAgainstPathAndQuery = CreateCompiledRegex(s.RegexToMatchAgainstPathAndQuery),
                    DestinationHostHeader = s.DestinationHostHeader,
                    DestinationPathAndQuery = s.DestinationPathAndQuery,
                    DestinationIpAddresses = s.DestinationIpAddresses,
                    DestinationPorts = s.DestinationPorts
                }));

        }

        private static Regex CreateCompiledRegex(string regexString)
        {
            if (string.IsNullOrEmpty(regexString)) {
                return RegexMatchingEverything;
            }
            return new Regex(regexString, RegexOptions.Compiled |
                RegexOptions.Singleline | RegexOptions.IgnoreCase);
        }

        public void ApplyMatchingTamperingRules(IRequest request, TamperingContext context)
        {
            foreach (var transform in transformations) {
                var hostMatch = transform.RegexToMatchAgainsHost.Match(request.Host);
                var pathAndQueryMatch = transform.RegexToMatchAgainstPathAndQuery.Match(request.PathAndQuery);
                if (hostMatch.Success && pathAndQueryMatch.Success) {
                    var matchedPathAndQuery = new StringBuilder(transform.DestinationPathAndQuery);
                    for(int i = 1; i < pathAndQueryMatch.Groups.Count; i++) {
                        matchedPathAndQuery = matchedPathAndQuery.Replace("$" + i, pathAndQueryMatch.Groups[i].Value);
                    }
                    matchedPathAndQuery.Insert(0, request.PathAndQuery.Substring(0, pathAndQueryMatch.Index));
                    matchedPathAndQuery.Append(request.PathAndQuery.Substring(pathAndQueryMatch.Index + pathAndQueryMatch.Length));

                    context.PathAndQuery = matchedPathAndQuery.Length == 0 ? null : matchedPathAndQuery.ToString();
                    context.HostHeader = transform.DestinationHostHeader;
                    context.CustomServerIpAddresses = transform.DestinationIpAddresses;
                    context.CustomServerPorts = transform.DestinationPorts;
                    break;
                }
            }
        }
    }
}
