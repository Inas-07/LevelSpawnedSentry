using AK;
using EOSExt.LevelSpawnedSentry.Definition;
using Il2CppInterop.Runtime.Injection;
using UnityEngine;

namespace EOSExt.LevelSpawnedSentry
{
    public class LSSComp: MonoBehaviour
    {
        public LSS LSS { get; private set; }

        public LevelSpawnedSentryDefinition Def => LSS.Def;

        public SentryGunInstance_ScannerVisuals_Plane Visuals => gameObject.GetComponent<SentryGunInstance_ScannerVisuals_Plane>();

        public SentryGunInstance Sentry => gameObject.GetComponent<SentryGunInstance>();

        public NavMarker marker { get; private set; }

        public bool IsSetup => LSS != null;

        internal LSSComp Setup(LSS lss)
        {
            if (!IsSetup)
            {
                LSS = lss;
                var def = Def;

                marker = GuiManager.NavMarkerLayer.PrepareGenericMarker(gameObject);
                if (marker != null)
                {
                    var c = LSS.GetScanningColor(def.InitialState.TargetEnemy, def.InitialState.TargetPlayer);
                    marker.SetStyle(eNavMarkerStyle.LocationBeacon);
                    marker.SetColor(c);
                    marker.SetVisible(def.InitialState.Enabled);
                    marker.SetTitle(LSS.GetMarkerText(def.InitialState.TargetEnemy, def.InitialState.TargetPlayer));
                }
            }

            return this;
        }

        // Visuals, state changed
        internal void OnStateChanged(LSSState oldState, LSSState newState, bool isRecall)
        {
            var s = Sentry;
            var v = Visuals;

            if (newState.Enabled)
            {
                s.m_isSetup = true;
                s.m_isScanning = false;
                s.m_startScanTimer = Clock.Time + s.m_initialScanDelay;
            }
            else
            {
                v.m_scannerPlane.SetColor(LSS.OffColor);
                v.UpdateLightProps(LSS.OffColor, false);
                s.m_isSetup = false;
                s.m_isScanning = false;
                s.m_isFiring = false;
            }

            var d = s.m_detection?.TryCast<SentryGunInstance_Detection>();
            if (d != null)
            {
                d.m_targetPlayers = newState.TargetPlayer;
            }

            var c = LSS.GetScanningColor(newState.TargetEnemy, newState.TargetPlayer);
            v.m_scanningColor = c;
            if (v.m_core != null) // visual was setup
            {
                v.SetVisualStatus(eSentryGunStatus.Scanning, true);
            }

            marker?.SetColor(c);
            marker?.SetVisible(newState.Enabled);
            marker?.SetTitle(LSS.GetMarkerText(newState.TargetEnemy, newState.TargetPlayer));

            // sound
            if (oldState.Enabled != newState.Enabled)
            {
                s.Sound.Post(newState.Enabled ? EVENTS.SENTRYGUN_LOW_AMMO_WARNING : EVENTS.SENTRYGUN_STOP_ALL_LOOPS);
            }
            else
            {
                if (newState.Enabled &&
                    (oldState.TargetPlayer != newState.TargetPlayer || oldState.TargetEnemy != newState.TargetEnemy))
                {
                    s.Sound.Post(EVENTS.SENTRYGUN_LOW_AMMO_WARNING);
                }
            }
        }

        private void OnDestroy()
        {
            GuiManager.NavMarkerLayer.RemoveMarker(marker);
            marker = null;
        }

        static LSSComp() 
        { 
            ClassInjector.RegisterTypeInIl2Cpp<LSSComp>();
        }
    }
}
