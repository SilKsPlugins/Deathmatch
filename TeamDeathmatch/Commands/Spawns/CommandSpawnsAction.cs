using Cysharp.Threading.Tasks;
using Deathmatch.Core.Spawns;
using Microsoft.Extensions.Localization;
using OpenMod.API.Commands;
using OpenMod.Unturned.Commands;
using System;
using System.Collections.Generic;
using TeamDeathmatch.Teams;

namespace TeamDeathmatch.Commands.Spawns
{
    public abstract class CommandSpawnsAction : UnturnedCommand
    {
        protected readonly TeamDeathmatchPlugin Plugin;
        protected readonly IStringLocalizer StringLocalizer;

        protected CommandSpawnsAction(TeamDeathmatchPlugin plugin,
            IStringLocalizer stringLocalizer,
            IServiceProvider serviceProvider) : base(serviceProvider)
        {
            Plugin = plugin;
            StringLocalizer = stringLocalizer;
        }

        private string GetKey(Team team)
        {
            switch (team)
            {
                case Team.Red:
                    return TeamDeathmatchPlugin.RedSpawnsKey;
                case Team.Blue:
                    return TeamDeathmatchPlugin.BlueSpawnsKey;
                default:
                    throw new ArgumentException("Bad team (not red or blue)", nameof(team));
            }
        }

        protected async UniTask<List<PlayerSpawn>> LoadSpawnsAsync(Team team)
        {
            var list = await Plugin.DataStore.ExistsAsync(GetKey(team))
                ? await Plugin.DataStore.LoadAsync<List<PlayerSpawn>>(GetKey(team))
                : null;

            return list ?? new List<PlayerSpawn>();
        }

        protected async UniTask SaveSpawnsAsync(Team team, List<PlayerSpawn> spawns)
        {
            await Plugin.DataStore.SaveAsync(GetKey(team), spawns);

            await Plugin.ReloadSpawns();
        }

        protected override async UniTask OnExecuteAsync()
        {
            string strTeam = await Context.Parameters.GetAsync<string>(0);

            Team team = Team.None;

            switch (strTeam.Trim().ToLower())
            {
                case "r":
                case "red":
                    team = Team.Red;
                    break;
                case "b":
                case "blue":
                    team = Team.Blue;
                    break;
            }

            if (team == Team.None)
            {
                throw new UserFriendlyException(StringLocalizer["commands:spawns:common:unknown_team", new { Team = strTeam }]);
            }

            await OnExecuteAsync(team);
        }

        protected abstract UniTask OnExecuteAsync(Team team);
    }
}
