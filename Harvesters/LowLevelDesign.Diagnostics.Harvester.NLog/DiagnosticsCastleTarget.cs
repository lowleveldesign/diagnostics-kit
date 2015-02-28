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

        protected override void Write(LogEventInfo logEvent) {
        }

        protected override void CloseTarget() {
            base.CloseTarget();

            // close any opened HTTP connection
        }
    }
}
