using Deathmatch.API.Loadouts;
using Deathmatch.Core.Loadouts;
using OpenMod.API;
using OpenMod.API.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TeamDeathmatch.Items;
using TeamDeathmatch.Teams;

namespace TeamDeathmatch.Loadouts
{
    public class TeamLoadoutCategory : LoadoutCategory
    {
        public TeamLoadoutCategory(string title, IReadOnlyCollection<string>? aliases,
            IOpenModComponent component, IDataStore dataStore, List<ILoadout>? loadouts = null)
            : base(title, aliases, component, dataStore, loadouts)
        {
        }

        public override async Task LoadLoadouts()
        {
            var loadouts = new List<ILoadout>();

            if (await DataStore.ExistsAsync(DataStoreKey))
            {
                loadouts.AddRange(await DataStore.LoadAsync<List<TeamLoadout>>(DataStoreKey) ??
                                  new List<TeamLoadout>());
            }

            Loadouts = loadouts;
        }

        public override async Task SaveLoadouts()
        {
            await DataStore.SaveAsync(DataStoreKey, Loadouts.OfType<Loadout>().ToList());
        }

        public override void AddLoadout(ILoadout loadout)
        {
            if (this.GetLoadout(loadout.Title) != null)
            {
                throw new ArgumentException("Loadout with given title already exists", nameof(loadout));
            }

            if (loadout is not LoadoutBase knownLoadout)
            {
                throw new NotSupportedException($"Argument {nameof(loadout)} must implement {typeof(Loadout).FullName}.");
            }

            var teamLoadout = new TeamLoadout(loadout.Title, loadout.Permission);

            foreach (var item in knownLoadout.GetItems())
            {
                teamLoadout.Items.Add(new TeamItem()
                {
                    Id = item.Id,
                    Amount = item.Amount,
                    Quality = item.Quality,
                    State = item.State,
                    Team = Team.None
                });
            }
        }

        public override bool RemoveLoadout(ILoadout loadout)
        {
            return Loadouts.RemoveAll(x => loadout.Title.Equals(x.Title, StringComparison.OrdinalIgnoreCase)) > 0;
        }
    }
}
