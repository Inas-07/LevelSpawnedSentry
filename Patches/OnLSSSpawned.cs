using ExtraObjectiveSetup.Utils;
using HarmonyLib;

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
            if (!publicName.StartsWith(LevelSpawnedSentryManager.PUBLIC_NAME_PREFIX))
            {
                return;
            }

            var expDef = LevelSpawnedSentryManager.Current.ExpeditionDefinitions;
            if (!int.TryParse(publicName.Split('_')[1], out int instanceIndex) || instanceIndex < 0 || instanceIndex >= expDef.Count)
            {
                EOSLogger.Error($"SentryGunInstance.OnSpawn: got publicName '{publicName}' with in valid instanceIndex");
                return;
            }

            var def = expDef[instanceIndex];

            var pickupInt = __instance.PickupInteraction.Cast<Interact_Timed>();
            pickupInt.enabled = false;
            //__instance.LocallyPlaced = true;

            __instance.gameObject.AddComponent<LSS>().Setup(def);
            LevelSpawnedSentryManager.Current.RegisterLSS(def.WorldEventObjectFilter, __instance);
        }
    }
}
