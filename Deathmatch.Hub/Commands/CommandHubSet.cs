using Microsoft.Extensions.Localization;
using OpenMod.Core.Commands;
using OpenMod.Unturned.Users;
using System;
using System.Threading.Tasks;

namespace Deathmatch.Hub.Commands
{
    [Command("set")]
    [CommandDescription("Sets the hub's center and radius")]
    [CommandSyntax("<radius>")]
    [CommandActor(typeof(UnturnedUser))]
    [CommandParent(typeof(CommandHub))]
    public class CommandHubSet : Command
    {
        private readonly DeathmatchHubPlugin _plugin;
        private readonly IStringLocalizer _stringLocalizer;

        public CommandHubSet(DeathmatchHubPlugin plugin,
            IStringLocalizer stringLocalizer,
            IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _plugin = plugin;
            _stringLocalizer = stringLocalizer;
        }

        protected override async Task OnExecuteAsync()
        {
            var radius = await Context.Parameters.GetAsync<float>(0);

            var user = (UnturnedUser)Context.Actor;

            await _plugin.SaveHub(new Hub(user, radius));

            await PrintAsync(_stringLocalizer["commands:hub_set:success", new { Radius = radius }]);
        }
    }
}
