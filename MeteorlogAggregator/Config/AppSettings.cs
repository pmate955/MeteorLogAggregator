using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeteorlogAggregator
{
    class AppSettings
    {
        private static readonly IConfigurationRoot Config;

        /// <summary>
        /// How many backup files we want to keep
        /// </summary>
        public static int KeepMaxBackups { get; private set; }

        /// <summary>
        /// File prefix to identify backup files
        /// </summary>
        public static string BackupFilePrefix { get; private set; }

        /// <summary>
        /// Send notifications to DC?
        /// </summary>
        public static bool UseDiscordBot { get; private set; }

        /// <summary>
        /// Webhook URL
        /// </summary>
        public static string DiscordWebhook { get; private set; }

        /// <summary>
        /// Min freq (Hz)
        /// </summary>
        public static int MinFreq { get; private set; }

        /// <summary>
        /// Mas freq (Hz)
        /// </summary>
        public static int MaxFreq { get; private set; }

        /// <summary>
        /// If there were no detections in the current hour after this time, it sends notification
        /// </summary>
        public static int NoDetectionThresholdMinutes { get; private set; }

        /// <summary>
        /// Use audio level check
        /// </summary>
        public static bool UseAudioDetection { get; private set; }

        /// <summary>
        /// Full name or just the part of the audio input device name. Like CABLE as Virtual audio cable
        /// </summary>
        public static string AudioDeviceName { get; private set; }

        /// <summary>
        /// If true, we restart the SdrUno if there is no audio
        /// </summary>
        public static bool IsSdrUnoRestart { get; private set; }    

        /// <summary>
        /// Path of the SdrUno.exe
        /// </summary>
        public static string SdrUnoPath { get; private set; }
           
        static AppSettings()
        {
            var path = AppContext.BaseDirectory; 
            Config = new ConfigurationBuilder()
                .SetBasePath(path)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

            KeepMaxBackups = Config["AppSettings:KeepMaxBackups"] != null ? Convert.ToInt32(Config["AppSettings:KeepMaxBackups"]) : 10;
            BackupFilePrefix = Config["AppSettings:BackupFilePrefix"] ?? "RMOB-backup-";
            UseDiscordBot = Config["AppSettings:UseDiscordBot"] != null ? Convert.ToBoolean(Config["AppSettings:UseDiscordBot"]) :  false;
            DiscordWebhook = Config["AppSettings:DiscordWebhook"] ?? "";
            MinFreq = Config["AppSettings:MinFreq"] != null ? Convert.ToInt32(Config["AppSettings:MinFreq"]) : 1150;
            MaxFreq = Config["AppSettings:MaxFreq"] != null ? Convert.ToInt32(Config["AppSettings:MaxFreq"]) : 1250;
            NoDetectionThresholdMinutes = Config["AppSettings:NoDetectionThresholdMinutes"] != null ? Convert.ToInt32(Config["AppSettings:NoDetectionThresholdMinutes"]) : 20;
            UseAudioDetection = Config["AppSettings:UseAudioDetection"] != null ? Convert.ToBoolean(Config["AppSettings:UseAudioDetection"]) : false;
            AudioDeviceName = Config["AppSettings:AudioDeviceName"] ?? "CABLE";
            IsSdrUnoRestart = Config["AppSettings:IsSdrUnoRestart"] != null ? Convert.ToBoolean(Config["AppSettings:IsSdrUnoRestart"]) : false;
            SdrUnoPath = Config["AppSettings:SdrUnoPath"] ?? "C:\\Program Files (x86)\\SDRplay\\SDRuno\\SDRuno.exe";
        }
    }
}
