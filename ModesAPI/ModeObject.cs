using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ModdedModesAPI.ModesAPI
{
	/// <summary>
	/// This class stores the data of the screen assigned to it.
	/// </summary>
	public class ModeObject
	{
		private ModeObject() => StandardButtonBuilder = new(this);

		private ModeObject(bool hasPageSystem) : this() // Parameterless means it'll create a new screen when instantiated
		{
			// .. create new screen here
		}
		private ModeObject(Transform parent) : this() // Expects an object to be the parent (such as the ModeSelectionScreen, it's a parent of many buttons inside it)
		{
			this.parent = parent;
			manager = CustomModesManager.AttachToSelectionScreen(parent, true);
		}

		/// <summary>
		/// This static constructor will create a new blank selection screen by default.
		/// </summary>
		public static ModeObject CreateBlankScreenInstance(bool hasPageSystem)
		{
			//.. Do blank screen here (no page system by default)
			return new(hasPageSystem);
		}

		/// <summary>
		/// This static constructor will create an instance of <see cref="ModeObject"/> assigned to an existing selection screen.
		/// </summary>
		/// <param name="parent">The selection screen transform's (Ex.: The <see cref="MainModeButtonController"/> object's transform).</param>
		public static ModeObject CreateModeObjectOverExistingScreen(Transform parent)
		{
			int idx = CustomModesHandler.existingModeObjects.FindIndex(x => x.parent == parent);
			if (idx != -1)
				return CustomModesHandler.existingModeObjects[idx];

			return new(parent);
		}

		/// <summary>
		/// This static constructor will create an instance of <see cref="ModeObject"/> assigned to an existing selection screen.
		/// </summary>
		/// <param name="screen">The selection screen enum needed.</param>
		public static ModeObject CreateModeObjectOverExistingScreen(SelectionScreen screen) =>
			screen switch
			{
				SelectionScreen.MainScreen => new(Resources.FindObjectsOfTypeAll<MainModeButtonController>()[0].transform),
				SelectionScreen.ChallengesScreen => new(Resources.FindObjectsOfTypeAll<CursorInitiator>().First(x => x.GetInstanceID() > 0 && x.name == "PickChallenge").transform),
				_ => throw new System.ArgumentException($"Invalid SelectionScreen value. ({screen})")
			};

		public ButtonBuilder StandardButtonBuilder { get; }

		// ************************ Internal Getters *************************

		internal TooltipController ToolTipControl { get
			{
				if (!toolTipControlRef)
				{
					// .. creation process of tool tip control
					
				}
				return toolTipControlRef;
			} 
		}

		TooltipController toolTipControlRef;

		/// <summary>
		/// This class holds a lot of useful methods to create your button inside the <see cref="ModeObject"/>.
		/// </summary>
		

		readonly Transform parent;

		readonly CustomModesManager manager;
	}
	/// <summary>
	/// An enum that refers to two existing screens in-game. Can be used for <see cref="ModeObject.CreateModeObjectOverExistingScreen(SelectionScreen)"/>
	/// </summary>
	public enum SelectionScreen
	{
		/// <summary>
		/// The Main Mode selection screen
		/// </summary>
		MainScreen,
		/// <summary>
		/// The challenges selection screen
		/// </summary>
		ChallengesScreen
	}
}
