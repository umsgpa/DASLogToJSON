public class LogHeader
{
    public string? UniqueFileID { get; set; }
    public string? HostName { get; set; }
    public string? Area { get; set; }
    public long LogEntriesCount { get; set; }
    public DateTime StartTime { get; set; }
    public double ElapsedTime { get; set; }
    public string? InputFile { get; set; } 
    public string? OutputFile { get; set; }
        public string? HeaderFile { get; set; }
    public LogHeader(string uniqueFileID, string hostName, string area, long logEntriesCount, DateTime startTime, double elapsedTime, string inputFile, string outputFile, string headerFile)
    {
        UniqueFileID = uniqueFileID;
        HostName = hostName;
        Area = area;
        LogEntriesCount = logEntriesCount;
        StartTime = startTime;
        ElapsedTime = elapsedTime;
        InputFile = inputFile;
        OutputFile = outputFile;
        HeaderFile = headerFile;
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
