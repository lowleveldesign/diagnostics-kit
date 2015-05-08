using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LowLevelDesign.Diagnostics.Commons.Models;

namespace LowLevelDesign.Diagnostics.Castle.Models
{
    public class ApplicationGrid
    {
        public SortedSet<String> Servers;

        public Dictionary<String, Dictionary<String, LastApplicationStatus>> ApplicationStatuses;
    }
}