using EOSExt.LevelSpawnedSentry.Impl;
using FX_EffectSystem;
using Gear;
using HarmonyLib;
using UnityEngine;
using System;
using Player;
using FloLib.Infos;
using ExtraObjectiveSetup.Utils;

namespace EOSExt.LevelSpawnedSentry.Patches
{
    [HarmonyPatch]
    internal static class Firing
    {
        // LSS.sentry.Owner == null -> causes exception when targeting player
        // Here we set owner to the targeted player
        // Also LSS should not be affected by localplayer's booster modifier
        [HarmonyPrefix]
        [HarmonyWrapSafe]
        [HarmonyPatch(typeof(SentryGunInstance_Firing_Bullets), nameof(SentryGunInstance_Firing_Bullets.FireBullet))]
        private static bool Pre_FireBullet(SentryGunInstance_Firing_Bullets __instance, bool doDamage, bool targetIsTagged)
        {
            var sentry = __instance.m_core?.TryCast<SentryGunInstance>();
            var lssComp = sentry?.gameObject.GetComponent<LSSComp>();

            if (lssComp == null) return true;
            var state = lssComp.LSS.State;

            EOSLogger.Warning("FireBullet: Override");

            //if (!state.TargetPlayer) return true;

            // =========================================================
            // copy-pasted code from the patched method from r6 mono 
            // =========================================================
            var s_weaponRayData = new Weapon.WeaponHitData
            {
                randomSpread = __instance.m_archetypeData.HipFireSpread,
                fireDir = __instance.MuzzleAlign.forward
            };

            SentryGunInstance_Firing_Bullets.s_weaponRayData = s_weaponRayData;

            if (__instance.m_archetypeData.Sentry_FireTowardsTargetInsteadOfForward && __instance.m_core.TryGetTargetAimPos(out var a))
            {
                SentryGunInstance_Firing_Bullets.s_weaponRayData.fireDir = (a - __instance.MuzzleAlign.position).normalized;
            }

            if (Weapon.CastWeaponRay(__instance.MuzzleAlign, ref s_weaponRayData, LayerManager.MASK_SENTRYGUN_RAY))
            {
                SentryGunInstance_Firing_Bullets.s_weaponRayData.owner = __instance.m_core.Owner;
                var maybePlayer = sentry.m_detection.Target?.GetComponent<PlayerAgent>();
                if (sentry.m_detection.HasTarget && maybePlayer != null)
                {
                    SentryGunInstance_Firing_Bullets.s_weaponRayData.owner = sentry.m_detection.Target.GetComponent<PlayerAgent>();
                    EOSLogger.Warning($"FireBullet: Owner <- Target: Player_{maybePlayer.Owner.PlayerSlotIndex()}");
                }
                else
                {
                    SentryGunInstance_Firing_Bullets.s_weaponRayData.owner = LocalPlayer.Agent;
                    EOSLogger.Warning($"FireBullet: Owner <- LocalPlayer: Player_{LocalPlayer.Agent.Owner.PlayerSlotIndex()}");
                }

                //SentryGunInstance_Firing_Bullets.s_weaponRayData.damage = __instance.m_archetypeData.GetSentryDamage(SentryGunInstance_Firing_Bullets.s_weaponRayData.owner, SentryGunInstance_Firing_Bullets.s_weaponRayData.rayHit.distance, targetIsTagged);
                SentryGunInstance_Firing_Bullets.s_weaponRayData.damage = targetIsTagged ?
                    (__instance.m_archetypeData.Damage * __instance.m_archetypeData.Sentry_DamageTagMulti) : __instance.m_archetypeData.Damage;


                SentryGunInstance_Firing_Bullets.s_weaponRayData.staggerMulti = __instance.m_archetypeData.GetSentryStaggerDamage(targetIsTagged);
                SentryGunInstance_Firing_Bullets.s_weaponRayData.precisionMulti = __instance.m_archetypeData.PrecisionDamageMulti;
                SentryGunInstance_Firing_Bullets.s_weaponRayData.damageFalloff = __instance.m_archetypeData.DamageFalloff;
                SentryGunInstance_Firing_Bullets.s_weaponRayData.vfxBulletHit = __instance.m_vfxBulletHit;
                

                BulletWeapon.BulletHit(SentryGunInstance_Firing_Bullets.s_weaponRayData, doDamage, 0f, 0U);
                

                FX_Manager.EffectTargetPosition = SentryGunInstance_Firing_Bullets.s_weaponRayData.rayHit.point;
                Debug.DrawLine(__instance.MuzzleAlign.position, SentryGunInstance_Firing_Bullets.s_weaponRayData.rayHit.point, Color.yellow);
            }
            else
            {
                FX_Manager.EffectTargetPosition = __instance.MuzzleAlign.position + __instance.MuzzleAlign.forward * 50f;
                Debug.DrawLine(__instance.MuzzleAlign.position, __instance.MuzzleAlign.position + __instance.MuzzleAlign.forward * 10f, Color.green);
            }

            __instance.OnBulletFired?.Invoke();

            SentryGunInstance_Firing_Bullets.s_tracerPool.AquireEffect().Play(null, __instance.MuzzleAlign.position, Quaternion.LookRotation(SentryGunInstance_Firing_Bullets.s_weaponRayData.fireDir));
            EX_SpriteMuzzleFlash muzzleFlash = __instance.m_muzzleFlash;
            if (muzzleFlash != null)
            {
                muzzleFlash.Play();
            }

            // From ghidra decompiled
            WeaponShellManager.EjectShell(
                ShellTypes.Shell_338, 
                1.0f, 
                UnityEngine.Random.RandomRangeInt(3, 5), 
                __instance.ShellEjectAlign
            );

            __instance.m_fireBulletTimer = Clock.Time + __instance.m_archetypeData.GetSentryShotDelay(__instance.m_core.Owner, targetIsTagged);

            // =========================================================
            // =========================================================

            return false;
        }


        [HarmonyPrefix]
        [HarmonyWrapSafe]
        [HarmonyPatch(typeof(SentryGunInstance_Firing_Bullets), nameof(SentryGunInstance_Firing_Bullets.UpdateFireShotgunSemi))]
        private static bool Pre_UpdateFireShotgunSemi(SentryGunInstance_Firing_Bullets __instance, bool isMaster, bool targetIsTagged)
        {
            var sentry = __instance.m_core?.TryCast<SentryGunInstance>();
            var lssComp = sentry?.gameObject.GetComponent<LSSComp>();

            if (lssComp == null) return true;
            var state = lssComp.LSS.State;

            if (Clock.Time > __instance.m_fireBulletTimer)
            {
                Vector3 position = __instance.MuzzleAlign.position;
                __instance.TriggerSingleFireAudio();
                for (int i = 0; i < __instance.m_archetypeData.ShotgunBulletCount; i++)
                {
                    float f = __instance.m_segmentSize * (float)i;
                    float num = 0f;
                    float num2 = 0f;
                    if (i > 0)
                    {
                        num += __instance.m_archetypeData.ShotgunConeSize * Mathf.Cos(f);
                        num2 += __instance.m_archetypeData.ShotgunConeSize * Mathf.Sin(f);
                    }
                    var s_weaponRayData = new Weapon.WeaponHitData
                    {
                        maxRayDist = __instance.MaxRayDist,
                        angOffsetX = num,
                        angOffsetY = num2,
                        randomSpread = (float)__instance.m_archetypeData.ShotgunBulletSpread,
                        fireDir = __instance.MuzzleAlign.forward
                    };

                    SentryGunInstance_Firing_Bullets.s_weaponRayData = s_weaponRayData;

                    if (Weapon.CastWeaponRay(__instance.MuzzleAlign, ref s_weaponRayData, position, LayerManager.MASK_SENTRYGUN_RAY))
                    {
                        SentryGunInstance_Firing_Bullets.s_weaponRayData.owner = __instance.m_core.Owner;
                        var maybePlayer = sentry.m_detection.Target?.GetComponent<PlayerAgent>();
                        if (sentry.m_detection.HasTarget && maybePlayer != null)
                        {
                            SentryGunInstance_Firing_Bullets.s_weaponRayData.owner = sentry.m_detection.Target.GetComponent<PlayerAgent>();
                            EOSLogger.Warning($"FireBullet: Owner <- Target: Player_{maybePlayer.Owner.PlayerSlotIndex()}");
                        }
                        else
                        {
                            SentryGunInstance_Firing_Bullets.s_weaponRayData.owner = LocalPlayer.Agent;
                            EOSLogger.Warning($"FireBullet: Owner <- LocalPlayer: Player_{LocalPlayer.Agent.Owner.PlayerSlotIndex()}");
                        }


                        //SentryGunInstance_Firing_Bullets.s_weaponRayData.damage = __instance.m_archetypeData.GetSentryDamage(SentryGunInstance_Firing_Bullets.s_weaponRayData.owner, SentryGunInstance_Firing_Bullets.s_weaponRayData.rayHit.distance, targetIsTagged);
                        SentryGunInstance_Firing_Bullets.s_weaponRayData.damage = targetIsTagged ?
                            (__instance.m_archetypeData.Damage * __instance.m_archetypeData.Sentry_DamageTagMulti) : __instance.m_archetypeData.Damage;


                        SentryGunInstance_Firing_Bullets.s_weaponRayData.staggerMulti = __instance.m_archetypeData.GetSentryStaggerDamage(targetIsTagged);
                        SentryGunInstance_Firing_Bullets.s_weaponRayData.precisionMulti = __instance.m_archetypeData.PrecisionDamageMulti;
                        SentryGunInstance_Firing_Bullets.s_weaponRayData.damageFalloff = __instance.m_archetypeData.DamageFalloff;
                        BulletWeapon.BulletHit(SentryGunInstance_Firing_Bullets.s_weaponRayData, isMaster, 0f, 0U);
                        FX_Manager.EffectTargetPosition = SentryGunInstance_Firing_Bullets.s_weaponRayData.rayHit.point;
                    }
                    else
                    {
                        FX_Manager.EffectTargetPosition = __instance.MuzzleAlign.position + __instance.MuzzleAlign.forward * 50f;
                    }

                    FX_Manager.PlayLocalVersion = false;
                    SentryGunInstance_Firing_Bullets.s_tracerPool.AquireEffect().Play(null, __instance.MuzzleAlign.position, Quaternion.LookRotation(SentryGunInstance_Firing_Bullets.s_weaponRayData.fireDir));
                }

                __instance.UpdateAmmo(-1);

                __instance.OnBulletFired?.Invoke();

                __instance.m_fireBulletTimer = Clock.Time + 
                    (targetIsTagged ? 
                    __instance.m_archetypeData.ShotDelay * __instance.m_archetypeData.Sentry_ShotDelayTagMulti 
                    : __instance.m_archetypeData.ShotDelay
                );
            }

            return false;
        }
    }
}
