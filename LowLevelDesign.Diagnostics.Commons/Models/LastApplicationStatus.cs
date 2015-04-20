using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LowLevelDesign.Diagnostics.Commons.Models
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
    }
}
