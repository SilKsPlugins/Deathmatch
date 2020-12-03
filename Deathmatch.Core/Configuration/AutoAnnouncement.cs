using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deathmatch.Core.Configuration
{
    [Serializable]
    public class AutoAnnouncement
    {
        public int SecondsBefore { get; set; }

        public string MessageTime { get; set; }
    }
}
