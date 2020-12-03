using Cysharp.Threading.Tasks;
using Deathmatch.API.Matches;
using Deathmatch.API.Players;
using Deathmatch.API.Preservation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OpenMod.API.Users;
using OpenMod.Unturned.Players;
using OpenMod.Unturned.Users;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Deathmatch.Core.Matches
{
    public abstract class MatchBase : IMatch
    {
        protected static Random Rng = new Random();

        protected readonly IConfiguration Configuration;
        protected readonly IStringLocalizer StringLocalizer;
        protected readonly IPreservationManager PreservationManager;
        protected readonly IMatchExecutor MatchExecutor;
        protected readonly IUserManager UserManager;
        protected readonly List<IGamePlayer> Players;

        protected MatchBase(IServiceProvider serviceProvider)
        {
            Configuration = serviceProvider.GetRequiredService<IConfiguration>();
            StringLocalizer = serviceProvider.GetRequiredService<IStringLocalizer>();
            PreservationManager = serviceProvider.GetRequiredService<IPreservationManager>();
            MatchExecutor = serviceProvider.GetRequiredService<IMatchExecutor>();
            UserManager = serviceProvider.GetRequiredService<IUserManager>();

            Players = new List<IGamePlayer>();
        }

        public IMatchRegistration Registration { get; set; }

        public bool IsRunning { get; protected set; }
        public bool HasRun { get; protected set; }

        public IReadOnlyCollection<IGamePlayer> GetPlayers() => Players.AsReadOnly();

        public IGamePlayer GetPlayer(CSteamID steamId) =>
            Players.FirstOrDefault(x => x.SteamId == steamId);

        public IGamePlayer GetPlayer(Player player) => GetPlayer(player.channel.owner.playerID.steamID);

        public IGamePlayer GetPlayer(UnturnedPlayer player) => GetPlayer(player.SteamId);

        public IGamePlayer GetPlayer(UnturnedUser user) => GetPlayer(user.SteamId);

        public IGamePlayer GetPlayer(IGamePlayer player) => GetPlayer(player.SteamId);

        public abstract UniTask AddPlayer(IGamePlayer player);
        public abstract UniTask AddPlayers(IEnumerable<IGamePlayer> player);
        public abstract UniTask RemovePlayer(IGamePlayer user);


        public abstract UniTask<bool> StartMatch();
        public abstract UniTask<bool> EndMatch();
    }
}
