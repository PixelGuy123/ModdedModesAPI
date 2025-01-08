using System.Collections.Generic;
using UnityEngine;

namespace ModdedModesAPI.ModesAPI
{
	internal class CustomModesManager : MonoBehaviour
	{
		internal static CustomModesManager AttachToSelectionScreen(ModeObject modeObject, bool makePageSystem, params Vector2[] positions)
		{
			var man = modeObject.ScreenTransform.gameObject.AddComponent<CustomModesManager>();

			List<StandardMenuButton> buttons = new(modeObject.ScreenTransform.GetComponentsInChildren<StandardMenuButton>());
			buttons.RemoveAll(x => x.name == "BackButton" || x.name == "SeedInput"); // List all buttons except the ones that aren't "real" buttons

			man.supportsPages = makePageSystem;
			ModePage firstPage;
			

			if (positions.Length == 0) // If no position given, just use the positions from the buttons
			{
				firstPage = new(buttons.Count);
				man.pages = [firstPage];

				man.available_Positions_For_Each_Screen = new Vector2[buttons.Count];

				for (int i = 0; i < man.available_Positions_For_Each_Screen.Length; i++)
					man.available_Positions_For_Each_Screen[i] = buttons[i].transform.localPosition;

				for (int i = 0; i < firstPage.uiElements.Length; i++)
					firstPage.uiElements[i] = buttons[i].gameObject;

				return man;
			}

			firstPage = new(positions.Length);
			man.pages = [firstPage];

			man.available_Positions_For_Each_Screen = positions;

			return man;
		}

		internal void UpdateTogglersOffset(Vector2 offset)
		{
			// It expects two pageTogglers only (0 is left, 1 is right)
			pageTogglers[0].transform.localPosition = offset;

			offset.x = -offset.x;
			pageTogglers[1].transform.localPosition = offset;
		}

		internal void SwitchPage(bool advanceOne)
		{
			for (int i = 0; i < pages[pageIdx].uiElements.Length; i++)
			{
				var transform = pages[pageIdx].uiElements[i];
				if (transform)
					transform.gameObject.SetActive(false);
			}

			pageIdx += advanceOne ? 1 : -1;
			pageIdx = pageIdx < 0 ? pages.Count - 1 : pageIdx % pages.Count;

			for (int i = 0; i < pages[pageIdx].uiElements.Length; i++)
			{
				var transform = pages[pageIdx].uiElements[i];
				if (transform)
					transform.gameObject.SetActive(true);
			}
		}

		internal void AddButton(Transform uiObj)
		{
			for (int i = 0; i < pages.Count; i++)
			{
				for (int x = 0; x < pages[i].uiElements.Length; x++)
				{
					if (!pages[i].uiElements[x]) // Searches all available slots of each page, to find one that fits
					{
						pages[i].uiElements[x] = uiObj.gameObject;
						uiObj.localPosition = available_Positions_For_Each_Screen[x]; // Each page *must* have the same positions set, that's a general rule
						uiObj.gameObject.SetActive(i == pageIdx);
						return;
					}
				}
			}

			if (!supportsPages)
				throw new System.ArgumentOutOfRangeException($"Failed to add a button to the selection screen ({this}) due to the lack of space.");

			var newPage = new ModePage(available_Positions_For_Each_Screen.Length);
			newPage.uiElements[0] = uiObj.gameObject;
			uiObj.localPosition = available_Positions_For_Each_Screen[0];

			pages.Add(newPage);
			uiObj.gameObject.SetActive((pages.Count - 1) == pageIdx);
		}

		int pageIdx = 0;
		internal bool supportsPages = false;

		internal Vector2[] available_Positions_For_Each_Screen;

		List<ModePage> pages;

		internal StandardMenuButton[] pageTogglers;
	}

	internal class ModePage(int buttonsNeeded)
	{
		public GameObject[] uiElements = new GameObject[buttonsNeeded];
	}
}
