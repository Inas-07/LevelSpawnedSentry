using ExtraObjectiveSetup.Utils;
using HarmonyLib;
using LevelGeneration;

namespace EOSExt.LevelSpawnedSentry.Patches
{
    [HarmonyPatch]
    internal static class OnLSSSpawned
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(SentryGunInstance), nameof(SentryGunInstance.OnSpawn))]
        private static void Post_(SentryGunInstance __instance, pGearSpawnData spawnData)
        {
            var publicName = spawnData.gearIDRange.publicName.data;
            if (!publicName.StartsWith(LSS.PUBLIC_NAME_PREFIX))
            {
                return;
            }

            var instances = LevelSpawnedSentryManager.Current.LSSInstances;
            if (!int.TryParse(publicName.Split('_')[1], out int instanceIndex) || instanceIndex < 0 || instanceIndex >= instances.Count)
            {
                EOSLogger.Error($"SentryGunInstance.OnSpawn: got publicName '{publicName}' with in valid instanceIndex");
                return;
            }

            var lss = instances[instanceIndex];

            LevelSpawnedSentryManager.Current.LSSInstances[instanceIndex].AssignInstance(__instance.gameObject.AddComponent<LSSComp>().Setup(lss));

            var pickupInt = __instance.PickupInteraction.Cast<Interact_Timed>();
            pickupInt.enabled = false;

            Dimension.RemoveWarpable(__instance.Cast<IWarpableObject>());
        }
    }
}
