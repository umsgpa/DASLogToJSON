
using System.Text.Json.Serialization;
using System.Collections.Generic; // Don't forget this if you serialize List<T>

// Assuming your LogEntry and LogHeader classes are defined in the same namespace
// or you add the appropriate 'using' statements.
// For example:
// namespace DASLogToJSON
// {
//    public class LogEntry { /* ... */ }
//    public class LogHeader { /* ... */ }
// }

// Place this in a file like AppJsonContext.cs
namespace DASLogToJSON // Or your project's root namespace
{
    // Define your LogEntry and LogHeader classes if they aren't already here
    // public class LogEntry { /* ... your properties ... */ }
    // public class LogHeader { /* ... your properties ... */ }

    // This attribute tells the source generator which types to generate serialization code for.
    [JsonSerializable(typeof(List<LogEntry>))] // For your List<LogEntry> serialization
    [JsonSerializable(typeof(LogHeader))]      // For your LogHeader serialization
    // Add any other types you serialize/deserialize here
    // For example, if LogEntry contains other custom types, you might need to add them too.
    // [JsonSerializable(typeof(MyNestedType))]
    internal partial class AppJsonContext : JsonSerializerContext
    {
    }
}