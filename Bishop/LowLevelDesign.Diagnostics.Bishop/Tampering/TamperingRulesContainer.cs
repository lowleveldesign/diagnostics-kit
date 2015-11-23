using LowLevelDesign.Diagnostics.Bishop.Common;
using LowLevelDesign.Diagnostics.Bishop.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace LowLevelDesign.Diagnostics.Bishop.Tampering
{
    public class TamperingRulesContainer
    {
        private sealed class RequestTransformationWithCompiledRegex
        {
            public Regex RegexToMatchAgainstPathAndQuery { get; set; }

            public string DestinationPathAndQuery { get; set; }

            public string DestinationHostHeader { get; set; }
        }

        private readonly IEnumerable<RequestTransformationWithCompiledRegex> transformations;

        public TamperingRulesContainer(PluginSettings settings)
        {
            transformations = new List<RequestTransformationWithCompiledRegex>(
                settings.UserDefinedTransformations.Select(s => new RequestTransformationWithCompiledRegex {
                    RegexToMatchAgainstPathAndQuery = new Regex(s.RegexToMatchAgainstPathAndQuery, 
                            RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase),
                    DestinationHostHeader = s.DestinationHostHeader,
                    DestinationPathAndQuery = s.DestinationPathAndQuery
                }));

        }

        public void ApplyMatchingTamperParameters(IRequest request, TamperParameters parameters)
        {
            foreach (var transform in transformations) {
                var m = transform.RegexToMatchAgainstPathAndQuery.Match(request.PathAndQuery);
                if (m.Success) {
                    var matchedPathAndQuerty = new StringBuilder(transform.DestinationPathAndQuery);
                    for(int i = 1; i < m.Groups.Count; i++) {
                        matchedPathAndQuerty = matchedPathAndQuerty.Replace("$" + i, m.Groups[i].Value);
                    }
                    matchedPathAndQuerty.Insert(0, request.PathAndQuery.Substring(0, m.Index));
                    matchedPathAndQuerty.Append(request.PathAndQuery.Substring(m.Index + m.Length));

                    parameters.PathAndQuery = matchedPathAndQuerty.ToString();
                    parameters.HostHeader = transform.DestinationHostHeader;
                    break;
                }
            }
        }
    }
}
