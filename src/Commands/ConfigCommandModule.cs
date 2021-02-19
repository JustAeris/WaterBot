using System;
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
        public async Task Save(CommandContext ctx, TimeSpan wakeTime, TimeSpan bedTime, int amountPerInterval)
        {
            if (amountPerInterval > 2000)
            {
                await ctx.RespondAsync("You cannot set an amount per interval higher than 2000mL (2L)!");
                return;
            }

            if (wakeTime > new TimeSpan(24, 0, 0) || bedTime > new TimeSpan(24, 0, 0))
            {
                await ctx.RespondAsync("You cannot set a wake/sleep time per interval higher than 24h!");
                return;
            }

            UserDataManager.SaveData(new UserData
            {
                AmountPerInterval = amountPerInterval,
                WakeTime = wakeTime,
                SleepTime = bedTime,
                UserId = ctx.User.Id,
                ReminderEnabled = true
            });

            await ctx.RespondAsync("Config saved!");
        }

        [Command("show")]
        public async Task Show(CommandContext ctx)
        {
            var userData = UserDataManager.GetData(ctx.Member);

            await ctx.RespondAsync(new DiscordEmbedBuilder {Color = DiscordColor.CornflowerBlue}
                .WithAuthor($"{ctx.Member.Username}'s water reminder configuration", iconUrl: ctx.Member.AvatarUrl)
                .AddField("Wake Time", $"```{userData.WakeTime}```", true)
                .AddField("Sleep Time", $"```{userData.SleepTime}```", true)
                .AddField("Amount in mL", $"```{userData.AmountPerInterval}```", true));
        }

        [Command("reminderon")]
        public async Task ReminderOn(CommandContext ctx)
        {
            var userData = UserDataManager.GetData(ctx.Member);
            userData.ReminderEnabled = true;
            UserDataManager.SaveData(userData);

            await ctx.RespondAsync("Your reminder has been turned on!");
        }

        [Command("reminderoff")]
        public async Task ReminderOff(CommandContext ctx)
        {
            var userData = UserDataManager.GetData(ctx.Member);
            userData.ReminderEnabled = false;
            UserDataManager.SaveData(userData);

            await ctx.RespondAsync("Your reminder has been turned off!");
        }
    }
}
