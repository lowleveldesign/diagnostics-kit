using LowLevelDesign.Diagnostics.Commons.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LowLevelDesign.Diagnostics.Models
{
    public interface ILogStore
    {
        void AddLogRecord(LogRecord logrec);
    }
}
