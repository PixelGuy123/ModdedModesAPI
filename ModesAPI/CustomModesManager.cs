using System.Collections.Generic;
using UnityEngine;

namespace ModdedModesAPI.ModesAPI
{
	internal class CustomModesManager : MonoBehaviour
	{
		internal static CustomModesManager AttachToSelectionScreen(Transform parent, bool makePageSystem)
		{
			var man = parent.gameObject.AddComponent<CustomModesManager>();
			man.buttons = new(parent.GetComponentsInChildren<StandardMenuButton>());
			man.buttons.RemoveAll(x => x.name == "BackButton" || x.name == "SeedInput");

			man.available_Positions_For_Each_Screen = new Vector2[man.buttons.Count];
			for (int i = 0; i < man.available_Positions_For_Each_Screen.Length; i++)
				man.available_Positions_For_Each_Screen[i] = man.buttons[i].transform.position;

			return man;
		}

		internal Vector2[] available_Positions_For_Each_Screen;

		internal List<StandardMenuButton> buttons;
	}
}
