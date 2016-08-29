using LowLevelDesign.Diagnostics.Musketeer.Models;
using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Session;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace LowLevelDesign.Diagnostics.Musketeer.CLR
{
    sealed class ClrDac : IDisposable
    {
        private const ushort DCStartInitEventId = 147;
        private const ushort AppDomainDCStartEventId = 157;

        private static readonly TimeSpan rundownTimeout = TimeSpan.FromSeconds(3);
        private static readonly int currentProcessId = Process.GetCurrentProcess().Id;
        private readonly Dictionary<int, List<AppDomainInfo>> processAppDomainsMap = new Dictionary<int, List<AppDomainInfo>>();
        private readonly TraceEventSession session;

        private DateTime lastTimeEventWasReceivedUtc;
        private bool completed;

        public ClrDac()
        {
            session = new TraceEventSession("MusketeerEtwSession");
            session.Source.Dynamic.All += ProcessTraceEvent;
            session.EnableProvider("Microsoft-Windows-DotNETRuntimeRundown", TraceEventLevel.Verbose,
                0x40L | // StartRundownKeyword
                0x8L    // LoaderRundownKeyword 
            );
        }

        public void CollectAppDomainInfo()
        {
            Debug.Assert(!completed);

            ThreadPool.QueueUserWorkItem(WatchDog);
            lastTimeEventWasReceivedUtc = DateTime.UtcNow;

            session.Source.Process();
            completed = true;
        }

        private void WatchDog(object o)
        {
            while (true) {
                Thread.Sleep(TimeSpan.FromSeconds(1));
                if (session.IsActive && DateTime.UtcNow.Subtract(
                    lastTimeEventWasReceivedUtc) > rundownTimeout) {
                    // rundown should be finished by now
                    session.Stop();
                    break;
                }
            }
        }

        void ProcessTraceEvent(TraceEvent traceEvent)
        {
            lastTimeEventWasReceivedUtc = DateTime.UtcNow;

            if (traceEvent.ProcessID == currentProcessId) {
                return;
            }

            if ((ushort)traceEvent.ID == DCStartInitEventId) {
                Debug.Assert(!processAppDomainsMap.ContainsKey(traceEvent.ProcessID));
                processAppDomainsMap.Add(traceEvent.ProcessID, new List<AppDomainInfo>());
            } else if ((ushort)traceEvent.ID == AppDomainDCStartEventId) {
                Debug.Assert(processAppDomainsMap.ContainsKey(traceEvent.ProcessID));
                processAppDomainsMap[traceEvent.ProcessID].Add(new AppDomainInfo() {
                    Id = (long)traceEvent.PayloadByName("AppDomainID"),
                    Name = (string)traceEvent.PayloadByName("AppDomainName")
                });
            }
        }

        public IEnumerable<int> GetManagedProcessIds()
        {
            Debug.Assert(completed);
            return processAppDomainsMap.Keys;
        }

        public AppDomainInfo[] GetAppDomainsForProcess(int pid)
        {
            Debug.Assert(completed);
            List<AppDomainInfo> appDomains;
            return processAppDomainsMap.TryGetValue(pid, out appDomains) ? appDomains.ToArray() : new AppDomainInfo[0];
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (session.IsActive) {
                session.Stop();
            }
            if (disposing) {
                session.Dispose();
            }
        }

        ~ClrDac()
        {
            Dispose(false);
        }
    }
}
