using System.Globalization;

internal class Program
{
    private static void Main(string[] args)
    {
        if (args.Length == 1 && args[0] == "-h")
        {
            Console.WriteLine("Usage: MeteorlogAggregator.exe SOURCE_FOLDER_PATH DESTINATION_FOLDER_PATH");
            return;
        }

        if (args.Length < 2)
        {
            Console.WriteLine("Not enugh parameter. See -h for more details!");
            return;
        }

        // Determine the current year and month
        string currentYear  = DateTime.Now.ToString("yyyy");
        string currentMonth = DateTime.Now.ToString("MM");

        // Construct the input and output file names
        string inputFilePath  = Path.Combine(args[0], $"MeteorLog-{currentYear}{currentMonth}.dat");
        string outputFilePath = Path.Combine(args[1], $"RMOB-{currentYear}{currentMonth}.dat");

        Console.WriteLine($"Source: {inputFilePath}, Dest: {outputFilePath}");

        // Check if input file exists
        if (!File.Exists(inputFilePath))
        {
            Console.WriteLine($"Input file {inputFilePath} does not exist.");
            return;
        }

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
        var lastDate = DateTime.UtcNow;

        // Generate all hour keys for the given month
        List<string> allHours = new List<string>();
        for (DateTime date = firstDate; date <= lastDate; date = date.AddHours(1))
        {
            allHours.Add(date.ToString("yyyyMMddHH"));
        }

        if (!Directory.Exists(args[1]))
        {
            Console.WriteLine("Directory not exists, I create it!");
            Directory.CreateDirectory(args[1]);
        }

        // Write the output to the output DAT file
        using (var writer = new StreamWriter(outputFilePath))
        {
            foreach (var hour in allHours)
            {
                string day = hour.Substring(0, 8);
                string hourPart = hour.Substring(8, 2);
                int count = hourCounts.ContainsKey(hour) ? hourCounts[hour] : 0;

                string outputLine = $"{day}{hourPart} , {hourPart} , {count:D2}";
                writer.WriteLine(outputLine);
            }
        }

        Console.WriteLine("Processing completed. Output written to " + outputFilePath);

    }
}