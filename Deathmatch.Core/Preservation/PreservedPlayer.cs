using Deathmatch.API.Players;
using Deathmatch.Core.Players.Extensions;
using Deathmatch.Core.Preservation.Clothing;
using Deathmatch.Core.Preservation.Groups;
using Deathmatch.Core.Preservation.Inventory;
using Deathmatch.Core.Preservation.Life;
using Deathmatch.Core.Preservation.Skills;
using HarmonyLib;
using SDG.Unturned;
using Steamworks;
using System.Reflection;
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
                Player.ForceRespawn();
            }

            _position = Player.Player.transform.position;
            _yaw = Player.Transform.eulerAngles.y;

            _stance = Player.Stance.stance;

            _clothing = new PreservedClothing(Player.Clothing);
            _inventory = new PreservedInventory(Player.Inventory);

            _skills = new PreservedSkills(Player.Skills);

            _life = new PreservedLife(Player.Life);

            _group = new PreservedGroup(Player.Quests);

            Player.Player.save();
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
                Player.ForceRespawn(_position, _yaw);
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
