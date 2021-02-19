using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace WaterBot.Commands
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class GeneralCommandModule : BaseCommandModule
    {
        [Command("greet")]
        public async Task GreetCommand(CommandContext ctx)
        {
            await ctx.RespondAsync("Hello!");
        }
    }
}
