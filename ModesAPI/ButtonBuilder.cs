using UnityEngine;
using UnityEngine.UI;

namespace ModdedModesAPI.ModesAPI
{
	/// <summary>
	/// A class filled up with functions to make your button.
	/// </summary>
	public class ButtonBuilder(ModeObject modeObject)
	{
		
		public StandardMenuButton CreateBlankButton(string name, Sprite visual)
		{
			var but = new GameObject(name)
			{
				tag = "Button",
				layer = LayerMask.NameToLayer("UI")
			}.AddComponent<StandardMenuButton>();
			but.image = but.gameObject.AddComponent<Image>();
			but.image.sprite = visual;

			// .. Setup it to be child of modeObject.ScreenTransform

			modeObject.manager.AddButton(but);
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
		public static void AddHighlightAnimation(this StandardMenuButton but, Sprite highlightOn, Sprite highlightOff)
		{
			but.swapOnHigh = true;
			but.highlightedSprite = highlightOn;
			but.unhighlightedSprite = highlightOff;
		}
	}
}
