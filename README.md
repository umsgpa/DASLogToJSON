
DIGISTAT log files are typically generated on a daily basis and follow a standard naming convention, such as:

`{SystemArea}_{yyyyMMdd}.log`

Another naming format, not native but commonly used by technical support teams and often found in Jira tickets, is:

`{HostName}_{SystemArea}_{yyyyMMdd}.log`

Inside each file, log entries follow a well-defined structure, typically formatted as:

`{HH:mm:ss}{ – }<log entry record 1>{cr}{lf}`    
`{HH:mm:ss}{ – }<log entry record 2>{cr}{lf}`    
`{HH:mm:ss}{ – }<log entry record …>{cr}{lf}`    
`{HH:mm:ss}{ – }<log entry record _n_>{cr}{lf}`

or multi-line:

`{HH:mm:ss}{ – }<log entry record 1 line 1>{cr}{lf}`   
`<log entry record 1 line 2>{cr}{lf}`     
`<log entry record 1 line …>{cr}{lf}`     
`<log entry record 1 line n>{cr}{lf}`     
`{HH:mm:ss}{ – }<log entry record 2>{cr}{lf}`    
`{HH:mm:ss}{ – }<log entry record …>{cr}{lf}`    
`{HH:mm:ss}{ – }<log entry record _n_>{cr}{lf}`

To reconstruct a complete and coherent log record, it is necessary to combine information from both the file name and the content within the file.

Key elements to consider include:

-          System Area (DAS3, DBv2, MessageCenter, etc.)

-          File date (yyyyMMdd)

-          Timestamp (HH:mm:ss)

-          Log message content (both single-line and/or multi-line)

After a thorough analysis of the log files, it was possible to precisely identify the timestamp and the delimiter that separates it from the log message.

Within the log message itself, there are often one or two distinctive elements that further characterize the entry based on the system area. These elements are optional, may appear consecutively (up to two), and help to specialize the content of the log. For example:           

`{HH:mm:ss}{ – }<[category] log entry record …>{cr}{lf}`    

or

`{HH:mm:ss}{ – }<[category] [sub-category] log entry record …>{cr}{lf}`

These elements are useful for filtering and are referred to as _tag1_ and _tag2_.

## How the Utility Works

Now that the structure of DIGISTAT log files has been clarified, we can describe how the DASLogToJSON utility functions.

This command-line tool takes a .log file that follows the expected structure and generates two output files with the same base name as the original log file:       

-          `{SystemArea}_{yyyyMMdd}.log.json`

-          `{SystemArea}_{yyyyMMdd}_header.log.json`

The first file contains only the extracted and structured log data, these are the actual log entries.

The second file includes additional information useful for identifying the context of the log file, such as the customer name, optional descriptive notes, and other metadata relevant for traceability and analysis.

**{SystemArea}_{yyyyMMdd}.log.json**
```json
[
  {
    "uniqueFileIDRef": "68552e54b3624a5c68a63c74",
    "logLevel": "TRACE",
    "hostName": "SRVDIGISTAT",
    "area": "DAS3",
    "sequence": 7,
    "dateTime": "2025-06-19T12:08:24",
    "tag1": "[UMSMessageCenterClient]",
    "tag2": "",
    "content": "[UMSMessageCenterClient] Connection to Message Center successfully, try authorization"
  },
  {
    ...
  }
]
```
  

| **Field**         | **Description**                                                                                                                                                                                                                                                             |
| ----------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `uniqueFileIDRef` | Unique file identifier, used to link with the corresponding header file                                                                                                                                                                                                     |
| `logLevel`        | Log level according to [NLOG](https://github.com/nlog/nlog/wiki/Configuration-file#log-levels):  <br>TRACE \| DEBUG \| INFO \| WARN \| ERROR \| FATAL  <br>  <br><br>This field will be *null* for DIGISTAT versions prior to 10.x, which do not implement the NLOG library |
| `hostName`        | Source host name                                                                                                                                                                                                                                                            |
| `area`            | Identifies the system area the log file belongs to                                                                                                                                                                                                                          |
| `sequence`        | Sequential number, generated during import, corresponding to the line number of the log entry. Useful for sorting entries with identical HH:mm:ss timestamps                                                                                                                |
| `dateTime`        | Date and time of the log entry (derived from the file name and the timestamp in the line)                                                                                                                                                                                   |
| `tag1`            | First optional tag                                                                                                                                                                                                                                                          |
| `tag2`            | Second optional tag                                                                                                                                                                                                                                                         |
| `content`         | Log entry full text                                                                                                                                                                                                                                                         |

**{SystemArea}_{yyyyMMdd}_header.log.json**
```json
{
    "uniqueFileID": "68552e54b3624a5c68a63c74",
    "customerName": "MY CUSTOMER",
    "hostName": "SRVDIGISTAT",
    "swVersion": "\u003E= 10.0.0.0",
    "nLogType": true,
    "area": "DAS3",
    "logEntriesCount": 147,
    "startTime": "2025-06-20T11:48:04.1265938+02:00",
    "elapsedTimeSeconds": 0.0106744,
    "inputFile": ".\\DAS3_20250619.log",
    "outputFile": ".\\DAS3_20250619.log.json",
    "headerFile": ".\\DAS3_20250619_header.log.json",
    "creatorHostName": "ITFLO-H725.mydomain.com",
    "creatorUserName": "MYDOMAIN\\gpancani",
    "creatorOperatingSystem": "Windows Microsoft Windows NT 10.0.19045.0",
    "creatorNotes": "My notes related my logs files and reason to collect these info"
}
```

| Field                    | Description                                                                                                        |
| ------------------------ | ------------------------------------------------------------------------------------------------------------------ |
| `uniqueFileIDRef`        | Refer to the Unique file identifier                                                                                |
| `customerName`           | Customer and/or System name                                                                                        |
| `hostName`               | Source host name                                                                                                   |
| `swVersion`              | DIGISTAT software version                                                                                          |
| `nLogType`               | true if the log was generated by NLOG                                                                              |
| `area`                   | System Area                                                                                                        |
| `logEntriesCount`        | Number of processed log entries (not the number of lines in the file)                                              |
| `startTime`              | Timestamp marking the start of the conversion                                                                      |
| `elapsedTimeSeconds`     | Duration of the conversion in seconds                                                                              |
| `inputFile`              | Path and name of the input file                                                                                    |
| `outputFile`             | Path and name of the output file                                                                                   |
| `headerFile`             | Path and name of the output header file                                                                            |
| `creatorHostName`        | The host machine where the conversion was performed                                                                |
| `creatorUserName`        | Operating system used                                                                                              |
| `creatorOperatingSystem` | Version of the operating system on which the conversion was executed                                               |
| `creatorNotes`           | User-defined notes, it is recommended to include the reason for the analysis and, if possible, a time-related clue |

## Command-Line Parameters

DASLogToJSON accepts several command-line parameters that must be provided to successfully convert the log files:

| **Parameter**      | **Functionality**                                                                                                                                                                                                       |
| ------------------ | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `--logfolder`      | (Required) Specifies the path to the .log files that need to be converted.                                                                                                                                              |
| `--hostname`       | (Optional) Overrides the host name if it is not included as a prefix in the file name.                                                                                                                                  |
| `--swversion`      | (Optional but recommended) Indicates the software version in the format x.y.z.w such as: 9.2.3.1 or 10.0.0.1 etc.<br><br>f omitted, the version will be automatically inferred as eithe: “>= 10.0.0.0” or  “< 10.0.0.0” |
| `--customername`   | (Optional) Specifies the name of the customer or system.                                                                                                                                                                |
| `--notes`          | (Optional) Allows the user to add descriptive notes.                                                                                                                                                                    |
| `--mongodbextjson` | (Optional) If set, converts the dateTime field into a MongoDB-compatible date format.                                                                                                                                   |

## Command-Line Example

`.\DASLogToJSON.exe --logfolder="X:\LogToAnalyse\Customers\UK London\Royal London Hospital" --customername="UK | London | Royal London Hospital" --hostname="srvtest01.rl.nhs.uk" --notes="acquisition stops around midnight" --mongodbextjson=no`

## Example of application launch

`C:\GitHubRepo\DASLogToJSON\bin\Debug\net6.0>.\DASLogToJSON.exe --logfolder="F:\LogToAnalyse\Customers\UK London\Royal London Hospital" --customername="UK | London | Royal London Hospital" --hostname="srvtest01.rl.nhs.uk" --notes="acquisition stops around midnight" --mongodbextjson=no`

`DAS Log to JSON Converter`
`---------------------------------------------------`
`Header File F:\LogToAnalyse\Customers\UK London\Royal London Hospital\DAS3_20250626_header.log.json`
`Input file: F:\LogToAnalyse\Customers\UK London\Royal London Hospital\DAS3_20250626.log`
`Output file: F:\LogToAnalyse\Customers\UK London\Royal London Hospital\DAS3_20250626.log.json`
`Log entry NLog: True`
`Host Name: srvtest01.rl.nhs.uk`
`Area: DAS3`
`Number of log entries: 808`
`Unique file ID: 6874f65931f98d45cc154aae`
`Elapsed time: 0.0233343 seconds`
`---------------------------------------------------`
`Header File F:\LogToAnalyse\Customers\UK London\Royal London Hospital\DBv2_20250626_header.log.json`
`Input file: F:\LogToAnalyse\Customers\UK London\Royal London Hospital\DBv2_20250626.log`
`Output file: F:\LogToAnalyse\Customers\UK London\Royal London Hospital\DBv2_20250626.log.json`
`Log entry NLog: True`
`Host Name: srvtest01.rl.nhs.uk`
`Area: DBv2`
`Number of log entries: 13`
`Unique file ID: 6874f65931f98d45cc154ab0`
`Elapsed time: 0.0019599 seconds`