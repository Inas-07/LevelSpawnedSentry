using GTFO.API;
using HarmonyLib;
using FloLib.Infos;
using ExtraObjectiveSetup.Utils;
using EOSExt.LevelSpawnedSentry.PlayerGUIMessage;
using SNetwork;
using Player;
using EOSExt.LevelSpawnedSentry.Impl;
namespace EOSExt.LevelSpawnedSentry.Patches
{
    [HarmonyPatch]
    internal static partial class Targeting
    {
        [HarmonyPrefix]
        [HarmonyWrapSafe]
        [HarmonyPatch(typeof(SentryGunInstance_Sync), nameof(SentryGunInstance_Sync.UpdateMaster))]
        private static void Post_UpdateMaster(SentryGunInstance_Sync __instance)
        {
            //if (!SNet.IsMaster) return; // Impossible, isnt it? huh

            if (Clock.Time - __instance.m_lastSyncTime <= 0.5f) return;
            var sentry = __instance.m_core.TryCast<SentryGunInstance>();
            var lssComp = sentry?.gameObject.GetComponent<LSSComp>();
            if (lssComp == null) return;

            lssComp.LSS.OnUpdateMaster();
        }

        [HarmonyPrefix]
        [HarmonyWrapSafe]
        [HarmonyPatch(typeof(SentryGunInstance_Sync), nameof(SentryGunInstance_Sync.UpdateClient))]
        private static void Post_UpdateClient(SentryGunInstance_Sync __instance)
        {
            if (Clock.Time - __instance.m_lastSyncTime > __instance.m_syncTimeout)
            {
                var sentry = __instance.m_core.TryCast<SentryGunInstance>();
                var lssComp = sentry?.gameObject.GetComponent<LSSComp>();
                if (lssComp == null) return;

                lssComp.LSS.OnTargetedPlayer(null);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(SentryGunInstance), nameof(SentryGunInstance.UpdateMaster))]
        private static void Post_UpdateMaster(SentryGunInstance __instance)
        {
            var lssComp = __instance?.gameObject.GetComponent<LSSComp>();

            if (lssComp == null) return;

            if (!lssComp.LSS.State.Enabled || !lssComp.LSS.State.TargetPlayer
                || !__instance.m_detection.HasTarget || __instance.m_detection.Target == null)
            {
                PlayerGUIMessageManager.Current.OnLocalPlayerUnTargeted(lssComp.LSS);
                lssComp.LSS.OnTargetedPlayer(null);
                return;
            }

            var maybePlayer = __instance.m_detection.Target.GetComponent<PlayerAgent>();
            if (maybePlayer == null)
            {
                PlayerGUIMessageManager.Current.OnLocalPlayerUnTargeted(lssComp.LSS);
                lssComp.LSS.OnTargetedPlayer(null);
            }
            else
            {
                if (maybePlayer.IsLocallyOwned)
                {
                    PlayerGUIMessageManager.Current.OnLocalPlayerTargeted(lssComp.LSS);
                }
                else
                {
                    PlayerGUIMessageManager.Current.OnLocalPlayerUnTargeted(lssComp.LSS);
                }

                lssComp.LSS.OnTargetedPlayer(maybePlayer);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(SentryGunInstance), nameof(SentryGunInstance.UpdateClient))]
        private static void Post_UpdateClient(SentryGunInstance __instance)
        {
            var lssComp = __instance?.gameObject.GetComponent<LSSComp>();

            if (lssComp == null) return;

            if (!lssComp.LSS.State.Enabled || !lssComp.LSS.State.TargetPlayer 
                || !__instance.m_sync.LastSyncData.hasTarget)
            {
                PlayerGUIMessageManager.Current.OnLocalPlayerUnTargeted(lssComp.LSS);
                return;
            }

            if (!LocalPlayer.TryGetLocalAgent(out var localPlayer))
            {
                EOSLogger.Error("Post_UpdateClient: Cannot get local player agent!");
                return;
            }

            if (lssComp.LSS.LastSyncedTargetedPlayer == localPlayer.Owner.PlayerSlotIndex())
            {
                PlayerGUIMessageManager.Current.OnLocalPlayerTargeted(lssComp.LSS);
                //lssComp.LSS.OnTargetedPlayer(localPlayer);
            }
            else
            {
                PlayerGUIMessageManager.Current.OnLocalPlayerUnTargeted(lssComp.LSS);
                //lssComp.LSS.OnTargetedPlayer(null);
            }
        }
    }
}
