using AIGraph;
using EOSExt.DimensionWarp.Definition;
using ExtraObjectiveSetup.BaseClasses;
using ExtraObjectiveSetup.ExtendedWardenEvents;
using ExtraObjectiveSetup.Utils;
using GameData;
using LevelGeneration;
using Player;
using SNetwork;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using UnityEngine;

namespace EOSExt.DimensionWarp
{
    public class DimensionWarpManager: GenericExpeditionDefinitionManager<DimensionWarpDefinition>
    {
        public static DimensionWarpManager Current { get; private set; }

        private readonly ImmutableList<Vector3> lookDirs = new List<Vector3>() { Vector3.forward, Vector3.back, Vector3.left, Vector3.right }.ToImmutableList();

        protected override string DEFINITION_NAME => "DimensionWarp";

        public AIG_NodeCluster GetNodeFromDimensionPosition(eDimensionIndex dimensionIndex, Vector3 position)
        {
            if (!AIG_GeomorphNodeVolume.TryGetGeomorphVolume(0, dimensionIndex, position, out var resultingGeoVolume)
                || !resultingGeoVolume.m_voxelNodeVolume.TryGetPillar(position, out var pillar)
                || !pillar.TryGetVoxelNode(position.y, out var bestNode)
                || !AIG_NodeCluster.TryGetNodeCluster(bestNode.ClusterID, out var nodeCluster)
                )
            {
                EOSLogger.Error("GetNodeFromDimensionPosition: Position is not valid, try again inside an area.");
                return null;
            }

            return nodeCluster;
        }

        public DimensionWarpDefinition GetWarpDefinition(string worldEventObjectFilter)
        {
            if(GameStateManager.CurrentStateName != eGameStateName.InLevel
                || !definitions.ContainsKey(RundownManager.ActiveExpedition.LevelLayoutData))
            {
                EOSLogger.Error($"GetWarpPositions: Didn't find config with MainLevelLayout {RundownManager.ActiveExpedition.LevelLayoutData}");
                return null;
            }

            var def = definitions[RundownManager.ActiveExpedition.LevelLayoutData].Definitions.Find(def => def.WorldEventObjectFilter == worldEventObjectFilter);
            if(def == null)
            {
                EOSLogger.Error($"GetWarpPositions: Didn't find config for {worldEventObjectFilter} with MainLevelLayout {RundownManager.ActiveExpedition.LevelLayoutData}");
                return new();
            }

            return def;
        }

        public void WarpItem(ItemInLevel item, eDimensionIndex warpToDim, Vector3 warpToPosition)
        {
            if (!SNet.IsMaster || GameStateManager.CurrentStateName != eGameStateName.InLevel) return;

            if (item != null)
            {
                var targetNodeCluster = GetNodeFromDimensionPosition(warpToDim, warpToPosition);
                if (targetNodeCluster != null)
                {
                    item.GetSyncComponent().AttemptPickupInteraction(
                        ePickupItemInteractionType.Place,
                        null,
                        item.pItemData.custom,
                        warpToPosition,
                        Quaternion.identity,
                        targetNodeCluster.CourseNode,
                        true,
                        true);
                }
                else
                {
                    EOSLogger.Error($"WarpTeam: cannot find course node for item to warp");
                }
            }
        }

        public void WarpItemsInZone(eDimensionIndex dimensionIndex, LG_LayerType layer, eLocalZoneIndex localIndex, string worldEventObjectFilter)
        {
            if (!SNet.IsMaster || GameStateManager.CurrentStateName != eGameStateName.InLevel) return;

            PlayerAgent localPlayer = PlayerManager.GetLocalPlayerAgent();
            if (localPlayer == null)
            {
                EOSLogger.Error($"WarpItemsInZone: master - LocalPlayerAgent is null???");
                return;
            }

            var def = GetWarpDefinition(worldEventObjectFilter);
            if(def == null)
            {
                EOSLogger.Error($"WarpItemsInZone: worldEventObjectFilter '{worldEventObjectFilter}' is not defined");
                return;
            }

            var warpLocations = def.Locations;
            if (warpLocations.Count < 1)
            {
                EOSLogger.Error($"WarpItemsInZone: no warp position found");
                return;
            }

            int itemPositionIdx = 0;
            foreach (var warpable in Dimension.WarpableObjects)
            {
                var itemInLevel = warpable.TryCast<ItemInLevel>();
                if (itemInLevel != null)
                {
                    var prevNode = itemInLevel.CourseNode;
                    if(itemInLevel.internalSync.GetCurrentState().placement.droppedOnFloor
                        && prevNode.m_dimension.DimensionIndex == dimensionIndex 
                        && prevNode.LayerType == layer 
                        && prevNode.m_zone.LocalIndex == localIndex
                        
                        && (!def.OnWarp.WarpItemsInZone_OnlyWarpWarpable || itemInLevel.CanWarp)) 
                    {
                        var itemPosition = warpLocations[itemPositionIdx].Position.ToVector3();
                        WarpItem(itemInLevel, dimensionIndex, itemPosition);
                    }
                    itemPositionIdx = (itemPositionIdx + 1) % warpLocations.Count;
                }
            }
        }

        internal void WarpItemsInZone(WardenObjectiveEventData e)
        {
            var dimensionIndex = e.DimensionIndex;
            var layer = e.Layer;
            var localIndex = e.LocalIndex;
            string worldEventObjectFilter = e.WorldEventObjectFilter;
            WarpItemsInZone(dimensionIndex, layer, localIndex, worldEventObjectFilter);
        }

        /// <summary>
        /// WarpAlivePlayersAndItemsInRange
        /// </summary>
        /// <param name="rangeOrigin"></param>
        /// <param name="range"></param>
        /// <param name="worldEventObjectFilter"></param>
        public void WarpRange(Vector3 rangeOrigin, float range, string worldEventObjectFilter) 
        {
            if (GameStateManager.CurrentStateName != eGameStateName.InLevel) return;

            PlayerAgent localPlayer = PlayerManager.GetLocalPlayerAgent();
            if (localPlayer == null)
            {
                EOSLogger.Error($"WarpTeam: LocalPlayerAgent is null");
                return;
            }

            var def = GetWarpDefinition(worldEventObjectFilter);
            if (def == null)
            {
                EOSLogger.Error($"WarpItemsInZone: worldEventObjectFilter '{worldEventObjectFilter}' is not defined");
                return;
            }

            var warpLocations = def.Locations;
            if (warpLocations.Count < 1)
            {
                EOSLogger.Error($"WarpAlivePlayersInRange: no warp locations found");
                return;
            }

            int positionIndex = localPlayer.PlayerSlotIndex % warpLocations.Count;
            var warpPosition = warpLocations[positionIndex].Position.ToVector3();
            int lookDirIndex = warpLocations[positionIndex].LookDir % lookDirs.Count;
            var lookDir = lookDirs[lookDirIndex];

            // warp warpable items within range
            int itemPositionIdx = 0;
            List<SentryGunInstance> sentryGunToWarp = new();
            foreach (var warpable in Dimension.WarpableObjects)
            {
                var sentryGun = warpable.TryCast<SentryGunInstance>();
                if (sentryGun != null)
                {
                    if (sentryGun.LocallyPlaced
                        && sentryGun.Owner.Alive && (rangeOrigin - sentryGun.Owner.Position).magnitude < range // owner is gonna warp
                        && ((rangeOrigin - sentryGun.transform.position).magnitude < range || def.OnWarp.WarpRange_WarpDeployedSentryOutsideRange) // sentry is in the warp range
                        ) 
                    {
                        sentryGunToWarp.Add(sentryGun);
                    }
                    continue;
                }

                if (SNet.IsMaster)
                {
                    var itemInLevel = warpable.TryCast<ItemInLevel>();
                    if (itemInLevel != null && (itemInLevel.transform.position - rangeOrigin).magnitude < range)
                    {
                        var itemPosition = warpLocations[itemPositionIdx].Position.ToVector3();
                        WarpItem(itemInLevel, def.DimensionIndex, itemPosition);
                        itemPositionIdx = (itemPositionIdx + 1) % warpLocations.Count;
                    }

                    continue;
                }
            }

            sentryGunToWarp.ForEach(sentryGun => sentryGun.m_sync.WantItemAction(sentryGun.Owner, SyncedItemAction_New.PickUp));

            if (localPlayer.Alive && (rangeOrigin - localPlayer.Position).magnitude < range)
            {
                // warp player
                if (!localPlayer.TryWarpTo(def.DimensionIndex, warpPosition, lookDir, true))
                {
                    EOSLogger.Error($"WarpAlivePlayersInRange: TryWarpTo failed, Position: {warpPosition}");
                }
            }
        }

        internal void WarpRange(WardenObjectiveEventData e)
        {
            var rangeOrigin = e.Position;
            float range = e.FogTransitionDuration;
            string worldEventObjectFilter = e.WorldEventObjectFilter;

            WarpRange(rangeOrigin, range, worldEventObjectFilter);
        }

        public void WarpTeam(string worldEventObjectFilter)
        {
            if (GameStateManager.CurrentStateName != eGameStateName.InLevel) return;

            PlayerAgent localPlayer = PlayerManager.GetLocalPlayerAgent();
            if (localPlayer == null)
            {
                EOSLogger.Error($"WarpTeam: LocalPlayerAgent is null");
                return;
            }

            var def = GetWarpDefinition(worldEventObjectFilter);
            if (def == null)
            {
                EOSLogger.Error($"WarpTeam: worldEventObjectFilter '{worldEventObjectFilter}' is not defined");
                return;
            }

            var warpLocations = def.Locations;
            if (warpLocations.Count < 1)
            {
                EOSLogger.Error($"WarpTeam: no warp locations found");
                return;
            }

            int positionIndex = localPlayer.PlayerSlotIndex % warpLocations.Count;
            var warpPosition = warpLocations[positionIndex].Position.ToVector3();
            int lookDirIndex = warpLocations[positionIndex].LookDir % lookDirs.Count;
            var lookDir = lookDirs[lookDirIndex];

            int itemPositionIdx = 0;
            List<SentryGunInstance> sentryGunToWarp = new();

            foreach (var warpable in Dimension.WarpableObjects)
            {
                var sentryGun = warpable.TryCast<SentryGunInstance>();
                if (sentryGun != null)
                {
                    if (sentryGun.LocallyPlaced)
                    {
                        sentryGunToWarp.Add(sentryGun);
                    }
                    continue;
                }

                if (SNet.IsMaster && def.OnWarp.WarpTeam_WarpAllWarpableBigPickupItems)
                {
                    var itemInLevel = warpable.TryCast<ItemInLevel>();
                    if (itemInLevel != null && itemInLevel.CanWarp && itemInLevel.internalSync.GetCurrentState().placement.droppedOnFloor)
                    {
                        var itemPosition = warpLocations[itemPositionIdx].Position.ToVector3();
                        WarpItem(itemInLevel, def.DimensionIndex, itemPosition);
                        EOSLogger.Warning($"{itemInLevel.PublicName}");

                        itemPositionIdx = (itemPositionIdx + 1) % warpLocations.Count;
                        continue;
                    }
                }
            }

            sentryGunToWarp.ForEach(sentryGun => sentryGun.m_sync.WantItemAction(sentryGun.Owner, SyncedItemAction_New.PickUp));

            if (!localPlayer.TryWarpTo(def.DimensionIndex, warpPosition, lookDir, true))
            {
                EOSLogger.Error($"WarpTeam: TryWarpTo failed. Position: {warpPosition}, playerSlotIndex: {localPlayer.PlayerSlotIndex}, warpLocationIndex: {positionIndex}");
            }
        }

        internal void WarpTeam(WardenObjectiveEventData e)
        {
            WarpTeam(e.WorldEventObjectFilter);
        }

        public enum WardenEvents_Warp
        {
            WarpTeam = 160,
            WarpRange = 161,
            WarpItemsInZone = 162,
        }

        private DimensionWarpManager() 
        {
            EOSWardenEventManager.Current.AddEventDefinition(WardenEvents_Warp.WarpTeam.ToString(), (uint)WardenEvents_Warp.WarpTeam, WarpTeam);
            EOSWardenEventManager.Current.AddEventDefinition(WardenEvents_Warp.WarpRange.ToString(), (uint)WardenEvents_Warp.WarpRange, WarpRange);
            EOSWardenEventManager.Current.AddEventDefinition(WardenEvents_Warp.WarpItemsInZone.ToString(), (uint)WardenEvents_Warp.WarpItemsInZone, WarpItemsInZone);
        }

        static DimensionWarpManager()
        {
            Current = new();
        }
    }
}
