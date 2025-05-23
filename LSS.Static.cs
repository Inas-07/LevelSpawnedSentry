using BepInEx.Unity.IL2CPP.Utils.Collections;
using EOSExt.LevelSpawnedSentry.Definition;
using ExtraObjectiveSetup.Utils;
using GTFO.API;
using SNetwork;
using System;
using UnityEngine;

namespace EOSExt.LevelSpawnedSentry
{
    public partial class LSS
    {
        internal static LSS Instantiate(LevelSpawnedSentryDefinition def, int instanceIndex)
        {
            var lss = new LSS(def, instanceIndex);
            LevelAPI.OnBuildDone += lss.SetupReplicator;
            LevelAPI.OnEnterLevel += lss.SpawnLSS_OnEnterLevel;
            return lss;
        }

        private const float SYNC_ALL_IN = 15f; // seconds

        private static Coroutine s_syncLSSAmmoCoroutine = null;

        // Sync ammo of all sentries periodically.
        // Note that WEEvent (ChangeLSSState) will sync sentry ammo as well,
        private static System.Collections.IEnumerator s_sync()
        {
            int i = 0;
            WaitForSeconds waitOutOfLevel = new(SYNC_ALL_IN);
            WaitForSeconds waitInLevel = null;

            while (true)
            {
                if (GameStateManager.CurrentStateName == eGameStateName.InLevel)
                {
                    if(SNet.IsMaster && LevelSpawnedSentryManager.Current.LSSInstances.Count > 0)
                    {
                        if (0 <= i && i < LevelSpawnedSentryManager.Current.LSSInstances.Count)
                        {
                            var instance = LevelSpawnedSentryManager.Current.LSSInstances[i];
                            var sentry = instance.LSSComp.Sentry;
                            if (sentry != null)
                            {
                                var state = instance.StateReplicator.State;
                                if (Math.Abs(sentry.Ammo - state.Ammo) > 1e-4)
                                {
                                    var syncState = new LSSState(state) with
                                    {
                                        Ammo = sentry.Ammo
                                    };
                                    instance.StateReplicator.SetState(syncState);
                                }
                            }
                        }

                        if (i == LevelSpawnedSentryManager.Current.LSSInstances.Count - 1)
                        {
                            EOSLogger.Debug($"LSS: master synced {LevelSpawnedSentryManager.Current.LSSInstances.Count} in {SYNC_ALL_IN} seconds");
                        }
                        
                        i = (i + 1) % LevelSpawnedSentryManager.Current.LSSInstances.Count;
                    }

                    if (waitInLevel == null)
                    {
                        waitInLevel = new(SYNC_ALL_IN / Math.Max(1, LevelSpawnedSentryManager.Current.LSSInstances.Count));
                    }

                    yield return waitInLevel;
                }

                else
                {
                    waitInLevel = null;
                    yield return waitOutOfLevel;
                }
            }
        }

        static LSS()
        {
            s_syncLSSAmmoCoroutine = CoroutineManager.StartPersistantCoroutine(s_sync().WrapToIl2Cpp());
            EOSLogger.Debug("===========================");
            EOSLogger.Debug("LSS Sync coroutine started");
            EOSLogger.Debug("===========================");
        }
    }
}
