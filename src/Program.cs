using WaterBot.Discord;

namespace WaterBot
{
    public class Program
    {
        private static void Main(string[] args)
        {
            using (DiscordBot bot = new DiscordBot("config.json"))
                bot.RunAsync().GetAwaiter().GetResult();
        }
    }
}
