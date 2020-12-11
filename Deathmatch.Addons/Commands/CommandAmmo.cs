using Cysharp.Threading.Tasks;
using OpenMod.API.Commands;
using OpenMod.Core.Commands;
using OpenMod.Unturned.Commands;
using OpenMod.Unturned.Users;
using SDG.Unturned;
using System;

namespace Deathmatch.Addons.Commands
{
    [Command("ammo")]
    [CommandDescription("Gives ammo for the currently selected weapon")]
    [CommandSyntax("[amount]")]
    [CommandActor(typeof(UnturnedUser))]
    public class CommandAmmo : UnturnedCommand
    {
        public CommandAmmo(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        protected override async UniTask OnExecuteAsync()
        {
            byte amount = 1;

            var user = (UnturnedUser) Context.Actor;

            if (Context.Parameters.Count > 0)
            {
                amount = await Context.Parameters.GetAsync<byte>(0);
            }

            var itemId = user.Player.Player.equipment.itemID;

            if (itemId == 0)
                throw new UserFriendlyException("You currently have no gun equipped");

            var gunAsset = Assets.find(EAssetType.ITEM, itemId) as ItemGunAsset;

            if (gunAsset == null)
                throw new UserFriendlyException("You currently have no gun equipped");

            var magId = gunAsset.getMagazineID();

            if (magId == 0)
                throw new UserFriendlyException("This gun has no pre-defined magazine");

            ItemTool.tryForceGiveItem(user.Player.Player, gunAsset.getMagazineID(), amount);

            await PrintAsync("You've been given ammo for this weapon.");
        }
    }
}
