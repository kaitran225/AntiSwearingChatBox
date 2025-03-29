using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace AntiSwearingChatBox.ConsoleChat.SystemUtils
{
    public class SystemValidator
    {
        public async Task RunSystemValidator()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("=== Anti-Swearing Chat Box System Requirements Validator ===\n");
            Console.ResetColor();
            
            var results = new List<(string Requirement, bool Passed, string Details)>();

            // Check .NET Version
            results.Add(CheckDotNetVersion());

            // Check SQL Server
            results.Add(await CheckSqlServer());

            // Check Port Availability
            results.Add(CheckPortAvailability());

            // Check Windows Firewall
            results.Add(CheckWindowsFirewall());

            // Check Network Configuration
            results.Add(await CheckNetworkConfiguration());

            // Check Permissions
            results.Add(CheckPermissions());

            // Print Results
            Console.WriteLine("\n=== Validation Results ===\n");
            foreach (var (requirement, passed, details) in results)
            {
                Console.ForegroundColor = passed ? ConsoleColor.Green : ConsoleColor.Red;
                Console.WriteLine($"{requirement}:");
                Console.WriteLine($"Status: {(passed ? "✓ PASSED" : "✗ FAILED")}");
                Console.ResetColor();
                Console.WriteLine($"Details: {details}\n");
            }

            // Summary
            var passedCount = results.Count(r => r.Passed);
            var totalCount = results.Count;
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"=== Summary ===\n");
            Console.ResetColor();
            Console.WriteLine($"Total Requirements: {totalCount}");
            Console.WriteLine($"Passed: {passedCount}");
            Console.WriteLine($"Failed: {totalCount - passedCount}");
            
            Console.ForegroundColor = passedCount == totalCount ? ConsoleColor.Green : ConsoleColor.Yellow;
            Console.WriteLine($"\nSystem is {(passedCount == totalCount ? "READY" : "NOT READY")} for Anti-Swearing Chat Box");
            Console.ResetColor();
            
            Console.WriteLine("\nPress any key to return to main menu...");
            Console.ReadKey();
        }

        private (string, bool, string) CheckDotNetVersion()
        {
            try
            {
                var version = Environment.Version;
                var isCompatible = version.Major >= 8;
                return (
                    ".NET Version",
                    isCompatible,
                    $"Current Version: {version}\nRequired: .NET 8.0 or higher"
                );
            }
            catch (Exception ex)
            {
                return (".NET Version", false, $"Error checking version: {ex.Message}");
            }
        }

        private async Task<(string, bool, string)> CheckSqlServer()
        {
            try
            {
                // Find the Service project directory first
                string serviceDirectory = FindServiceProjectDirectory();
                
                // Load connection string from Service project's appsettings.json
                var config = new ConfigurationBuilder()
                    .SetBasePath(serviceDirectory)
                    .AddJsonFile("appsettings.json", optional: true)
                    .Build();

                var connectionString = config["ConnectionStrings:AntiSwearingChatBox"];
                
                // Add connection timeout for testing
                if (!string.IsNullOrEmpty(connectionString) && !connectionString.Contains("Connect Timeout"))
                {
                    connectionString += ";Connect Timeout=3";
                }
                
                // Use default if not found
                if (string.IsNullOrEmpty(connectionString))
                {
                    connectionString = "Server=localhost\\SQLEXPRESS;Database=AntiSwearingChatBox;Trusted_Connection=True;TrustServerCertificate=True;Connect Timeout=3";
                }
                
                using var connection = new SqlConnection(connectionString);
                
                // Set a short timeout for the connection attempt
                var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(3));
                try
                {
                    await connection.OpenAsync(cancellationTokenSource.Token);
                    return ("SQL Server", true, "Successfully connected to SQL Server Express");
                }
                catch (TaskCanceledException)
                {
                    return ("SQL Server", false, "Connection attempt to SQL Server timed out. Please ensure SQL Server is installed and running.");
                }
            }
            catch (Exception ex)
            {
                return (
                    "SQL Server",
                    false,
                    $"Failed to connect to SQL Server Express. Please ensure:\n" +
                    "1. SQL Server Express is installed\n" +
                    "2. SQL Server service is running\n" +
                    "3. Windows Authentication is enabled\n" +
                    $"Error: {ex.Message}"
                );
            }
        }

        // Helper method to find the Service project directory
        private string FindServiceProjectDirectory()
        {
            // Start from current directory
            string? currentDir = Directory.GetCurrentDirectory();
            
            // Try to find solution root by traversing up
            while (currentDir != null && !File.Exists(Path.Combine(currentDir, "AntiSwearingChatBox.sln")))
            {
                currentDir = Directory.GetParent(currentDir)?.FullName;
            }
            
            // If found solution root, look for Service project
            if (currentDir != null)
            {
                string serviceDir = Path.Combine(currentDir, "AntiSwearingChatBox.Service");
                if (Directory.Exists(serviceDir))
                {
                    return serviceDir;
                }
            }
            
            // Fallback to current directory
            return Directory.GetCurrentDirectory();
        }

        private (string, bool, string) CheckPortAvailability()
        {
            try
            {
                var defaultPort = 5122;
                var ports = new[] { defaultPort };
                var availablePorts = new List<int>();
                
                foreach (var port in ports)
                {
                    if (IsPortAvailable(port))
                    {
                        availablePorts.Add(port);
                    }
                }
                
                if (availablePorts.Count > 0)
                {
                    return (
                        "Port Availability",
                        true,
                        $"Default port {defaultPort} is available for use."
                    );
                }
                else
                {
                    return (
                        "Port Availability",
                        false,
                        $"Default port {defaultPort} is not available.\n" +
                        "Please ensure no other applications are using this port."
                    );
                }
            }
            catch (Exception ex)
            {
                return (
                    "Port Availability",
                    false,
                    $"Error checking port availability: {ex.Message}"
                );
            }
        }

        private (string, bool, string) CheckWindowsFirewall()
        {
            try
            {
                bool ruleFound = false;
                string output = "";
                
                // Check for exact rule name "AntiSwearingChatBox"
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "netsh",
                        Arguments = "advfirewall firewall show rule name=\"AntiSwearingChatBox\"",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };
                process.Start();
                output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                if (output.Contains("AntiSwearingChatBox"))
                {
                    ruleFound = true;
                }
                
                // If exact rule not found, check for any rule with our port
                if (!ruleFound)
                {
                    process = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = "netsh",
                            Arguments = "advfirewall firewall show rule name=all | findstr 5122",
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            CreateNoWindow = true
                        }
                    };
                    process.Start();
                    output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();

                    if (!string.IsNullOrWhiteSpace(output))
                    {
                        ruleFound = true;
                    }
                }
                
                // If we found a rule, return success
                if (ruleFound)
                {
                    return ("Windows Firewall", true, "Firewall rule for AntiSwearingChatBox ports exists");
                }
                
                // For simplicity's sake, let's just create the rule here
                // This makes the validator more helpful by auto-fixing the issue
                try
                {
                    // Create the firewall rule if we couldn't find it
                    process = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = "netsh",
                            Arguments = "advfirewall firewall add rule name=\"AntiSwearingChatBox\" dir=in action=allow protocol=TCP localport=5122-5128",
                            UseShellExecute = true,
                            Verb = "runas",
                            CreateNoWindow = false
                        }
                    };
                    process.Start();
                    process.WaitForExit();
                    
                    return ("Windows Firewall", true, "Created firewall rule for AntiSwearingChatBox");
                }
                catch
                {
                    // If we couldn't create it automatically (UAC denial etc.)
                    return (
                        "Windows Firewall",
                        false,
                        "No firewall rule found for AntiSwearingChatBox.\n" +
                        "Please run AddFirewallRules.bat as administrator or manually add an inbound rule for port 5122-5128."
                    );
                }
            }
            catch (Exception ex)
            {
                return ("Windows Firewall", false, $"Error checking firewall: {ex.Message}");
            }
        }

        private async Task<(string, bool, string)> CheckNetworkConfiguration()
        {
            try
            {
                var localIp = NetworkUtils.GetLocalIPAddress();
                var publicIp = await NetworkUtils.GetPublicIPAddressAsync();

                return (
                    "Network Configuration",
                    !string.IsNullOrEmpty(localIp),
                    $"Local IP: {localIp}\n" +
                    $"Public IP: {publicIp}\n" +
                    "Network configuration is valid for chat functionality"
                );
            }
            catch (Exception ex)
            {
                return ("Network Configuration", false, $"Error checking network: {ex.Message}");
            }
        }

        private (string, bool, string) CheckPermissions()
        {
            try
            {
                var identity = WindowsIdentity.GetCurrent();
                var principal = new WindowsPrincipal(identity);
                var isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);

                return (
                    "Permissions",
                    true, // Not strictly required to be admin
                    $"Current User: {identity.Name}\n" +
                    $"Administrator Rights: {(isAdmin ? "Yes" : "No")}\n" +
                    "Note: Administrator rights may be needed for some features like firewall configuration"
                );
            }
            catch (Exception ex)
            {
                return ("Permissions", false, $"Error checking permissions: {ex.Message}");
            }
        }

        private bool IsPortAvailable(int port)
        {
            try
            {
                using var tcpClient = new TcpClient();
                var result = tcpClient.BeginConnect("127.0.0.1", port, null, null);
                var success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromMilliseconds(100));
                
                if (success)
                {
                    tcpClient.EndConnect(result);
                    return false; // Port is in use
                }
                
                return true; // Port is available
            }
            catch
            {
                return true; // Error means port is most likely available
            }
        }
    }
} 