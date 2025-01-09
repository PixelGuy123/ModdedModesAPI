using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
//using ModdedModesAPI.ModesAPI;
//using UnityEngine;

namespace ModdedModesAPI.BepInEx
{
    [BepInPlugin(mod_guid, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    internal class ModesPlugin : BaseUnityPlugin
    {
        const string mod_guid = "pixelguy.pixelmodding.baldiplus.moddedmodesapi";
        internal static ManualLogSource logger;
        private void Awake()
        {
            logger = Logger;
			var h = new Harmony(mod_guid);
			h.PatchAll();

			// Funny Experiments
			//CustomModesHandler.OnMainMenuInitialize += () =>
			//{
				
			//};

		}
	}
}
