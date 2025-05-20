using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TenChambers.EventHandlers;

namespace EOSExt.LevelSpawnedSentry.Patches
{
    [HarmonyPatch]
    internal static class DisableAnalytic
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(AnalyticsManager), nameof(AnalyticsManager.SentryEventsPayload))]
        private static bool Pre_(SentryGunInstance __instance) => __instance != null && __instance.Owner != null;

        [HarmonyPrefix]
        [HarmonyPatch(typeof(SentryPlaced), nameof(SentryPlaced.OnGameEvent))]
        private static bool Pre_(GameEvent.GameEventData data) => data.player != null;
    }
}
