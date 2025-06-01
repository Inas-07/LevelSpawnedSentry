using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EOSExt.LevelSpawnedSentry.Impl
{
    public struct LSSTargeted
    {
        public int LSSInstanceIndex { get; set; } = -1;

        public int PlayerSlotIndex { get; set; } = -1;

        public LSSTargeted() { }

        public LSSTargeted(LSSTargeted o)
        {
            LSSInstanceIndex = o.LSSInstanceIndex;
            PlayerSlotIndex = o.PlayerSlotIndex;
        }
    }
}
