using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EOSExt.LevelSpawnedSentry.Definition
{
    public struct LSSSyncState
    {
        public bool Enabled { get; set; } = true;

        public bool TargetEnemy { get; set; } = true;

        public bool TargetPlayer { get; set; } = false;

        public bool MarkerVisible { get; set; } = true;

        public bool CanRefill { get; set; } = true;

        public float Ammo { get; set; } = 0.5f;

        public LSSSyncState() { }

        public LSSSyncState(LSSSyncState o)
        {
            Enabled = o.Enabled;
            TargetEnemy = o.TargetEnemy;
            TargetPlayer = o.TargetPlayer;
            MarkerVisible = o.MarkerVisible;
            CanRefill = o.CanRefill;
            Ammo = o.Ammo;
        }

        public LSSSyncState(LSSState o)
        {
            Enabled = o.Enabled;
            TargetEnemy = o.TargetEnemy;
            TargetPlayer = o.TargetPlayer;
            MarkerVisible = o.MarkerVisible;
            CanRefill = o.CanRefill;
            //Ammo = o.Ammo;
        }

        public override string ToString()
        {
            return $"Enabled: {Enabled}, TargetEnemy/Player: {TargetEnemy}/{TargetPlayer}, CanRefill: {CanRefill}";
        }
    }
}
