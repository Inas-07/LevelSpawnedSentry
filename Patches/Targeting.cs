using Enemies;
using EOSExt.LevelSpawnedSentry.PlayerGUIMessage;
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
        [HarmonyPrefix]
        [HarmonyWrapSafe]
        [HarmonyPatch(typeof(SentryGunInstance), nameof(SentryGunInstance.WantToScan))]
        private static bool Pre_WantToScan(SentryGunInstance __instance, ref bool __result)
        {
            var lssComp = __instance.gameObject.GetComponent<LSSComp>();

            if (lssComp == null) return true;

            if(!lssComp.LSS.State.Enabled)
            {
                __result = false;
                return false;
            }

            return true;
        }


        [HarmonyPrefix]
        [HarmonyWrapSafe]
        [HarmonyPatch(typeof(SentryGunInstance_Detection), nameof(SentryGunInstance_Detection.UpdateDetection))]
        private static bool Pre_UpdateDetection(SentryGunInstance_Detection __instance) 
        {
            var lssComp = __instance.m_core.TryCast<SentryGunInstance>()?.gameObject.GetComponent<LSSComp>();

            if (lssComp == null) return true;

            if (!lssComp.LSS.State.Enabled) return false;

            if (lssComp.LSS.State.TargetEnemy) return true;

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

                            if(playerAgent.IsLocallyOwned)
                            {
                                PlayerGUIMessageManager.Current.OnPlayerTargeted();
                            }
                        }
                        else
                        {
                            PlayerGUIMessageManager.Current.Clear();
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
