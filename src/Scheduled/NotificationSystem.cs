using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using WaterBot.Data;
using WaterBot.Discord;

namespace WaterBot.Scheduled
{
    public class NotificationSystem
    {
        private readonly Timer _timer;
        private readonly DiscordClient _client;
        private readonly string _dropletMain =  Configuration.UseCustomEmojis ? Configuration.CustomEmojis.DropletMain : ":droplet:";
        private readonly string _dropletCheck =  Configuration.UseCustomEmojis ? Configuration.CustomEmojis.DropletCheck : ":white_check_mark:";
        private readonly string _dropletFire =  Configuration.UseCustomEmojis ? Configuration.CustomEmojis.DropletFire : ":fire:";
        private readonly string _dropletTrophy =  Configuration.UseCustomEmojis ? Configuration.CustomEmojis.DropletTrophy : ":trophy:";

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
                    if (now < data.WakeTime || now > data.SleepTime) continue;

                    try
                    {
                        DiscordGuild guild = await _client.GetGuildAsync(data.GuildId);
                        DiscordMember user = await guild.GetMemberAsync(data.UserId);

                        foreach (var timeSpan in data.RemindersList)
                        {
                            if (timeSpan < now && timeSpan > data.LatestReminder)
                            {
                                var notification = await user.SendMessageAsync(
                                $"Hey! it's time to drink {data.AmountPerInterval}mL of water to stay hydrated! {_dropletMain}");
                                data.LatestReminder = UserData.CalculateLatestReminder(data.RemindersList, now);
                                UserDataManager.SaveData(data);
                                _ = Task.Run(async () =>
                                {
                                    await notification.CreateReactionAsync(Configuration.UseCustomEmojis ?
                                        DiscordEmoji.FromGuildEmote(_client, ulong.Parse(Regex.Matches(_dropletCheck, @"[0-9]+", RegexOptions.Compiled).First().Value)) :
                                        DiscordEmoji.FromName(_client, _dropletCheck, false));

                                    var interactivity = _client.GetInteractivity();

                                    var result = await interactivity.WaitForReactionAsync(notification, user,
                                        Configuration.WaterStreakBreakDelay);

                                    if (result.TimedOut)
                                    {
                                        if (data.WaterStreak == 0) return;
                                        await user.SendMessageAsync($":cry: You lost your water-drinking streak.\n{_dropletTrophy} Your best water-drinking streak is {data.BestWaterStreak}!");
                                        data.WaterStreak = 0;
                                        return;
                                    }

                                    data.WaterStreak++;
                                    if (data.WaterStreak > data.BestWaterStreak)
                                        data.BestWaterStreak = data.WaterStreak;

                                    if (data.WaterStreak % 10 == 0)
                                        await user.SendMessageAsync($"{_dropletFire} Keep up drinking water, you're on a {data.WaterStreak} streak!");

                                    UserDataManager.SaveData(data);
                                });
                                break;
                            }

                            if (timeSpan == data.RemindersList.First() || data.LatestReminder == data.RemindersList.Last() || timeSpan < now || now < data.LatestReminder)
                            {
                                var notification = await user.SendMessageAsync(
                                    $"Hey! it's time to drink {data.AmountPerInterval}mL of water to stay hydrated! {_dropletMain}");
                                data.LatestReminder = UserData.CalculateLatestReminder(data.RemindersList, now);
                                UserDataManager.SaveData(data);
                                _ = Task.Run(async () =>
                                {
                                    await notification.CreateReactionAsync(Configuration.UseCustomEmojis ?
                                        DiscordEmoji.FromGuildEmote(_client, ulong.Parse(Regex.Matches(_dropletCheck, @"[0-9]+", RegexOptions.Compiled).First().Value)) :
                                        DiscordEmoji.FromName(_client, _dropletCheck, false));

                                    var interactivity = _client.GetInteractivity();

                                    var result = await interactivity.WaitForReactionAsync(notification, user,
                                        Configuration.WaterStreakBreakDelay);

                                    if (result.TimedOut)
                                    {
                                        if (data.WaterStreak == 0) return;
                                        await user.SendMessageAsync($":cry: You lost your water-drinking streak.\n{_dropletTrophy} Your best water-drinking streak is {data.BestWaterStreak}!");
                                        data.WaterStreak = 0;
                                        return;
                                    }

                                    data.WaterStreak++;
                                    if (data.WaterStreak > data.BestWaterStreak)
                                        data.BestWaterStreak = data.WaterStreak;

                                    if (data.WaterStreak % 10 == 0)
                                        await user.SendMessageAsync($"{_dropletFire} Keep up drinking water, you're on a {data.WaterStreak} streak!");

                                    UserDataManager.SaveData(data);
                                });
                                break;
                            }
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
