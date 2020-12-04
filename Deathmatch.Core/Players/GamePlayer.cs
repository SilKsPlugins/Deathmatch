using Deathmatch.API.Matches;
using Deathmatch.API.Players;
using OpenMod.Unturned.Users;
using SDG.Unturned;
using Steamworks;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Deathmatch.Core.Players
{
    public sealed class GamePlayer : IGamePlayer
    {
        public UnturnedUser User { get; }

        public IMatch CurrentMatch { get; set; }

        private readonly Dictionary<string, object> _matchData;

        internal GamePlayer(UnturnedUser user)
        {
            User = user;

            _matchData = new Dictionary<string, object>();
        }

        public T GetMatchData<T>(string key)
        {
            if (_matchData.TryGetValue(key, out var value) && value is T data)
            {
                return data;
            }

            return default;
        }

        public void SetMatchData<T>(string key, T value)
        {
            if (_matchData.ContainsKey(key))
            {
                _matchData[key] = value;
            }
            else
            {
                _matchData.Add(key, value);
            }
        }

        public void ClearMatchData() => _matchData.Clear();

        public Task PrintMessageAsync(string message) => User.PrintMessageAsync(message);

        public CSteamID SteamId => User.SteamId;

        public string DisplayName => User.DisplayName;

        public Player Player => User.Player.Player;

        public Transform Transform => Player.transform;

        public PlayerClothing Clothing => Player.clothing;

        private static readonly byte[] EmptyArray = new byte[0];

        public void ClearClothing() => Clothing.updateClothes(
            0, 0, EmptyArray,
            0, 0, EmptyArray,
            0, 0, EmptyArray,
            0, 0, EmptyArray,
            0, 0, EmptyArray,
            0, 0, EmptyArray,
            0, 0, EmptyArray);

        public PlayerInventory Inventory => Player.inventory;

        public void ClearInventory()
        {
            for (byte page = 0; page < PlayerInventory.PAGES - 2; page++)
            {
                if (Inventory.items[page] == null) continue;

                int count = Inventory.getItemCount(page);

                for (int i = 0; i < count; i++)
                {
                    Inventory.removeItem(page, 0);
                }
            }
        }

        public PlayerLife Life => Player.life;
        public bool IsDead => Life.isDead;

        public void Heal()
        {
            Life.askHeal(100, true, true);
            Life.serverModifyFood(100);
            Life.serverModifyWater(100);
            Life.serverModifyStamina(100);
            Life.serverModifyVirus(100);
        }

        public PlayerMovement Movement => Player.movement;

        public PlayerQuests Quests => Player.quests;

        public PlayerSkills Skills => Player.skills;

        public PlayerStance Stance => Player.stance;
    }
}
