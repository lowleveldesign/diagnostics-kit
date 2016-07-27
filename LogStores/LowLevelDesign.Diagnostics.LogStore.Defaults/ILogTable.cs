using LowLevelDesign.Diagnostics.Commons.Models;
using LowLevelDesign.Diagnostics.LogStore.Commons.Models;
using System;
using System.Data;
using System.Threading.Tasks;

namespace LowLevelDesign.Diagnostics.LogStore.Defaults
{
    public interface ILogTable
    {
        bool IsLogTableAvailable(string tableName);

        void CreateLogTableIfNotExists(IDbConnection conn, IDbTransaction tran, string tableName);

        Task<uint> SaveLogRecord(IDbConnection conn, IDbTransaction tran, string tableName, LogRecord logrec);

        Task UpdateApplicationStatus(IDbConnection conn, IDbTransaction tran, string apphash, LastApplicationStatus status);

        Task ManageTablePartitions(IDbConnection conn, string tableName, TimeSpan keepTime);
    }
}
