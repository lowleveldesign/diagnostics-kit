
## Collecting logs from ASP.NET WebAPI applications ##

The package contains an exception filter which will send logs to the log store (Castle). Then add a key **lowleveldesign.diagnostics.url** in the appsettings section of the configuration file to store the main diagnostics application (Castle) url, eg.:

```xml
<configuration>
  <appSettings>
    <add key="lowleveldesign.diagnostics.url" value="http://diagnostics.mycompany.com" />
  </appSettings>
</configuration>
```

Finally you need to inject the exception filter into the WebAPI processing pipeline. You can either make it on the controller, action or on the global level. Make sure that no other exception filter precedes  this filter as no exceptions will be logged. Example usage for each level:

### Global configuration ###

```csharp
public static class WebApiConfig
{
    public static void Register(HttpConfiguration config)
    {
        ...
        config.Filters.Add(new DiagnosticsKitExceptionFilterAttribute());
        ...
    }
}
```

### Controller level ###

```csharp
[DiagnosticsKitExceptionFilter]
public class TestApiController : ApiController
{
    public String TestMethod() {
        return "test";
    }
}
```

### Action level ###

```csharp
public class TestApiController : ApiController
{
    [DiagnosticsKitExceptionFilter]
    public String TestMethod() {
        return "test";
    }
}
```
