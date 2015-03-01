using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace LowLevelDesign.Diagnostics.Commons.Models
{
    public struct SyncModel
    {
        public String IpAddr { get; set; }

        public String Path { get; set; }

        public int Port { get; set; }

        public String Host { get; set; }

        public String ToUrl() {
            var b = new StringBuilder("http://");
            b.Append(IpAddr != null ? IpAddr : Host);
            if (Port > 0 && Port != 80) {
                b.AppendFormat(":{0}", Port);
            }
            return b.ToString();
        }
    }
}