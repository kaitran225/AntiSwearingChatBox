@echo off
echo Running Anti-Swearing Chat Box Setup Script

:: Set paths
set SolutionPath="%~dp0AntiSwearingChatBox.sln"
set ServerExe="%~dp0AntiSwearingChatBox.Server\bin\Debug\net9.0\AntiSwearingChatBox.Server.exe"
set WpfExe="%~dp0AntiSwearingChatBox.WPF\bin\Debug\net9.0-windows\AntiSwearingChatBox.WPF.exe"
set MSBuildPath="C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe"
echo Solution path: %SolutionPath%

:: Clean and Build the solution
echo.
echo Cleaning the solution...
%MSBuildPath% %SolutionPath% /t:Clean /p:Configuration=Debug
if %ERRORLEVEL% NEQ 0 (
    echo Failed to clean the solution.
    pause
    exit /b %ERRORLEVEL%
)

echo.
echo Building the solution...
%MSBuildPath% %SolutionPath% /t:Build /p:Configuration=Debug
if %ERRORLEVEL% NEQ 0 (
    echo Failed to build the solution.
    pause
    exit /b %ERRORLEVEL%
)

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
echo.

:: Exit immediately
exit /b 0 