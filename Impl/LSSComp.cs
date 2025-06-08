using AK;
using EOSExt.LevelSpawnedSentry.Definition;
using ExtraObjectiveSetup.Utils;
using Il2CppInterop.Runtime.Injection;
using UnityEngine;

namespace EOSExt.LevelSpawnedSentry.Impl
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

                UpdateVisuals();
                SetCanRefill(def.InitialState.CanRefill);
            }

            return this;
        }

        public void SetCanRefill(bool canRefill) => Sentry.AmmoMaxCap = canRefill ? Def.AmmoCap : 0f;

        // Visuals, state changed
        internal void OnStateChanged(LSSSyncState oldState, LSSSyncState newState, bool isRecall)
        {
            EOSLogger.Debug($"LSSOnStateChanged \"{Def.WorldEventObjectFilter}\"! OldState: [{oldState}], NewState: {newState}");

            var s = Sentry;

            if (newState.Enabled)
            {
                s.m_isSetup = true;
                s.m_isScanning = false;
                s.m_startScanTimer = Clock.Time + s.m_initialScanDelay;

                s.Sound.Post(EVENTS.SENTRYGUN_LOW_AMMO_WARNING);
            }
            else
            {
                s.m_isSetup = false;
                s.m_isScanning = false;
                s.m_isFiring = false;

                s.Sound.Post(EVENTS.SENTRYGUN_STOP_ALL_LOOPS);
            }

            var d = s.m_detection?.TryCast<SentryGunInstance_Detection>();
            if (d != null)
            {
                d.m_targetPlayers = newState.TargetPlayer;
            }

            SetCanRefill(newState.CanRefill);

            UpdateVisuals();
        }

        internal void UpdateVisuals()
        {
            var state = LSS.State;
            var v = Visuals;

            var c = LSS.GetScanningColor(state.TargetEnemy, state.TargetPlayer);
            v.m_scanningColor = c;
            
            if (state.Enabled)
            {
                if (v.m_core != null) // visual was setup
                {
                    v.SetVisualStatus(eSentryGunStatus.Scanning, true);
                }

                if (state.MarkerVisible)
                {
                    marker?.SetColor(c);
                    marker?.SetVisible(true);
                    marker?.SetTitle(LSS.GetMarkerText(state.TargetEnemy, state.TargetPlayer));
                }
                else
                {
                    marker?.SetVisible(false);
                }
            }

            else
            {
                if (v.m_core != null)
                {
                    //v.SetVisualStatus(eSentryGunStatus.Disabled, true);
                    v.m_scannerPlane.SetColor(LSS.OffColor);
                    v.UpdateLightProps(LSS.OffColor, false);
                }
                
                marker?.SetVisible(false);
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
