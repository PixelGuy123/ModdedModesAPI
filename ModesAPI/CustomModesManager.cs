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

			ModePage firstPage = new(buttons.Count);
			man.supportsPages = makePageSystem;
			man.pages = [firstPage];

			for (int i = 0; i < firstPage.buttons.Length; i++)
				firstPage.buttons[i] = buttons[i];

			if (positions.Length == 0) // If no position given, just use the positions from the buttons
			{
				man.available_Positions_For_Each_Screen = new Vector2[buttons.Count];
				for (int i = 0; i < man.available_Positions_For_Each_Screen.Length; i++)
					man.available_Positions_For_Each_Screen[i] = buttons[i].transform.localPosition; // I have to explicitly make the Vector2 bc it cannot do when it's a Vector3?
				return man;
			}

			man.available_Positions_For_Each_Screen = positions;
			return man;
		}

		internal void UpdateTogglersYOffset(float offset)
		{
			for (int i = 0; i < pageTogglers.Length; i++)
			{
				var pos = pageTogglers[i].transform.localPosition;
				pos.y = offset;
				pageTogglers[i].transform.localPosition = pos;
			}
		}

		internal void SwitchPage(bool advanceOne)
		{
			for (int i = 0; i < pages[pageIdx].buttons.Length; i++)
				pages[pageIdx].buttons[i].gameObject.SetActive(false);

			pageIdx += advanceOne ? 1 : -1;
			pageIdx %= pages.Count;

			for (int i = 0; i < pages[pageIdx].buttons.Length; i++)
				pages[pageIdx].buttons[i].gameObject.SetActive(true);
		}

		internal void AddButton(StandardMenuButton button)
		{
			for (int i = 0; i < pages.Count; i++)
			{
				for (int x = 0; x < pages[i].buttons.Length; x++)
				{
					if (!pages[i].buttons[x]) // Searches all available slots of each page, to find one that fits
					{
						pages[i].buttons[x] = button;
						button.transform.localPosition = available_Positions_For_Each_Screen[x]; // Each page *must* have the same positions set, that's a general rule
						return;
					}
				}
			}

			if (!supportsPages)
				throw new System.ArgumentOutOfRangeException($"Failed to add a button to the selection screen ({this}) due to the lack of space.");

			var newPage = new ModePage(available_Positions_For_Each_Screen.Length);
			newPage.buttons[0] = button;
			button.transform.localPosition = available_Positions_For_Each_Screen[0];

			pages.Add(newPage);
		}

		int pageIdx = 0;
		internal bool supportsPages = false;

		internal Vector2[] available_Positions_For_Each_Screen;

		List<ModePage> pages;

		internal StandardMenuButton[] pageTogglers;
	}

	internal class ModePage(int buttonsNeeded)
	{
		public StandardMenuButton[] buttons = new StandardMenuButton[buttonsNeeded];
	}
}
