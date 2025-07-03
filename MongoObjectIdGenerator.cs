using System;
using System.Linq;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;
using System.Diagnostics;
using System.Threading;

namespace MongoDB.ObjectID
{
    /// <summary>
    /// Represents a MongoDB ObjectID.
    /// An ObjectID is a 12-byte unique identifier consisting of:
    /// - 4 bytes: the timestamp (seconds since the Unix epoch)
    /// - 3 bytes: machine identifier
    /// - 2 bytes: process ID
    /// - 3 bytes: counter
    /// </summary>
    public class ObjectID
    {
        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private static readonly byte[] MachineID;
        private static readonly short ProcessID;
        private static int _counter;
        private static readonly object _counterLock = new object();

        private readonly byte[] _value = new byte[12];

        static ObjectID()
        {
            // Initialize machine ID (3 bytes)
            MachineID = GenerateMachineID();

            // Initialize process ID (2 bytes)
            ProcessID = GenerateProcessID();

            // Initialize counter
            _counter = new Random().Next();
        }

        /// <summary>
        /// Initializes a new instance of the ObjectID class.
        /// </summary>
        public ObjectID()
        {
            Initialize(DateTimeOffset.UtcNow);
        }

        /// <summary>
        /// Initializes a new instance of the ObjectID class with the specified creation time.
        /// </summary>
        /// <param name="timestamp">The creation time.</param>
        public ObjectID(DateTime timestamp)
        {
            Initialize(timestamp);
        }

        /// <summary>
        /// Initializes a new instance of the ObjectID class from a 24-character hexadecimal string.
        /// </summary>
        /// <param name="value">The 24-character hexadecimal string.</param>
        public ObjectID(string value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            if (value.Length != 24)
                throw new ArgumentException("ObjectID string must be 24 characters", nameof(value));

            for (int i = 0; i < 12; i++)
            {
                string hex = value.Substring(i * 2, 2);
                _value[i] = Convert.ToByte(hex, 16);
            }
        }

        /// <summary>
        /// Initializes a new instance of the ObjectID class from a 12-byte array.
        /// </summary>
        /// <param name="bytes">The 12-byte array.</param>
        public ObjectID(byte[] bytes)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));

            if (bytes.Length != 12)
                throw new ArgumentException("ObjectID bytes must be 12 bytes", nameof(bytes));

            Array.Copy(bytes, _value, 12);
        }

        /// <summary>
        /// Gets the creation time of this ObjectID.
        /// </summary>
        public DateTime CreationTime
        {
            get
            {

                // MongoDB stores timestamp in big-endian format
                int timestamp = _value[0] << 24 | _value[1] << 16 | _value[2] << 8 | _value[3];

                // int timestamp = BitConverter.ToInt32(new[] { _value[0], _value[1], _value[2], _value[3] }, 0);
                return UnixEpoch.AddSeconds(timestamp);
            }
        }

        /// <summary>
        /// Gets the value of this ObjectID as a byte array.
        /// </summary>
        public byte[] ToByteArray()
        {
            byte[] bytes = new byte[12];
            Array.Copy(_value, bytes, 12);
            return bytes;
        }

        /// <summary>
        /// Returns a 24-character hexadecimal string representation of this ObjectID.
        /// </summary>
        /// <returns>A 24-character hexadecimal string.</returns>
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder(24);
            foreach (byte b in _value)
            {
                builder.Append(b.ToString("x2"));
            }
            return builder.ToString();
        }

        /// <summary>
        /// Generates a new ObjectID.
        /// </summary>
        /// <returns>A new ObjectID.</returns>
        public static ObjectID NewObjectID()
        {
            return new ObjectID();
        }

        /// <summary>
        /// Parses a 24-character hexadecimal string into an ObjectID.
        /// </summary>
        /// <param name="s">The 24-character hexadecimal string.</param>
        /// <returns>An ObjectID.</returns>
        public static ObjectID Parse(string s)
        {
            return new ObjectID(s);
        }

        /// <summary>
        /// Tries to parse a 24-character hexadecimal string into an ObjectID.
        /// </summary>
        /// <param name="s">The 24-character hexadecimal string.</param>
        /// <param name="objectID">The ObjectID.</param>
        /// <returns>True if the string was parsed successfully; otherwise, false.</returns>
        public static bool TryParse(string s, out ObjectID? objectID)
        {
            objectID = null;

            if (s == null || s.Length != 24)
                return false;

            try
            {
                objectID = new ObjectID(s);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void Initialize(DateTimeOffset timestamp)
        {
            // Set timestamp (4 bytes)
            int seconds = (int)(timestamp - UnixEpoch).TotalSeconds;
            _value[0] = (byte)(seconds >> 24);
            _value[1] = (byte)(seconds >> 16);
            _value[2] = (byte)(seconds >> 8);
            _value[3] = (byte)seconds;

            // Set machine ID (3 bytes)
            Array.Copy(MachineID, 0, _value, 4, 3);

            // Set process ID (2 bytes)
            _value[7] = (byte)(ProcessID >> 8);
            _value[8] = (byte)ProcessID;

            // Set counter (3 bytes)
            int counter = GetNextCounter();
            _value[9] = (byte)(counter >> 16);
            _value[10] = (byte)(counter >> 8);
            _value[11] = (byte)counter;
        }

        private static byte[] GenerateMachineID()
        {
            // Try to use the MAC address first
            try
            {
                // Get the first non-loopback network interface
                var networkInterface = NetworkInterface.GetAllNetworkInterfaces()
                    .FirstOrDefault(i => i.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                                       i.OperationalStatus == OperationalStatus.Up &&
                                       i.GetPhysicalAddress()?.ToString()?.Length > 0);

                if (networkInterface != null)
                {
                    var mac = networkInterface.GetPhysicalAddress().GetAddressBytes();
                    using (var md5 = MD5.Create())
                    {
                        var hash = md5.ComputeHash(mac);
                        return new[] { hash[0], hash[1], hash[2] };
                    }
                }
            }
            catch
            {
                // Fallback to hostname-based machine ID if MAC address fails
            }

            // Use hostname as fallback
            try
            {
                var hostname = Environment.MachineName;
                using (var md5 = MD5.Create())
                {
                    var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(hostname));
                    return new[] { hash[0], hash[1], hash[2] };
                }
            }
            catch
            {
                // Final fallback to random bytes if all else fails
                using (var rng = RandomNumberGenerator.Create())
                {
                    var bytes = new byte[3];
                    rng.GetBytes(bytes);
                    return bytes;
                }
            }
        }

        private static short GenerateProcessID()
        {
            try
            {
                // Use the actual process ID if available
                int pid = Process.GetCurrentProcess().Id;
                return (short)(pid % 65536); // Ensure it fits in 2 bytes
            }
            catch
            {
                // Fallback to random PID if system call fails
                return (short)new Random().Next(65536);
            }
        }

        private static int GetNextCounter()
        {
            lock (_counterLock)
            {
                _counter = _counter + 1 & 0xFFFFFF; // Keep within 3 bytes (24 bits)
                return _counter;
            }
        }

        // Equality and comparison methods
        public override bool Equals(object? obj)
        {
            if (obj is ObjectID other)
            {
                return _value.SequenceEqual(other._value);
            }
            return false;
        }

        public override int GetHashCode()
        {
            int hash = 17;
            foreach (byte b in _value)
            {
                hash = hash * 31 + b;
            }
            return hash;
        }

        
        public static DateTime GetTimestampFromString(string objectIdString)
        {
            if (string.IsNullOrEmpty(objectIdString) || objectIdString.Length != 24)
                throw new ArgumentException("ObjectID string must be 24 characters", nameof(objectIdString));

            try
            {
                // Extract the first 8 characters (4 bytes) which represent the timestamp
                string timestampHex = objectIdString.Substring(0, 8);
                int timestamp = Convert.ToInt32(timestampHex, 16);
                return UnixEpoch.AddSeconds(timestamp);
            }
            catch (Exception ex)
            {
                throw new FormatException($"Invalid ObjectID format: {ex.Message}", ex);
            }
        }
    }
}