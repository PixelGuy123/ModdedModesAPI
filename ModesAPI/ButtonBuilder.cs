using ModdedModesAPI.BepInEx;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ModdedModesAPI.ModesAPI
{
	/// <summary>
	/// A class filled up with functions to make your button.
	/// </summary>
	public class ButtonBuilder(ModeObject modeObject)
	{
		/// <summary>
		/// Create a blank button with no additional functionality. Only use this method if you want to create something "out of the box".
		/// <para>Note that the button created will be placed on a pre-set position in the screen.</para>
		/// <para>DON'T change its position, you can always insert a new position slot if you're making it in a blank screen.</para>
		/// </summary>
		/// <param name="name">The name of the button's <see cref="GameObject"/></param>
		/// <returns>A button instance.</returns>
		public StandardMenuButton CreateBlankButton(string name) =>
			CreateBlankButton(name, true);
		internal StandardMenuButton CreateBlankButton(string name, bool registerButton = true) // Internal, because it's just a method to literally create a blank button
		{
			var but = new GameObject(name)
			{
				tag = "Button",
				layer = LayerMask.NameToLayer("UI")
			}.AddComponent<StandardMenuButton>();

			but.transform.SetParent(modeObject.ScreenTransform);
			but.transform.localScale = Vector3.one; // it's set to scale 0 for some reason?
			if (modeObject.ScreenTransform.childCount != 0)
				but.transform.SetSiblingIndex(1); // Should avoid being above the cursor

			but.OnRelease = new();
			but.OnHighlight = new();
			but.OffHighlight = new();
			but.OnPress = new();

			if (registerButton)
				modeObject.manager.AddButton(but.transform);
			else
				but.transform.localPosition = Vector3.zero;

			return but;
		}
		/// <summary>
		/// Creates a button that changes screens.
		/// <para>Note that the button created will be placed on a pre-set position in the screen.</para>
		/// <para>DON'T change its position, you can always insert a new position slot if you're making it in a blank screen.</para>
		/// </summary>
		/// <param name="screenToGo">The screen that the button will redirect the player to.</param>
		/// <param name="transitionTime">The time the transition takes (by default, standard value from the game).</param>
		/// <param name="transitionType">The transition type it uses (by default, standard value from the game).</param>
		/// <returns>The button instance.</returns>
		public StandardMenuButton CreateTransitionButton(ModeObject screenToGo, float transitionTime = 0.0167f, UiTransition transitionType = UiTransition.Dither) =>
			CreateTransitionButton(screenToGo, false, transitionTime, transitionType);

		internal StandardMenuButton CreateTransitionButton(ModeObject screenToGo, bool isABackButton, float transitionTime = 0.0167f, UiTransition transitionType = UiTransition.Dither)
		{
			if (screenToGo == modeObject)
				throw new System.ArgumentException("Target screen cannot be the same screen that the transition button is located in.");

			if (!isABackButton)
			{
				if (screenToGo.IsLinked)
					throw new System.InvalidOperationException($"Cannot create transition button because the ModeObject ({screenToGo.ScreenTransform}) is already linked to another transition button.");

				screenToGo.CreateLink(modeObject);
			}

			var but = CreateBlankButton("GenericTransitionButton", !isABackButton);
			but.OnPress.AddListener(() =>
			{
				modeObject.ScreenTransform.gameObject.SetActive(false);
				screenToGo.ScreenTransform.gameObject.SetActive(true);
			});
			but.transitionOnPress = true;
			but.transitionType = transitionType;
			but.transitionTime = transitionTime;
			return but;
		}
		/// <summary>
		/// Creates a button that actually redirects you to a level (like the Hide-and-seek button).
		/// <para>Note that the button created will be placed on a pre-set position in the screen.</para>
		/// <para>DON'T change its position, you can always insert a new position slot if you're making it in a blank screen.</para>
		/// </summary>
		/// <param name="sceneToStart">The <see cref="SceneObject"/> that the game will load.</param>
		/// <param name="createsASave">If the button triggers the game to create a save for that level.</param>
		/// <param name="lives">How many lives do you start in the elevator (2 by default, don't be mistaken by 3, the life counter starts by 0).</param>
		/// <param name="mode">The mode set for that level (Main by default).</param>
		/// <param name="elevatorScreen">The <see cref="ElevatorScreen"/> used (leaving null will use the an existent prefab of the elevator).</param>
		/// <returns></returns>
		public StandardMenuButton CreateModeButton(SceneObject sceneToStart, bool createsASave = false, int lives = 2, Mode mode = Mode.Main, ElevatorScreen elevatorScreen = null)
		{
			var but = CreateBlankButton("GenericModeButton");


			but.OnPress.AddListener(() =>
			{ // this follows almost the same calls that the game does when starting Hide-and-seek
				modeObject.ScreenTransform.gameObject.SetActive(false);

				if (modeObject.SeedInput) // Make sure the GameLoader uses the right seed input in screen
				{
					ResourceStorage.loaderInstance.seedInput = modeObject.SeedInput;
					ResourceStorage.loaderInstance.CheckSeed();
				}
				else
					ResourceStorage.loaderInstance.useSeed = false;

				ResourceStorage.loaderInstance.Initialize(System.Math.Max(0, lives));

				var screenUsed = elevatorScreen ?? ResourceStorage.elvScreen;
				ResourceStorage.loaderInstance.AssignElevatorScreen(screenUsed);
				ResourceStorage.loaderInstance.LoadLevel(sceneToStart);
				screenUsed.gameObject.SetActive(true);
				ResourceStorage.loaderInstance.SetMode((int)mode);
				ResourceStorage.loaderInstance.SetSave(createsASave);
			});


			return but;
		}
		/// <summary>
		/// Creates a <see cref="SeedInput"/> button in the screen.
		/// <para>Note that trying to create a <see cref="SeedInput"/> inside a <see cref="ModeObject"/> that has one already will throw an exception.</para>
		/// <para>You cannot make a <see cref="SeedInput"/> in the challenge screen. If your challenge requires it, make a button that goes to a different screen and add a seed input there, in order to accomplish that.</para>
		/// </summary>
		/// <param name="input">Output of the <see cref="SeedInput"/> instance.</param>
		/// <returns>The button instance that holds the <see cref="SeedInput"/>.</returns>
		/// <exception cref="System.InvalidOperationException"></exception>
		public StandardMenuButton CreateSeedInput(out SeedInput input)
		{
			if (!modeObject.allowSeedInputCreation || modeObject.SeedInput)
				throw new System.InvalidOperationException("This ModeObject already contains a seed input or it\'s not allowed to have one.");

			var but = CreateBlankButton("SeedInput", false)
				.AddTextVisual("Seed: Random", out var text);
			but.transform.localPosition = Vector2.up * 148f;

			input = but.gameObject.AddComponent<SeedInput>();
			input.tmp = text;

			text.alignment = TextAlignmentOptions.Top;

			but.OnPress.AddListener(input.ChangeMode);

			modeObject.SeedInput = input;

			return but;
		}
		/// <summary>
		/// Adds a tool tip animation when you hover the cursor on the button.
		/// </summary>
		/// <param name="but">The button instance.</param>
		/// <param name="toolTipKey">The subtitle key that the tool tip will use.</param>
		public void AddTooltipAnimation(StandardMenuButton but, string toolTipKey)
		{
			var tipController = modeObject.ToolTipControl;
			tipController.enabled = true;

			but.eventOnHigh = true;
			but.OnHighlight.AddListener(() => tipController.UpdateTooltip(toolTipKey));
			but.OffHighlight.AddListener(tipController.CloseTooltip);
		}
		/// <summary>
		/// Allows the button to have a description below when hovering the cursor on.
		/// <para>When this method is used by the first time, in a blank screen, it creates a text object to display the description below.</para>
		/// <para>This text object uses the default settings from the other selection screens. You can modify this text by using the <see cref="ModeObject.DescriptionText"/> property.</para>
		/// </summary>
		/// <param name="but">The button instance.</param>
		/// <param name="descriptionKey">The subtitle key that the description has, to be displayed.</param>
		public void AddDescriptionText(StandardMenuButton but, string descriptionKey)
		{
			but.eventOnHigh = true;
			TextLocalizer text = (modeObject.allowedToChangeDescriptionText ? modeObject.DescriptionText : modeObject.descriptionTextRef).GetComponent<TextLocalizer>();
			but.OnHighlight.AddListener(() => text.GetLocalizedText(descriptionKey));
		}
		/// <summary>
		/// Creates a label for the screen.
		/// <para>Note that the button created will be placed on a pre-set position in the screen.</para>
		/// <para>DON'T change its position, you can always insert a new position slot if you're making it in a blank screen.</para>
		/// </summary>
		/// <param name="textKey">The subtitle key for the label.</param>
		/// <returns>A <see cref="TextMeshProUGUI"/> instance.</returns>
		public TextMeshProUGUI CreateTextLabel(string textKey) =>
			CreateTextLabel(textKey, false);
		/// <summary>
		/// Creates a label for the screen.
		/// <para>Note that the button created will be placed on a pre-set position in the screen.</para>
		/// <para>DON'T change its position, you can always insert a new position slot if you're making it in a blank screen.</para>
		/// </summary>
		/// <param name="textKey">The subtitle key for the label.</param>
		/// <param name="encrypted">If the subtitle is encrypted by the game's standard encryption or not.</param>
		/// <returns>A <see cref="TextMeshProUGUI"/> instance.</returns>
		public TextMeshProUGUI CreateTextLabel(string textKey, bool encrypted)
		{
			var text = new GameObject("GenericLabel")
			{
				layer = LayerMask.NameToLayer("UI")
			}.AddComponent<TextMeshProUGUI>();

			text.transform.SetParent(modeObject.ScreenTransform);
			text.transform.localScale = Vector3.one; // it's set to scale 0 for some reason?
			text.rectTransform.sizeDelta = new(250f, 33f);
			text.alignment = TextAlignmentOptions.Top;
			text.color = Color.black;
			text.fontSizeMin = 18;
			text.fontSizeMax = 72;

			if (modeObject.ScreenTransform.childCount != 0)
				text.transform.SetSiblingIndex(1); // Should avoid being above the cursor

			var loc = text.gameObject.AddComponent<TextLocalizer>();
			loc.key = textKey;
			loc.encrypted = encrypted;

			modeObject.manager.AddButton(text.transform);

			return text;
		}

		readonly ModeObject modeObject = modeObject;
	}
	/// <summary>
	/// This class holds some useful extension methods to make the <see cref="StandardMenuButton"/> building process easier.
	/// </summary>
	public static class ButtonExtensions
	{
		/// <summary>
		/// Adds a visual appearance for the button.
		/// <para>This is usually used for making icons, to change the size of the visual, change the <see cref="RectTransform.sizeDelta"/> property.</para>
		/// </summary>
		/// <param name="but">The button instance.</param>
		/// <param name="visual">The sprite it'll use to display.</param>
		/// <returns>The button instance.</returns>
		public static StandardMenuButton AddVisual(this StandardMenuButton but, Sprite visual)
		{
			but.image = but.gameObject.AddComponent<Image>();
			but.image.sprite = visual;
			but.unhighlightOnEnable = true;
			return but;
		}
		/// <summary>
		/// Adds a text appearance for the button. It uses the same properties that the other buttons uses for the text mesh.
		/// <para>It also sets a default size delta of </para>
		/// </summary>
		/// <param name="but">The button instance.</param>
		/// <param name="textKey">The subtitle key this button will use (every menu button uses a <see cref="TextLocalizer"/>).</param>
		/// <param name="textMesh">The text mesh created by the method.</param>
		/// <returns>The button instance.</returns>
		public static StandardMenuButton AddTextVisual(this StandardMenuButton but, string textKey, out TextMeshProUGUI textMesh) =>
			but.AddTextVisual(textKey, false, out textMesh);
		/// <summary>
		/// Adds a text appearance for the button. It uses the same properties that the other buttons uses for the text mesh.
		/// <para>It also sets a default size delta of </para>
		/// </summary>
		/// <param name="but">The button instance.</param>
		/// <param name="textKey">The subtitle key this button will use (every menu button uses a <see cref="TextLocalizer"/>).</param>
		/// <param name="encrypted">Tells the <see cref="TextLocalizer"/> if the subtitle key used is encrypted using the game's encryption standards.</param>
		/// <param name="textMesh">The text mesh created by the method.</param>
		/// <returns>The button instance.</returns>
		public static StandardMenuButton AddTextVisual(this StandardMenuButton but, string textKey, bool encrypted, out TextMeshProUGUI textMesh)
		{
			textMesh = but.gameObject.AddComponent<TextMeshProUGUI>();
			but.text = textMesh;
			but.underlineOnHigh = true;

			textMesh.alignment = TextAlignmentOptions.Center;
			textMesh.color = Color.black;
			textMesh.fontSizeMin = 18;
			textMesh.fontSizeMax = 72;

			textMesh.gameObject.AddComponent<TextClickableInit>().text = textMesh; // To make sure the cursor actually sees the button (for some reason, setting raycastTarget to true earlier doesn't work)

			var localizer = but.gameObject.AddComponent<TextLocalizer>();
			localizer.encrypted = encrypted;
			localizer.key = textKey;

			return but;
		}

		/// <summary>
		/// Adds a highlight animation, in the form of sprites, to the button.
		/// </summary>
		/// <param name="but">The button instance.</param>
		/// <param name="highlightOn">Display sprite when the cursor is hovering the button.</param>
		/// <param name="highlightOff">Display sprite when the cursor is not hovering the button.</param>
		/// <returns>The button instance.</returns>
		public static StandardMenuButton AddHighlightAnimation(this StandardMenuButton but, Sprite highlightOn, Sprite highlightOff)
		{
			but.swapOnHigh = true;
			but.highlightedSprite = highlightOn;
			but.unhighlightedSprite = highlightOff;
			return but;
		}
	}

	class TextClickableInit : MonoBehaviour
	{
		void Start()
		{
			text.raycastTarget = true;
			Destroy(this);
		}

		[SerializeField]
		internal TextMeshProUGUI text;
	}
}
