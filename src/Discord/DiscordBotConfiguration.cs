using Newtonsoft.Json;
using System.IO;

namespace WaterBot.Discord
{
    public class DiscordBotConfiguration
    {
        public string Token { get; set; }

        public static DiscordBotConfiguration Load(string path)
        {
            if (!File.Exists(path))
                return null;

            string content = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<DiscordBotConfiguration>(content);
        }
    }
}
