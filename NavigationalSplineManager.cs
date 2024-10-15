using AssetShards;
using ChainedPuzzles;
using EOSExt.NavigationalSpline.Definition;
using EOSExt.NavigationalSpline;
using ExtraObjectiveSetup.BaseClasses;
using ExtraObjectiveSetup.ExtendedWardenEvents;
using ExtraObjectiveSetup.Utils;
using FluffyUnderware.Curvy;
using FluffyUnderware.Curvy.Generator.Modules;
using GTFO.API;
using System.Collections.Generic;
using UnityEngine;

namespace EOSExt.NavigationalSpline
{
    public class NavigationalSplineManager: GenericExpeditionDefinitionManager<NavigationalSplineDefinition>
    {
        public static NavigationalSplineManager Current { get; private set; }

        protected override string DEFINITION_NAME => "NavigationalSpline";

        private Dictionary<string, GameObject> SplineGroups { get; } = new();

        private void InstantiateSplines()
        {
            if (!definitions.TryGetValue(RundownManager.ActiveExpedition.LevelLayoutData, out var def)) return;

            foreach (var groupDef in def.Definitions)
            {
                if (SplineGroups.ContainsKey(groupDef.WorldEventObjectFilter))
                {
                    EOSLogger.Error($"NavigationalSplineManager: duplicate 'WorldEventObjectFilter': {groupDef.WorldEventObjectFilter}, won't build");
                    continue;
                }

                GameObject go = new GameObject($"NavigationalSpline_{groupDef.WorldEventObjectFilter}");

                for (int i = 0; i < groupDef.Splines.Count; i++ ) 
                {
                    var splineDef = groupDef.Splines[i];
                    GameObject splineGO = new GameObject($"NavigationalSpline_{groupDef.WorldEventObjectFilter}_{i}");
                    splineGO.transform.SetParent(go.transform);

                    var spline = splineGO.AddComponent<CP_Holopath_Spline>();

                    spline.m_splineGeneratorPrefab = Asset.SplineGeneratorGO; // spline.Setup will do the copy-instantiation 
                    spline.Setup(false);
                    spline.GeneratePath(splineDef.From.ToVector3(), splineDef.To.ToVector3());
                    if(groupDef.RevealSpeedMulti > 0f)
                    {
                        spline.m_revealSpeed *= groupDef.RevealSpeedMulti;
                    }
                }

                SplineGroups[groupDef.WorldEventObjectFilter] = go;
            }
        }

        internal void ToggleSplineState(string WorldEventObjectFilter, int state)
        {
            if(!SplineGroups.TryGetValue(WorldEventObjectFilter, out var splineGroupGO))
            {
                EOSLogger.Error($"NavigationalSplineManager: cannot find Spline Group with name '{WorldEventObjectFilter}"); 
                return;
            }

            switch (state)
            {
                case 0: // activate 
                    for(int i = 0; i < splineGroupGO.transform.childCount; i++)
                    {
                        var spline = splineGroupGO.transform.GetChild(i).gameObject.GetComponent<CP_Holopath_Spline>();
                        if(spline != null)
                        {
                            spline.SetSplineProgress(0);
                            spline.Reveal(0);
                        }
                    }

                    break;

                case 1: // instant reveal
                    for (int i = 0; i < splineGroupGO.transform.childCount; i++)
                    {
                        var spline = splineGroupGO.transform.GetChild(i).gameObject.GetComponent<CP_Holopath_Spline>();
                        if (spline != null)
                        {
                            spline.SetSplineProgress(0.95f);
                            spline.Reveal(0);
                        }
                    }
                    break;
                case 2: // deactivate
                    for (int i = 0; i < splineGroupGO.transform.childCount; i++)
                    {
                        var spline = splineGroupGO.transform.GetChild(i).gameObject.GetComponent<CP_Holopath_Spline>();
                        if (spline != null)
                        {
                            spline.SetVisible(false);
                        }
                    }
                    break;
            }
        }

        private void Clear()
        {
            foreach (var group in SplineGroups.Values) 
            {
                GameObject.Destroy(group);
            }

            SplineGroups.Clear();
        }

        public enum WardenEvents_NavigationalSpline
        {
            ToggleSplineState = 610,
        }

        private NavigationalSplineManager() 
        {
            LevelAPI.OnBuildDone += InstantiateSplines;
            LevelAPI.OnBuildStart += Clear;
            //LevelAPI.OnBuildStart += FindSplineGeneratorGO;
            LevelAPI.OnLevelCleanup += Clear;

            EOSWardenEventManager.Current.AddEventDefinition(
                WardenEvents_NavigationalSpline.ToggleSplineState.ToString(),
                (int)WardenEvents_NavigationalSpline.ToggleSplineState,

                (e) => {
                    ToggleSplineState(e.WorldEventObjectFilter, e.Count);
                }
            );
        }

        //public static bool TrySetupCurvyComponents(GameObject go, out GameObject splineGenerator, out BuildShapeExtrusion curvyExtrusion, out CurvySpline curvySpline)
        //{
        //    splineGenerator = null;
        //    curvyExtrusion = null;
        //    curvySpline = null;
        //    if (SplineGeneratorGO == null) return false;

        //    splineGenerator = GameObject.Instantiate(SplineGeneratorGO, go.transform);
        //    curvyExtrusion = splineGenerator.GetComponentInChildren<BuildShapeExtrusion>();
        //    curvySpline = splineGenerator.GetComponentInChildren<CurvySpline>();

        //    return true;
        //}

        static NavigationalSplineManager()
        {
            Current = new();
            AssetShardManager.add_OnStartupAssetsLoaded(new System.Action(Asset.Init));
        }
    }
}
