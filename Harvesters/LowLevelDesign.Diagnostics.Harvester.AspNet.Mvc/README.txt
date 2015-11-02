
## Collecting logs from ASP.NET MVC applications ##

The package contains an exception filter which will send logs to the log store (Castle). Add a key **lowleveldesign.diagnostics.url** in the appsettings section of the configuration file to store the main diagnostics application (Castle) url, eg.:

```xml
<configuration>
  <appSettings>
    <add key="lowleveldesign.diagnostics.url" value="http://diagnostics.mycompany.com" />
  </appSettings>
</configuration>
```

Then you need to inject the error filter into the MVC processing pipeline. You can either make it on the controller, action or on the global level. Make sure that no other error filter precedes  this filter as no exceptions will be logged. Example usage for each level:

### Global configuration ###

```csharp
public static void RegisterGlobalFilters()
{
    GlobalFilters.Filters.Add(new DiagnosticsKitHandleErrorAttribute());
}
```

### Controller level ###

```csharp
[DiagnosticsKitHandleError]
public class TestController : Controller
{
        public ActionResult Index()
        {
            return Content("test");
        }
}
```

### Action level ###

```csharp
public class TestController : Controller
{
        [DiagnosticsKitHandleError]
        public ActionResult Index()
        {
            return Content("test");
        }
}
```
