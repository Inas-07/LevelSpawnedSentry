using EOSExt.LevelSpawnedSentry.PlayerGUIMessage;
using ExtraObjectiveSetup.Utils;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace EOSExt.LevelSpawnedSentry.Patches
{
    [HarmonyPatch]
    internal static class OnPlayerGuiLayerSetup
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerGuiLayer), nameof(PlayerGuiLayer.Setup))]
        private static void Post_(PlayerGuiLayer __instance, Transform root, string name)
        {
            PlayerGUIMessageManager.Current.Setup(root);
        }
    }
}
