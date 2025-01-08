using HarmonyLib;
using ModdedModesAPI.BepInEx;
using ModdedModesAPI.ModesAPI;
using System.Linq;
using UnityEngine;

namespace ModdedModesAPI.Patches
{
	[HarmonyPatch(typeof(MainMenu), "Start")]
	internal static class MainMenuPatch
	{
		static void Postfix(MainMenu __instance)
		{
			if (ResourceStorage.togglersSheet == null)
			{
				ResourceStorage.togglersSheet = new Sprite[4];
				for (int i = 0; i < 4; i++)
				{
					string name = "MenuArrowSheet_" + i;
					ResourceStorage.togglersSheet[i] = Resources.FindObjectsOfTypeAll<Sprite>().First(x => x.GetInstanceID() > 0 && x.name == name);
				}
			}

			ModeObject.CreateModeObjectOverExistingScreen(SelectionScreen.MainScreen);
			ModeObject.CreateModeObjectOverExistingScreen(SelectionScreen.ChallengesScreen).SetThePageButtonsAxis(new(195f, 75f));

			CustomModesHandler.InvokeMainMenuInit(__instance);

			CustomModesHandler.existingModeObjects.Clear(); // Clears out since no ModeObject should be instantiated after the invoke
		}
	}
}
