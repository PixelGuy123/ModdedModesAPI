using HarmonyLib;
using ModdedModesAPI.BepInEx;
using ModdedModesAPI.ModesAPI;
using System.Linq;
using TMPro;
using UnityEngine;

namespace ModdedModesAPI.Patches
{
	[HarmonyPatch(typeof(MainMenu), "Start")]
	internal static class MainMenuPatch
	{
		static void Postfix()
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

			if (ResourceStorage.backArrowSheet == null)
			{
				ResourceStorage.backArrowSheet = new Sprite[2];
				for (int i = 0; i < 2; i++)
				{
					string name = "BackArrow_" + i;
					ResourceStorage.backArrowSheet[i] = Resources.FindObjectsOfTypeAll<Sprite>().First(x => x.GetInstanceID() > 0 && x.name == name);
				}
			}

			if (!ResourceStorage.cursorPre)
				ResourceStorage.cursorPre = Resources.FindObjectsOfTypeAll<CursorController>().First(x => x.GetInstanceID() > 0);

			if (!ResourceStorage.bottomPre)
				ResourceStorage.bottomPre = Resources.FindObjectsOfTypeAll<RectTransform>().First(x => x.GetInstanceID() > 0 && x.name == "Bottom");

			if (!ResourceStorage.tooltipBase)
				ResourceStorage.tooltipBase = Resources.FindObjectsOfTypeAll<RectTransform>().First(x => x.GetInstanceID() > 0 && x.name == "TooltipBase");

			var mod = ModeObject.CreateModeObjectOverExistingScreen(SelectionScreen.MainScreen);
			mod.IsLinked = true;
			mod.allowedToChangeDescriptionText = false;
			mod.HasSeedInput = true;
			mod.descriptionTextRef = mod.ScreenTransform.Find("ModeText").GetComponent<TextMeshProUGUI>();

			mod = ModeObject.CreateModeObjectOverExistingScreen(SelectionScreen.ChallengesScreen);
			mod.SetThePageButtonsAxis(new(195f, 75f));
			mod.allowedToChangeDescriptionText = false;
			mod.IsLinked = true;
			mod.HasSeedInput = true; // To avoid making one, there's no point for it in there - make your own screen if you wanna add a challenge with this functionality.
			mod.descriptionTextRef = mod.ScreenTransform.Find("ModeText").GetComponent<TextMeshProUGUI>();

			CustomModesHandler.InvokeMainMenuInit();

			CustomModesHandler.existingModeObjects.Clear(); // Clears out since no ModeObject should be instantiated after the invoke
		}
	}
}
