using Deathmatch.API.Loadouts;
using Microsoft.Extensions.DependencyInjection;
using OpenMod.API.Ioc;
using OpenMod.API.Prioritization;
using System;
using System.Collections.Generic;

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

        public void AddCategory(ILoadoutCategory category)
        {
            if (category == null)
                throw new ArgumentNullException(nameof(category));

            if (this.GetCategory(category.Title) != null)
                throw new ArgumentException("A category with the given title already exists", nameof(category));

            _categories.Add(category);
        }

        public void RemoveCategory(ILoadoutCategory category)
        {
            _categories.Remove(category);
        }
    }
}
