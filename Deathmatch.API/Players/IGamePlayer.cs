using Deathmatch.API.Matches;
using OpenMod.Unturned.Users;
using SDG.Unturned;
using Steamworks;
using System.Threading.Tasks;
using UnityEngine;

namespace Deathmatch.API.Players
{
    public interface IGamePlayer
    {
        UnturnedUser User { get; }

        IMatch CurrentMatch { get; set; }

        T GetMatchData<T>(string key);
        void SetMatchData<T>(string key, T value);
        void ClearMatchData();

        Task PrintMessageAsync(string message);

        CSteamID SteamId { get; }
        string DisplayName { get; }

        Player Player { get; }

        Transform Transform { get; }

        PlayerClothing Clothing { get; }
        void ClearClothing();

        PlayerInventory Inventory { get; }
        void ClearInventory();

        PlayerLife Life { get; }
        bool IsDead { get; }

        PlayerMovement Movement { get; }

        PlayerQuests Quests { get; }

        PlayerSkills Skills { get; }

        void MaxSkills(bool overpower = false);

        PlayerStance Stance { get; }
    }
}
