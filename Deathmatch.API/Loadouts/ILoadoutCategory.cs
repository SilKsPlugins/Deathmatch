using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deathmatch.API.Loadouts
{
    public interface ILoadoutCategory
    {
        string Title { get; }

        List<ILoadout> GetLoadouts();
    }
}
