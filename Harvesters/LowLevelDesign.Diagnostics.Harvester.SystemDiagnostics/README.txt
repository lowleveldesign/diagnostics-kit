
Add the following section to your application configuration file:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<configuation>
  ...
  <system.diagnostics>
    <trace autoflush="true">
      <listeners>
        <add name="diag" />
      </listeners>
    </trace>
    <sharedListeners>
      <add name="diag" type="LowLevelDesign.Diagnostics.Harvester.SystemDiagnostics.DiagnosticsKitTraceListener, LowLevelDesign.Diagnostics.Harvester.SystemDiagnostics"
           initializeData="http://your-diagnostics-castle-url" />
    </sharedListeners>
    <sources>
      <source name="TestSource" switchValue="Verbose">
        <listeners>
          <add name="diag" />
        </listeners>
      </source>
    </sources>
  </system.diagnostics>
  ...
</configuration>
```

Replace the `TestSource` with a trace source name used in your application and set the Diagnostics Castle url to a valid one. Of course you may also add other trace sources. The following configuration will send logs to the Diagnostics Castle for both `Trace.Write` calls as well as `TraceSource` events, eg.:

```csharp
TraceSource logger = new TraceSource("TestSource");
logger.TraceEvent(TraceEventType.Information, 0, "test-system.diagnostics-tracesource");

Trace.WriteLine("test-system.diagnostics-trace");
```
