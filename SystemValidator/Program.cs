using System.Net;
using System.Net.Sockets;
using Microsoft.Data.SqlClient;
using System.Diagnostics;
using System.Security.Principal;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=== SimpleChat System Requirements Validator ===\n");
        
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
    }

    static (string, bool, string) CheckDotNetVersion()
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

    static async Task<(string, bool, string)> CheckSqlServer()
    {
        try
        {
            using var connection = new SqlConnection("Server=localhost\\SQLEXPRESS;Database=SimpleChat;Trusted_Connection=True;TrustServerCertificate=True;");
            await connection.OpenAsync();
            return ("SQL Server", true, "Successfully connected to SQL Server Express");
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

    static (string, bool, string) CheckPortAvailability()
    {
        try
        {
            using var listener = new TcpListener(IPAddress.Any, 5000);
            listener.Start();
            listener.Stop();
            return ("Port 5000", true, "Port 5000 is available for use");
        }
        catch (Exception ex)
        {
            return (
                "Port 5000",
                false,
                $"Port 5000 is not available. Please ensure:\n" +
                "1. No other application is using port 5000\n" +
                "2. You have sufficient permissions\n" +
                $"Error: {ex.Message}"
            );
        }
    }

    static (string, bool, string) CheckWindowsFirewall()
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
                "No firewall rule found for SimpleChat. Please add an inbound rule for port 5000"
            );
        }
        catch (Exception ex)
        {
            return ("Windows Firewall", false, $"Error checking firewall: {ex.Message}");
        }
    }

    static (string, bool, string) CheckNetworkConfiguration()
    {
        try
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            var localIp = host.AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);
            var publicIp = GetPublicIpAddress();

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

    static (string, bool, string) CheckPermissions()
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

    static string GetPublicIpAddress()
    {
        try
        {
            using var client = new WebClient();
            return client.DownloadString("http://ifconfig.me/ip").Trim();
        }
        catch
        {
            return "Unable to determine";
        }
    }
}
