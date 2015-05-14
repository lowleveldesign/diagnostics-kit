using LowLevelDesign.Diagnostics.Commons.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LowLevelDesign.Diagnostics.Castle.Models
{
    public class ExtendedApplicationStatus
    {
        public String ApplicationName { get; set; }

        public LastApplicationStatus ApplicationStatus { get; set; }
    }

    public class ApplicationGridModel
    {
        public String[] Servers { get; set; }

        public ExtendedApplicationStatus[] ApplicationStatuses { get; set; }
    }
}