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

			CustomModesHandler.OnMainMenuInitialize += () =>
			{
				var newScreen = ModeObject.CreateBlankScreenInstance("TestScreen", false, new(-100f, 50f), new(100f, 50f));

				newScreen.StandardButtonBuilder.CreateBlankButton("TestButton")
				.AddTextVisual("I\'m a test button!", out _);
				newScreen.StandardButtonBuilder.CreateBlankButton("TestButton2")
				.AddTextVisual("I\'m a test button!", out _);

				newScreen.StandardButtonBuilder.CreateSeedInput(out _);

				// ------------- Main screen test ---------------
				var modeObj = ModeObject.CreateModeObjectOverExistingScreen(SelectionScreen.MainScreen);

				var but = modeObj.StandardButtonBuilder.CreateTransitionButton(newScreen)
				.AddTextVisual("Vfx_PRI_60", out _); // Testing text property

				modeObj.StandardButtonBuilder.AddDescriptionText(but, "This is a test description. Used to see if it actually works!");

				// ---------- Challenges Screen Test -------------
				modeObj = ModeObject.CreateModeObjectOverExistingScreen(SelectionScreen.ChallengesScreen);

				but = modeObj.StandardButtonBuilder.CreateBlankButton("Something")
				.AddTextVisual("Vfx_PRI_60", out _); // Testing text property

				modeObj.StandardButtonBuilder.AddDescriptionText(but, "This is a test description. Used to see if it actually works!");
				modeObj.StandardButtonBuilder.AddTooltipAnimation(but, "This is a test description.\nUsed to see if it actually works!");

			};
        }
    }
}
