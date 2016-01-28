using System.Collections.Generic;
using Fiddler;
using LowLevelDesign.Diagnostics.Bishop.Config;

namespace LowLevelDesign.Diagnostics.Bishop
{
    public interface IBishop
    {
        IEnumerable<string> AvailableServers { get; }

        string PluginConfigurationFilePath { get; }

        bool IsDiagnosticsCastleConfigured();

        void ReloadSettings(PluginSettings newSettings);

        void SelectApplicationServer(string srv);

        void SelectCustomServer(string customServerAddressWithPort);

        void SetHttpsLocalInterception(bool enabled);

        void TurnOffServerRedirection();
    }
}