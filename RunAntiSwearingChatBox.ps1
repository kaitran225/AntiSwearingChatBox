# Anti-Swearing Chat Box Setup Script
Write-Host "Running Anti-Swearing Chat Box Setup Script" -ForegroundColor Green

# Get the script directory
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$solutionPath = Join-Path $scriptPath "AntiSwearingChatBox.sln"
$serverExePath = Join-Path $scriptPath "AntiSwearingChatBox.Server\bin\Debug\net9.0\AntiSwearingChatBox.Server.exe"
$wpfExePath = Join-Path $scriptPath "AntiSwearingChatBox.WPF\bin\Debug\net9.0-windows\AntiSwearingChatBox.WPF.exe"

Write-Host "Solution path: $solutionPath"

# Find MSBuild.exe
$vsPath = & "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" -latest -products * -requires Microsoft.Component.MSBuild -property installationPath
if (-not $vsPath) {
    $vsPath = "C:\Program Files\Microsoft Visual Studio\2022\Community"
}
$msbuildPath = Join-Path $vsPath "MSBuild\Current\Bin\MSBuild.exe"

if (-not (Test-Path $msbuildPath)) {
    $msbuildPath = "MSBuild.exe"  # Try using MSBuild from PATH if the direct path fails
}

# Clean the solution
Write-Host "`nCleaning the solution..." -ForegroundColor Cyan
& $msbuildPath $solutionPath /t:Clean /p:Configuration=Debug
if ($LASTEXITCODE -ne 0) {
    Write-Host "Failed to clean the solution." -ForegroundColor Red
    exit $LASTEXITCODE
}

# Build the solution
Write-Host "`nBuilding the solution..." -ForegroundColor Cyan
& $msbuildPath $solutionPath /t:Build /p:Configuration=Debug
if ($LASTEXITCODE -ne 0) {
    Write-Host "Failed to build the solution." -ForegroundColor Red
    exit $LASTEXITCODE
}

# Start the server
Write-Host "`nStarting Server..." -ForegroundColor Cyan
Start-Process -FilePath $serverExePath
Write-Host "Server started."

# Wait for the server to initialize
Write-Host "Waiting for server to initialize..."
Start-Sleep -Seconds 3

# Start the first WPF client
Write-Host "`nStarting WPF Client 1..." -ForegroundColor Cyan
Start-Process -FilePath $wpfExePath
Write-Host "WPF Client 1 started."

# Wait a bit before starting the second client
Start-Sleep -Seconds 1

# Start the second WPF client
Write-Host "`nStarting WPF Client 2..." -ForegroundColor Cyan
Start-Process -FilePath $wpfExePath
Write-Host "WPF Client 2 started."

Write-Host "`nAll applications launched successfully." -ForegroundColor Green

# Exit immediately
exit 0 