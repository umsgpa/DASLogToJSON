public class LogHeader
{
    public string? UniqueFileID { get; set; }
    public string? CustomerName { get; set; }
    public string? HostName { get; set; }
    public string? SWVersion { get; set; }
    public bool NLogType { get; set; }
    public string? Area { get; set; }
    public long LogEntriesCount { get; set; }
    public DateTime StartTime { get; set; }
    public double ElapsedTimeSeconds { get; set; }
    public string? InputFile { get; set; }
    public string? OutputFile { get; set; }
    public string? HeaderFile { get; set; }

    public string? CreatorHostName { get; set; }
    public string? CreatorUserName { get; set; }

    public string? CreatorOperatingSystem { get; set; }
    
    public string? CreatorNotes { get; set; }

    public LogHeader(string uniqueFileID, string customerName, string hostName, string swversion, bool nlogtype, string area
                    , long logEntriesCount, DateTime startTime, double elapsedTimeSeconds, string inputFile, string outputFile, string headerFile
                    , string? creatorHostName = null, string? creatorUserName = null, string? creatorOperatingSystem = null, string? creatorNotes = null
                    )
    {
        UniqueFileID = uniqueFileID;
        CustomerName = customerName;
        HostName = hostName;
        SWVersion = swversion;
        NLogType = nlogtype;
        Area = area;
        LogEntriesCount = logEntriesCount;
        StartTime = startTime;
        ElapsedTimeSeconds = elapsedTimeSeconds;
        InputFile = inputFile;
        OutputFile = outputFile;
        HeaderFile = headerFile;
        CreatorHostName = creatorHostName ?? Environment.MachineName;
        CreatorUserName = creatorUserName ?? Environment.UserName;
        CreatorOperatingSystem = creatorOperatingSystem ?? Environment.OSVersion.VersionString;
        CreatorNotes = creatorNotes;


    }       
/*
                        Console.WriteLine("Input file: " + folderPath + fileName);
                        Console.WriteLine("Output file: " + folderPath + fileName + ".json");
                        Console.WriteLine("Host Name: " + hostname);
                        Console.WriteLine("Area: " + prefix);
                        Console.WriteLine("Number of log entries: " + myObjectList.Count);
                        Console.WriteLine("Unique file ID: " + uniqueid.ToString());
                        Console.WriteLine("Elapsed time: " + elapsedTime.TotalSeconds + " seconds");
                        startTime

        */

}
