#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using WaterBot.Data;
using WaterBot.Http.WorldTimeAPI;

#endregion

// ReSharper disable UnusedMember.Global

namespace WaterBot.Commands
{
    [Group("config")]
    // ReSharper disable once ClassNeverInstantiated.Global
    public class ConfigCommandModule : BaseCommandModule
    {
        [Command("save")]
        public async Task Save(CommandContext ctx,
            TimeSpan wakeTime,
            TimeSpan sleepTime,
            int amountPerInterval,
            int amountPerDay = 2000)
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
                await ctx.RespondAsync("You cannot set a wake/sleep time per interval higher than 24h!");
                return;
            }

            if (wakeTime > sleepTime)
            {
                await ctx.RespondAsync("You cannot set a wake time higher than the sleep time!");
                return;
            }

            if (amountPerInterval > amountPerDay)
            {
                await ctx.RespondAsync("The amount per interval cannot be higher than the amount per day!");
                return;
            }

            if (wakeTime.Seconds > 0 || sleepTime.Seconds > 0)
            {
                await ctx.RespondAsync(":warning: Seconds will not be taken in account!");
                sleepTime = sleepTime.Subtract(new TimeSpan(0,
                    0,
                    sleepTime.Seconds));
                wakeTime = wakeTime.Subtract(new TimeSpan(0,
                    0,
                    wakeTime.Seconds));
            }

            InteractivityExtension interactivity = ctx.Client.GetInteractivity();

            DiscordMessage regionSelection = await ctx.RespondAsync(new DiscordEmbedBuilder
                {
                    Color = DiscordColor.CornflowerBlue,
                    Title = "Timezone selection",
                    Description = "Choose the region you're in:"
                }
                .AddField("Available regions:",
                    ":one: Africa\n:two: America\n:three: Antarctica\n:four: Asia\n:five: Atlantic\n:six: Australia\n:seven: Europe\n:eight: Indian\n:nine: Pacific")
                .WithFooter("Use reactions below to make your choice."));

            await regionSelection.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client,
                ":one:"));
            await regionSelection.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client,
                ":two:"));
            await regionSelection.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client,
                ":three:"));
            await regionSelection.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client,
                ":four:"));
            await regionSelection.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client,
                ":five:"));
            await regionSelection.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client,
                ":six:"));
            await regionSelection.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client,
                ":seven:"));
            await regionSelection.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client,
                ":eight:"));
            await regionSelection.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client,
                ":nine:"));

            InteractivityResult<MessageReactionAddEventArgs> result =
                await interactivity.WaitForReactionAsync(regionSelection,
                    ctx.User);

            if (result.TimedOut)
            {
                await regionSelection.ModifyAsync(new DiscordEmbedBuilder
                {
                    Color = DiscordColor.Grayple,
                    Title = "Timezone selection",
                    Description = ":x: Timed out!"
                }.Build());
                await regionSelection.DeleteAllReactionsAsync();
                return;
            }

            string selectedRegion = result.Result.Emoji.GetDiscordName() switch
            {
                ":one:" => "Africa",
                ":two:" => "America",
                ":three:" => "Antarctica",
                ":four:" => "Asia",
                ":five:" => "Atlantic",
                ":six:" => "Australia",
                ":seven:" => "Europe",
                ":eight:" => "Indian",
                ":nine:" => "Pacific",
                _ => ""
            };

            if (string.IsNullOrWhiteSpace(selectedRegion))
            {
                await regionSelection.DeleteAsync();
                await ctx.RespondAsync("Invalid reaction! Please run the command again again.");
                return;
            }

            using WorldTimeApiClient api = new WorldTimeApiClient();

            List<string> regions = (List<string>) await api.GetRegions(selectedRegion);

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
                    Title = "Timezone selection",
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
                    Title = "Timezone selection",
                    Description = ":x: Timed out!"
                }.Build());
                await regionSelection.DeleteAllReactionsAsync();
                return;
            }

            string selectedCity = answer.Result.Content;

            TimeZoneResponse timeZone;
            try
            {
                timeZone = await api.GetTimeZone(
                        $"{selectedRegion}/{selectedCity.Replace(selectedCity[0], char.ToUpper(selectedCity[0]))}");
            }
            catch (Exception)
            {
                await regionSelection.ModifyAsync(new DiscordEmbedBuilder
                {
                    Color = DiscordColor.Grayple,
                    Title = "Timezone selection",
                    Description = "Incorrect selection! Please run the command again."
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
                AmountPerDay = amountPerDay,
                ReminderEnabled = true
            };

            userData.RemindersList = UserData.CalculateReminders(userData)
                .ToList();

            UserDataManager.SaveData(userData);

            await regionSelection.ModifyAsync(new DiscordEmbedBuilder
            {
                Color = DiscordColor.Grayple,
                Title = "Timezone selection",
                Description = ":white_check_mark: Configuration saved! Thanks you!"
            }.Build());
            await answer.Result.DeleteAsync();
            await regionSelection.DeleteAllReactionsAsync();
        }

        [Command("show")]
        public async Task Show(CommandContext ctx)
        {
            UserData userData = UserDataManager.GetData(ctx.Member);

            await ctx.RespondAsync(new DiscordEmbedBuilder
                {
                    Color = DiscordColor.CornflowerBlue
                }
                .WithAuthor($"{ctx.Member.Username}'s water reminder configuration",
                    iconUrl: ctx.Member.AvatarUrl)
                .AddField("Wake Time",
                    $"```{userData.WakeTime}```",
                    true)
                .AddField("Sleep Time",
                    $"```{userData.SleepTime}```",
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

        [Command("reminderon")]
        public async Task ReminderOn(CommandContext ctx)
        {
            UserData userData = UserDataManager.GetData(ctx.Member);
            userData.ReminderEnabled = true;
            UserDataManager.SaveData(userData);

            await ctx.RespondAsync("Your reminder has been turned on!");
        }

        [Command("reminderoff")]
        public async Task ReminderOff(CommandContext ctx)
        {
            UserData userData = UserDataManager.GetData(ctx.Member);
            userData.ReminderEnabled = false;
            UserDataManager.SaveData(userData);

            await ctx.RespondAsync("Your reminder has been turned off!");
        }
    }
}
