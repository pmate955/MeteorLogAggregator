using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeteorlogAggregator
{
    class ErrorHandling
    {
        public static void Handle(string message, Exception ex = null)
        {
            string formattedMessage = $"Something went wrong: {message}.";
            if (ex != null)
            {
                formattedMessage += $" Exception: {ex}";
            }

            Console.WriteLine(formattedMessage);
            DiscordBot.SendMessage(formattedMessage).Wait();
            
            if (ex != null)
                throw ex;
        }
    }
}
