using ExtraObjectiveSetup.Utils;
using GameData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EOSExt.LevelSpawnedSentry.Definition
{
    public class LevelSpawnedSentryDefinition
    {
        public string WorldEventObjectFilter { get; set; } = string.Empty;

        public uint PlayerOfflineGearDBId { get; set; } 

        public eDimensionIndex DimensionIndex { get; set; }

        public Vec3 Position { get; set; } = new();

        public Vec3 Rotation { get; set; } = new();

        public LSSState InitialState { get; set; } = new();

        public float InitialAmmo { get; set; } = 0.5f;

        public float AmmoCap { get; set; } = 1.0f;
    }
}
