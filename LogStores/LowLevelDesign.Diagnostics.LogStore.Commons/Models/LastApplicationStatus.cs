using System;

namespace LowLevelDesign.Diagnostics.LogStore.Commons.Models
{
    public class LastApplicationStatus
    {
        public string ApplicationPath { get; set; }

        public string Server { get; set; }

        public float? Cpu { get; set; }

        public float? Memory { get; set; }

        public DateTime? LastPerformanceDataUpdateTimeUtc { get; set; }

        public string LastErrorType { get; set; }

        public DateTime? LastErrorTimeUtc { get; set; }

        public DateTime? LastUpdateTimeUtc { get; set; }

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
