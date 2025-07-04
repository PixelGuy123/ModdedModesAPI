using System.Collections.Generic;
using UnityEngine;

namespace ModdedModesAPI.ModesAPI
{
	internal class CustomModesManager : MonoBehaviour
	{
		internal static CustomModesManager AttachToSelectionScreen(ModeObject modeObject, bool makePageSystem, params Vector2[] positions)
		{
			var man = modeObject.ScreenTransform.gameObject.AddComponent<CustomModesManager>();

			List<StandardMenuButton> buttons = [.. modeObject.ScreenTransform.GetComponentsInChildren<StandardMenuButton>()];
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

				man.buttonSizes = new float[buttons.Count];

				for (int i = 0; i < man.buttonSizes.Length; i++)
					if (buttons[i].transform is RectTransform rect)
						man.buttonSizes[i] = rect.sizeDelta.y;



				for (int i = 0; i < firstPage.buttons.Length; i++)
					firstPage.buttons[i] = buttons[i];

				return man;
			}

			firstPage = new(positions.Length);
			man.pages = [firstPage];

			man.available_Positions_For_Each_Screen = positions;

			return man;
		}

		void OnEnable()
		{
			for (int i = 0; i < pages.Count; i++)
			{
				bool isMyPage = i == pageIdx;
				for (int x = 0; x < pages[i].buttons.Length; x++)
					pages[i].buttons[x]?.gameObject.SetActive(isMyPage); // To avoid buttons just getting enabled randomly (main menu has that bc of the save system)
			}
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
			for (int i = 0; i < pages[pageIdx].buttons.Length; i++)
			{
				var transform = pages[pageIdx].buttons[i];
				if (transform)
					transform.gameObject.SetActive(false);
			}

			pageIdx += advanceOne ? 1 : -1;
			pageIdx = pageIdx < 0 ? pages.Count - 1 : pageIdx % pages.Count;

			for (int i = 0; i < pages[pageIdx].buttons.Length; i++)
			{
				var transform = pages[pageIdx].buttons[i];
				if (transform)
				{
					transform.gameObject.SetActive(true);
					if (buttonSizes != null && transform.transform is RectTransform rect) // at least forcefully change the y size, so the alignment is consistent
					{
						Vector2 size = rect.sizeDelta;
						size.y = buttonSizes[i];
						rect.sizeDelta = size;
					}
				}
			}
		}

		internal void AddButton(StandardMenuButton but)
		{
			for (int i = 0; i < pages.Count; i++)
			{
				for (int x = 0; x < pages[i].buttons.Length; x++)
				{
					if (!pages[i].buttons[x]) // Searches all available slots of each page, to find one that fits
					{
						pages[i].buttons[x] = but;
						but.transform.localPosition = available_Positions_For_Each_Screen[x]; // Each page *must* have the same positions set, that's a general rule
						but.gameObject.SetActive(i == pageIdx);
						return;
					}
				}
			}

			if (!supportsPages)
				throw new System.ArgumentOutOfRangeException($"Failed to add a button to the selection screen ({this}) due to the lack of space.");

			var newPage = new ModePage(available_Positions_For_Each_Screen.Length);
			newPage.buttons[0] = but;
			but.transform.localPosition = available_Positions_For_Each_Screen[0];


			pages.Add(newPage);
			but.gameObject.SetActive((pages.Count - 1) == pageIdx);
		}

		int pageIdx = 0;
		internal bool supportsPages = false;

		internal Vector2[] available_Positions_For_Each_Screen;

		internal float[] buttonSizes;

		List<ModePage> pages;

		internal StandardMenuButton[] pageTogglers;
	}

	internal class ModePage(int buttonsNeeded)
	{
		public StandardMenuButton[] buttons = new StandardMenuButton[buttonsNeeded];
	}
}
