
param([Parameter(Mandatory=$True)][string]$ReleaseFolder,
      [Parameter(Mandatory=$True)][string]$ProductName,
      [Parameter(Mandatory=$True)][string]$OutputFolder,
      [Parameter(Mandatory=$True)][string]$MainAssemblyName)

$cmdpath = Split-Path $MyInvocation.MyCommand.Path
$version = $(Get-ChildItem -Path "$ReleaseFolder" -Filter $MainAssemblyName | Sort-Object -Property Name | Select-Object -First 1).VersionInfo.FileVersion
$outputZipPath = "$OutputFolder\$ProductName`_$version.zip"

Write-Host $ProductName
Write-Host $outputZipPath

Remove-Item -Force "$ReleaseFolder\*.zip"
& $cmdpath\7za.exe a -r -x!*vshost* "$outputZipPath" "$ReleaseFolder\*"

