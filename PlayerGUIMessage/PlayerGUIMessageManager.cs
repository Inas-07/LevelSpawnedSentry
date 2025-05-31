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

        internal void Setup(Transform root)
        {
            m_timerLayer = PlayerGUILayer.AddComp("Gui/Player/PUI_ObjectiveTimer", GuiAnchor.TopCenter, new Vector2(0f, -200f)).Cast<PUI_ObjectiveTimer>();
        }

        public void SetText(string text)
        {
            m_timerLayer.gameObject.active = true;
            m_timerLayer.UpdateTimerTitle(text);
            if(GameStateManager.CurrentStateName == eGameStateName.InLevel)
            {
                CoroutineManager.BlinkIn(m_timerLayer.m_titleText.gameObject);
            }
        }

        internal void OnPlayerTargeted() 
        {
            if (m_timerLayer.gameObject.active) return;
            var b = GameDataBlockBase<TextDataBlock>.GetBlock("LSS.OnPlayerTargeted.GUIText");

            SetText(b != null ? Text.Get(b.persistentID) : "<color=red>OMG IM SO FKING DEAD</color>");
        }


        public void Clear()
        {
            m_timerLayer.gameObject.active = false;
            m_timerLayer.SetTimerTextEnabled(false);
        }

        private PlayerGUIMessageManager()
        {
            LevelAPI.OnBuildStart += Clear;
            LevelAPI.OnLevelCleanup += Clear;
        }

        static PlayerGUIMessageManager()
        {

        }
    }
}
