using System;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using WaterBot.Commands;

namespace WaterBot.Discord
{
    public class DiscordBot : IDisposable
    {
        private DiscordBotConfiguration _config;

        private DiscordClient _client;

        public DiscordBot(string configurationPath)
        {
            if (string.IsNullOrWhiteSpace(configurationPath))
                throw new ArgumentNullException(nameof(configurationPath));

            _config = DiscordBotConfiguration.Load(configurationPath);
        }

        public async Task RunAsync()
        {
            _client = new DiscordClient(new DiscordConfiguration()
            {
                Token = _config.Token,
                TokenType = TokenType.Bot,
            });

            CommandsNextModule commands = _client.UseCommandsNext(new CommandsNextConfiguration()
            {
                StringPrefix = "!water",
            });

            commands.RegisterCommands<GeneralCommandModule>();

            await _client.ConnectAsync();
            await Task.Delay(-1);
        }

        public void Dispose()
        {
            _client.DisconnectAsync();
            _client.Dispose();

            GC.SuppressFinalize(this);
        }
    }
}
