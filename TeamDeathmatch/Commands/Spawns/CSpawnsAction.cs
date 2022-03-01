using Cysharp.Threading.Tasks;
using Deathmatch.Core.Spawns;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OpenMod.API.Commands;
using OpenMod.Unturned.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using TeamDeathmatch.Spawns;
using TeamDeathmatch.Teams;

namespace TeamDeathmatch.Commands.Spawns
{
    public abstract class CSpawnsAction : UnturnedCommand
    {
        protected readonly BlueSpawnDirectory BlueSpawnDirectory;
        protected readonly RedSpawnDirectory RedSpawnDirectory;
        protected readonly IStringLocalizer StringLocalizer;

        protected CSpawnsAction(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            BlueSpawnDirectory = serviceProvider.GetRequiredService<BlueSpawnDirectory>();
            RedSpawnDirectory = serviceProvider.GetRequiredService<RedSpawnDirectory>();
            StringLocalizer = serviceProvider.GetRequiredService<IStringLocalizer>();
        }

        private SpawnDirectory GetSpawnDirectory(Team team)
        {
            return team switch
            {
                Team.Red => BlueSpawnDirectory,
                Team.Blue => RedSpawnDirectory,
                _ => throw new ArgumentException("Bad team (not red or blue)", nameof(team))
            };
        }

        protected List<PlayerSpawn> GetSpawns(Team team)
        {
            return GetSpawnDirectory(team).Spawns.ToList();
        }

        protected async UniTask SaveSpawns(Team team, IEnumerable<PlayerSpawn> spawns)
        {
            var spawnDirectory = GetSpawnDirectory(team);

            await spawnDirectory.SaveSpawns(spawns);
        }

        protected override async UniTask OnExecuteAsync()
        {
            var strTeam = await Context.Parameters.GetAsync<string>(0);

            var team = Team.None;

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
