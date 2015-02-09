using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LowLevelDesign.Diagnostics.Commons.LogRecords
{
    public sealed class PerformanceLogRecord : GenericLogRecord
    {
        public class PerformanceCounterValue {
            public String CounterName;

            public float Value;
        }

        public IEnumerable<PerformanceCounterValue> CounterValues;
    }
}
