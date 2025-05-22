using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EOSExt.LevelSpawnedSentry.Definition
{
    public struct LSSState
    {
        public bool Enabled { get; set; } = true;

        public bool TargetEnemy { get; set; } = true;

        public bool TargetPlayer { get; set; } = false;

        public LSSState() { }
    }
}
