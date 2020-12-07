using System.Collections.Generic;

namespace Deathmatch.Core.Loadouts
{
    public class LoadoutSelections
    {
        public List<LoadoutSelection> Selections { get; set; }

        public LoadoutSelections()
        {
            Selections = new List<LoadoutSelection>();
        }
    }
}
