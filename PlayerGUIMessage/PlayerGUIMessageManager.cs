using GameData;
using GTFO.API;
using Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace EOSExt.LevelSpawnedSentry.PlayerGUIMessage
{
    // i dont wanna bother myself with TMPPro, then just copy-instans from PlayerGuiLayer.PUI_WardenIntel
    public partial class PlayerGUIMessageManager
    {
        public static PlayerGUIMessageManager Current { get; } = new();

        public static PlayerGuiLayer PlayerGUILayer => GuiManager.PlayerLayer;

        private PUI_ObjectiveTimer m_timerLayer;

        private ISet<int> m_targetedByLSS = new HashSet<int>();

        internal void Setup(Transform root)
        {
            m_timerLayer = PlayerGUILayer.AddComp("Gui/Player/PUI_ObjectiveTimer", GuiAnchor.TopCenter, new Vector2(0f, -200f)).Cast<PUI_ObjectiveTimer>();
        }

        internal void OnLocalPlayerTargeted(LSS sentry) 
        {
            m_targetedByLSS.Add(sentry.InstanceIndex);

            if (m_targetedByLSS.Count > 0 && !m_timerLayer.gameObject.active)
            {
                var b = GameDataBlockBase<TextDataBlock>.GetBlock("LSS.OnPlayerTargeted.GUIText");
                SetText(b != null ? Text.Get(b.persistentID) : "<color=red>OMG IM SO FKING DEAD</color>");
            }
        }

        internal void OnLocalPlayerUnTargeted(LSS sentry)
        {
            m_targetedByLSS.Remove(sentry.InstanceIndex);
            if(m_targetedByLSS.Count == 0)
            {
                Clear();
            }
        }

        public void SetText(string text)
        {
            m_timerLayer.gameObject.active = true;
            m_timerLayer.UpdateTimerTitle(text);
            if (GameStateManager.CurrentStateName == eGameStateName.InLevel)
            {
                CoroutineManager.BlinkIn(m_timerLayer.m_titleText.gameObject);
            }
        }

        public void Clear() 
        {
            m_timerLayer.gameObject.active = false;
            m_timerLayer.SetTimerTextEnabled(false);
        }

        private void OnLevelCleanup()
        {
            Clear();
            m_targetedByLSS.Clear();
        }

        private PlayerGUIMessageManager()
        {
            LevelAPI.OnBuildStart += OnLevelCleanup;
            LevelAPI.OnLevelCleanup += OnLevelCleanup;
        }

        static PlayerGUIMessageManager()
        {

        }
    }
}
