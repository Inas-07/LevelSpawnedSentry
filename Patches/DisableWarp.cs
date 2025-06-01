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
    internal static class DisableWarp
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(SentryGunInstance), nameof(SentryGunInstance.OnWarp))]
        private static bool Pre_(SentryGunInstance __instance) => __instance.gameObject.GetComponent<LSSComp>() == null;
    }
}
