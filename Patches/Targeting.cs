using Enemies;
using ExtraObjectiveSetup.Utils;
using HarmonyLib;
using Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace EOSExt.LevelSpawnedSentry.Patches
{
    [HarmonyPatch]
    internal static class Targeting
    {

        // TODO: Test
        [HarmonyPrefix]
        [HarmonyWrapSafe]
        [HarmonyPatch(typeof(SentryGunInstance_Detection), nameof(SentryGunInstance_Detection.UpdateDetection))]
        private static bool Pre_UpdateDetection(SentryGunInstance_Detection __instance) 
        {
            var lssComp = __instance.m_core.TryCast<SentryGunInstance>()?.gameObject.GetComponent<LSSComp>();

            if (lssComp == null) return true;
            //EOSLogger.Warning($"Overriding: enemy: {lss.StateReplicator.State.TargetEnemy}, player {lss.StateReplicator.State.TargetPlayer} + {__instance.m_targetPlayers}");

            if (lssComp.LSS.StateReplicator.State.TargetEnemy) return true;

            if (Clock.Time > __instance.m_scanningTimer)
            {
                Vector3 forward = __instance.DetectionSource.transform.forward;
                Vector3 position = __instance.DetectionSource.transform.position;
                if (__instance.m_core.Ammo >= __instance.m_core.CostOfBullet)
                {
                    __instance.Target = null;
                    __instance.TargetAimTrans = null;
                    __instance.TargetIsTagged = false;

                    if (__instance.m_targetPlayers)
                    {
                        PlayerAgent playerAgent = SentryGunInstance_Detection.CheckForPlayerTarget(__instance.m_archetypeData, __instance.DetectionSource, __instance.Target);
                        if (playerAgent != null)
                        {
                            __instance.Target = playerAgent.gameObject;
                            __instance.TargetAimTrans = playerAgent.TentacleTarget;
                        }
                    }
                }
                if (__instance.Target != null)
                {
                    Vector3 position2 = __instance.Target.transform.position;
                    if (!__instance.HasTarget)
                    {
                        __instance.HasTarget = true;
                        __instance.OnFoundTarget?.Invoke();
                    }
                    Debug.DrawLine(position, position2);
                }
                else
                {
                    __instance.HasTarget = false;
                    __instance.TargetIsTagged = false;
                }
            }

            return false;
        }
    }
}
