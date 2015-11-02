
In order to send NLog log events to the Diagnostics Castle you need to add a Diagnostics Kit extension assembly. Example:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
  <configSections>
    <section name="nlog" type="NLog.Config.ConfigSectionHandler, NLog"/>
  </configSections>
  <nlog throwExceptions="true">
    <extensions>
      <add assembly="LowLevelDesign.Diagnostics.Harvester.NLog" />
    </extensions>
    <targets>
      <target name="diag" type="DiagnosticsKit" diagnosticsCastleUrl="http://your-diagnostics-castle-url"
              layout="${longdate}|${processid}(${threadid})|${logger}|${uppercase:${level}}|${message}${onexception:|Exception occurred\:${exception:format=tostring}}" />
    </targets>
    <rules>
      <logger name="*" minLevel="Trace" writeTo="diag" />
    </rules>
  </nlog>

</configuration>
```

Adjust logger levels and set the Diagnostics Castle url to a valid one and NLog log events should start appearing in the diagnostics log viewer.

