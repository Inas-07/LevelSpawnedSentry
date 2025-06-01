using Enemies;
using EOSExt.LevelSpawnedSentry.Impl;
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
    internal static partial class Targeting
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
    }
}
