using LowLevelDesign.Diagnostics.Bishop.Common;
using LowLevelDesign.Diagnostics.Bishop.Config;
using System;

namespace LowLevelDesign.Diagnostics.Bishop.Tampering
{
    public class TamperingRulesContainer
    {
        private readonly PluginSettings settings;

        public TamperingRulesContainer(PluginSettings settings)
        {
            this.settings = settings;
        }

        public TamperParameters FindMatchingTamperParameters(RequestDescriptor request)
        {
            var url = request.FiddlerSession.fullUrl;

            foreach 

            throw new NotImplementedException();
        }
    }
}
