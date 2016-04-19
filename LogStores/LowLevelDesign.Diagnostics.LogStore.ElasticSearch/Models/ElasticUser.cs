using Nest;
using System;
using System.Collections.Generic;

namespace LowLevelDesign.Diagnostics.LogStore.ElasticSearch.Models
{
    [ElasticType(Name = "appusers")]
    public class ElasticUser
    {
        public string Id { get; set; }

        [ElasticProperty(Index = FieldIndexOption.NotAnalyzed, Store = true)]
        public string UserName { get; set; }

        [ElasticProperty(Index = FieldIndexOption.No, Store = true)]
        public string PasswordHash { get; set; }

        [ElasticProperty(Index = FieldIndexOption.No, Type = FieldType.Date, Store = true)]
        public DateTime RegistrationDateUtc { get; set; }

        [ElasticProperty(Index = FieldIndexOption.No, Store = true)]
        public IDictionary<string, string> Claims { get; set; }
    }
}
