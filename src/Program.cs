using System;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
using Microsoft.Extensions.Logging;
using WaterBot.Commands;
using WaterBot.Discord;
using WaterBot.Scheduled;

namespace WaterBot
{
    public static class Program
    {
        private static DiscordClient _client;

        private static void Main()
        {
            MainAsync().GetAwaiter().GetResult();
        }

        private static async Task MainAsync()
        {
            _client = new DiscordClient(new DiscordConfiguration
            {
                Token = DiscordBotConfiguration.Token,
                TokenType = TokenType.Bot,
#if DEBUG
                MinimumLogLevel = LogLevel.Debug,
#else
                MinimumLogLevel = LogLevel.Information,
#endif
                Intents = DiscordIntents.All
            });

            CommandsNextExtension commands = _client.UseCommandsNext(new CommandsNextConfiguration
            {
                StringPrefixes = new[] {"wbot!", "wb!", "water!"},
                IgnoreExtraArguments = true
            });

            _client.UseInteractivity(new InteractivityConfiguration()
            {
                PollBehaviour = PollBehaviour.KeepEmojis,
                Timeout = TimeSpan.FromSeconds(30)
            });

            commands.RegisterCommands<GeneralCommandModule>();
            commands.RegisterCommands<ConfigCommandModule>();

            commands.CommandErrored += (s, e) =>
                { Console.WriteLine(e.Exception); return Task.CompletedTask; };

            NotificationSystem dm = new NotificationSystem(_client);
            dm.Start();

            await _client.ConnectAsync();
            await Task.Delay(-1);
        }
    }
}
