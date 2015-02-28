using LowLevelDesign.Diagnostics.Commons.Models;

namespace LowLevelDesign.Diagnostics.Commons.Storage
{
    public interface ILogStore
    {
        void AddLogRecord(LogRecord logrec);
    }
}
