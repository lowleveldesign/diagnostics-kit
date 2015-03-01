using LowLevelDesign.Diagnostics.Commons.Connectors;
using NLog;
using NLog.Config;
using NLog.Targets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace LowLevelDesign.Diagnostics.Harvester.NLog
{
    [Target("DiagnosticsCastle")]
    public class DiagnosticsCastleTarget : TargetWithLayout
    {
        [RequiredParameter]
        public String DiagnosticsCastleUrl { get; set; }


        private HttpCastleConnector connector;
        protected override void InitializeTarget() {
            base.InitializeTarget();

            connector = new HttpCastleConnector(new Uri(DiagnosticsCastleUrl));
        }

        protected override void Write(LogEventInfo logEvent) {
            // FIXME convert to LogRecord

            //var logrec
            //connector.SendLogRecord(logrec)
        }

        protected override void CloseTarget() {
            base.CloseTarget();

            // close any opened HTTP connection
        }
    }
}
