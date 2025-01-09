using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using ModdedModesAPI.ModesAPI;
using System.Linq;
using UnityEngine;

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
			//	var newScreen = ModeObject.CreateBlankScreenInstance("blankScreen", false, Vector3.zero);
			//	newScreen.StandardButtonBuilder.CreateSeedInput(out _);
			//	newScreen.Background = Resources.FindObjectsOfTypeAll<Sprite>().First(x => x.name == "ClipBoard_Full");

			//	newScreen.StandardButtonBuilder.CreateImage(newScreen.Background, new(-35f, -55f), Vector2.one * 32f);

			//	ModeObject.CreateModeObjectOverExistingScreen(SelectionScreen.MainScreen)
			//	.StandardButtonBuilder.CreateTransitionButton(newScreen).AddTextVisual("Cool", out _);
			//};

		}
	}
}
