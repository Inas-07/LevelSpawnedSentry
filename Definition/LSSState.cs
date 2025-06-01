using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EOSExt.LevelSpawnedSentry.Definition
{
    public struct LSSState
    {
        public bool Enabled { get; set; } = true;

        public bool TargetEnemy { get; set; } = true;

        public bool TargetPlayer { get; set; } = false;

        public bool MarkerVisible { get; set; } = true;

        [JsonIgnore]
        public float Ammo { get; set; } = 0.5f;

        [JsonIgnore]
        public float AmmoMaxCap { get; set; } = 1.0f;

        public LSSState() { }

        public LSSState(LSSState o)
        {
            Enabled = o.Enabled;
            TargetEnemy = o.TargetEnemy;
            TargetPlayer = o.TargetPlayer;
            MarkerVisible = o.MarkerVisible;
            Ammo = o.Ammo;
            AmmoMaxCap = o.AmmoMaxCap;
        }

        public override string ToString()
        {
            return $"Enabled: {Enabled}, TargetEnemy/Player: {TargetEnemy}/{TargetPlayer}";
        }
    }
}
