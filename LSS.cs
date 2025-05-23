using EOSExt.LevelSpawnedSentry.Definition;
using ExtraObjectiveSetup;
using FloLib.Networks.Replications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static UnityEngine.CullingGroup;
using UnityEngine;
using AK;
using static EAB_ProjectileShooterSquidBoss;
using ExtraObjectiveSetup.Utils;
using FluffyUnderware.Curvy.Generator;
using GameData;
using Il2CppInterop.Runtime.Injection;
using GTFO.API;
using AIGraph;
using Gear;
using SNetwork;

namespace EOSExt.LevelSpawnedSentry
{
    public partial class LSS
    {
        public const string PUBLIC_NAME_PREFIX = "E0$L$$";

        public LevelSpawnedSentryDefinition Def { get; }

        public int InstanceIndex { get; }

        public StateReplicator<LSSState> StateReplicator { get; private set; }

        public LSSComp LSSComp { get; private set; }

        internal void AssignInstance(LSSComp comp)
        {
            if(!comp.IsSetup)
            {
                EOSLogger.Error($"LSS: Assigning to LSS '{comp.Def.WorldEventObjectFilter}' an un-setup LSSComp!");
                return;
            }

            bool isReassign = LSSComp != null;
            if (isReassign)
            {
                EOSLogger.Warning($"LSS: re-assigning LSSComp to '{comp.Def.WorldEventObjectFilter}'!");
            }

            LSSComp = comp;

            if(isReassign)
            {
                OnStateChanged(StateReplicator.State, StateReplicator.State, false);
            }
        }

        internal void SetupReplicator()
        {
            if (StateReplicator != null) return;
            var def = Def;

            uint sid = EOSNetworking.AllotReplicatorID();
            if (sid != EOSNetworking.INVALID_ID)
            {
                StateReplicator = StateReplicator<LSSState>.Create(sid, def.InitialState, LifeTimeType.Level);
                StateReplicator.OnStateChanged += OnStateChanged;
                //StateReplicator.SetState(def.InitialState);
            }
        }

        private void OnStateChanged(LSSState oldState, LSSState newState, bool isRecall)
        {
            if(isRecall)
            {
                EOSLogger.Warning("LSS.OnStateChanged recall! Gonna re-assign LSSComp!");
                // respawn sentry here
                SpawnLSS();
                return;
            }

            if(LSSComp == null)
            {
                EOSLogger.Error("LSS.OnStateChanged: LSSComp is null!");
            }
            else
            {
                LSSComp.OnStateChanged(oldState, newState, isRecall);
            }
        }

        internal void Destroy()
        {
            EOSLogger.Error($"LSS Destroyed: {Def.WorldEventObjectFilter}");

            LevelAPI.OnBuildDone -= SetupReplicator;
            LevelAPI.OnEnterLevel -= SpawnLSS;

            StateReplicator?.Unload();
            StateReplicator = null;
        }

        private void SpawnLSS()
        {
            if (!SNet.IsMaster) return;

            var def = Def;

            var pos = def.Position.ToVector3();
            if (!AIG_GeomorphNodeVolume.TryGetGeomorphVolume(0, def.DimensionIndex, pos, out var resultingGeoVolume)
                || !resultingGeoVolume.m_voxelNodeVolume.TryGetPillar(pos, out var pillar)
                || !pillar.TryGetVoxelNode(pos.y, out var bestNode)
                || !AIG_NodeCluster.TryGetNodeCluster(bestNode.ClusterID, out var nodeCluster))
            {
                EOSLogger.Error("SpawnLSS: Position is not valid, try again inside an area.");
                return;
            }

            var node = nodeCluster.CourseNode;

            var db = GameDataBlockBase<PlayerOfflineGearDataBlock>.GetBlock(def.PlayerOfflineGearDBId);
            if (db == null)
            {
                EOSLogger.Error($"SpawnLSS: missing PlayerOfflineGearDataBlock with Id {def.PlayerOfflineGearDBId}");
                return;
            }

            var idrange = new GearIDRange(db.GearJSON);

            // For indexing config on sentry instantiation
            idrange.PublicGearName = $"{PUBLIC_NAME_PREFIX}_{InstanceIndex}";

            idrange.PlayfabItemId = db.name;
            idrange.PlayfabItemInstanceId = $"OfflineGear_LSS_ID_{db.persistentID}";
            idrange.OfflineGearType = eOfflineGearType.SpawnedInLevel;

            var itemdb = (idrange.GetCompID(eGearComponent.BaseItem) > 0U) ?
                GameDataBlockBase<ItemDataBlock>.GetBlock(idrange.GetCompID(eGearComponent.BaseItem)) : null;

            ItemReplicationManager.SpawnGear(idrange,
                //new System.Action<ISyncedItem, PlayerAgent>(OnLSSSpawned), 
                null, // Callback cannot be invoked, fk
                ItemMode.Instance,
                pos,
                def.Rotation.ToQuaternion(),
                node,
                null,
                def.InitialAmmo,
                def.AmmoCap);
        }

        internal static LSS Instantiate(LevelSpawnedSentryDefinition def, int instanceIndex)
        {
            var lss = new LSS(def, instanceIndex);
            LevelAPI.OnBuildDone += lss.SetupReplicator;
            LevelAPI.OnEnterLevel += lss.SpawnLSS;
            return lss;
        }

        private LSS(LevelSpawnedSentryDefinition def, int instanceIndex)
        {
            this.Def = def;
            this.InstanceIndex = instanceIndex;
        }
    }
}
