using Fiddler;
using LowLevelDesign.Diagnostics.Bishop.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LowLevelDesign.Diagnostics.Bishop
{
    public class FiddlerPlugin : IAutoTamper
    {
        private PluginSettings settings;
        private Tamperer tamperer;

        public void AutoTamperRequestAfter(Session oSession)
        {
        }

        public void AutoTamperRequestBefore(Session oSession)
        {
            
        }

        public void AutoTamperResponseAfter(Session oSession)
        {
        }

        public void AutoTamperResponseBefore(Session oSession)
        {
        }

        public void OnBeforeReturningError(Session oSession)
        {
        }

        public void OnBeforeUnload()
        {
        }

        public void OnLoad()
        {
            // FIXME load the configuration file

            // ask for Diagnostics Kit 

            // connect with the Diagnostics Castle
        }
    }
}
