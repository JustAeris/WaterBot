using System;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using WaterBot.Commands;
using WaterBot.Data;

namespace WaterBot.Discord
{
    public class DiscordBot : IDisposable
    {
        private DiscordBotConfiguration _config;

        private DiscordClient _client;

        private UserDataManager _users;

        public DiscordBot(string configurationPath)
        {
            if (string.IsNullOrWhiteSpace(configurationPath))
                throw new ArgumentNullException(nameof(configurationPath));

            _config = DiscordBotConfiguration.Load(configurationPath);
            _users = new UserDataManager("data/users");
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
