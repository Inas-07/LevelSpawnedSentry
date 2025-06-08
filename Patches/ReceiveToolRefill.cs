using EOSExt.LevelSpawnedSentry.Impl;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EOSExt.LevelSpawnedSentry.Patches
{
    [HarmonyPatch]
    internal static class ReceiveToolRefill
    {
        // NOTE: unpatchable, prolly inline
        //[HarmonyPrefix]
        //[HarmonyPatch(typeof(SentryGunInstance), nameof(SentryGunInstance.NeedToolAmmo))]
        //private static bool Pre_(SentryGunInstance __instance, ref bool __result)
        //{
        //    var lssComp = __instance.gameObject.GetComponent<LSSComp>();
        //    if (lssComp == null || lssComp.LSS.State.CanRefill) return true;
        //    __result = false;
        //    return false;
        //}
    }
}
