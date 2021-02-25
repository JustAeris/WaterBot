using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using DSharpPlus;
using DSharpPlus.Entities;
using WaterBot.Data;
using WaterBot.Discord;

namespace WaterBot.Scheduled
{
    public class NotificationSystem
    {
        private readonly Timer _timer;
        private readonly DiscordClient _client;
        private readonly string _dropletMain =  Configuration.UseCustomEmojis ? Configuration.CustomEmojis.DropletMain : ":droplet:";

        public NotificationSystem(DiscordClient client)
        {
            _timer = new Timer
            {
                AutoReset = true,
                Enabled = true,
                Interval = 60000
            };
            _timer.Elapsed += TimerOnElapsed;
            _client = client;
        }

        public void Start() => _timer.Start();

        private void TimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                TimeSpan now = DateTime.UtcNow.TimeOfDay.KeepHoursMinutes();
                IEnumerable<UserData> enumerable = UserDataManager.GetAllUserData();

                foreach (UserData data in enumerable)
                {
                    if (!data.ReminderEnabled) continue;
                    if (now < data.WakeTime && now > data.SleepTime) continue;

                    try
                    {
                        DiscordGuild guild = await _client.GetGuildAsync(data.GuildId);
                        DiscordMember user = await guild.GetMemberAsync(data.UserId);

                        foreach (var timeSpan in data.RemindersList)
                        {
                            if (timeSpan < now && timeSpan > data.LatestReminder)
                            {
                                await user.SendMessageAsync(
                                $"Hey! it's time to drink {data.AmountPerInterval}mL of water to stay hydrated! {_dropletMain}");
                                data.LatestReminder = UserData.CalculateLatestReminder(data.RemindersList, now);
                                UserDataManager.SaveData(data);
                                break;
                            }

                            if (timeSpan != data.RemindersList.First() || data.LatestReminder != data.RemindersList.Last() || timeSpan >= now || now >= data.LatestReminder) continue;

                            await user.SendMessageAsync(
                                $"Hey! it's time to drink {data.AmountPerInterval}mL of water to stay hydrated! {_dropletMain}");
                            data.LatestReminder = UserData.CalculateLatestReminder(data.RemindersList, now);
                            UserDataManager.SaveData(data);
                            break;
                        }
                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine(exception);
                    }
                }
            });
        }
    }
}
