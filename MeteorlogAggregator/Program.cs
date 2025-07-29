using MeteorlogAggregator;

internal class Program
{
    public static async Task Main(string[] args)
    {
        if (args.Contains("-t"))
        {
            int position = Array.FindIndex(args, arg => arg == "-t");
            int period = Convert.ToInt32(args[position + 1]);

            using var cts = new CancellationTokenSource();

            // Ctrl+C 
            Console.CancelKeyPress += (sender, e) =>
            {
                Console.WriteLine("Stopping...");
                cts.Cancel();
                e.Cancel = true; 
            };

            Console.WriteLine("Starting... Ctrl+C-vel to exit.");

            await ThreadedAggregation(args, period, cts.Token);

            Console.WriteLine("Main ended");
        }
        else
        {
            DoAggregation(args);
        }

    }

    private static async Task ThreadedAggregation(string[] args, int period, CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            Console.WriteLine("Start Threaded aggregation");

            try
            {
                DoAggregation(args);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Something went wrong during aggregation. {ex.Message}");
            }

            try
            {
                await Task.Delay(TimeSpan.FromSeconds(period), token);
            }
            catch (TaskCanceledException)
            {
                break;
            }
        }
    }

    private static void DoAggregation(string[] args)
    {
        try
        {
            if (AppSettings.UseAudioDetection)
            {
                AudioTester audio = new(AppSettings.AudioDeviceName);

                if (!audio.IsAudioSignal())
                {
                    DiscordBot.SendMessage("No signal on input device! Check SDRUno!").Wait();

                    if (AppSettings.IsSdrUnoRestart)
                    {
                        SdrRestarter.Restart();

                        Thread.Sleep(5000);

                        if (!audio.IsAudioSignal())
                        {
                            DiscordBot.SendMessage("No signal after restart! Please check manually!").Wait();
                        }
                    }
                }
            }

            if (args.Length == 1 && args[0] == "-h")
            {
                Console.WriteLine("Usage: MeteorlogAggregator.exe SOURCE_FOLDER_PATH DESTINATION_FOLDER_PATH [-o]");
                Console.WriteLine("-o means it accepts to override RMOB_YYYYMM.dat");
                Console.WriteLine("-i means inverse function. It creates the MeteorLog from RMOB.dat");
                Console.WriteLine("-t [sec] Threaded mode");
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