using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LowLevelDesign.Diagnostics.Castle.Models
{
    public sealed class SyncModel
    {
        public String IpAddr { get; set; }

        public int Port { get; set; }

        public String Host { get; set; }
    }
}