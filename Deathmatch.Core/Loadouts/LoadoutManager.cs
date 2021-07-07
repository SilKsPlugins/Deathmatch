using Deathmatch.API.Loadouts;
using Microsoft.Extensions.DependencyInjection;
using OpenMod.API.Ioc;
using OpenMod.API.Prioritization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Deathmatch.Core.Loadouts
{
    [ServiceImplementation(Lifetime = ServiceLifetime.Singleton, Priority = Priority.Lowest)]
    public class LoadoutManager : ILoadoutManager
    {
        private readonly List<ILoadoutCategory> _categories;

        public LoadoutManager()
        {
            _categories = new List<ILoadoutCategory>();
        }

        public IReadOnlyCollection<ILoadoutCategory> GetCategories() => _categories.AsReadOnly();

        public ILoadoutCategory? GetCategory(string title) =>
            _categories.FirstOrDefault(x => x.Title.Equals(title, StringComparison.OrdinalIgnoreCase)) ??
            _categories.FirstOrDefault(x => x.Aliases.Any(y => y.Equals(title, StringComparison.OrdinalIgnoreCase)));

        public void AddCategory(ILoadoutCategory category)
        {
            if (category == null)
                throw new ArgumentNullException(nameof(category));

            if (GetCategory(category.Title) != null)
                throw new ArgumentException("A category with the given title already exists", nameof(category));

            _categories.Add(category);
        }

        public void RemoveCategory(ILoadoutCategory category)
        {
            _categories.Remove(category);
        }
    }
}
