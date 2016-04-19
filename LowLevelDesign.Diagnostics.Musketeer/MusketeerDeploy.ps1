param([String]$Action = "install")

$here = Split-Path -parent $MyInvocation.MyCommand.Definition
$script = $MyInvocation.MyCommand.Name

$identity = [System.Security.Principal.WindowsIdentity]::GetCurrent()
$principal = New-Object System.Security.Principal.WindowsPrincipal($identity)
if (-not $principal.IsInRole([System.Security.Principal.WindowsBuiltInRole]::Administrator)) {
    Write-Warning "Not running with administrative rights. Attempting to elevate..."
    $command = "-ExecutionPolicy bypass -File `"$here\$script`""
    Start-Process powershell -verb runas -argumentlist $command
    Exit
}

if ($Action -eq "install") {
    Write-Host "========= Installing Musketeer Service ========="
    & "$here\Musketeer.exe" install

    Write-Host "`nStarting the Musketeer service"
    Get-Service LowLevelDesign.Diagnostics.Musketeer | Start-Service
} elseif ($Action -eq "uninstall") {
    Write-Host "========= Uninstalling Musketeer Service ========="
    Write-Host "`nStopping the Musketeer service"
    Get-Service LowLevelDesign.Diagnostics.Musketeer | Stop-Service
    & "$here\Musketeer.exe" uninstall
} else {
    Write-Host "ERROR: Unknown action. Please select either install or uninstall."
}

Write-Host "`nPress any key to continue..."
$x = $host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
