using AK;
using EOSExt.LevelSpawnedSentry.Definition;
using ExtraObjectiveSetup;
using ExtraObjectiveSetup.Utils;
using FloLib.Networks.Replications;
using GameData;
using Il2CppInterop.Runtime.Injection;
using LevelGeneration;
using Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace EOSExt.LevelSpawnedSentry
{
    public class LSS: MonoBehaviour
    {
        public LevelSpawnedSentryDefinition Def { get; private set; }

        // just for simple state-sync.
        public StateReplicator<LSSState> StateReplicator { get; private set; }

        public SentryGunInstance_ScannerVisuals_Plane Visuals => gameObject.GetComponent<SentryGunInstance_ScannerVisuals_Plane>();

        public SentryGunInstance Sentry => gameObject.GetComponent<SentryGunInstance>();

        public NavMarker NavMarker { get; private set; }

        internal void Setup(LevelSpawnedSentryDefinition def)
        {
            if (this.Def != null) return;
            this.Def = def;

            uint sid = EOSNetworking.AllotReplicatorID();
            if (sid != EOSNetworking.INVALID_ID)
            {
                StateReplicator = StateReplicator<LSSState>.Create(sid, def.InitialState, LifeTimeType.Level);
                StateReplicator.OnStateChanged += OnStateChanged;

                StateReplicator.SetState(def.InitialState);
            }

            NavMarker = GuiManager.NavMarkerLayer.PrepareGenericMarker(gameObject);
            if (NavMarker != null)
            {
                var c = LSS.GetScanningColor(def.InitialState.TargetEnemy, def.InitialState.TargetPlayer);
                NavMarker.SetColor(c);
                //NavMarker.SetStyle(eNavMarkerStyle.LocationBeaconNoText);
                NavMarker.SetStyle(eNavMarkerStyle.LocationBeacon);
                NavMarker.SetTitle(GetMarkerText(def.InitialState.TargetEnemy, def.InitialState.TargetPlayer));
                NavMarker.SetVisible(def.InitialState.Enabled);
            }
        }

        private void OnStateChanged(LSSState oldState, LSSState newState, bool isRecall)
        {
            var s = Sentry;
            var v = Visuals;

            if (oldState.Enabled != newState.Enabled)
            {
                if (newState.Enabled)
                {
                    s.m_isSetup = true;
                    s.m_visuals.SetVisualStatus(eSentryGunStatus.BootUp);
                    s.m_isScanning = false;
                    s.m_startScanTimer = Clock.Time + s.m_initialScanDelay;
                    //s.Sound.Post(EVENTS.SENTRYGUN_LOW_AMMO_WARNING);
                }
                else
                {
                    v.m_scannerPlane.SetColor(_offColor);
                    v.UpdateLightProps(_offColor, false);
                    s.m_isSetup = false;
                    s.m_isScanning = false;
                    s.m_isFiring = false;
                    //s.Sound.Post(EVENTS.SENTRYGUN_STOP_ALL_LOOPS);
                }
            }

            var d = s.m_detection?.TryCast<SentryGunInstance_Detection>();
            if(d != null)
            {
                d.m_targetPlayers = newState.TargetPlayer;
            }

            var c = LSS.GetScanningColor(newState.TargetEnemy, newState.TargetPlayer);
            v.m_scanningColor = c;
            if (v.m_core != null) // visual was setup
            {
                v.SetVisualStatus(eSentryGunStatus.Scanning, true);
                //v.m_spotLight.color = c;
                //v.m_scannerPlane?.SetColor(c);
                //v.m_targetingAlign?.Visor?.material.SetColor("_Color", c);
            }

            NavMarker?.SetColor(c);
            NavMarker?.SetTitle(GetMarkerText(newState.TargetEnemy, newState.TargetPlayer));
            NavMarker?.SetVisible(newState.Enabled);

            // sound
            if (oldState.Enabled != newState.Enabled)
            {
                s.Sound.Post(newState.Enabled ? EVENTS.SENTRYGUN_LOW_AMMO_WARNING : EVENTS.SENTRYGUN_STOP_ALL_LOOPS);
            }
            else
            {
                if(newState.Enabled && 
                    (oldState.TargetPlayer != newState.TargetPlayer || oldState.TargetEnemy != newState.TargetEnemy))
                {
                    s.Sound.Post(EVENTS.SENTRYGUN_LOW_AMMO_WARNING);
                }
            }
        }

        private void OnDestroy()
        {
            StateReplicator?.Unload();
            GameObject.Destroy(NavMarker);
            NavMarker = null;
            EOSLogger.Error($"LSS Destroyed: {Def.WorldEventObjectFilter}");
        }

        private static readonly Color _offColor = ColorExt.Hex("#00000000");

        private static readonly Color _targetBothColor = ColorExt.Hex("FFFA00");

        private static readonly Color _targetPlayerOnlyColor = ColorExt.Hex("#F0192EFF");

        private static readonly Color _targetEnemyOnlyColor = ColorExt.Hex("#00895CFF");

        private static readonly Color _doesNotTargetColor = ColorExt.Hex("#198AF0FF") * 0.5f;

        internal static Color GetScanningColor(bool targetEnemy, bool targetPlayer)
        {
            Color c = Color.grey * 0.5f;
            if (targetEnemy && targetPlayer)
            {
                c = _targetBothColor;
            }
            else if (targetEnemy && !targetPlayer)
            {
                c = _targetEnemyOnlyColor;
            }
            else if (!targetEnemy && targetPlayer)
            {
                c = _targetPlayerOnlyColor;
            }
            else
            {
                c = _doesNotTargetColor;
            }

            return c;
        }

        internal static string GetMarkerText(bool targetEnemy, bool targetPlayer)
        {
            string textDBName = string.Empty;
            string text = string.Empty;
            if (targetEnemy && targetPlayer)
            {
                textDBName = "LSS.State.Both";
                text = "HAVOC";
            }
            else if (targetEnemy && !targetPlayer)
            {
                textDBName = "LSS.State.Normal";
                text = "NORMAL";
            }
            else if (!targetEnemy && targetPlayer)
            {
                textDBName = "LSS.State.Player";
                text = "HOSTILE";
            }
            else
            {
                textDBName = "LSS.State.Idle";
                text = "IDLE";
            }

            var textDB = GameDataBlockBase<TextDataBlock>.GetBlock(textDBName);
            return textDB != null ? Text.Get(textDB.persistentID) : text;
        }

        static LSS() 
        { 
            ClassInjector.RegisterTypeInIl2Cpp<LSS>();
        }
    }
}
