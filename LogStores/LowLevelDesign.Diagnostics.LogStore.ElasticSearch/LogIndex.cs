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

using LowLevelDesign.Diagnostics.LogStore.ElasticSearch.Models;
using Nest;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace LowLevelDesign.Diagnostics.LogStore.ElasticSearch
{
    internal class LogIndexManager
    {
        public const string AliasName = "lldlogs";
        private const string baseIndexName = "lldlogs";

        private static HashSet<string> existingIndices;

        private DateTime lastCheckedUtcDate;
        private string lastUsedIndexName;
        private readonly Func<DateTime> currentUtcDateRetriever;
        private readonly TraceSource logger = new TraceSource("LowLevelDesign.Diagnostics.LogStore");

        private readonly ElasticClient esclient;

        public LogIndexManager(ElasticClient esclient, Func<DateTime> currentUtcDateRetriever)
        {
            this.esclient = esclient;
            this.currentUtcDateRetriever = currentUtcDateRetriever;
        }

        public string GetCurrentIndexName()
        {
            DateTime currdt = currentUtcDateRetriever();
            if (!currdt.Equals(lastCheckedUtcDate)) {
                lastCheckedUtcDate = currdt;
                lastUsedIndexName = string.Format("{0}{1:yyyyMMdd}", baseIndexName, currdt);
            }
            return lastUsedIndexName;
        }

        public async Task MakeSureCurrentIndexExistsAsync()
        {
            var currIndexName = GetCurrentIndexName();
            var resp = await esclient.IndexExistsAsync(currIndexName);
            if (resp.IsValid && !resp.Exists)
            {
                try
                {
                    await CreateIndexAsync(currIndexName);
                }
                catch (Exception ex)
                {
                    logger.TraceEvent(TraceEventType.Warning, 0, "Error while creating an index - probably two simultanous tries: {0}", ex);
                }
            }
        }

        public async Task ManageIndicesAsync(TimeSpan keepTime)
        {
            var currentUtcDate = currentUtcDateRetriever();
            var pastIndexName = string.Format("{0}{1:yyyyMMdd}", baseIndexName, currentUtcDate.Subtract(keepTime));
            var currIndexName = GetCurrentIndexName();
            var futureIndexName = string.Format("{0}{1:yyyyMMdd}", baseIndexName, currentUtcDate.AddDays(1));

            var indices = await esclient.GetIndicesPointingToAliasAsync(AliasName);
            bool isCurrentIndexCreated = false, isFutureIndexCreated = false;
            foreach (var index in indices)
            {
                if (string.CompareOrdinal(index, pastIndexName) < 0)
                {
                    logger.TraceInformation("Deleting index {0}", index);
                    await esclient.DeleteIndexAsync(index, null); 
                }
                else if (string.CompareOrdinal(index, currIndexName) == 0)
                {
                    isCurrentIndexCreated = true;
                }
                else if (string.CompareOrdinal(index, futureIndexName) == 0)
                {
                    isFutureIndexCreated = true;
                }
            }

            if (!isCurrentIndexCreated)
            {
                logger.TraceInformation("Creating index {0}", currIndexName);
                await CreateIndexAsync(currIndexName);
            }
            if (!isFutureIndexCreated)
            {
                logger.TraceInformation("Creating index {0}", futureIndexName);
                await CreateIndexAsync(futureIndexName);
            }
            existingIndices = null; // force refresh on the next call on query
        }

        private async Task CreateIndexAsync(string indexName)
        {
            await esclient.CreateIndexAsync(indexName,
                c => c.Settings(s => s.Analysis(analysisDescriptor => analysisDescriptor.TokenFilters(
                        tokenFilters => tokenFilters.WordDelimiter("camelfilter", t => t.SplitOnCaseChange())
                    ).Analyzers(analyzers => analyzers.Pattern(
                        "loggername", p => new PatternAnalyzer {
                            Lowercase = true,
                            Pattern = @"[^\w]+"
                        }).Custom("camelcase", analyzer => analyzer.Tokenizer("whitespace").Filters(
                            "camelfilter", "lowercase"))))
                        .NumberOfShards(ElasticSearchClientConfiguration.ShardsNum)
                        .NumberOfReplicas(ElasticSearchClientConfiguration.ReplicasNum))
                    .Mappings(m => m.Map<ElasticLogRecord>(am => am.AutoMap())));
            await esclient.AliasAsync(a => a.Add(add => add.Index(indexName).Alias(AliasName)));
        }

        public async Task<Indices> GetQueryIndicesOrAliasAsync(DateTime fromUtc, DateTime endUtc, int maxIndicesCount)
        {
            var existingIndicesCopy = existingIndices;
            if (existingIndices == null) {
                existingIndicesCopy = existingIndices = new HashSet<string>(
                    await esclient.GetIndicesPointingToAliasAsync(AliasName), StringComparer.Ordinal);
            }
            var indices = new List<string>(maxIndicesCount);
            for (var dt = fromUtc.Date; dt <= endUtc.Date; dt = dt.AddDays(1)) {
                var indname = string.Format("{0}{1:yyyyMMdd}", baseIndexName, dt);
                if (existingIndices.Contains(indname)) {
                    indices.Add(indname);
                }
                if (indices.Count == maxIndicesCount) {
                    return Indices.Index(AliasName);
                }
            }
            return Indices.Parse(string.Join(",", indices)); // FIXME: to optimize - it should be possible to generate the indices collection directly
        }
    }
}
