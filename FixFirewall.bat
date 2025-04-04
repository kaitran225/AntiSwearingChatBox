@echo off
echo This script will add a Windows Firewall rule for AntiSwearingChatBox
echo It must be run as administrator
echo.

echo Removing any existing rules with the same name...
netsh advfirewall firewall delete rule name="AntiSwearingChatBox"

echo Adding new TCP rule...
netsh advfirewall firewall add rule name="AntiSwearingChatBox" dir=in action=allow protocol=TCP localport=5000-5010 program=ANY enable=yes profile=any description="Allow AntiSwearingChatBox communication"

echo.
echo Verifying rule was created:
netsh advfirewall firewall show rule name="AntiSwearingChatBox"

echo.
echo Rule setup complete. Please run the System Validator again.
pause 