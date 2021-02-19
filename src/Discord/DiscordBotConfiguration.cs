using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WaterBot.Discord
{
    public static class DiscordBotConfiguration
    {
        public static string Token { get; set; }
        public static string DataDir { get; set; }

        static DiscordBotConfiguration()
        {
            if (!File.Exists("config.json"))
                return;

            var content = JObject.Parse(File.ReadAllText("config.json"));

            Token = (string) content["Token"];
            DataDir = (string) content["DataDir"];
        }
    }
}
