﻿using EOSExt.LevelSpawnedSentry.Definition;
using ExtraObjectiveSetup.Utils;
using GameData;
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

            bool targetEnemy = e.EnemyID == 0;
            bool targetPlayer = e.SustainedEventSlotIndex > 0;
            
            var newState = new LSSState(lss.StateReplicator.State) with
            {
                Enabled = e.Enabled,
                TargetEnemy = targetEnemy,
                TargetPlayer = targetPlayer,
                Ammo = lss.LSSComp.Sentry.Ammo,
                AmmoMaxCap = lss.LSSComp.Sentry.AmmoMaxCap,
            };

            lss.StateReplicator.SetState(newState);
        }
    }
}
