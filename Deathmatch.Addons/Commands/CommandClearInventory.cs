using Cysharp.Threading.Tasks;
using Deathmatch.API.Players;
using OpenMod.Core.Commands;
using OpenMod.Unturned.Commands;
using OpenMod.Unturned.Users;
using System;

namespace Deathmatch.Addons.Commands
{
    [Command("clearinventory")]
    [CommandDescription("Clears your inventory")]
    [CommandAlias("clearinv")]
    [CommandAlias("ci")]
    [CommandActor(typeof(UnturnedUser))]
    public class CommandClearInventory : UnturnedCommand
    {
        private readonly IGamePlayerManager _playerManager;

        public CommandClearInventory(IGamePlayerManager playerManager,
            IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _playerManager = playerManager;
        }

        protected override async UniTask OnExecuteAsync()
        {
            var player = _playerManager.GetPlayer((UnturnedUser)Context.Actor);

            await UniTask.SwitchToMainThread();

            player.ClearInventory();
            player.ClearClothing();
        }
    }
}
