nuget pack -Build -Properties "Configuration=Release" LowLevelDesign.Diagnostics.Commons\LowLevelDesign.Diagnostics.Commons.csproj
nuget pack -Build -Properties "Configuration=Release" LowLevelDesign.Diagnostics.Commons.net35\LowLevelDesign.Diagnostics.Commons.net35.csproj
nuget pack -Build -Properties "Configuration=Release" LogStores\LowLevelDesign.Diagnostics.LogStore.Commons\LowLevelDesign.Diagnostics.LogStore.Commons.csproj
nuget pack -Build -Properties "Configuration=Release" Harvesters\LowLevelDesign.Diagnostics.Harvester.AspNet\LowLevelDesign.Diagnostics.Harvester.AspNet.csproj
nuget pack -Build -Properties "Configuration=Release" Harvesters\LowLevelDesign.Diagnostics.Harvester.AspNet.Mvc\LowLevelDesign.Diagnostics.Harvester.AspNet.Mvc.csproj
nuget pack -Build -Properties "Configuration=Release" Harvesters\LowLevelDesign.Diagnostics.Harvester.AspNet.WebAPI\LowLevelDesign.Diagnostics.Harvester.AspNet.WebAPI.csproj
nuget pack -Build -Properties "Configuration=Release" Harvesters\LowLevelDesign.Diagnostics.Harvester.log4net\LowLevelDesign.Diagnostics.Harvester.log4net.csproj
nuget pack -Build -Properties "Configuration=Release" Harvesters\LowLevelDesign.Diagnostics.Harvester.NLog\LowLevelDesign.Diagnostics.Harvester.NLog.csproj
nuget pack -Build -Properties "Configuration=Release" Harvesters\LowLevelDesign.Diagnostics.Harvester.SystemDiagnostics\LowLevelDesign.Diagnostics.Harvester.SystemDiagnostics.csproj
pause
