using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
namespace DASLogToJSON
{


    public class CommandLineArguments
    {
        public string? Param_LogFolder { get; private set; }
        public string Param_HostName { get; private set; } = string.Empty;
        public string Param_SWVersion { get; private set; } = string.Empty;
        public string Param_CustomerName { get; private set; } = string.Empty;
        public string Param_Notes { get; private set; } = string.Empty;
        public bool Param_MongoDBExtJSON { get; private set; } = false;

        private readonly Dictionary<string, Action<string>> _parameterHandlers;

        public static string rx_swversion = @"^(\d+)(?:\.(\d+))?(?:\.(\d+))?(?:\.(\d+))?$"; // Matches the software version format x | x.y | x.y.z | x.y.z.w

        public CommandLineArguments()
        {
            _parameterHandlers = new Dictionary<string, Action<string>>(StringComparer.OrdinalIgnoreCase)
            {
                { "--logfolder", val => Param_LogFolder = val },
                { "--hostname", val => Param_HostName = val },
                { "--swversion", val => Param_SWVersion = val },
                { "--customername", val => Param_CustomerName = val },
                { "--notes", val => Param_Notes = val },
                { "--mongodbextjson", val => Param_MongoDBExtJSON = val.Equals("yes", StringComparison.OrdinalIgnoreCase) }
            };
        }

        public bool Parse(string[] args)
        {
            foreach (var arg in args)
            {
                var split = arg.Split(new[] { '=' }, 2);
                if (split.Length != 2)
                    continue;

                var key = split[0].Trim().ToLower();
                var value = split[1].Trim().Trim('"');

                if (_parameterHandlers.TryGetValue(key, out var handler))
                {
                    if (key == "--logfolder" && !string.IsNullOrWhiteSpace(value))
                    {
                        if (!value.EndsWith("\\"))
                        {
                            value += "\\";
                        }

                    }
                    handler(value);
                }
                else
                {
                    Console.WriteLine($"Unknown parameter: {key}");
                    ShowHelp();
                    return false;
                }
            }

            if (string.IsNullOrWhiteSpace(Param_LogFolder))
            {
                Console.WriteLine("Missing mandatory parameter: --logfolder");
                ShowHelp();
                return false;
            }



            if (!string.IsNullOrWhiteSpace(Param_SWVersion))
            {
                if (!Regex.IsMatch(Param_SWVersion, rx_swversion))
                {
                    Console.WriteLine($"It was provided a wrong value for --swversion=\"{Param_SWVersion}\"");
                    ShowHelp();
                    return false;
                }
            }

            return true;
        }


        public void ShowHelp()
        {
            Console.WriteLine("Usage:\r\n--------");
            Console.WriteLine("\r\n--logfolder=\"<folder path>\"\r\n  [Mandatory]  \r\n  Specify the folder hosting the logs to transform");
            Console.WriteLine("\r\n--hostname=\"<hostname>\"\r\n  [Optional]\r\n  Specify the hostname if not present as prefix in the log file name");
            Console.WriteLine("\r\n--swversion=\"<x|x.y|x.y.z|x.y.z.w>\"\r\n  [Optional]\r\n  Specify the software version that originated log files");
            Console.WriteLine("\r\n--customername=\"<customer name>\"\r\n  [Optional]\r\n  Specify a customer name");
            Console.WriteLine("\r\n--notes=\"<text note>\"\r\n  [Optional]\r\n  Specify some additional notes to describe the reason of the log analysis / collection");
            Console.WriteLine("\r\n--mongodbextjson=[yes|no]\r\n  [Optional]\r\n  Add the necessary BSON types to import JSON in MongoDB");
        }
    }
    /*
        class Program
        {
            static void Main(string[] args)
            {
                var parser = new CommandLineArguments();
                if (!parser.Parse(args))
                {
                    return;
                }

                // Proceed with logic using parsed parameters
                Console.WriteLine("Log Folder: " + parser.Param_LogFolder);
                Console.WriteLine("Host Name: " + parser.Param_HostName);
                Console.WriteLine("Customer Name: " + parser.Param_CustomerName);
                Console.WriteLine("Notes: " + parser.Param_Notes);
                Console.WriteLine("MongoDB Extended JSON: " + parser.Param_MongoDBExtJSON);
            }
        }

    */
}
