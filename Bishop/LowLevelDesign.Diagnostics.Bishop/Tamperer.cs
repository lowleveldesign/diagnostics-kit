using LowLevelDesign.Diagnostics.Bishop.Config;
using LowLevelDesign.Diagnostics.Bishop.Models;
using System;

namespace LowLevelDesign.Diagnostics.Bishop
{
    public class TamperingRulesContainer
    {
        private readonly PluginSettings settings;

        public TamperingRulesContainer(PluginSettings settings)
        {
            this.settings = settings;
        }

        public TamperParameters FindMatchingTamperParameters()
        {
            throw new NotImplementedException();
        }
    }
}
