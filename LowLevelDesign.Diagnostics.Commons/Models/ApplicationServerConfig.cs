using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LowLevelDesign.Diagnostics.Commons.Models
{
    public sealed class ApplicationServerConfig
    {
        public String AppPath { get; set; }

        public String Server { get; set; }

        public String AppPoolName { get; set; }

        public String[] Bindings { get; set; }
    }

}
