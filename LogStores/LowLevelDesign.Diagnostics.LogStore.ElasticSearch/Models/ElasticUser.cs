using Nest;
using System;
using System.Collections.Generic;

namespace LowLevelDesign.Diagnostics.LogStore.ElasticSearch.Models
{
    [ElasticsearchType(Name = "appusers")]
    public class ElasticUser
    {
        public string Id { get; set; }

        [String(Index = FieldIndexOption.NotAnalyzed, Store = true)]
        public string UserName { get; set; }

        [String(Index = FieldIndexOption.No, Store = true)]
        public string PasswordHash { get; set; }

        [Date(Store = true)]
        public DateTime RegistrationDateUtc { get; set; }

        [Nested(IncludeInParent = true)]
        public IDictionary<string, string> Claims { get; set; }
    }
}
