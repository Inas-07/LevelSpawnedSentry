using Agents;
using EOSExt.LevelSpawnedSentry.PlayerGUIMessage;
using ExtraObjectiveSetup.Utils;
using FloLib.Infos;
using Gear;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using Player;
using SNetwork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EOSExt.LevelSpawnedSentry
{
    [HarmonyPatch]
    public static class LSSDebug
    {
        //[HarmonyPrefix]
        //[HarmonyWrapSafe]
        //[HarmonyPatch(typeof(BulletWeapon), nameof(BulletWeapon.BulletHit))]
        //private static void Pre_BulletHit(ref Weapon.WeaponHitData weaponRayData)
        //{
        //    if(weaponRayData.owner == null && LocalPlayer.TryGetLocalAgent(out var player))
        //    {
        //        weaponRayData.owner = player;
        //    }
        //}

        //[HarmonyPrefix]
        //[HarmonyWrapSafe]
        //[HarmonyPatch(typeof(Dam_PlayerDamageLimb), nameof(Dam_PlayerDamageLimb.BulletDamage))]
        //private static void Post_(Dam_PlayerDamageLimb __instance, Agent sourceAgent)
        //{
        //    EOSLogger.Warning($"Dam_PlayerDamageLimb.BulletDamage:\n sourceAgent: {sourceAgent?.name ?? null}, base != null : {__instance.m_base != null}");
        //    if(__instance.m_base != null)
        //    {
        //        EOSLogger.Warning($"PlayerData != null: {__instance.m_base.m_playerData != null}");
        //    }
        //}

        // UpdateFireMaster异常，m_sync.UpdateMaster也不调用

        /*
         * [Error  :Il2CppInterop] During invoking native->managed trampoline
Exception: Il2CppInterop.Runtime.Il2CppException: System.NullReferenceException: Object reference not set to an instance of an object.
--- BEGIN IL2CPP STACK TRACE ---
System.NullReferenceException: Object reference not set to an instance of an object.
  at Dam_PlayerDamageLimb.BulletDamage (System.Single dam, Agents.Agent sourceAgent, UnityEngine.Vector3 position, UnityEngine.Vector3 direction, UnityEngine.Vector3 normal, System.Boolean allowDirectionalBonus, System.Single staggerMulti, System.Single precisionMulti, System.UInt32 gearCategoryId) [0x00000] in <00000000000000000000000000000000>:0
  at Gear.BulletWeapon.BulletHit (Weapon+WeaponHitData weaponRayData, System.Boolean doDamage, System.Single additionalDis, System.UInt32 damageSearchID, System.Boolean allowDirectionalBonus) [0x00000] in <00000000000000000000000000000000>:0
  at SentryGunInstance_Firing_Bullets.UpdateFireShotgunSemi (System.Boolean isMaster, System.Boolean targetIsTagged) [0x00000] in <00000000000000000000000000000000>:0
  at System.Action`2[T1,T2].Invoke (T1 arg1, T2 arg2) [0x00000] in <00000000000000000000000000000000>:0
  at SentryGunInstance_Firing_Bullets.UpdateFireMaster (System.Boolean targetIsTagged) [0x00000] in <00000000000000000000000000000000>:0
  at SentryGunInstance.UpdateMaster () [0x00000] in <00000000000000000000000000000000>:0
--- END IL2CPP STACK TRACE ---

   at Il2CppInterop.Runtime.Il2CppException.RaiseExceptionIfNecessary(IntPtr returnedException) in /home/runner/work/Il2CppInterop/Il2CppInterop/Il2CppInterop.Runtime/Il2CppException.cs:line 36
   at DMD<SentryGunInstance::UpdateMaster>(SentryGunInstance this)
   at (il2cpp -> managed) UpdateMaster(IntPtr , Il2CppMethodInfo* )
         */
    }
}
