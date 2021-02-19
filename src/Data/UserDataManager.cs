using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DSharpPlus.Entities;
using Newtonsoft.Json;
using WaterBot.Discord;

namespace WaterBot.Data
{
    public static class UserDataManager
    {
        static UserDataManager()
        {
            Directory = DiscordBotConfiguration.DataDir;

            if (!System.IO.Directory.Exists(Directory))
                System.IO.Directory.CreateDirectory(Directory);
        }

        private static string Directory { get; }

        public static UserData GetData(ulong userId)
        {
            var path = $"{Directory}/{userId}.json";

            if (!File.Exists(path))
                return null;

            var content = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<UserData>(content);
        }

        public static UserData GetData(DiscordUser user)
        {
            return GetData(user.Id);
        }

        public static UserData GetData(DiscordMember member)
        {
            return GetData(member.Id);
        }

        public static void SaveData(UserData userData)
        {
            var userFile = $"{Directory}/{userData.UserId}.json";

            var json = JsonConvert.SerializeObject(userData, Formatting.Indented);

            using var f =
                new FileStream(userFile, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite)
                    {Position = 0};

            f.Write(Encoding.UTF8.GetBytes(json));
        }

        public static IEnumerable<UserData> GetAllUserData()
        {
            var files = System.IO.Directory.GetFiles(Directory);
            var list = new List<UserData>();

            return files.Select(file => JsonConvert.DeserializeObject<UserData>(File.ReadAllText(file))).ToList();
        }
    }
}
