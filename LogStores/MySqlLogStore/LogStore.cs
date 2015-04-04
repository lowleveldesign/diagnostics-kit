using LowLevelDesign.Diagnostics.Commons.Models;
using LowLevelDesign.Diagnostics.Commons.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MySqlLogStore
{
    public class LogStore : ILogStore
    {
        public void Initialize() {
            throw new NotImplementedException();
        }

        public void AddLogRecord(LogRecord logrec)
        {
            throw new NotImplementedException();
        }

        public void AddLogRecords(IEnumerable<LogRecord> logrecs) {
            throw new NotImplementedException();
        }

        public IEnumerable<LogRecord> SearchLogs(LogSearchCriteria searchCriteria) {
            throw new NotImplementedException();
        }
    }
}
