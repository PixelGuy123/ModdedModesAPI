﻿using System;
using System.Collections.Generic;

namespace ModdedModesAPI.ModesAPI
{
	/// <summary>
	/// Main class of the API. Which comes with a few methods that you'll need to use, in order to create your custom mode.
	/// </summary>
	public static class CustomModesHandler
	{
		/// <summary>
		/// An event that expects every receiver to create their own screens or buttons. Use the <see cref="ModeObject"/> static constructors to make your own mode/screen/buttons from it.
		/// </summary>
		public static event Action<MainMenu> OnMainMenuInitialize;

		internal static void InvokeMainMenuInit(MainMenu menu)
		{
			existingModeObjects.Clear();
			OnMainMenuInitialize?.Invoke(menu);
		}

		internal static List<ModeObject> existingModeObjects = [];
	}
}