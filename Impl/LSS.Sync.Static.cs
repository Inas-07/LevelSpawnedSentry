using EOSExt.LevelSpawnedSentry.Impl;
using ExtraObjectiveSetup.Utils;
using GTFO.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EOSExt.LevelSpawnedSentry
{
    public partial class LSS
    {
        public const string LSS_SYNC_EVT = "LSS_SYNC_EVT";

        private static void OnReceive(ulong sender, LSSTargeted data)
        {
            int instanceIndex = data.LSSInstanceIndex;
            if (instanceIndex < 0 || instanceIndex >= LevelSpawnedSentryManager.Current.LSSInstances.Count)
            {
                EOSLogger.Error($"LSS_OnReceive: got packet with invalid instance index {instanceIndex}, LSSCount: {LevelSpawnedSentryManager.Current.LSSInstances.Count}, CurrentGameState: {GameStateManager.CurrentStateName}");
                return;
            }

            var lss = LevelSpawnedSentryManager.Current.LSSInstances[instanceIndex];
            lss.m_LSSTargeted.PlayerSlotIndex = data.PlayerSlotIndex;
        }

        private static void SetupSync()
        {
            NetworkAPI.RegisterEvent<LSSTargeted>(LSS_SYNC_EVT, OnReceive);
            EOSLogger.Debug("===========================");
            EOSLogger.Debug("LSSTargeted Sync EVT Registered");
            EOSLogger.Debug("===========================");
        }
    }
}
