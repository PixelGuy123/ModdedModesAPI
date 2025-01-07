using HarmonyLib;
using ModdedModesAPI.ModesAPI;
using System.Collections.Generic;

namespace ModdedModesAPI.Patches
{
	[HarmonyPatch(typeof(MainMenu), "Start")]
	internal static class MainMenuPatch
	{
		static void Postfix(MainMenu __instance)
		{
			ModeObject.CreateModeObjectOverExistingScreen(SelectionScreen.MainScreen);
			ModeObject.CreateModeObjectOverExistingScreen(SelectionScreen.ChallengesScreen);

			CustomModesHandler.InvokeMainMenuInit(__instance);
		}
	}
}
