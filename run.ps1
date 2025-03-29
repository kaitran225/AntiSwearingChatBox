Set-Location $PSScriptRoot
Write-Host 'Building AntiSwearingChatBox solution...' -ForegroundColor Cyan
dotnet build AntiSwearingChatBox.sln
if ($LASTEXITCODE -ne 0) {
    Write-Host 'Build failed.' -ForegroundColor Red
    Read-Host -Prompt 'Press Enter to exit'
    exit $LASTEXITCODE
}
Write-Host 'Running AntiSwearingChatBox.App...' -ForegroundColor Green
dotnet run --project AntiSwearingChatBox.App
Read-Host -Prompt 'Press Enter to exit'
