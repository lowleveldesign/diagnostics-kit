using System;

namespace LowLevelDesign.Diagnostics.LogStore.Commons.Models
{
    public class LastApplicationStatus
    {
        private static readonly TimeSpan StatusValidityPeriod = TimeSpan.FromSeconds(60);

        public string ApplicationPath { get; set; }

        public string Server { get; set; }

        public float Cpu { get; set; }

        public float Memory { get; set; }

        public DateTime? LastPerformanceDataUpdateTimeUtc { get; set; }

        public bool ContainsPerformanceData()
        {
            return LastPerformanceDataUpdateTimeUtc.HasValue && DateTime.UtcNow.Subtract(
                LastPerformanceDataUpdateTimeUtc.Value) < StatusValidityPeriod;
        }

        public string LastErrorType { get; set; }

        public DateTime? LastErrorTimeUtc { get; set; }

        public bool ContainsErrorInformation()
        {
            return LastErrorTimeUtc.HasValue && DateTime.UtcNow.Subtract(
                LastErrorTimeUtc.Value) < StatusValidityPeriod;
        }

        public DateTime LastUpdateTimeUtc { get; set; }

        public string LastErrorTypeName
        {
            get
            {
                if (LastErrorType == null) {
                    return null;
                }
                var ind = LastErrorType.LastIndexOf('.');
                return ind == -1 ? LastErrorType : LastErrorType.Substring(ind + 1);
            }
        }
    }
}
