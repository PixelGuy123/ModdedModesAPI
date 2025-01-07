using UnityEngine;
using UnityEngine.UI;

namespace ModdedModesAPI.ModesAPI
{
	/// <summary>
	/// A class filled up with functions to make your button.
	/// </summary>
	public class ButtonBuilder(ModeObject modeObject)
	{
		public StandardMenuButton CreateBlankButton(string name)
		{
			var but = new GameObject(name)
			{
				tag = "Button",
				layer = LayerMask.NameToLayer("UI")
			}.AddComponent<StandardMenuButton>();
			but.image = but.gameObject.AddComponent<Image>();
			return but;
		}

		public StandardMenuButton AddHighlightAnimation(StandardMenuButton but, Sprite highlightOn, Sprite highlightOff)
		{
			but.swapOnHigh = true;
			but.highlightedSprite = highlightOn;
			but.unhighlightedSprite = highlightOff;
			return but;
		}

		public StandardMenuButton AddTooltipAnimation(StandardMenuButton but, string toolTipKey)
		{
			var tipController = modeObject.ToolTipControl;
			but.eventOnHigh = true;
			but.OnHighlight.AddListener(() => tipController.UpdateTooltip(toolTipKey));
			but.OffHighlight.AddListener(tipController.CloseTooltip);
			return but;
		}

		readonly ModeObject modeObject = modeObject;
	}
}
