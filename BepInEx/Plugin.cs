using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using ModdedModesAPI.ModesAPI;
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
			//	var newNewScreen = ModeObject.CreateBlankScreenInstance("CoolerScreen", true, Vector2.up * 70, Vector2.up * 35f, Vector2.zero, Vector2.down * 35f, Vector2.down * 70f);
			//	for (int i = 0; i < 12; i++)
			//		newNewScreen.StandardButtonBuilder.CreateBlankButton("BlankButton_" + i)
			//		.AddTextVisual("I am a button " + i, out _);

			//	var newScreen = ModeObject.CreateBlankScreenInstance("CoolScreen", true, Vector2.up * 50f, Vector2.zero, Vector2.down * 50f);
			//	for (int i = 0; i < 6; i++)
			//		newScreen.StandardButtonBuilder.CreateBlankButton("BlankButton_" + i)
			//		.AddTextVisual("I am a button " + i, out _);

			//	var coolButton = newScreen.StandardButtonBuilder.CreateTransitionButton(newNewScreen).AddTextVisual("Go to funny screen twoo", out _);

			//	var modeObj = ModeObject.CreateModeObjectOverExistingScreen(SelectionScreen.MainScreen);
			//	coolButton = modeObj.StandardButtonBuilder.CreateTransitionButton(newScreen).AddTextVisual("Go to funny screen", out _);
			//};

		}
    }
}
