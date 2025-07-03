using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

public class CrossPlatformUniqueId
{
    public static string GenerateUniqueId()
    {
        // Get machine-specific identifier
        string machineId = GetMachineId();

        // Get current timestamp (4 bytes)
        byte[] timestamp = BitConverter.GetBytes((int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds);

        // Get a random counter (3 bytes)
        byte[] counter = BitConverter.GetBytes(GetRandomCounter());

        // Combine all components
        byte[] uniqueIdBytes = new byte[timestamp.Length + Encoding.UTF8.GetByteCount(machineId) + counter.Length];
        Array.Copy(timestamp, 0, uniqueIdBytes, 0, timestamp.Length);
        Array.Copy(Encoding.UTF8.GetBytes(machineId), 0, uniqueIdBytes, timestamp.Length, Encoding.UTF8.GetByteCount(machineId));
        Array.Copy(counter, 0, uniqueIdBytes, timestamp.Length + Encoding.UTF8.GetByteCount(machineId), counter.Length);

        // Convert to hexadecimal string
        string uniqueId = BitConverter.ToString(uniqueIdBytes).Replace("-", "").ToLower();

        return uniqueId;
    }

    private static string GetMachineId()
    {
        if (File.Exists("/etc/machine-id"))
        {
            return File.ReadAllText("/etc/machine-id").Trim();
        }
        else
        {
            // Fallback to hostname or other unique identifier on Windows
            return Environment.MachineName;
        }
    }

    private static int GetRandomCounter()
    {
        // Initialize a random counter
        Random random = new Random();
        return random.Next(0, 16777216); // 2^24 to fit in 3 bytes
    }
}

// Example usage
//string uniqueId = CrossPlatformUniqueId.GenerateUniqueId();
//Console.WriteLine($"Unique ID: {uniqueId}");
