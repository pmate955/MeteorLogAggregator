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
        }
    }
}
