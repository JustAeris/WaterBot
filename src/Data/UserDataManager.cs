using System;
using System.IO;
using DSharpPlus.Entities;
using Newtonsoft.Json;

namespace WaterBot.Data
{
    public class UserDataManager
    {
        public string Directory { get; set; }

        public UserDataManager(string directory)
        {
            if (string.IsNullOrWhiteSpace(directory))
                throw new ArgumentNullException(nameof(directory));

            Directory = directory;
        }

        public UserData GetData(ulong userId)
        {
            string path = Path.Combine(Directory, userId.ToString());

            if (!File.Exists(path))
                return null;

            string content = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<UserData>(content);
        }

        public UserData GetData(DiscordUser user)
        {
            return GetData(user.Id);
        }

        // TODO: Make data saving possible
    }
}
