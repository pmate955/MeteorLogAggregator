using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeteorlogAggregator
{
    class DiscordBot
    {
        public static async Task SendMessage(string message)
        {
            if (AppSettings.UseDiscordBot)
            {
                try
                {
                    using (var httpClient = new HttpClient())
                    {
                        var payload = new
                        {
                            content = message
                        };

                        var jsonPayload = JsonConvert.SerializeObject(payload);
                        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                        var response = await httpClient.PostAsync(AppSettings.DiscordWebhook, content);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }
    }
}
