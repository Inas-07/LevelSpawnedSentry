using ExtraObjectiveSetup.BaseClasses;
using ExtraObjectiveSetup.ExtendedWardenEvents;
using ExtraObjectiveSetup.Utils;
using GTFO.API;
using System.Collections.Generic;
using EOSExt.LevelSpawnedSentry.Definition;
using GameData;
using Gear;
using AIGraph;
using SNetwork;

namespace EOSExt.LevelSpawnedSentry
{
    public partial class LevelSpawnedSentryManager: GenericExpeditionDefinitionManager<LevelSpawnedSentryDefinition>
    {
        public const string PUBLIC_NAME_PREFIX = "E0$L$$";

        public static LevelSpawnedSentryManager Current { get; private set; } = new();

        protected override string DEFINITION_NAME => "LevelSpawnedSentry";

        private Dictionary<string, SentryGunInstance> LSS { get; } = new();

        // For indexing the config on sentry instantiation
        // Looks ugly? Someone help me figure out a better way.
        internal List<LevelSpawnedSentryDefinition> ExpeditionDefinitions { get; } = new();

        public void RegisterLSS(string worldEventObjectFilter, SentryGunInstance instance)
        {
            if(LSS.ContainsKey(worldEventObjectFilter))
            {
                EOSLogger.Error($"RegisterLSS: duplicate LSS '{worldEventObjectFilter}'");
            }

            LSS[worldEventObjectFilter] = instance;
        }

        private void Build(LevelSpawnedSentryDefinition def, int instanceIndex)
        {
            var pos = def.Position.ToVector3();
            if (!AIG_GeomorphNodeVolume.TryGetGeomorphVolume(0, def.DimensionIndex, pos, out var resultingGeoVolume)
                || !resultingGeoVolume.m_voxelNodeVolume.TryGetPillar(pos, out var pillar)
                || !pillar.TryGetVoxelNode(pos.y, out var bestNode)
                || !AIG_NodeCluster.TryGetNodeCluster(bestNode.ClusterID, out var nodeCluster))
            {
                EOSLogger.Error("SpawnSentry: Position is not valid, try again inside an area.");
                return;
            }

            var node = nodeCluster.CourseNode;

            var db = GameDataBlockBase<PlayerOfflineGearDataBlock>.GetBlock(def.PlayerOfflineGearDBId);
            if (db == null)
            {
                EOSLogger.Error($"LevelSpawnedSentryManager: missing PlayerOfflineGearDataBlock with Id {def.PlayerOfflineGearDBId}");
                return;
            }

            var idrange = new GearIDRange(db.GearJSON);

            // For indexing config on sentry instantiation
            idrange.PublicGearName = $"{PUBLIC_NAME_PREFIX}_{instanceIndex}"; 

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


        private void BuildDefinitions()
        {
            if (!SNet.IsMaster) return;
            for(int instanceIndex = 0; instanceIndex < ExpeditionDefinitions.Count; instanceIndex++)
            {
                Build(ExpeditionDefinitions[instanceIndex], instanceIndex);
            }
        }

        private void PrepareForBuild()
        {
            Clear();
            if (definitions.TryGetValue(CurrentMainLevelLayout, out var defs))
            {
                ExpeditionDefinitions.AddRange(defs.Definitions);
            }
        }

        private void Clear()
        {
            LSS.Clear();
            ExpeditionDefinitions.Clear();
        }

        private LevelSpawnedSentryManager() 
        {
            LevelAPI.OnEnterLevel += BuildDefinitions;
            LevelAPI.OnBuildStart += PrepareForBuild;
            LevelAPI.OnLevelCleanup += Clear;
        }

        static LevelSpawnedSentryManager()
        {
            EOSWardenEventManager.Current.AddEventDefinition(LSS_WardenEvents.ChangeLSSState.ToString(), (uint)LSS_WardenEvents.ChangeLSSState, ChangeLSSState);
        }
    }
}
