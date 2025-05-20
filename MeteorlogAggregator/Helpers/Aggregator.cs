using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeteorlogAggregator
{
    class Aggregator
    {
        // RMOB.dat -> Meteorlog (lossy, but works if something bad happened)
        public static void InverseOriginalAggregation(string inputPath, string outputPath, bool isOverrideEnabled)
        {
            // Determine the current year and month
            var currDate = DateTime.UtcNow;
            string currentYear = currDate.ToString("yyyy");
            string currentMonth = currDate.ToString("MM");

            // Check the output directory
            if (!Directory.Exists(outputPath))
            {
                Console.WriteLine("Output directory not exists, I create it!");
                Directory.CreateDirectory(outputPath);
            }

            // Construct the input and output file names
            string outLogPath = Path.Combine(outputPath, $"MeteorLog-{currentYear}{currentMonth}.dat");
            string inputFilePath = Path.Combine(inputPath, $"RMOB-{currentYear}{currentMonth}.dat");

            if (outLogPath == inputFilePath)
            {
                Console.WriteLine("Error! The output directory is the same as the input. It will overwrite RMOB-YYYYMM.dat!");
                if (!isOverrideEnabled)
                {
                    Console.WriteLine("To enable override, use -o flag! Exit...");
                    return;
                }
            }

            Console.WriteLine($"Source: {inputFilePath}, Dest: {outLogPath}");

            // Check if input file exists
            if (!File.Exists(inputFilePath))
            {
                Console.WriteLine($"Input file {inputFilePath} does not exist.");
                return;
            }

            // Read all lines from the input CSV file
            var lines = File.ReadAllLines(inputFilePath);

            using (var writer = new StreamWriter(outLogPath))
            {
                foreach (var line in lines)
                {
                    var parts = line.Split(" , ");
                    string datetime = parts[0];

                    // Parse the datetime string
                    DateTime dt = DateTime.ParseExact(datetime, "yyyyMMddHH", CultureInfo.InvariantCulture);

                    // Add some time       
                    dt = dt.AddMinutes(1);
                    int count = Convert.ToInt32(parts[2]);

                    for (int i = 0; i < count; i++)
                    {
                        string row = $"{dt:yyyyMMdd HHmmss},{i + 1},-10,-20,1200,10";
                        writer.WriteLine(row);
                        dt = dt.AddSeconds(1);
                    }
                }
            }

            Console.WriteLine("Processing completed. Output written to " + outLogPath);
        }

        /// <summary>
        /// Meteorlog -> RMOB.dat
        /// </summary>
        /// <param name="args"></param>
        public static void OriginalAggregation(string inputPath, string outputPath, bool isOverrideEnabled)
        {
            var currDateTime = DateTime.UtcNow;
            string currentYear = currDateTime.ToString("yyyy");
            string currentMonth = currDateTime.ToString("MM");

            // Check the output directory
            if (!Directory.Exists(outputPath))
            {
                Console.WriteLine("Output directory not exists, I create it!");
                Directory.CreateDirectory(outputPath);
            }

            CreateFileForNextMonth(outputPath, currDateTime);

            // Construct the input and output file names
            string meteorLogInputFilePath = Path.Combine(inputPath, $"MeteorLog-{currentYear}{currentMonth}.dat");
            string outputFilePath = Path.Combine(outputPath, $"RMOB-{currentYear}{currentMonth}.dat");
            string outputBackupFilePath = Path.Combine(outputPath, $"{AppSettings.BackupFilePrefix}{DateTime.UtcNow:yyyy-MM-dd-HH-mm-ss}-{currentYear}{currentMonth}.dat");
            string speclabFilePath = Path.Combine(inputPath, $"RMOB-{currentYear}{currentMonth}.dat");

            if (speclabFilePath == outputFilePath)
            {
                Console.WriteLine("Error! The output directory is the same as the input. It will overwrite RMOB-YYYYMM.dat!");

                if (!isOverrideEnabled)
                {
                    ErrorHandling.Handle("To enable override, use -o flag! Exit...");
                    return;
                }
            }

            Console.WriteLine($"Source: {meteorLogInputFilePath}, Dest: {outputFilePath}");

            // Check if input file exists
            if (!File.Exists(meteorLogInputFilePath))
            {
                ErrorHandling.Handle($"Input file {meteorLogInputFilePath} does not exist.");
                return;
            }

            var existingHours = GetExistingHours(speclabFilePath);

            Dictionary<string, int> hourCounts = ReadMeteorLogFile(meteorLogInputFilePath);

            var firstDate = new DateTime(int.Parse(currentYear), int.Parse(currentMonth), 1);

            // Generate all hour keys for the given month
            List<string> allHours = new();
            for (DateTime date = firstDate; date <= currDateTime; date = date.AddHours(1))
            {
                allHours.Add(date.ToString("yyyyMMddHH"));
            }

            ProcessData(currDateTime, outputFilePath, existingHours, hourCounts, allHours);

            // Backup
            if (currDateTime.Minute >= 50)
                File.Copy(outputFilePath, outputBackupFilePath);

            Console.WriteLine("Processing completed. Output written to " + outputFilePath);
        }

        /// <summary>
        /// It processes all the data, and writes to <paramref name="outputFilePath"/> 
        /// </summary>
        /// <param name="currDateTime">Current date and time for aggregation. UTC!</param>
        /// <param name="outputFilePath">Result RMOB.dat path</param>
        /// <param name="existingHours">DateTime in yyyyMMddHH format, which ones are in Speclab's RMOB.dat</param>
        /// <param name="hourCounts">Data from MeteorLog file grouped to yyyyMMddHH dates</param>
        /// <param name="allHours">All datetime in yyyyMMddHH format </param>
        private static void ProcessData(DateTime currDateTime, string outputFilePath, HashSet<string> existingHours, Dictionary<string, int> hourCounts, List<string> allHours)
        {
            string currentHour = currDateTime.ToString("yyyyMMddHH");

            // Write the output to the output DAT file
            using StreamWriter writer = new(outputFilePath);
            foreach (var hour in allHours)
            {
                string day = hour[..8];
                string hourPart = hour.Substring(8, 2);
                int count = hourCounts.ContainsKey(hour) ? hourCounts[hour] : 0;

                if (existingHours.Contains(hour) || count > 0)
                {
                    string outputLine = $"{day}{hourPart} , {hourPart} , {count:D2}";
                    writer.WriteLine(outputLine);
                }
                else
                {
                    Console.WriteLine($"No data: {hour}");
                }

                if (count == 0 && hour == currentHour && currDateTime.Minute >= AppSettings.NoDetectionThresholdMinutes)
                {
                    DiscordBot.SendMessage("There were no detections in the current hour. Please check the SDR and Spectrumlab app!").Wait();
                }
            }
        }

        /// <summary>
        /// Read all lines from the input CSV file
        /// </summary>
        /// <param name="meteorLogInputFilePath"></param>
        /// <returns></returns>
        private static Dictionary<string, int> ReadMeteorLogFile(string meteorLogInputFilePath)
        {
            var lines = File.ReadAllLines(meteorLogInputFilePath);

            Dictionary<string, int> hourCounts = new();

            foreach (var line in lines)
            {
                var parts = line.Split(',');
                string datetimeString = parts[0];
                var centerFreq = Convert.ToDecimal(parts[4].Replace(".", ","));

                // Parse the datetime string
                DateTime datetime = DateTime.ParseExact(datetimeString, "yyyyMMdd HHmmss", CultureInfo.InvariantCulture);

                // Get the hour part
                string hourKey = datetime.ToString("yyyyMMddHH");

                if (!hourCounts.ContainsKey(hourKey))
                {
                    hourCounts[hourKey] = 0;
                }

                if (centerFreq < AppSettings.MinFreq || centerFreq > AppSettings.MaxFreq)
                {
                    //Console.WriteLine($"{datetimeString} Outside of the frequency! {centerFreq} Hz");
                    continue;
                }

                hourCounts[hourKey]++;
            }

            return hourCounts;
        }

        /// <summary>
        /// We create the next month RMOB file for the colorgramme lab, because of local time.
        /// </summary>
        /// <param name="outputPath">Path for the output</param>
        /// <param name="currDateTime"></param>
        private static void CreateFileForNextMonth(string outputPath, DateTime currDateTime)
        {
            if (currDateTime.Day == DateTime.DaysInMonth(currDateTime.Year, currDateTime.Month))
            {
                var nextMonth = currDateTime.AddDays(1);
                var nextMonthFileName = Path.Combine(outputPath, $"RMOB-{nextMonth:yyyy}{nextMonth:MM}.dat");
                Console.WriteLine($"Check next month file. {nextMonthFileName}");

                if (!File.Exists(nextMonthFileName))
                {
                    Console.WriteLine("Created!");
                    File.Create(nextMonthFileName);
                }
                else
                {
                    Console.WriteLine("File exists already");
                }
            }
        }

        /// <summary>
        /// Reads the RMOB dat file for existing Speclab measurements. 
        /// </summary>
        /// <param name="rmobInputPath"></param>
        /// <returns></returns>
        private static HashSet<string> GetExistingHours(string rmobInputPath)
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
            catch (FileNotFoundException ex)
            {
                Console.WriteLine($"Nem található az RMOB fájl! {ex.Message}" );
            }
            catch (Exception ex)
            {
                ErrorHandling.Handle("Error: " + ex.Message);
            }

            return res;
        }
    }
}
