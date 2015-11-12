using LowLevelDesign.Diagnostics.Bishop.Config;
using LowLevelDesign.Diagnostics.Bishop.Models;
using System;

namespace LowLevelDesign.Diagnostics.Bishop
{
    public class Tamperer
    {
        private readonly PluginSettings settings;

        public Tamperer(PluginSettings settings)
        {
            this.settings = settings;
        }

        public TamperParameters FindMatchingRedirectConfiguration()
        {
            throw new NotImplementedException();
        }
    }
}
