using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace ModdedModesAPI.ModesAPI
{
	/// <summary>
	/// A class filled up with functions to make your button.
	/// </summary>
	public class ButtonBuilder(ModeObject modeObject)
	{
		internal StandardMenuButton CreateBlankButton(string name, Sprite visual, bool registerButton = true) // Internal, because it's just a method to literally create a blank button
		{
			var but = new GameObject(name)
			{
				tag = "Button",
				layer = LayerMask.NameToLayer("UI")
			}.AddComponent<StandardMenuButton>();
			but.image = but.gameObject.AddComponent<Image>();
			but.image.sprite = visual;
			but.unhighlightOnEnable = true;

			but.transform.SetParent(modeObject.ScreenTransform);
			but.transform.localScale = Vector3.one; // it's set to scale 0 for some reason?
			but.transform.SetSiblingIndex(but.transform.parent.childCount - 1); // Should avoid being above the cursor

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

		public StandardMenuButton CreateModeButton<G, LV>(string nameKey, Sprite visual, G manager, LV builder) where G : BaseGameManager where LV : LevelBuilder
		{
			var but = CreateBlankButton(Singleton<LocalizationManager>.Instance.GetLocalizedText(nameKey, false), visual);

			//.. make it start a game, like hide and seek does

			return but;
		}

		public StandardMenuButton CreateModeButton<G>(string nameKey, Sprite visual, G manager, LevelBuilderType lvlType) where G : BaseGameManager =>
			lvlType switch
			{
				LevelBuilderType.LevelGenerator => CreateModeButton(nameKey, visual, manager, Resources.FindObjectsOfTypeAll<LevelGenerator>().First(x => x.GetInstanceID() > 0)),
				LevelBuilderType.LevelLoader => CreateModeButton(nameKey, visual, manager, Resources.FindObjectsOfTypeAll<LevelLoader>().First(x => x.GetInstanceID() > 0)),
				_ => throw new System.ArgumentException("Invalid LevelBuilderType given.")
			};
		public StandardMenuButton CreateModeButton<LV>(string nameKey, Sprite visual, ManagerType manType, LV builder) where LV : LevelBuilder =>
			manType switch
			{
				ManagerType.MainGameManager => CreateModeButton(nameKey, visual, Resources.FindObjectsOfTypeAll<MainGameManager>().First(x => x.GetInstanceID() > 0), builder),
				ManagerType.EndlessGameManager => CreateModeButton(nameKey, visual, Resources.FindObjectsOfTypeAll<EndlessGameManager>().First(x => x.GetInstanceID() > 0), builder),
				_ => throw new System.ArgumentException("Invalid LevelBuilderType given.")
			};

		public StandardMenuButton CreateModeButton(string nameKey, Sprite visual, ManagerType manType, LevelBuilderType builder)
		{
			var gen = builder == LevelBuilderType.LevelGenerator ?
				Resources.FindObjectsOfTypeAll<LevelGenerator>().First(x => x.GetInstanceID() > 0) : Resources.FindObjectsOfTypeAll<LevelBuilder>().First(x => x.GetInstanceID() > 0);

			BaseGameManager manager = manType == ManagerType.MainGameManager ?
				Resources.FindObjectsOfTypeAll<MainGameManager>().First(x => x.GetInstanceID() > 0) : Resources.FindObjectsOfTypeAll<EndlessGameManager>().First(x => x.GetInstanceID() > 0);

			return CreateModeButton(nameKey, visual, manager, gen); // I hope it works with base types lol
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

		readonly ModeObject modeObject = modeObject;
	}
	/// <summary>
	/// This class holds some useful extension methods to make the <see cref="StandardMenuButton"/> building process easier.
	/// </summary>
	public static class ButtonExtensions
	{
		/// <summary>
		/// Adds a highlight animation, in the form of sprites, to the button.
		/// </summary>
		/// <param name="but">The button instance.</param>
		/// <param name="highlightOn">Display sprite when the cursor is hovering the button.</param>
		/// <param name="highlightOff">Display sprite when the cursor is not hovering the button.</param>
		public static StandardMenuButton AddHighlightAnimation(this StandardMenuButton but, Sprite highlightOn, Sprite highlightOff)
		{
			but.swapOnHigh = true;
			but.highlightedSprite = highlightOn;
			but.unhighlightedSprite = highlightOff;
			return but;
		}

		/// <summary>
		/// Adds a transition to the button.
		/// </summary>
		/// <param name="but">The button instance</param>
		/// <param name="transitionTime">The transition time.</param>
		/// <param name="transitionType">The transition type.</param>
		/// <param name="toDisable">The "scene" to be disabled when switching screens.</param>
		/// <param name="toEnable">The "scene" to be enabled when switching screens.</param>
		/// <returns></returns>
		public static StandardMenuButton AddTransitionOnPress(this StandardMenuButton but, float transitionTime, UiTransition transitionType, Transform toDisable, Transform toEnable)
		{
			but.OnPress.AddListener(() =>
			{
				toDisable.gameObject.SetActive(false);
				toEnable.gameObject.SetActive(true);
			});
			but.transitionOnPress = true;
			but.transitionType = transitionType;
			but.transitionTime = transitionTime;
			return but;
		}
	}

	/// <summary>
	/// If you will use an existent <see cref="LevelBuilder"/> inherited class from the game, use this enum for <see cref="ButtonBuilder.CreateModeButton{G}(string, Sprite, G, LevelBuilderType)"/>
	/// </summary>
	public enum LevelBuilderType
	{
		/// <summary>
		/// The <see cref="LevelGenerator"/> type
		/// </summary>
		LevelGenerator,
		/// <summary>
		/// The <see cref="LevelLoader"/> type
		/// </summary>
		LevelLoader
	}

	/// <summary>
	/// If you will use an existent <see cref="BaseGameManager"/> inherited class from the game, use this enum for <see cref="ButtonBuilder.CreateModeButton{LV}(string, Sprite, ManagerType, LV)"/>
	/// </summary>
	public enum ManagerType
	{
		/// <summary>
		/// The <see cref="MainGameManager"/> type
		/// </summary>
		MainGameManager,
		/// <summary>
		/// The <see cref="EndlessGameManager"/> type
		/// </summary>
		EndlessGameManager
	}
}
