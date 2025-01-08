using TMPro;
using System.Linq;
using UnityEngine;
using ModdedModesAPI.BepInEx;

namespace ModdedModesAPI.ModesAPI
{
	/// <summary>
	/// This class stores the data of the screen assigned to it.
	/// </summary>
	public class ModeObject
	{
		private ModeObject() => StandardButtonBuilder = new(this);

		private ModeObject(bool hasPageSystem, params Vector2[] positions) : this() // Creates a new screen by default
		{
			if (positions.Length == 0)
				throw new System.ArgumentException("Vector2[] positions cannot be of length 0");

			CustomModesHandler.existingModeObjects.Add(this);
			// .. create new screen here

			// ..
			manager = CustomModesManager.AttachToSelectionScreen(this, hasPageSystem, positions);
		}
		private ModeObject(Transform parent) : this() // Expects an object to be the parent (such as the ModeSelectionScreen, it's a parent of many buttons inside it)
		{
			ScreenTransform = parent;
			manager = CustomModesManager.AttachToSelectionScreen(this, true);

			CustomModesHandler.existingModeObjects.Add(this);

			manager.pageTogglers = new StandardMenuButton[2];

			var leftToggler = StandardButtonBuilder.CreateBlankButton("PageLeftToggler", ResourceStorage.togglersSheet[2], false) // Left toggler
				.AddHighlightAnimation(ResourceStorage.togglersSheet[0], ResourceStorage.togglersSheet[2]);

			leftToggler.OnPress.AddListener(() => manager.SwitchPage(false));

			leftToggler.transform.localPosition = Vector2.left * togglerOffset;
			((RectTransform)leftToggler.transform).sizeDelta = Vector2.one * 32;

			manager.pageTogglers[0] = leftToggler;

			var rightToggler = StandardButtonBuilder.CreateBlankButton("PageRightToggler", ResourceStorage.togglersSheet[1], false) // Right toggler
				.AddHighlightAnimation(ResourceStorage.togglersSheet[1], ResourceStorage.togglersSheet[3]);

			rightToggler.OnPress.AddListener(() => manager.SwitchPage(true));

			rightToggler.transform.localPosition = Vector2.right * togglerOffset;
			((RectTransform)rightToggler.transform).sizeDelta = Vector2.one * 32;

			manager.pageTogglers[1] = rightToggler;
		}

		/// <summary>
		/// This static constructor will create an instance of <see cref="ModeObject"/> assigned to a new blank screen.
		/// </summary>
		/// <param name="hasPageSystem">If True, this blank screen will follow a similar behavior of pages that the existing ones have.</param>
		/// <param name="availableButtonPositions">The positions that the buttons created by the <see cref="ModeObject"/> will be fixated to.</param>
		/// <returns>An instance of <see cref="ModeObject"/>.</returns>
		public static ModeObject CreateBlankScreenInstance(bool hasPageSystem, params Vector2[] availableButtonPositions) =>
			new(hasPageSystem, availableButtonPositions);


		/// <summary>
		/// This static constructor will create an instance of <see cref="ModeObject"/> assigned to an existing selection screen.
		/// </summary>
		/// <param name="parent">The selection screen transform's (Ex.: The <see cref="MainModeButtonController"/> object's transform).</param>
		/// <returns>An instance of <see cref="ModeObject"/>.</returns>
		public static ModeObject CreateModeObjectOverExistingScreen(Transform parent)
		{
			int idx = CustomModesHandler.existingModeObjects.FindIndex(x => x.ScreenTransform == parent);
			if (idx != -1)
				return CustomModesHandler.existingModeObjects[idx];

			return new(parent);
		}

		/// <summary>
		/// This static constructor will create an instance of <see cref="ModeObject"/> assigned to an existing selection screen.
		/// </summary>
		/// <param name="screen">The selection screen enum needed.</param>
		/// <returns>An instance of <see cref="ModeObject"/>.</returns>
		public static ModeObject CreateModeObjectOverExistingScreen(SelectionScreen screen) =>
			screen switch
			{
				SelectionScreen.MainScreen => CreateModeObjectOverExistingScreen(Resources.FindObjectsOfTypeAll<MainModeButtonController>()[0].transform),
				SelectionScreen.ChallengesScreen => CreateModeObjectOverExistingScreen(Resources.FindObjectsOfTypeAll<CursorInitiator>().First(x => x.GetInstanceID() > 0 && x.name == "PickChallenge").transform),
				_ => throw new System.ArgumentException($"Invalid SelectionScreen value. ({screen})")
			};

		// ********************* Public Methods ***********************

		/// <summary>
		/// By default, all page buttons are built in (230,0). But you can change the both axis through this method. 
		/// </summary>
		/// <param name="offset">The offset it goes to (note that the X axis starts from (-230,0), the right page button will mirror the X axis from the left one).</param>
		/// <exception cref="System.InvalidOperationException"></exception>
		public void SetThePageButtonsAxis(Vector2 offset)
		{
			if (!manager.supportsPages)
				throw new System.InvalidOperationException("This ModeObject instance was set to not support pages. The page toggler position cannot be changed then.");
			offset.x = Mathf.Min(offset.x - togglerOffset, togglerTouchLimit);
			manager.UpdateTogglersOffset(offset);
		}

		/// <summary>
		/// This class holds a lot of useful methods to create your button inside the <see cref="ModeObject"/>.
		/// </summary>
		public ButtonBuilder StandardButtonBuilder { get; }

		public TextMeshProUGUI DescriptionText { get
			{
				if (!DescriptionText)
				{
					// .. creation process of description text

				}
				return DescriptionText;
			} 
		}

		TextMeshProUGUI descriptionTextRef;

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
		/// The screen this instance is overriding.
		/// </summary>
		public Transform ScreenTransform { get; }

		readonly internal CustomModesManager manager;

		const float togglerOffset = 230f, togglerTouchLimit = -20f;
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
