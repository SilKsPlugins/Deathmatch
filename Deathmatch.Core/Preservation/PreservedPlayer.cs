using Deathmatch.API.Players;
using Deathmatch.Core.Preservation.Clothing;
using Deathmatch.Core.Preservation.Groups;
using Deathmatch.Core.Preservation.Inventory;
using Deathmatch.Core.Preservation.Life;
using Deathmatch.Core.Preservation.Skills;
using SDG.Unturned;
using Steamworks;
using UnityEngine;

namespace Deathmatch.Core.Preservation
{
    public class PreservedPlayer
    {
        public IGamePlayer Player { get; }
        public CSteamID SteamId => Player.SteamId;

        private readonly Vector3 _position;
        private readonly float _yaw;

        private readonly EPlayerStance _stance;

        private readonly PreservedClothing _clothing;
        private readonly PreservedInventory _inventory;

        private readonly PreservedSkills _skills;

        private readonly PreservedLife _life;

        private readonly PreservedGroup _group;

        public PreservedPlayer(IGamePlayer player)
        {
            Player = player;

            if (player.IsDead)
            {
                player.Player.life.askRespawn(player.SteamId, false);
            }

            _position = player.Player.transform.position;
            _yaw = player.Transform.eulerAngles.y;

            _stance = player.Stance.stance;

            _clothing = new PreservedClothing(player.Clothing);
            _inventory = new PreservedInventory(player.Inventory);

            _skills = new PreservedSkills(player.Skills);

            _life = new PreservedLife(player.Life);

            _group = new PreservedGroup(player.Quests);

            player.Player.save();
        }

        public void Restore()
        {
            // Preemptive clearing
            Player.Inventory.closeStorageAndNotifyClient();
            Player.ClearInventory();
            Player.ClearClothing();

            // Position - this is first as we'll revive the player if they're dead
            if (Player.Life.isDead)
            {
                byte b = MeasurementTool.angleToByte(_yaw);

                Player.Life.sendRevive();
                Player.Life.channel.send("tellRevive", ESteamCall.ALL, ESteamPacket.UPDATE_RELIABLE_BUFFER, new object[]
                {
                    _position,
                    b
                });
            }
            else
            {
                Player.Player.teleportToLocationUnsafe(_position, _yaw);
            }

            // Stance
            Player.Stance.stance = _stance;

            // Clothing/inventory
            _clothing.Restore(Player.Clothing);
            _inventory.Restore(Player.Inventory);

            // Skills/experience/reputation
            _skills.Restore(Player.Skills);

            // Life
            _life.Restore(Player.Life);

            // Group
            _group.Restore(Player.Quests);
        }
    }
}
