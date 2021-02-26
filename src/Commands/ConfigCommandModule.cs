using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using WaterBot.Data;
using WaterBot.Http.WorldTimeAPI;
using WaterBot.Discord;
using System.Globalization;

// ReSharper disable UnusedMember.Global

namespace WaterBot.Commands
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class ConfigCommandModule : BaseCommandModule
    {
        private readonly string _dropletMain = Configuration.UseCustomEmojis ? Configuration.CustomEmojis.DropletMain : ":droplet:";
        private readonly string _dropletCheck = Configuration.UseCustomEmojis ? Configuration.CustomEmojis.DropletCheck : ":white_check_mark:";
        private readonly string _dropletCross = Configuration.UseCustomEmojis ? Configuration.CustomEmojis.DropletCross : ":x:";
        private readonly string _dropletWarning = Configuration.UseCustomEmojis ? Configuration.CustomEmojis.DropletWarning : ":warning:";
        private readonly string _dropletTrophy = Configuration.UseCustomEmojis ? Configuration.CustomEmojis.DropletTrophy : ":trophy:";
        private readonly string _dropletFire = Configuration.UseCustomEmojis ? Configuration.CustomEmojis.DropletFire : ":fire:";

        [Command("setup"), Description("Allows you to save a reminder configuration.")]
        public async Task Save(CommandContext ctx,
            [Description("Time you usually wake up. Example: 8h")] TimeSpan wakeTime,
            [Description("Time you usually sleep. Example: 22h")] TimeSpan sleepTime,
            [Description("Amount of water in mL you'd like to drink each time.")] int amountPerInterval,
            [Description("Total amount of water you'd like to drink per day. Defaults to 2000mL (2L).")] int amountPerDay = 2000)
        {
            if (wakeTime >
                new TimeSpan(24,
                    0,
                    0) ||
                sleepTime >
                new TimeSpan(24,
                    0,
                    0))
            {
                await ctx.RespondAsync($"{_dropletCross} You cannot set a wake/sleep time higher than 24h!");
                return;
            }

            if (wakeTime > sleepTime)
            {
                await ctx.RespondAsync($"{_dropletCross} You cannot set a wake time higher than the sleep time!");
                return;
            }

            if (amountPerInterval > amountPerDay)
            {
                await ctx.RespondAsync($"{_dropletCross} The amount per interval cannot be higher than the amount per day!");
                return;
            }

            if (wakeTime.Seconds > 0 || sleepTime.Seconds > 0)
            {
                await ctx.RespondAsync($"{_dropletWarning} Seconds will not be taken in account!");
                sleepTime = sleepTime.KeepHoursMinutes();
                wakeTime = wakeTime.KeepHoursMinutes();
            }

            UserData data = UserDataManager.GetData(ctx.Member);
            if (data?.UtcOffset != null)
            {
                UserData userDataNoTz = new UserData
                {
                    AmountPerInterval = amountPerInterval,
                    WakeTime = wakeTime - data.UtcOffset,
                    SleepTime = sleepTime - data.UtcOffset,
                    UtcOffset = data.UtcOffset,
                    UserId = ctx.User.Id,
                    GuildId = ctx.Guild.Id,
                    AmountPerDay = amountPerDay,
                    ReminderEnabled = true,
                    WaterStreak = data.WaterStreak,
                    BestWaterStreak = data.BestWaterStreak
                };

                userDataNoTz.RemindersList = UserData.CalculateReminders(userDataNoTz)
                    .ToList();

                TimeSpan utcNoTz = DateTime.UtcNow.TimeOfDay.KeepHoursMinutes();
                userDataNoTz.LatestReminder = UserData.CalculateLatestReminder(userDataNoTz.RemindersList, utcNoTz);

                UserDataManager.SaveData(userDataNoTz);

                await ctx.RespondAsync(new DiscordEmbedBuilder
                {
                    Color = DiscordColor.Grayple,
                    Title = $"{_dropletMain} Water Bot",
                    Description = $"{_dropletCheck} Configuration saved! Thank you!"
                });
                return;
            }

            InteractivityExtension interactivity = ctx.Client.GetInteractivity();

            DiscordMessage regionSelection = await ctx.RespondAsync(new DiscordEmbedBuilder
                {
                    Color = DiscordColor.CornflowerBlue,
                    Title = $"{_dropletMain} Timezone selection",
                    Description = "Choose the region you're in:"
                }
                .AddField("Available regions:",
                    "Africa\nAmerica\nAntarctica\nAsia\nAtlantic\nAustralia\nEurope\nIndian\nPacific")
                .WithFooter("Answer with the name of your corresponding region."));


            InteractivityResult<DiscordMessage> result =
                await interactivity.WaitForMessageAsync(message => message.Author == ctx.Member);

            if (result.TimedOut)
            {
                await regionSelection.ModifyAsync(new DiscordEmbedBuilder
                {
                    Color = DiscordColor.Grayple,
                    Title = $"{_dropletMain} Timezone selection",
                    Description = $"{_dropletCross} Timed out!"
                }.Build());
                await regionSelection.DeleteAllReactionsAsync();
                return;
            }

            TextInfo ti = CultureInfo.InvariantCulture.TextInfo;
            string selectedRegion = ti.ToTitleCase(result.Result.Content);

            await result.Result.DeleteAsync();

            if (string.IsNullOrWhiteSpace(selectedRegion))
            {
                await regionSelection.DeleteAsync();
                await ctx.RespondAsync("Invalid reaction! Please run the command again again.");
                return;
            }

            using WorldTimeApiClient api = new WorldTimeApiClient();

            List<string> regions;
            try
            {
                regions = (List<string>) await api.GetRegions(selectedRegion);
            }
            catch (Exception)
            {
                await regionSelection.ModifyAsync(new DiscordEmbedBuilder
                {
                    Color = DiscordColor.Grayple,
                    Title = $"{_dropletMain} Timezone selection",
                    Description = "Incorrect selection! Please run the command again."
                }.Build());
                await regionSelection.DeleteAllReactionsAsync();
                return;
            }

            string regionsList = regions.Aggregate("",
                (current,
                        region) => current +
                                   region.Replace(selectedRegion + "/",
                                       "") +
                                   "\n");

            await regionSelection.DeleteAllReactionsAsync();

            DiscordEmbedBuilder builder = new DiscordEmbedBuilder
                {
                    Color = DiscordColor.CornflowerBlue,
                    Title =  $"{_dropletMain} Timezone selection",
                    Description = "Choose the region you're in:"
                }
                .WithFooter("Answer with the name of the city corresponding to your region.");

            if (regionsList.Length < 1024)
            {
                builder.AddField("Available regions:", regionsList);
            }
            else
            {
                IEnumerable<IEnumerable<char>> chunks = regionsList.ChunkBy(1024);

                foreach (IEnumerable<char> chars in chunks)
                {
                    string s = new string(chars.ToArray());
                    builder.AddField("Available regions", s);
                }
            }

            await regionSelection.ModifyAsync(builder.Build());

            InteractivityResult<DiscordMessage> answer =
                await interactivity.WaitForMessageAsync(message => message.Author == ctx.Member);

            if (result.TimedOut)
            {
                await regionSelection.ModifyAsync(new DiscordEmbedBuilder
                {
                    Color = DiscordColor.Grayple,
                    Title = $"{_dropletMain} Timezone selection",
                    Description = $"{_dropletCross} Timed out!"
                }.Build());
                await regionSelection.DeleteAllReactionsAsync();
                return;
            }

            string selectedCity = ti.ToTitleCase(answer.Result.Content);

            TimeZoneResponse timeZone;
            try
            {
                timeZone = await api.GetTimeZone(
                        $"{selectedRegion}/{selectedCity}");
            }
            catch (Exception)
            {
                await regionSelection.ModifyAsync(new DiscordEmbedBuilder
                {
                    Color = DiscordColor.Grayple,
                    Title = $"{_dropletMain} Timezone selection",
                    Description = $"{_dropletCross} Incorrect selection! Please run the command again."
                }.Build());
                await regionSelection.DeleteAllReactionsAsync();
                return;
            }

            TimeSpan utcOffset = TimeSpan.FromHours(timeZone.UtcOffset);

            UserData userData = new UserData
            {
                AmountPerInterval = amountPerInterval,
                WakeTime = wakeTime - utcOffset,
                SleepTime = sleepTime - utcOffset,
                UtcOffset = utcOffset,
                UserId = ctx.User.Id,
                GuildId = ctx.Guild.Id,
                AmountPerDay = amountPerDay,
                ReminderEnabled = true,
                // ReSharper disable once PossibleNullReferenceException
                WaterStreak = data.WaterStreak,
                BestWaterStreak = data.BestWaterStreak
            };

            userData.RemindersList = UserData.CalculateReminders(userData)
                .ToList();

            TimeSpan utc = DateTime.UtcNow.TimeOfDay.KeepHoursMinutes();
            userData.LatestReminder = UserData.CalculateLatestReminder(userData.RemindersList, utc);

            UserDataManager.SaveData(userData);

            await regionSelection.ModifyAsync(new DiscordEmbedBuilder
            {
                Color = DiscordColor.Grayple,
                Title = $"{_dropletMain} Timezone selection",
                Description = $"{_dropletCheck} Configuration saved! Thank you!"
            }.Build());
            await answer.Result.DeleteAsync();
            await regionSelection.DeleteAllReactionsAsync();
        }

        [Command("show"), Description("Show your current configuration.")]
        public async Task Show(CommandContext ctx)
        {
            UserData userData = UserDataManager.GetData(ctx.Member);

            if (userData == null)
            {
                await ctx.RespondAsync(new DiscordEmbedBuilder
                {
                    Color = DiscordColor.Red,
                    Title = $"{_dropletMain} No configuration found!",
                    Description = $"{_dropletCross} For more information, type `wb!help setup`"
                });
                return;
            }

            await ctx.RespondAsync(new DiscordEmbedBuilder
                {
                    Color = DiscordColor.CornflowerBlue
                }
                .WithAuthor($"{ctx.Member.Username}'s {_dropletMain} reminder configuration",
                    iconUrl: ctx.Member.AvatarUrl)
                .AddField("Wake Time",
                    $"```{userData.WakeTime.Add(userData.UtcOffset)}```",
                    true)
                .AddField("Sleep Time",
                    $"```{userData.SleepTime.Add(userData.UtcOffset)}```",
                    true)
                .AddField("UTC time offset",
                    $"```{userData.UtcOffset}```",
                    true)
                .AddField("Amount in mL per reminder",
                    $"```{userData.AmountPerInterval}```",
                    true)
                .AddField("Amount in mL per day",
                    $"```{userData.AmountPerDay}```",
                    true)
                .AddField("Reminders enabled",
                    $"```{userData.ReminderEnabled}```",
                    true));
        }

        [Command("reminderson"), Description("Enable your reminders.")]
        public async Task ReminderOn(CommandContext ctx)
        {
            UserData userData = UserDataManager.GetData(ctx.Member);

            if (userData == null)
            {
                await ctx.RespondAsync(new DiscordEmbedBuilder
                {
                    Color = DiscordColor.Red,
                    Title = $"{_dropletMain} No configuration found!",
                    Description = $"{_dropletCross} For more information, type `wb!help setup`"
                });
            }

            Debug.Assert(userData != null, nameof(userData) + " != null");
            userData.ReminderEnabled = true;
            UserDataManager.SaveData(userData);

            await ctx.RespondAsync($"Your reminders have been turned on! {_dropletCheck}");
        }

        [Command("remindersoff"), Description("Disable your reminders.")]
        public async Task ReminderOff(CommandContext ctx)
        {
            UserData userData = UserDataManager.GetData(ctx.Member);

            if (userData == null)
            {
                await ctx.RespondAsync(new DiscordEmbedBuilder
                    {
                        Color = DiscordColor.Red,
                        Title = $"{_dropletMain} No configuration found!",
                        Description = "For more information, type `wb!help setup`"
                    });
            }

            Debug.Assert(userData != null, nameof(userData) + " != null");
            userData.ReminderEnabled = false;
            UserDataManager.SaveData(userData);

            await ctx.RespondAsync($"Your reminders have been turned off! {_dropletCross}");
        }

        [Command("next"), Description("Show the next reminder for you")]
        public async Task ShowNextReminder(CommandContext ctx)
        {
            UserData data = UserDataManager.GetData(ctx.Member);
            TimeSpan now = DateTime.UtcNow.TimeOfDay.KeepHoursMinutes();

            TimeSpan nextReminder = (now > data.RemindersList.Last() ? data.RemindersList.First()
                : data.RemindersList.First(ts => ts > now));

            await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                {
                    Description = $"```{nextReminder + data.UtcOffset}```{(nextReminder < now ? $"\n{_dropletWarning} You have no reminders left for today.": null)}",
                    Color = DiscordColor.CornflowerBlue
                }
                .WithAuthor($"{ctx.Member.Username}'s next reminder is at:", iconUrl: ctx.Member.AvatarUrl));
        }

        [Command("waterstreak"), Description("Show your current and best water-drinking streak."), Aliases("streak")]
        public async Task WaterStreak(CommandContext ctx)
        {
            if (!Configuration.WaterStreakEnabled)
            {
                await ctx.RespondAsync($"{_dropletCross} Water-drinking streaks are not enabled in bot's settings!");
                return;
            }

            UserData data = UserDataManager.GetData(ctx.Member);

            await ctx.RespondAsync(new DiscordEmbedBuilder
                {
                    Description = $"{(data.WaterStreak == 0 ? "You don't have a current water-drinking streak :frowning:." : $"You have a water-drinking streak of **{data.WaterStreak}**, keep on hydrating! {_dropletFire}")}" +
                                  $"\n{(data.BestWaterStreak == 0 ? "You don't have a water-drinking streak :frowning2:, you should seriously start hydrating." : $"Your best water-drinking streak is **{data.BestWaterStreak}**, great job! {_dropletTrophy}")}",
                    Color = DiscordColor.CornflowerBlue
                }
                .WithAuthor($"{ctx.Member.Username}'s stats:", iconUrl: ctx.Member.AvatarUrl));
        }
    }
}
