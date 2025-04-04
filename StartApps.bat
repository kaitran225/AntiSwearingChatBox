@echo off
echo Starting Anti-Swearing Chat Box Applications...

:: Set paths
set ServerExe="%~dp0AntiSwearingChatBox.Server\bin\Debug\net9.0\AntiSwearingChatBox.Server.exe"
set WpfExe="%~dp0AntiSwearingChatBox.WPF\bin\Debug\net9.0-windows\AntiSwearingChatBox.WPF.exe"

:: Run the applications
echo.
echo Starting Server...
start "" %ServerExe%
echo Server started.

:: Wait for the server to initialize
timeout /t 3 /nobreak

echo.
echo Starting WPF Client 1...
start "" %WpfExe%
echo WPF Client 1 started.

:: Wait a moment before starting the second client
timeout /t 1 /nobreak

echo.
echo Starting WPF Client 2...
start "" %WpfExe%
echo WPF Client 2 started.

echo.
echo All applications launched successfully.

:: Exit
exit /b 0 