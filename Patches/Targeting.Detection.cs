using Agents;
using EOSExt.LevelSpawnedSentry.Impl;
using EOSExt.LevelSpawnedSentry.PlayerGUIMessage;
using GTFO.API;
using HarmonyLib;
using Player;
using UnityEngine;

namespace EOSExt.LevelSpawnedSentry.Patches
{
    [HarmonyPatch]
    internal static partial class Targeting
    {
        // FIXME: this is only used on host side
        [HarmonyPrefix]
        [HarmonyWrapSafe]
        [HarmonyPatch(typeof(SentryGunInstance_Detection), nameof(SentryGunInstance_Detection.UpdateDetection))]
        private static bool Pre_UpdateDetection_OnlyTargetPlayer_Master(SentryGunInstance_Detection __instance)
        {
            var sentry = __instance.m_core?.TryCast<SentryGunInstance>();
            var lssComp = sentry?.gameObject.GetComponent<LSSComp>();

            if (sentry == null || lssComp == null) return true;

            if (!lssComp.LSS.State.Enabled)
            {
                PlayerGUIMessageManager.Current.OnLocalPlayerUnTargeted(lssComp.LSS);
                return false;
            }

            if (lssComp.LSS.State.TargetEnemy) return true; // use vanilla code, which targets player as well

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

                            //if (playerAgent.IsLocallyOwned)
                            //{
                            //    PlayerGUIMessageManager.Current.OnLocalPlayerTargeted(lssComp.LSS);
                            //}
                            //else
                            //{
                            //    //PlayerGUIMessageManager.Current.OnLocalPlayerUnTargeted(lssComp.LSS);
                            //    //NetworkAPI.InvokeEvent(NETWORK_EVENT_NAME, new TargetClient() {
                            //    //    PlayerSlotIndex = playerAgent.Owner.PlayerSlotIndex(),
                            //    //    LSSInstanceIndex = lssComp.LSS.InstanceIndex,
                            //    //},
                            //    //SNetwork.SNet_ChannelType.GameNonCritical);
                            //}
                        }
                        //else
                        //{
                        //    PlayerGUIMessageManager.Current.OnLocalPlayerUnTargeted(lssComp.LSS);
                        //}
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

        // merged to Targeting.Client.Post_UpdateMaster
        //[HarmonyPostfix]
        //[HarmonyWrapSafe]
        //[HarmonyPatch(typeof(SentryGunInstance_Detection), nameof(SentryGunInstance_Detection.UpdateDetection))]
        //private static void Post_UpdateDetection_TargetPlayerAndEnemy_Master(SentryGunInstance_Detection __instance)
        //{
        //    var sentry = __instance.m_core?.TryCast<SentryGunInstance>();
        //    var lssComp = sentry?.gameObject.GetComponent<LSSComp>();

        //    if (sentry == null || lssComp == null) return;

        //    var state = lssComp.LSS.State;
        //    if (!state.Enabled || !state.TargetPlayer || __instance.Target == null)
        //    {
        //        PlayerGUIMessageManager.Current.OnLocalPlayerUnTargeted(lssComp.LSS);
        //    }

        //    else
        //    {
        //        var maybePlayer = __instance.Target.GetComponent<PlayerAgent>();
        //        if (maybePlayer != null)
        //        {
        //            if(maybePlayer.IsLocallyOwned)
        //            {
        //                PlayerGUIMessageManager.Current.OnLocalPlayerTargeted(lssComp.LSS);
        //            }
        //            else
        //            {
        //                PlayerGUIMessageManager.Current.OnLocalPlayerUnTargeted(lssComp.LSS);
        //                //NetworkAPI.InvokeEvent(NETWORK_EVENT_NAME, new TargetClient()
        //                //{
        //                //    PlayerSlotIndex = maybePlayer.Owner.PlayerSlotIndex(),
        //                //    LSSInstanceIndex = lssComp.LSS.InstanceIndex,
        //                //},
        //                //SNetwork.SNet_ChannelType.GameNonCritical);
        //            }
        //        }
        //    }
        //}
    }
}
