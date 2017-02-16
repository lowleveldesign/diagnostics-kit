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

using Nest;

namespace LowLevelDesign.Diagnostics.LogStore.ElasticSearch.Models
{
    [ElasticsearchType(Name = "appconfigs")]
    internal sealed class ElasticApplicationConfig
    {
        public string Id { get; set; }

        [Keyword(Index = true, Store = true)]
        public string Path { get; set; }

        [Keyword(Index = true, Store = true)]
        public string Server { get; set; }

        [Keyword(Index = true, Store = true)]
        public string ServerFqdnOrIp { get; set; }

        [Keyword(Index = false, Store = true)]
        public string Binding { get; set; }

        [Keyword(Index = true, Store = true)]
        public string AppPoolName { get; set; }

        [Keyword(Index = true, Store = true)]
        public string ServiceName { get; set; }

        [Keyword(Index = true, Store = true)]
        public string DisplayName { get; set; }

        [Keyword(Index = true, Store = true)]
        public string AppType { get; set; }
    }

}
