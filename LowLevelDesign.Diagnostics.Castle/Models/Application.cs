using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LowLevelDesign.Diagnostics.Castle.Models
{
    public sealed class Application
    {
        public String Name { get; set; }

        public String Path { get; set; }

        public bool IsExcluded { get; set; }
    }
}