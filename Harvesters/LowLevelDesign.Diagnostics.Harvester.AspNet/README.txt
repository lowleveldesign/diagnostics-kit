
## Collecting logs from ASP.NET applications ##

Add a key **lowleveldesign.diagnostics.url** in the appsettings section of the configuration file to store the main diagnostics application (Castle) url, eg.:

```xml
<configuration>
  <appSettings>
    <add key="lowleveldesign.diagnostics.url" value="http://diagnostics.mycompany.com" />
  </appSettings>
</configuration>
```

### Log errors with HttpModule ###

In order to log errors from the HttpModule add the following lines to your application web.config file:

```xml
<?xml version="1.0"?>
<configuration>
  ...
  <system.webServer>
    <modules runAllManagedModulesForAllRequests="true">
      <add name="DiagHttpModule" type="LowLevelDesign.Diagnostics.Harvester.AspNet.DiagnosticsKitHttpModule, LowLevelDesign.Diagnostics.Harvester.AspNet" />
    </modules>
  </system.webServer>
</configuration>
```

### Log errors using ASP.NET Health Monitoring ###

ASP.NET Health Monitoring is a non-invasive method of collecting logs from ASP.NET application and it was present in ASP.NET from the very beginning. Its configuration is a bit complicated, but if you are interested only in collecting error events you can simply add the following lines to the web.config files and events should start appearing in the diagnostics log viewer:

```xml
<?xml version="1.0"?>
<configuration>
  ...
  <system.web>
    ...
    <healthMonitoring enabled="true">
      <providers>
        <add name="DiagnosticsCastleProvider" type="LowLevelDesign.Diagnostics.Harvester.AspNet.DiagnosticsKitWebEventProvider, LowLevelDesign.Diagnostics.Harvester.AspNet" />
      </providers>
      <rules>
        <add name="All Errors for DiagnosticsCastle" eventName="All Errors" provider="DiagnosticsCastleProvider"
            profile="Default" minInstances="1" maxLimit="Infinite" custom="" />
      </rules>
    </healthMonitoring>
  </system.web>
  ...
</configuration>
```
