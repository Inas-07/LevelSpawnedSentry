//using EOSExt.LevelSpawnedSentry.PlayerGUIMessage;
//using ExtraObjectiveSetup.Utils;
//using FloLib.Infos;
//using Il2CppInterop.Runtime.Injection;
//using Player;
//using SNetwork;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace EOSExt.LevelSpawnedSentry
//{
//    public partial class LSS
//    {
//        private struct TargetedPlayer
//        {
//            public int PlayerSlotIndex { get; set; } = -1;

//            public TargetedPlayer() { }

//            public TargetedPlayer(TargetedPlayer o)
//            {
//                PlayerSlotIndex = o.PlayerSlotIndex;
//            }
//        }

//        private TargetedPlayer m_targetedPlayer = new();

//        private SNet_Packet<TargetedPlayer> m_targetedPlayerPacket; // nope, can't use on our side

//        public int LastSyncedTargetedPlayer => m_targetedPlayer.PlayerSlotIndex;

//        internal void OnTargetedPlayer(PlayerAgent player) => m_targetedPlayer.PlayerSlotIndex = player?.Owner.PlayerSlotIndex() ?? -1;

//        private void ClearSync()
//        {
//            m_targetedPlayerPacket = null;
//        }

//        private void SetupSync()
//        {
//            ClearSync();

//            var sentry = LSSComp.Sentry;

//            void OnTargetingData(TargetedPlayer p) => m_targetedPlayer.PlayerSlotIndex = p.PlayerSlotIndex;

//            m_targetedPlayerPacket = sentry.Replicator.CreatePacket((Il2CppSystem.Action<TargetedPlayer>)OnTargetingData, null);
//        }

//        internal void OnUpdateMaster() 
//        {
//            if(m_targetedPlayerPacket == null)
//            {
//                EOSLogger.Error("SendSyncPacket: Got null packet??");
//                return;
//            }

//            m_targetedPlayerPacket.Send(m_targetedPlayer, SNet_ChannelType.GameNonCritical);
//        }
//    }
//}
