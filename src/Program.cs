﻿using System;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using Microsoft.Extensions.Logging;
using WaterBot.Commands;
using WaterBot.Discord;

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
                MinimumLogLevel = LogLevel.Debug
            });

            CommandsNextExtension commands = _client.UseCommandsNext(new CommandsNextConfiguration
            {
                StringPrefixes = new[] {"wbot!", "wb!", "water!"}
            });

            commands.RegisterCommands<GeneralCommandModule>();
            commands.RegisterCommands<ConfigCommandModule>();

            commands.CommandErrored += (s, e) =>
                { Console.WriteLine(e.Exception); return Task.CompletedTask; };

            await _client.ConnectAsync();
            await Task.Delay(-1);
        }
    }
}
