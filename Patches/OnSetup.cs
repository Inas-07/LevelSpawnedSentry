using EOSExt.LevelSpawnedSentry.Impl;
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
        [HarmonyPostfix]
        [HarmonyPatch(typeof(SentryGunInstance), nameof(SentryGunInstance.CheckIsSetup))]
        private static void Post_CheckSetup(SentryGunInstance __instance)
        {
            var lssComp = __instance.gameObject.GetComponent<LSSComp>();

            if (lssComp != null)
            {
                EOSLogger.Warning($"LSS CheckSetup: {lssComp.Def.WorldEventObjectFilter}");
                lssComp.UpdateVisuals();
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(SentryGunInstance_Detection), nameof(SentryGunInstance_Detection.Setup))]
        private static void Post_D_Setup(SentryGunInstance_Detection __instance)
        {
            var lssComp = __instance.m_core.TryCast<SentryGunInstance>()?.gameObject.GetComponent<LSSComp>();
            if (lssComp == null) return;

            __instance.m_targetPlayers = lssComp.LSS.State.TargetPlayer;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(SentryGunInstance_ScannerVisuals_Plane), nameof(SentryGunInstance_ScannerVisuals_Plane.Setup))]
        private static void Post_V_Setup(SentryGunInstance_ScannerVisuals_Plane __instance)
        {
            if (__instance.m_scannerPlane == null || __instance.m_targetingAlign == null)
            {
                EOSLogger.Error("SentryGunInstance_ScannerVisuals_Plane.Setup: got a null scannerPlane / targetingAlign?");
            }

            var lssComp = __instance.m_core.TryCast<SentryGunInstance>()?.gameObject.GetComponent<LSSComp>();
            if (lssComp == null) return;

            lssComp.UpdateVisuals();
        }
    }
}
