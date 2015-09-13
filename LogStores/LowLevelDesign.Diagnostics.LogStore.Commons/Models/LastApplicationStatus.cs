using System;

namespace LowLevelDesign.Diagnostics.LogStore.Commons.Models
{
    public class LastApplicationStatus
    {
        public String ApplicationPath { get; set; }

        public String Server { get; set; }

        public float? Cpu { get; set; }

        public float? Memory { get; set; }

        public String LastErrorType { get; set; }

        public DateTime? LastErrorTimeUtc { get; set; }

        public DateTime? LastUpdateTimeUtc { get; set; }

        public String LastErrorTypeName
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
