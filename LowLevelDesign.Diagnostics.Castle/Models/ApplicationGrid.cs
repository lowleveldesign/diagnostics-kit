using LowLevelDesign.Diagnostics.LogStore.Commons.Models;
using System;
using System.Collections.Generic;

namespace LowLevelDesign.Diagnostics.Castle.Models
{
    public class ApplicationGridModel
    {
        public DateTime LastUpdateTime { get; set; }

        public String[] Servers { get; set; }

        public IDictionary<String, Application> Applications { get; set; }

        public IDictionary<String, IDictionary<String, LastApplicationStatus>> ApplicationStatuses { get; set; }
    }
}