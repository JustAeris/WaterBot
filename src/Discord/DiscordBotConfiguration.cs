using System.IO;
using Newtonsoft.Json.Linq;

namespace WaterBot.Discord
{
    public static class DiscordBotConfiguration
    {
        static DiscordBotConfiguration()
        {
            if (!File.Exists("config.json"))
                return;

            JObject content = JObject.Parse(File.ReadAllText("config.json"));

            Token = (string) content["Token"];
            DataDir = (string) content["DataDir"];

            UseCustomEmojis = (bool) content["CustomEmojis"]?["UseCustomEmojis"];

            if (!UseCustomEmojis) return;
            CustomEmojis.DropletMain = (string) content["CustomEmojis"]?["DropletMain"];
            CustomEmojis.DropletCheck = (string) content["CustomEmojis"]?["DropletCheck"];
            CustomEmojis.DropletCross = (string) content["CustomEmojis"]?["DropletCross"];
            CustomEmojis.DropletTrophy = (string) content["CustomEmojis"]?["DropletTrophy"];
            CustomEmojis.DropletFire = (string) content["CustomEmojis"]?["DropletFire"];
            CustomEmojis.DropletWarning = (string) content["CustomEmojis"]?["DropletWarning"];
        }

        public static string Token { get; }
        public static string DataDir { get; }

        public static bool UseCustomEmojis { get; }

        public static class CustomEmojis
        {
            public static string DropletMain { get; set; }
            public static string DropletCheck { get; set; }
            public static string DropletCross { get; set; }
            public static string DropletTrophy { get; set; }
            public static string DropletFire { get; set; }
            public static string DropletWarning { get; set; }
        }
    }
}
