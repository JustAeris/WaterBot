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
            Directory = Configuration.DataDir;

            if (!System.IO.Directory.Exists(Directory))
                System.IO.Directory.CreateDirectory(Directory);
        }

        private static string Directory { get; }

        public static UserData GetData(ulong userId)
        {
            string path = $"{Directory}/{userId}.json";

            if (!File.Exists(path))
                return null;

            string content = File.ReadAllText(path);
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
            string userFile = $"{Directory}/{userData.UserId}.json";

            string json = JsonConvert.SerializeObject(userData, Formatting.Indented);

            using FileStream f =
                new FileStream(userFile, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite)
                    {Position = 0};

            f.SetLength(0);

            f.Write(Encoding.UTF8.GetBytes(json));
        }

        public static IEnumerable<UserData> GetAllUserData()
        {
            string[] files = System.IO.Directory.GetFiles(Directory);

            return files.Select(file => JsonConvert.DeserializeObject<UserData>(File.ReadAllText(file))).ToList();
        }
    }
}
