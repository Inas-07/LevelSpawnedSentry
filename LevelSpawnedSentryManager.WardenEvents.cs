using EOSExt.LevelSpawnedSentry.Definition;
using ExtraObjectiveSetup.Utils;
using GameData;
using SNetwork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EOSExt.LevelSpawnedSentry
{
    public partial class LevelSpawnedSentryManager
    {
        public enum LSS_WardenEvents
        {
            ChangeLSSState = 910,
        }

        private static void ChangeLSSState(WardenObjectiveEventData e)
        {
            if(!Current.LSSDict.TryGetValue(e.WorldEventObjectFilter, out var lss))
            {
                EOSLogger.Error($"ChangeLSSState: Cannot find LSS with name '{e.WorldEventObjectFilter}'");
                return;
            }

            if(SNet.IsMaster)
            {
                bool targetEnemy = e.EnemyID == 0;
                bool targetPlayer = e.SustainedEventSlotIndex > 0;
                bool markerVisible = e.Count == 0;
                bool canRefill = e.FogSetting == 0;

                var newState = new LSSSyncState(lss.State) with
                {
                    Enabled = e.Enabled,
                    TargetEnemy = targetEnemy,
                    TargetPlayer = targetPlayer,
                    MarkerVisible = markerVisible,
                    CanRefill = canRefill,
                    Ammo = lss.LSSComp.Sentry.Ammo,
                };

                lss.StateReplicator.SetState(newState);
            }
        }
    }
}
