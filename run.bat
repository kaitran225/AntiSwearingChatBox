@echo off
cd %~dp0
echo Building AntiSwearingChatBox solution...
dotnet build AntiSwearingChatBox.sln
if %ERRORLEVEL% NEQ 0 (
    echo Build failed.
    pause
    exit /b %ERRORLEVEL%
)
echo Running AntiSwearingChatBox.App...
dotnet run --project AntiSwearingChatBox.App
pause
