using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace WaterBot.Commands
{
    public class GeneralCommandModule
    {
        [Command("greet")]
        public async Task GreetCommand(CommandContext context)
        {
            await context.RespondAsync("Hello!");
        }
    }
}
