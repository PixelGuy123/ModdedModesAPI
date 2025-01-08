﻿using TMPro;
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
				modeObject.manager.AddButton(but);
			else
				but.transform.localPosition = Vector3.zero;

			return but;
		}
		/// <summary>
		/// Creates a button that changes screens.
		/// <para>Note that the button created will be placed on a pre-set position in the screen.</para>
		/// </summary>
		/// <param name="screenToGo">The screen that the button will redirect the player to.</param>
		/// <param name="transitionTime">The time the transition takes (by default, standard value from the game).</param>
		/// <param name="transitionType">The transition type it uses (by default, standard value from the game).</param>
		/// <returns>The button instance.</returns>
		public StandardMenuButton CreateTransitionButton(ModeObject screenToGo, float transitionTime = 0.0167f, UiTransition transitionType = UiTransition.Dither) =>
			CreateTransitionButton(screenToGo, false, transitionTime, transitionType);

		internal StandardMenuButton CreateTransitionButton(ModeObject screenToGo, bool isABackButton, float transitionTime = 0.0167f, UiTransition transitionType = UiTransition.Dither)
		{
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

		public StandardMenuButton CreateModeButton<G, LV>(G manager = null, LV builder = null) where G : BaseGameManager where LV : LevelBuilder
		{
			var but = CreateBlankButton("GenericModeButton");

			//.. make it start a game, like hide and seek does

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
