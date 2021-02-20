using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using WaterBot.Data;

// ReSharper disable UnusedMember.Global

namespace WaterBot.Commands
{
    [Group("config")]
    // ReSharper disable once ClassNeverInstantiated.Global
    public class ConfigCommandModule : BaseCommandModule
    {
        [Command("save")]
        public async Task Save(CommandContext ctx, TimeSpan wakeTime, TimeSpan sleepTime, int amountPerInterval, int amountPerDay = 2000)
        {
            if (wakeTime > new TimeSpan(24, 0, 0) || sleepTime > new TimeSpan(24, 0, 0))
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
                sleepTime = sleepTime.Subtract(new TimeSpan(0, 0, sleepTime.Seconds));
                wakeTime = wakeTime.Subtract(new TimeSpan(0, 0, wakeTime.Seconds));
            }

            UserData userData = new UserData
            {
                AmountPerInterval = amountPerInterval,
                WakeTime = wakeTime,
                SleepTime = sleepTime,
                UserId = ctx.User.Id,
                AmountPerDay = amountPerDay,
                ReminderEnabled = true
            };

            userData.RemindersList = UserData.CalculateReminders(userData).ToList();

            UserDataManager.SaveData(userData);

            await ctx.RespondAsync("Config saved!");
        }

        [Command("show")]
        public async Task Show(CommandContext ctx)
        {
            UserData userData = UserDataManager.GetData(ctx.Member);

            await ctx.RespondAsync(new DiscordEmbedBuilder {Color = DiscordColor.CornflowerBlue}
                .WithAuthor($"{ctx.Member.Username}'s water reminder configuration", iconUrl: ctx.Member.AvatarUrl)
                .AddField("Wake Time", $"```{userData.WakeTime}```", true)
                .AddField("Sleep Time", $"```{userData.SleepTime}```", true)
                .AddField("Amount in mL", $"```{userData.AmountPerInterval}```", true));
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
