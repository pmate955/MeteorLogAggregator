using System.Globalization;

internal class Program
{
    private static void Main(string[] args)
    {
        if (args.Length == 1 && args[0] == "-h")
        {
            Console.WriteLine("Usage: MeteorlogAggregator.exe SOURCE_FOLDER_PATH DESTINATION_FOLDER_PATH [-o]");
            Console.WriteLine("-o means it accepts to override RMOB_YYYYMM.dat");
            return;
        }

        if (args.Length < 2)
        {
            Console.WriteLine("Not enough parameter. See -h for more details!");
            return;
        }

        // Determine the current year and month
        var currDate = DateTime.UtcNow;        
        string currentYear  = currDate.ToString("yyyy");
        string currentMonth = currDate.ToString("MM");

        // Check the output directory
        if (!Directory.Exists(args[1]))
        {
            Console.WriteLine("Output directory not exists, I create it!");
            Directory.CreateDirectory(args[1]);
        }

        // We create the next month RMOB file for the colorgramme lab, because of local time.
        if (currDate.Day == DateTime.DaysInMonth(currDate.Year, currDate.Month))
        {
            var nextMonth = currDate.AddDays(1);
            var nextMonthFileName = Path.Combine(args[1], $"RMOB-{nextMonth.ToString("yyyy")}{nextMonth.ToString("MM")}.dat");
            Console.WriteLine($"Check next month file. {nextMonthFileName}");
            if(!File.Exists(nextMonthFileName))
            {
                Console.WriteLine("Created!");
                File.Create(nextMonthFileName);
            }
            else
            {
                Console.WriteLine("File exists already");
            }
        }

        // Construct the input and output file names
        string inputFilePath   = Path.Combine(args[0], $"MeteorLog-{currentYear}{currentMonth}.dat");
        string outputFilePath  = Path.Combine(args[1], $"RMOB-{currentYear}{currentMonth}.dat");
        string speclabFilePath = Path.Combine(args[0], $"RMOB-{currentYear}{currentMonth}.dat");

        if (speclabFilePath == outputFilePath)
        {
            Console.WriteLine("Error! The output directory is the same as the input. It will overwrite RMOB-YYYYMM.dat!");
            if(args.Length <= 2 || args[2] != "-o")
            {
                Console.WriteLine("To enable override, use -o flag! Exit...");
                return;
            }
        }

        Console.WriteLine($"Source: {inputFilePath}, Dest: {outputFilePath}");

        // Check if input file exists
        if (!File.Exists(inputFilePath))
        {
            Console.WriteLine($"Input file {inputFilePath} does not exist.");
            return;
        }

        var existingHours = _getExistingHours(speclabFilePath);

        // Read all lines from the input CSV file
        var lines = File.ReadAllLines(inputFilePath);

        // Dictionary to hold the counts per hour
        Dictionary<string, int> hourCounts = new Dictionary<string, int>();

        // Parse the input lines and count entries per hour
        foreach (var line in lines)
        {
            var parts = line.Split(',');
            string datetimeString = parts[0];

            // Parse the datetime string
            DateTime datetime = DateTime.ParseExact(datetimeString, "yyyyMMdd HHmmss", CultureInfo.InvariantCulture);

            // Get the hour part
            string hourKey = datetime.ToString("yyyyMMddHH");

            if (!hourCounts.ContainsKey(hourKey))
            {
                hourCounts[hourKey] = 0;
            }

            hourCounts[hourKey]++;
        }

        // Determine the range of dates in the given month
        var firstDate = new DateTime(int.Parse(currentYear), int.Parse(currentMonth), 1);
        
        // Generate all hour keys for the given month
        List<string> allHours = new List<string>();
        for (DateTime date = firstDate; date <= currDate; date = date.AddHours(1))
        {
            allHours.Add(date.ToString("yyyyMMddHH"));
        }

        // Write the output to the output DAT file
        using (var writer = new StreamWriter(outputFilePath))
        {
            foreach (var hour in allHours)
            {
                string day = hour.Substring(0, 8);
                string hourPart = hour.Substring(8, 2);
                int count = hourCounts.ContainsKey(hour) ? hourCounts[hour] : 0;

                if (existingHours.Contains(hour) || count > 0)
                {
                    string outputLine = $"{day}{hourPart} , {hourPart} , {count:D2}";
                    writer.WriteLine(outputLine);
                } else
                {
                    Console.WriteLine($"No data: {hour}");
                }
            }
        }

        Console.WriteLine("Processing completed. Output written to " + outputFilePath);

    }

    /// <summary>
    /// Reads the RMOB dat file for existing Speclab measurements. 
    /// </summary>
    /// <param name="rmobInputPath"></param>
    /// <returns></returns>
    private static HashSet<string> _getExistingHours(string rmobInputPath)
    {
        var res = new HashSet<string>();
        try
        {
            var lines = File.ReadAllLines(rmobInputPath);
            foreach (var line in lines)
            {
                if (line.Length == 0 || !line.Contains(" , "))
                    continue;

                string dateTimeString = line.Split(" , ")[0];
                res.Add(dateTimeString);
            }
        } 
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex);
        }
        return res;
    }
}