using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace AntiSwearingChatBox.ConsoleChat.SystemUtils
{
    public class SystemValidator
    {
        public async Task RunSystemValidator()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("=== SimpleChat System Requirements Validator ===\n");
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
            results.Add(CheckNetworkConfiguration());

            // Check Permissions
            results.Add(CheckPermissions());

            // Print Results
            Console.WriteLine("\n=== Validation Results ===\n");
            foreach (var (requirement, passed, details) in results)
            {
                Console.WriteLine($"{requirement}:");
                Console.WriteLine($"Status: {(passed ? "✓ PASSED" : "✗ FAILED")}");
                Console.WriteLine($"Details: {details}\n");
            }

            // Summary
            var passedCount = results.Count(r => r.Passed);
            var totalCount = results.Count;
            Console.WriteLine($"=== Summary ===\n");
            Console.WriteLine($"Total Requirements: {totalCount}");
            Console.WriteLine($"Passed: {passedCount}");
            Console.WriteLine($"Failed: {totalCount - passedCount}");
            Console.WriteLine($"\nSystem is {(passedCount == totalCount ? "READY" : "NOT READY")} for SimpleChat");
            
            Console.WriteLine("\nPress any key to return to main menu...");
            Console.ReadKey();
        }

        private (string, bool, string) CheckDotNetVersion()
        {
            try
            {
                var version = Environment.Version;
                var isCompatible = version.Major >= 9;
                return (
                    ".NET Version",
                    isCompatible,
                    $"Current Version: {version}\nRequired: .NET 9.0 or higher"
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
                var connectionString = "Server=localhost\\SQLEXPRESS;Database=SimpleChat;Trusted_Connection=True;TrustServerCertificate=True;Connect Timeout=3";
                using var connection = new SqlConnection(connectionString);
                
                // Set a short timeout for the connection attempt
                var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(3));
                await connection.OpenAsync(cancellationTokenSource.Token);
                
                return ("SQL Server", true, "Successfully connected to SQL Server Express");
            }
            catch (TaskCanceledException)
            {
                return (
                    "SQL Server",
                    false,
                    "Connection attempt to SQL Server timed out.\nPlease ensure SQL Server is installed and running."
                );
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

        private (string, bool, string) CheckPortAvailability()
        {
            try
            {
                var availablePorts = new List<int>();
                var startPort = 5000;
                
                for (int i = 0; i < 10; i++)
                {
                    int port = startPort + i;
                    if (NetworkUtils.IsPortAvailable(port))
                    {
                        availablePorts.Add(port);
                    }
                }
                
                if (availablePorts.Count > 0)
                {
                    return (
                        "Port Availability",
                        true,
                        $"Available ports: {string.Join(", ", availablePorts)}\n" +
                        "At least one port is available for use"
                    );
                }
                else
                {
                    return (
                        "Port Availability",
                        false,
                        "No available ports found in the range 5000-5009.\n" +
                        "Please ensure no other applications are using these ports."
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
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "netsh",
                        Arguments = "advfirewall firewall show rule name=\"SimpleChat\"",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };
                process.Start();
                var output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                if (output.Contains("SimpleChat"))
                {
                    return ("Windows Firewall", true, "Firewall rule for SimpleChat exists");
                }
                
                return (
                    "Windows Firewall",
                    false,
                    "No firewall rule found for SimpleChat.\n" +
                    "You may need to add an inbound rule for the application port."
                );
            }
            catch (Exception ex)
            {
                return ("Windows Firewall", false, $"Error checking firewall: {ex.Message}");
            }
        }

        private (string, bool, string) CheckNetworkConfiguration()
        {
            try
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                var localIp = host.AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);
                var publicIp = NetworkUtils.GetPublicIpAddress();

                return (
                    "Network Configuration",
                    true,
                    $"Local IP: {localIp}\n" +
                    $"Public IP: {publicIp}\n" +
                    "Network configuration is valid"
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
                    isAdmin,
                    $"Current User: {identity.Name}\n" +
                    $"Administrator Rights: {(isAdmin ? "Yes" : "No")}\n" +
                    "Note: Administrator rights are recommended for server setup"
                );
            }
            catch (Exception ex)
            {
                return ("Permissions", false, $"Error checking permissions: {ex.Message}");
            }
        }
    }
} 