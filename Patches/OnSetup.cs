using ExtraObjectiveSetup.Utils;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EOSExt.LevelSpawnedSentry.Patches
{
    [HarmonyPatch]
    internal static class OnSetup
    {
        // Called earlier than SentryGunInstance.OnSpawn
        [HarmonyPostfix]
        [HarmonyPatch(typeof(SentryGunInstance), nameof(SentryGunInstance.Setup))]
        private static void Post_D_Setup(SentryGunInstance __instance)
        {
            var lssComp = __instance.gameObject.GetComponent<LSSComp>();
            //EOSLogger.Error($"SentryGunInstance Setup. Is LSS? : {lssComp != null}");

            if(lssComp != null)
            {
                EOSLogger.Error($"LSS: {lssComp.Def.WorldEventObjectFilter}");
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(SentryGunInstance_Detection), nameof(SentryGunInstance_Detection.Setup))]
        private static void Post_D_Setup(SentryGunInstance_Detection __instance)
        {
            var lssComp = __instance.m_core.TryCast<SentryGunInstance>()?.gameObject.GetComponent<LSSComp>();
            if (lssComp == null) return;

            __instance.m_targetPlayers = lssComp.LSS.StateReplicator.State.TargetPlayer;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(SentryGunInstance_ScannerVisuals_Plane), nameof(SentryGunInstance_ScannerVisuals_Plane.Setup))]
        private static void Post_V_Setup(SentryGunInstance_ScannerVisuals_Plane __instance)
        {
            if(__instance.m_scannerPlane == null || __instance.m_targetingAlign == null)
            {
                EOSLogger.Error("SentryGunInstance_ScannerVisuals_Plane.Setup: got a null scannerPlane / targetingAlign?");
            }

            var lssComp = __instance.m_core.TryCast<SentryGunInstance>()?.gameObject.GetComponent<LSSComp>();
            if (lssComp == null) return;

            var state = lssComp.LSS.StateReplicator.State;
            var c = LSS.GetScanningColor(state.TargetEnemy, state.TargetPlayer);
            __instance.m_scanningColor = c;

            //__instance.m_spotLight.color = c;
            //__instance.m_scannerPlane?.SetColor(c);
            //__instance.m_targetingAlign?.Visor?.material.SetColor("_Color", c);
            __instance.SetVisualStatus(eSentryGunStatus.Scanning, true);
        }
    }
}
