using AK;
using EOSExt.LevelSpawnedSentry.Definition;
using ExtraObjectiveSetup;
using FloLib.Networks.Replications;
using Il2CppInterop.Runtime.Injection;
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
        private static readonly Color _offColor = new Color()
        {
            r = 0.0f,
            g = 0.0f,
            b = 0.0f,
            a = 0.0f
        };

        public LevelSpawnedSentryDefinition Def { get; private set; }

        // just for simple state-sync.
        public StateReplicator<LSSState> StateReplicator { get; private set; }

        public SentryGunInstance_ScannerVisuals_Plane Visuals => gameObject.GetComponent<SentryGunInstance_ScannerVisuals_Plane>();

        public SentryGunInstance Sentry => gameObject.GetComponent<SentryGunInstance>();

        internal void Setup(LevelSpawnedSentryDefinition def)
        {
            if (this.Def != null) return;
            this.Def = def;

            uint sid = EOSNetworking.AllotReplicatorID();
            if (sid != EOSNetworking.INVALID_ID)
            {
                StateReplicator = StateReplicator<LSSState>.Create(sid, def.InitialState, LifeTimeType.Level);
                StateReplicator.OnStateChanged += OnStateChanged;
            }
        }

        private void OnStateChanged(LSSState oldState, LSSState newState, bool isRecall)
        {
            if (oldState.enabled == newState.enabled) return;

            var s = Sentry;
            
            if(newState.enabled)
            {
                s.m_isSetup = true;
                s.m_visuals.SetVisualStatus(eSentryGunStatus.BootUp);
                s.m_isScanning = false;
                s.m_startScanTimer = Clock.Time + s.m_initialScanDelay;
                s.Sound.Post(EVENTS.SENTRYGUN_LOW_AMMO_WARNING);
            }
            else
            {
                var v = Visuals;
                v.m_scannerPlane.SetColor(_offColor);
                v.UpdateLightProps(_offColor, false);
                s.m_isSetup = false;
                s.m_isScanning = false;
                s.m_isFiring = false;
                s.Sound.Post(EVENTS.SENTRYGUN_STOP_ALL_LOOPS);
            }
        }

        private void OnDestroy()
        {
            StateReplicator?.Unload();
        }

        static LSS() 
        { 
            ClassInjector.RegisterTypeInIl2Cpp<LSS>();
        }
    }
}
