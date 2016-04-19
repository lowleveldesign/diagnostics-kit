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
                    await esclient.DeleteIndexAsync(index, null); // FIXME what about removing an index which has an alias?
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
                c => c.AddAlias(AliasName)
                    .Analysis(analysisDescriptor => analysisDescriptor.Analyzers(analyzers => analyzers.Add(
                        "loggername", new PatternAnalyzer {
                            Lowercase = true,
                            Pattern = @"[^\w]+"
                        })))
                    .AddMapping<ElasticLogRecord>(m => m.MapFromAttributes())
                    .NumberOfShards(ElasticSearchClientConfiguration.ShardsNum)
                    .NumberOfReplicas(ElasticSearchClientConfiguration.ReplicasNum));
            esclient.Alias(a => a.Add(add => add.Index(indexName).Alias(AliasName)));
        }

        public async Task<IEnumerable<IndexNameMarker>> GetQueryIndicesOrAliasAsync(DateTime fromUtc, DateTime endUtc, int maxIndicesCount)
        {
            var existingIndicesCopy = existingIndices;
            if (existingIndices == null) {
                existingIndicesCopy = existingIndices = new HashSet<string>(
                    await esclient.GetIndicesPointingToAliasAsync(AliasName), StringComparer.Ordinal);
            }
            var indices = new List<IndexNameMarker>(maxIndicesCount);
            for (var dt = fromUtc.Date; dt <= endUtc.Date; dt = dt.AddDays(1)) {
                var indname = string.Format("{0}{1:yyyyMMdd}", baseIndexName, dt);
                if (existingIndices.Contains(indname)) {
                    indices.Add(indname);
                }
                if (indices.Count == maxIndicesCount) {
                    return new IndexNameMarker[] { AliasName };
                }
            }
            return indices;
        }
    }
}
