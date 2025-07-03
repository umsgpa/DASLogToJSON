public class LogEntry
{
    public string? UniqueFileIDRef { get; set; } // string 255
    public string? HostName { get; set; } // string 255
    public string? Area { get; set; } // string 255
    //public DateTime Date { get; set; } // datetimeutc
    public long Sequence { get; set; } // long
    public DateTime DateTime { get; set; } // datetimeutc
    public string? Tag1 { get; set; } // string 255
    public string? Tag2 { get; set; } // string 255
    public string? Content { get; set; } // string max
}
