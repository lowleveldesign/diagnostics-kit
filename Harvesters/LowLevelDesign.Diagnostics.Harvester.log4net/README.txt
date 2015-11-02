
### log4net ###

Add the following settings to your application configuration file:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
  <configSections>
    <section name="log4net" type="log4net.Config.Log4NetConfigurationSectionHandler, log4net" />
  </configSections>
  <log4net>
    <appender name="diag" type="LowLevelDesign.Diagnostics.Harvester.log4net.DiagnosticsKitAppender, LowLevelDesign.Diagnostics.Harvester.log4net" >
      <diagnosticsCastleUrl value="http://your-diagnostics-url" />
      <layout type="log4net.Layout.PatternLayout">
        <conversionPattern value="%date [%thread] %-5level %logger [%ndc] - %message%newline" />
      </layout>
    </appender>
    <root>
      <level value="INFO" />
      <appender-ref ref="diag" />
    </root>
  </log4net>
</configuration>
```

Adjust logger parameters to your needs, set the Diagnostics Castle url to a valid one and log4net logs should appear in the diagnostics logs viewer.
