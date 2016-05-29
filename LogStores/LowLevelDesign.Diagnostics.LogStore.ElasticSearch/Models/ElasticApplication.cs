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
    [ElasticsearchType(Name = "apps")]
    internal sealed class ElasticApplication
    {
        public string Id { get; set; }

        [String(Index = FieldIndexOption.NotAnalyzed, Store = true)]
        public string Path { get; set; }

        [Number(Index = NonStringIndexOption.No, Store = true)]
        public byte? DaysToKeepLogs { get; set; }

        [Boolean(Index = NonStringIndexOption.No, Store = true)]
        public bool IsExcluded { get; set; }

        [Boolean(Index = NonStringIndexOption.No, Store = true)]
        public bool IsHidden { get; set; }

        [String(Index = FieldIndexOption.NotAnalyzed, Store = true)]
        public string Name { get; set; }
    }
}
