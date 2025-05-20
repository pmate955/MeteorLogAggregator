using MeteorlogAggregator;

internal class Program
{
    private static void Main(string[] args)
    {
        try
        {
            if (AppSettings.UseAudioDetection)
            {
                AudioTester audio = new(AppSettings.AudioDeviceName);

                if (!audio.IsAudioSignal())
                    DiscordBot.SendMessage("No signal on input device! Check SDRUno!").Wait();
            }

            if (args.Length == 1 && args[0] == "-h")
            {
                Console.WriteLine("Usage: MeteorlogAggregator.exe SOURCE_FOLDER_PATH DESTINATION_FOLDER_PATH [-o]");
                Console.WriteLine("-o means it accepts to override RMOB_YYYYMM.dat");
                Console.WriteLine("-i means inverse function. It creates the MeteorLog from RMOB.dat");
                return;
            }

            if (args.Length < 2)
            {
                ErrorHandling.Handle("Not enough parameter. See -h for more details!");                
                return;
            }

            string inputPath = args[0];
            string outputPath = args[1];
            bool isOverrideEnabled = args.Contains("-o");

            if (args.Contains("-i"))
            {
                Aggregator.InverseOriginalAggregation(inputPath, outputPath, isOverrideEnabled);
            }
            else
            {
                Aggregator.OriginalAggregation(inputPath, outputPath, isOverrideEnabled);
            }

            RemoveBackups(outputPath);
        }
        catch (Exception ex)
        {
            ErrorHandling.Handle("Unknown error", ex);
        }

    }

    /// <summary>
    /// Removes the old backup files
    /// </summary>
    /// <param name="path"></param>
    public static void RemoveBackups(string path)
    {
        Console.WriteLine($"Remove backups from {path}");

        var files = Directory.GetFiles(path, $"{AppSettings.BackupFilePrefix}*")
           .Select(file => new FileInfo(file))
           .OrderByDescending(file => file.CreationTime)
           .ToList();

        Console.WriteLine($"All files: {files.Count}");

        var filesToDelete = files.Skip(AppSettings.KeepMaxBackups).ToList();

        foreach (var file in filesToDelete)
        {
            file.Delete();
            Console.WriteLine($"Deleted: {file.Name}");
        }
    }
}