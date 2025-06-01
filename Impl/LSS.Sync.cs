using EOSExt.LevelSpawnedSentry.Impl;
using GTFO.API;
using Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace EOSExt.LevelSpawnedSentry
{
    public partial class LSS
    {
        private LSSTargeted m_LSSTargeted = new();

        public int LastSyncedTargetedPlayer => m_LSSTargeted.PlayerSlotIndex;

        internal void OnTargetedPlayer(PlayerAgent player) => m_LSSTargeted.PlayerSlotIndex = player?.Owner.PlayerSlotIndex() ?? -1;

        internal void OnUpdateMaster()
        {
            NetworkAPI.InvokeEvent<LSSTargeted>(LSS_SYNC_EVT, m_LSSTargeted);
        }
    }
}
