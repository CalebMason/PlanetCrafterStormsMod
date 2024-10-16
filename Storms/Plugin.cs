using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using SpaceCraft;
using System.Collections;
using UnityEngine;

namespace Storms
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        static ConfigEntry<bool> modEnabled;
        static ManualLogSource logger;

        private Storm storm;

        private void Awake()
        {
            Logger.LogInfo($"Plugin loading..");

            modEnabled = Config.Bind("General", "Enabled", true, "Is the mod enabled?");
            logger = Logger;

            Harmony.CreateAndPatchAll(typeof(Plugin));
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlanetLoader), "HandleDataAfterLoad")]
        static void Patch_PlanetLoader_HandleDataAfterLoad(PlanetLoader __instance)
        {
            __instance.StartCoroutine(WaitForProceduralInstances(__instance));
        }

        static IEnumerator WaitForProceduralInstances(PlanetLoader __instance)
        {
            while (!__instance.GetIsLoaded())
            {
                yield return null;
            }

            logger.LogInfo("Starting Storms");
            Plugin pluginInstance = BepInEx.Bootstrap.Chainloader.PluginInfos[PluginInfo.PLUGIN_GUID].Instance as Plugin;
            if (pluginInstance != null)
            {
                // Initialize the storm with a position and logger
                Vector3 stormPosition = new Vector3(0, 60, 0);
                Vector3 stormVelocity = new Vector3(10, 0, 10); // Move in the positive Z direction
                pluginInstance.storm = new Storm(stormPosition, stormVelocity, logger);
                __instance.StartCoroutine(pluginInstance.storm.Start());
            }
            else
            {
                logger.LogError("Failed to get plugin instance.");
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlayerMovable), nameof(PlayerMovable.UpdatePlayerMovement))]
        static void UpdatePlayerMovement(PlayerMovable __instance)
        {
            if (__instance.RunSpeed != 81)
            {
                __instance.RunSpeed = 81;
                logger.LogInfo($"Movement speed: {__instance.RunSpeed}");
            }
        }
    }
}
