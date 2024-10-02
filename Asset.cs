using GTFO.API;
using UnityEngine;
namespace EOSExt.NavigationalSpline
{
    internal static class Asset
    {
        public static GameObject SplineGeneratorGO { get; private set; }

        internal static void Init()
        {
            SplineGeneratorGO = AssetAPI.GetLoadedAsset<GameObject>("Assets/EOSAssets/LG_ChainedPuzzleSplineGenerator.prefab");
        }
    }
}
