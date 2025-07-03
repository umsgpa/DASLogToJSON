using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using MongoDB.ObjectID;
using System.Diagnostics;
public class LogParser  
{
    /// <summary>
    /// Extracts multi-line log entries that start with a timestamp pattern
    /// </summary>
    /// <param name="filePath">Path to the log file</param>
    /// <returns>List of multi-line log entries</returns>
    /// 
    


    public static string rx_LogFile = @"(^(.*[^_])_([^_]+)_(\d{8})(?=\.log$)|^([^_]+)_(\d{8})(?=\.log$))";
    public static string rx_tagsline = @"-\s*(\[[^\]]+\])(?:\s*(\[[^\]]+\]))?";
    public static string rx_timeseparator = @"^\d{2}[:|.]\d{2}[:|.]\d{2} - ";
    public static List<string> ExtractLogEntries(string filePath)
    {
        var logEntries = new List<string>();

        try
        {
            // Regular expression to match lines starting with timestamp pattern
            //var timestampPattern = @"^\d{2}[:|.]\d{2}[:|.]\d{2} - ";

            using (var reader = new StreamReader(filePath))
            {
                string? line;
                StringBuilder? currentEntry = null;

                while ((line = reader.ReadLine()) != null)
                {
                    // Check if the line starts with the timestamp pattern
                    if (Regex.IsMatch(line, rx_timeseparator))
                    {
                        // If we were building a previous entry, add it to the list
                        if (currentEntry != null)
                        {
                            logEntries.Add(currentEntry.ToString().Trim());
                        }

                        // Start a new entry
                        currentEntry = new StringBuilder(line);
                    }
                    else if (currentEntry != null)
                    {
                        // If we're in the middle of an entry, append the line
                        currentEntry.AppendLine(line);
                    }
                }

                // Add the last entry if exists
                if (currentEntry != null)
                {
                    logEntries.Add(currentEntry.ToString().Trim());
                }
            }

            return logEntries;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading log file: {ex.Message}");
            return logEntries;
        }
    }

    // Example usage
    public static void Main(string[] args)
    {
        //string logFilePath =  @"F:\SmartSup\Logs\DAS3_20250314.log"; //@"F:\SmartSup\Logs\src1\DAS3_20250509.log";
       // string folderPath =  @"F:\SmartSup\Logs\";
        /*

            --logfolder
            --hostname
            --customername
            --notes
            --mongotyping
        
        */
        

        var parser = new DASLogToJSON.CommandLineArguments();
                if (!parser.Parse(args))
                {
                    return;
                }

       string? folderPath = parser.Param_LogFolder;
       bool isMongoDBExtJSON = parser.Param_MongoDBExtJSON;

        Console.WriteLine("DAS Log to JSON Converter");
        Console.WriteLine("---------------------------------------------------");
        Console.WriteLine($"Processing folder: {folderPath}");


        List<string> fileNames = new List<string>();
        List<LogEntry> myObjectList = new List<LogEntry>();

        if (folderPath == null || !Directory.Exists(folderPath))
        {
            Console.WriteLine("The specified folder does not exist.");
            return;
        }
        // Enumerate files in the folder
        foreach (string file in Directory.EnumerateFiles(folderPath, "*.log"))
        {
            string fileName = Path.GetFileName(file);
            if (IsValidFileName(fileName))
            {
                fileNames.Add(fileName);
            }
        }
        
        try
        {





            // Read and print each line of each file
            foreach (string fileName in fileNames)
            {
                // List<string> logEntries = ExtractLogEntries(fileName);
                List<string> logEntries = ExtractLogEntries(Path.Combine(folderPath, fileName));
                DateTime startTime = DateTime.Now;

                bool IsMessageCenter = fileName.Contains("MessageCenter_", StringComparison.OrdinalIgnoreCase); 

                //logEntries.Clear();
                var uniqueid = ObjectID.NewObjectID();
                string hostname = string.Empty; 
                string prefix = string.Empty;
                string date = string.Empty;
                
                                                                    // string pattern = @"(^\w+)(_)(\d{8})\.log$";
                //string pattern = @"(^\w+)(_)(\d{8})\.log$";         // New pattern: /(^(.*[^_])_([^_]+)_(\d{8})(?=\.log$)|^([^_]+)_(\d{8})(?=\.log$))/gi
                // Group 2: Host Name
                // Group 3: Log Type (DAS3, DBv2, etc.)
                // Group 4: Date (yyyyMMdd)
                // -----------------------------------------
                // Group 5: Log Type (DAS3, DBv2, etc.)
                // Group 6: Date (yyyyMMdd)
                // Group 4: Date (yyyyMMdd)                


                Debug.WriteLine($"Processing file: {fileName}");
          
                Match match = Regex.Match(fileName, rx_LogFile, RegexOptions.IgnoreCase);

                Debug.WriteLine($"Match: " + match.Groups.Count +"");

                int usecase = 0;
                if (match.Success && match.Groups[5].Success) //4
                {

                    hostname = string.Empty; // First group: (^\w+)
                    prefix = match.Groups[5].Value; // Second group: (\w+) //5
                    date = match.Groups[6].Value;   // Fourth group: (\d{8}) //6
                    usecase = 1;
                }
                else if (match.Success && match.Groups[4].Success) //4
                {
                    hostname = match.Groups[2].Value; // First group: (^\w+) //1
                    prefix = match.Groups[3].Value; // First group: (^\w+) //3
                    date = match.Groups[4].Value;   // Third group: (\d{8}) //4
                    usecase = 2;
                }

                // string filePath = Path.Combine(folderPath, fileName);

                // Console.WriteLine($"\nContents of {fileName}:");
                long linenumber = 0;

                string tag1override = string.Empty;
                if (IsMessageCenter)
                {

                    switch (usecase)
                    {
                        case 1:
                            hostname = string.Empty;
                            break;
                        case 2:
                            hostname = hostname.Replace("_MessageCenter", string.Empty).Replace("MessageCenter_", string.Empty);
                            if (hostname == "MessageCenter")
                            {
                                hostname = string.Empty; // If hostname is just "MessageCenter", set it to empty
                            }
                            break;

                    }
                    
                    tag1override = $"[{prefix}]";
                    prefix = "MessageCenter";

                }


                foreach (var entry in logEntries)
                    {
                        linenumber++;

                        // string patterntagsline = @"\[([^\]]+)\]"; //@"(\[(\w+|\s+)\])+";
                        // string patterntagsline = @"-\s*\[([^\]]+)\](?:\s*\[([^\]]+)\])?"; // ONLY VALUES
                        // string patterntagsline = @"-\s*(\[[^\]]+\])(?:\s*(\[[^\]]+\]))?"; // VALUES AND BRACKETS
                        string tag1 = string.Empty;
                        string tag2 = string.Empty;

                        Regex regexLineTags = new Regex(rx_tagsline);


                        Match matchd = regexLineTags.Match(entry);
                        if (matchd.Success)
                        {
                            tag1 = matchd.Groups[1].Value;
                            if (matchd.Groups[2].Success)
                            {
                                tag2 = matchd.Groups[2].Value;
                            }
                            else
                            {
                                tag2 = string.Empty;
                            }
                        }

                        if (IsMessageCenter)
                        {
                            tag1 = tag1override; // Override tag1 with the prefix if it's a MessageCenter log
                        }


                    hostname = parser.Param_HostName != string.Empty ? parser.Param_HostName : hostname;

                    myObjectList.Add(new LogEntry
                        {
                            UniqueFileIDRef = uniqueid.ToString(),
                            HostName = hostname,
                            Area = prefix,
                            //  Date = DateTime.ParseExact(date, "yyyyMMdd", CultureInfo.InvariantCulture),
                            Sequence = linenumber,
                            DateTime = DateTime.ParseExact($"{date}T" + entry.Substring(0, 8).Replace(".", ":"), "yyyyMMddTHH:mm:ss", CultureInfo.InvariantCulture),
                            Tag1 = tag1,
                            Tag2 = tag2,
                            Content = entry.Substring(11, entry.Length - 11)
                        });

                    }

                /*
                                    // Assuming you have your JsonSerializerOptions defined
                                    var jsonSerializerOptions = new JsonSerializerOptions
                                    {
                                        WriteIndented = true,
                                         PropertyNamingPolicy = JsonNamingPolicy.CamelCase // Use camelCase for property names
                                        // Add other options as needed, e.g., PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                                    };
                */





                string json = JsonSerializer.Serialize(myObjectList, new JsonSerializerOptions
                {
                    WriteIndented = true // For pretty printing
                    ,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase // Use camelCase for property names
   
                });

                if (isMongoDBExtJSON)
                {
                    json = JsonTransformer.TransformDateTimeFormat(json, new[] { "dateTime" }, StringComparison.Ordinal);
                }                  
                //Console.WriteLine(json);
                File.WriteAllText(folderPath + fileName + ".json", json, System.Text.Encoding.UTF8);

                TimeSpan elapsedTimeSeconds = DateTime.Now - startTime;

                LogHeader logHeader = new LogHeader(
                    uniqueid.ToString(),
                    parser.Param_CustomerName,
                    hostname,
                    prefix,
                    myObjectList.Count,
                    startTime,
                    elapsedTimeSeconds.TotalSeconds,
                    folderPath + fileName,
                    folderPath + fileName + ".json",
                    folderPath + fileName.Replace(".log","_header.log") + ".json",
                    SystemInfo.GetFullHostname(),
                    SystemInfo.GetCurrentUserWithDomain(),
                    SystemInfo.GetOperatingSystemVersion(),
                    parser.Param_Notes
                    
                );

                Console.WriteLine($"Header File {logHeader.HeaderFile}");
                Console.WriteLine($"Input file: {logHeader.InputFile}");
                Console.WriteLine($"Output file: {logHeader.OutputFile}");
                Console.WriteLine($"Host Name: {logHeader.HostName}");
                Console.WriteLine($"Area: {logHeader.Area}"); 
                Console.WriteLine($"Number of log entries: {logHeader.LogEntriesCount}");
                Console.WriteLine($"Unique file ID: {logHeader.UniqueFileID}");
                Console.WriteLine($"Elapsed time: {logHeader.ElapsedTimeSeconds} seconds");
                
                Console.WriteLine("---------------------------------------------------");

                string headerJson = JsonSerializer.Serialize(logHeader, new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });


                if (isMongoDBExtJSON)
                {
                    headerJson = JsonTransformer.TransformDateTimeFormat(headerJson, new[] { "startTime" }, StringComparison.Ordinal);
                }

                //Console.WriteLine(json);
                File.WriteAllText(folderPath + fileName.Replace(".log","_header.log.json"), headerJson, System.Text.Encoding.UTF8);
/*
                                                                                Console.WriteLine("Input file: " + folderPath + fileName);
                                                                                Console.WriteLine("Output file: " + folderPath + fileName + ".json");
                                                                                Console.WriteLine("Host Name: " + hostname);
                                                                                Console.WriteLine("Area: " + prefix);
                                                                                Console.WriteLine("Number of log entries: " + myObjectList.Count);
                                                                                Console.WriteLine("Unique file ID: " + uniqueid.ToString());
                                                                                Console.WriteLine("Elapsed time: " + elapsedTime.TotalSeconds + " seconds");
                                                                                Console.WriteLine("---");
                                                                */
                logEntries.Clear();
                myObjectList.Clear();

/*
                string uniqueId = CrossPlatformUniqueId.GenerateUniqueId();
                Console.WriteLine($"Unique ID: {uniqueId}");

                // Generate a new ObjectID
                var id = ObjectID.NewObjectID();

                // Get the string representation
                string idString = id.ToString();
                Console.WriteLine($"MongoDB Object ID: {idString}");
                // Parse from string
                DateTime timestamp = ObjectID.GetTimestampFromString(idString);
                Console.WriteLine($"This ObjectID was created at: {timestamp}");

                timestamp =  ObjectID.GetTimestampFromString("6822ea3730d3e9dd69f09758");
                Console.WriteLine($"This 6822ea3730d3e9dd69f09758 was created at: {timestamp}");
                */
                
        }  
        


        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }

        static bool IsValidFileName(string fileName)
    {
        // Check if the file name matches the structure {prefixname}_yyyyMMdd.log
        // string pattern = @"(^\w+)(_)(\d{8})\.log$";
        return System.Text.RegularExpressions.Regex.IsMatch(fileName, rx_LogFile, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
    }
}