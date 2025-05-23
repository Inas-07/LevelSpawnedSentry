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
using System.Linq;

namespace EOSExt.LevelSpawnedSentry
{
    public partial class LevelSpawnedSentryManager: GenericExpeditionDefinitionManager<LevelSpawnedSentryDefinition>
    {
        public static LevelSpawnedSentryManager Current { get; private set; } = new();

        protected override string DEFINITION_NAME => "LevelSpawnedSentry";

        private Dictionary<string, LSS> LSSDict { get; } = new();

        internal List<LSS> LSSInstances { get; } = new();

        private void PrepareForBuild()
        {
            Clear();
            if (definitions.TryGetValue(CurrentMainLevelLayout, out var defs))
            {
                for(int i = 0; i < defs.Definitions.Count; i++)
                {
                    var def = defs.Definitions[i];
                    var lss = LSS.Instantiate(def, i);

                    LSSInstances.Add(lss);
                    if(LSSDict.ContainsKey(def.WorldEventObjectFilter))
                    {
                        // TODO: maybe auto-rename it?
                        EOSLogger.Warning($"LevelSpawnSentry: found duplicate WorldEventObjectFilter '{def.WorldEventObjectFilter}'");
                    }

                    LSSDict[def.WorldEventObjectFilter] = lss;
                }
            }
        }

        private void Clear()
        {
            LSSInstances.ForEach(i => i.Destroy());
            LSSInstances.Clear(); 
            LSSDict.Clear();
        }

        private LevelSpawnedSentryManager() 
        {
            /*
             Build step:
            1. PrepareForBuild
            2. OnBuildDone: LSS.SetupReplicator
            3.1 OnEnterLevel: SpawnLSS (but we cannot get the SentryGunInstance there)
            3.2 Patches.OnLSSSpawned: get the SentryGunInstance and do setup there (should also handle recall)
            */
            LevelAPI.OnBuildStart += PrepareForBuild;
            LevelAPI.OnLevelCleanup += Clear;
        }

        static LevelSpawnedSentryManager()
        {
            EOSWardenEventManager.Current.AddEventDefinition(LSS_WardenEvents.ChangeLSSState.ToString(), (uint)LSS_WardenEvents.ChangeLSSState, ChangeLSSState);
        }
    }
}
