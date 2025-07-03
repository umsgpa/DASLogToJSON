using System;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices; // Required for RuntimeInformation
public static class SystemInfo
{

    // Usage:
    // Console.WriteLine($"Full Hostname: {SystemInfo.GetFullHostname()}");
    public static string GetFullHostname()
    {
        string hostname = Environment.MachineName;
        string domainName = IPGlobalProperties.GetIPGlobalProperties().DomainName;

        if (!string.IsNullOrEmpty(domainName))
        {
            // If the hostname already contains the domain, avoid duplication
            if (hostname.EndsWith("." + domainName, StringComparison.OrdinalIgnoreCase))
            {
                return hostname;
            }
            else
            {
                return $"{hostname}.{domainName}";
            }
        }
        else
        {
            return hostname; // No domain found
        }
    }


    // Usage:
    // Console.WriteLine($"Current User: {SystemInfo.GetCurrentUserWithDomain()}");
    public static string GetCurrentUserWithDomain()
    {
        string userDomainName = Environment.UserDomainName;
        string userName = Environment.UserName;

        if (!string.IsNullOrEmpty(userDomainName))
        {
            return $"{userDomainName}\\{userName}"; // Windows domain format
        }
        else
        {
            return userName; // Local user or non-Windows system
        }
    }
    
    
// Usage:
    // Console.WriteLine($"Operating System: {SystemInfo.GetOperatingSystemVersion()}");
    public static string GetOperatingSystemVersion()
    {
        string osPlatform = "Unknown";
        string osVersion = Environment.OSVersion.VersionString; // Basic OS version string

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            osPlatform = "Windows";
            // For Windows, Environment.OSVersion.VersionString provides detailed info
            // For more granular Windows version names (e.g., "Windows 10", "Windows Server 2019"),
            // you might need to use WMI on Windows.
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            osPlatform = "Linux";
            // On Linux, Environment.OSVersion.VersionString might be generic.
            // For distro-specific information (e.g., "Ubuntu 22.04"), you'd typically read /etc/os-release.
            // C# doesn't have a direct cross-platform way to get detailed distro info out-of-the-box.
            try
            {
                string osReleasePath = "/etc/os-release";
                if (File.Exists(osReleasePath))
                {
                    var lines = File.ReadAllLines(osReleasePath);
                    foreach (var line in lines)
                    {
                        if (line.StartsWith("PRETTY_NAME="))
                        {
                            osVersion = line.Substring("PRETTY_NAME=".Length).Trim('"', '\'');
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle potential file access errors
                Console.WriteLine($"Could not read /etc/os-release: {ex.Message}");
            }
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            osPlatform = "macOS";
            // On macOS, Environment.OSVersion.VersionString might be generic.
            // For more specific macOS versions (e.g., "Ventura 13.x"), you might need to
            // execute a shell command like `sw_vers -productVersion` or use platform-specific APIs.
            try
            {
                // This is a simplistic approach for macOS to get the product version
                // A more robust solution might involve P/Invoke or external process execution.
                var process = new System.Diagnostics.Process
                {
                    StartInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "sw_vers",
                        Arguments = "-productVersion",
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                    }
                };
                process.Start();
                string productVersion = process.StandardOutput.ReadToEnd().Trim();
                process.WaitForExit();
                if (!string.IsNullOrEmpty(productVersion))
                {
                    osVersion = $"macOS {productVersion}";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not execute sw_vers: {ex.Message}");
            }
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD))
        {
            osPlatform = "FreeBSD";
        }
        // Add other OS platforms as needed, e.g., Android, iOS, WASM
        // Note: RuntimeInformation.IsOSPlatform only checks for the target platform on which the code is running.

        return $"{osPlatform} {osVersion}";
    }



}

