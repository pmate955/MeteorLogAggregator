using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeteorlogAggregator
{
    public class SdrRestarter
    {
        public static void Restart()
        {
            try
            {
                DiscordBot.SendMessage("SDRuno restart process initiated!").Wait();

                Stop();

                Thread.Sleep(2000);

                Start(AppSettings.SdrUnoPath);

                DiscordBot.SendMessage("SDRuno restarted successfully! (I hope so)").Wait();
            }
            catch (Exception ex)
            {
                ErrorHandling.Handle($"Error in {nameof(SdrRestarter)} {nameof(Restart)} method", ex);
            }
        }

        /// <summary>
        /// Stops the SdrUno if it's running
        /// </summary>
        /// <returns></returns>
        public static void Stop()
        {
            string processName = "SDRuno";

            var processes = Process.GetProcessesByName(processName);

            foreach (var process in processes)
            {
                try
                {
                    Console.WriteLine($"Stop: {process.ProcessName} (PID: {process.Id})");
                    process.Kill();
                    process.WaitForExit();
                    Console.WriteLine("Stop successfull");
                }
                catch (Exception ex)
                {
                    ErrorHandling.Handle($"Error in {nameof(SdrRestarter)} {nameof(Stop)} method. {ex}");
                }
            }
        }

        /// <summary>
        /// It starts the exe file
        /// </summary>
        /// <param name="path"></param>
        public static void Start(string path)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("SdrUno.exe not found, please check the settings!");
            }
            Console.WriteLine("Start");

            Process.Start(path);

            Console.WriteLine("Start successfull!");
        }
    }
}
