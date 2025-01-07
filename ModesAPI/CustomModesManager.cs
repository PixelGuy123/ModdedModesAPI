using System.Collections.Generic;
using UnityEngine;

namespace ModdedModesAPI.ModesAPI
{
	internal class CustomModesManager : MonoBehaviour
	{
		internal static CustomModesManager AttachToSelectionScreen(ModeObject modeObject, bool makePageSystem)
		{
			var man = modeObject.ScreenTransform.gameObject.AddComponent<CustomModesManager>();

			List<StandardMenuButton> buttons = new(modeObject.ScreenTransform.GetComponentsInChildren<StandardMenuButton>());
			buttons.RemoveAll(x => x.name == "BackButton" || x.name == "SeedInput");

			ModePage firstPage = new(buttons.Count);

			for (int i = 0; i < firstPage.buttons.Length; i++)
				firstPage.buttons[i] = buttons[i];
			

			man.available_Positions_For_Each_Screen = new Vector2[buttons.Count];
			for (int i = 0; i < man.available_Positions_For_Each_Screen.Length; i++)
				man.available_Positions_For_Each_Screen[i] = buttons[i].transform.position;

			if (makePageSystem)
			{
				man.supportsPages = true;
				// .. Create page togglers

				//for (int i = 0; i < )

				return man;
			}

			man.pages = [firstPage];

			return man;
		}

		internal void UpdateTogglersYOffset(float offset)
		{
			for (int i = 0; i < pageTogglers.Length; i++)
			{
				var pos = pageTogglers[i].transform.position;
				pos.y = offset;
				pageTogglers[i].transform.position = pos;
			}
		}

		internal void SwitchPage(bool advanceOne)
		{
			// .. Disable buttons that aren't from the current page (previous page and current page pattern)
			// .. Loop around the pageIdx
		}

		internal void AddButton(StandardMenuButton button)
		{
			for (int i = 0; i < pages.Count; i++)
			{
				for (int x = 0; x < pages[i].buttons.Length; x++)
				{
					if (!pages[i].buttons[x])
					{
						pages[i].buttons[x] = button;
						button.transform.position = available_Positions_For_Each_Screen[x];
						return;
					}
				}
			}

			if (!supportsPages)
				throw new System.ArgumentOutOfRangeException($"Failed to add a button to the selection screen ({this}) due to the lack of space.");

			var newPage = new ModePage(available_Positions_For_Each_Screen.Length);
			newPage.buttons[0] = button;
			button.transform.position = available_Positions_For_Each_Screen[0];

			pages.Add(newPage);
		}

		int pageIdx = 0;
		bool supportsPages = false;

		internal Vector2[] available_Positions_For_Each_Screen;

		List<ModePage> pages;

		StandardMenuButton[] pageTogglers;
	}

	internal class ModePage(int buttonsNeeded)
	{
		public StandardMenuButton[] buttons = new StandardMenuButton[buttonsNeeded];
	}
}
