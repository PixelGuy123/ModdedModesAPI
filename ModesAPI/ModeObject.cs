﻿using System.Linq;
using ModdedModesAPI.BepInEx;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ModdedModesAPI.ModesAPI
{
	/// <summary>
	/// This class stores the data of the screen assigned to it.
	/// </summary>
	public class ModeObject
	{
		private ModeObject() =>
			StandardButtonBuilder = new(this);


		private ModeObject(string screenName, bool hasPageSystem, params Vector2[] positions) : this() // Creates a new screen by default
		{
			if (positions.Length == 0)
				throw new System.ArgumentException("Vector2[] positions cannot be of length 0");

			CustomModesHandler.existingModeObjects.Add(this);

			// New Screen creation
			var screenCanvas = new GameObject(screenName).AddComponent<Canvas>();
			screenCanvas.gameObject.layer = LayerMask.NameToLayer("UI");
			screenCanvas.gameObject.SetActive(false); // It won't appear over the main menu, duh lol

			screenCanvas.additionalShaderChannels = AdditionalCanvasShaderChannels.TexCoord1 | AdditionalCanvasShaderChannels.Normal | AdditionalCanvasShaderChannels.Tangent;
			screenCanvas.referencePixelsPerUnit = 100;
			screenCanvas.renderMode = RenderMode.ScreenSpaceCamera;

			var canvScaler = screenCanvas.gameObject.AddComponent<CanvasScaler>();
			canvScaler.physicalUnit = CanvasScaler.Unit.Points;
			canvScaler.referenceResolution = new(480f, 360f);
			canvScaler.referencePixelsPerUnit = 100;
			canvScaler.dynamicPixelsPerUnit = 1;
			canvScaler.fallbackScreenDPI = 96;
			canvScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
			canvScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;

			var assigner = screenCanvas.gameObject.AddComponent<GlobalCamCanvasAssigner>();
			assigner.canvas = screenCanvas;
			assigner.planeDistance = 0.31f;

			var raycaster = screenCanvas.gameObject.AddComponent<GraphicRaycaster>();
			raycaster.blockingMask = -1;

			var cursor = screenCanvas.gameObject.AddComponent<CursorInitiator>();
			cursor.cursorColor = Color.white;
			cursor.cursorPre = ResourceStorage.cursorPre;
			cursor.screenSize = new(480f, 360f);
			cursor.graphicRaycaster = raycaster;

			screenCanvas.transform.position = Vector3.zero;

			bg = new GameObject("BG").AddComponent<Image>();
			bg.transform.SetParent(screenCanvas.transform);
			bg.transform.localPosition = Vector3.zero;
			bg.rectTransform.sizeDelta = new(480f, 360f);
			bg.color = Color.white;

			var bottom = Object.Instantiate(ResourceStorage.bottomPre);
			bottom.name = "Bottom";
			bottom.transform.SetParent(screenCanvas.transform);
			bottom.transform.localPosition = Vector3.zero;

			ScreenTransform = (RectTransform)screenCanvas.transform;
			manager = CustomModesManager.AttachToSelectionScreen(this, hasPageSystem, positions);

			CreateToolTipBase();
			if (hasPageSystem)
				CreateTogglers();
		}
		private ModeObject(Transform parent) : this() // Expects an object to be the parent (such as the ModeSelectionScreen, it's a parent of many buttons inside it)
		{
			if (parent is not RectTransform parentTrans)
				throw new System.ArgumentException("Provided transform for the screen is not of RectTransform type.");

			ScreenTransform = parentTrans;
			manager = CustomModesManager.AttachToSelectionScreen(this, true);

			CustomModesHandler.existingModeObjects.Add(this);

			CreateToolTipBase();
			CreateTogglers();
		}

		/// <summary>
		/// This static constructor will create an instance of <see cref="ModeObject"/> assigned to a new blank screen.
		/// </summary>
		/// <param name="screenName">A name for the screen's <see cref="GameObject"/>.</param>
		/// <param name="hasPageSystem">If True, this blank screen will follow a similar behavior of pages that the existing ones have.</param>
		/// <param name="availablePositions">The positions that the buttons created by the <see cref="ModeObject"/> will be fixated to.</param>
		/// <returns>An instance of <see cref="ModeObject"/>.</returns>
		/// <exception cref="System.InvalidOperationException"></exception>
		public static ModeObject CreateBlankScreenInstance(string screenName, bool hasPageSystem, params Vector2[] availablePositions)
		{
			ThrowIfNotAllowedToInstantiate();
			return new(screenName, hasPageSystem, availablePositions);
		}

		/// <summary>
		/// This static constructor will create an instance of <see cref="ModeObject"/> assigned to an existing selection screen.
		/// </summary>
		/// <param name="screen">The selection screen enum needed.</param>
		/// <returns>An instance of <see cref="ModeObject"/>.</returns>
		/// <exception cref="System.ArgumentException"></exception>
		/// <exception cref="System.InvalidOperationException"></exception>
		public static ModeObject CreateModeObjectOverExistingScreen(SelectionScreen screen)
		{
			return screen switch
			{
				SelectionScreen.MainScreen => CreateModeObjectOverExistingScreen(Resources.FindObjectsOfTypeAll<TutorialPrompt>()[0].transform),
				SelectionScreen.ChallengesScreen => CreateModeObjectOverExistingScreen(Resources.FindObjectsOfTypeAll<CursorInitiator>().First(x => x.GetInstanceID() > 0 && x.name == "PickChallenge").transform),
				_ => throw new System.ArgumentException($"Invalid SelectionScreen value. ({screen})")
			};

			static ModeObject CreateModeObjectOverExistingScreen(Transform parent)
			{
				ThrowIfNotAllowedToInstantiate();

				int idx = CustomModesHandler.existingModeObjects.FindIndex(x => x.ScreenTransform == parent);
				if (idx != -1)
					return CustomModesHandler.existingModeObjects[idx];

				return new(parent);
			}
		}

		static void ThrowIfNotAllowedToInstantiate()
		{
			if (!CustomModesHandler.allowModeObjectCreation)
				throw new System.InvalidOperationException("Cannot create a ModeObject outside the MainMenu initialization. Please, add a listener to the CustomModesHandler.OnMainMenuInitialize event.");
		}

		// ********************* Public Methods ***********************

		/// <summary>
		/// By default, all page buttons are built in (230,0). But you can change the both axis through this method. 
		/// </summary>
		/// <param name="offset">The offset it goes to (note that the X axis starts from (-230,0), the right page button will mirror the X axis from the left one).</param>
		/// <exception cref="System.InvalidOperationException"></exception>
		public void SetThePageButtonsAxis(Vector2 offset)
		{
			if (!manager.supportsPages || !allowAxisChanges)
				throw new System.InvalidOperationException("This ModeObject instance does not support pages or it isn\'t allowed to the change the page\'s buttons\' position.");
			offset.x = Mathf.Min(offset.x - togglerOffset, togglerTouchLimit);
			manager.UpdateTogglersOffset(offset);
		}

		internal bool allowAxisChanges = true;

		/// <summary>
		/// This class holds a lot of useful methods to create your buttons inside the <see cref="ModeObject"/>.
		/// </summary>
		public ButtonBuilder StandardButtonBuilder { get; }

		/// <summary>
		/// The description text that appears below buttons (they are only usable in custom screens to avoid unexpected changes in main screens).
		/// </summary>
		/// <exception cref="System.NotSupportedException"></exception>
		public TextMeshProUGUI DescriptionText
		{
			get
			{
				if (!allowedToChangeDescriptionText)
					throw new System.NotSupportedException("Cannot return an instance of the DescriptionText from this screen, it was set to be innacessible by default.");

				if (!descriptionTextRef)
				{
					descriptionTextRef = new GameObject("ModeText").AddComponent<TextMeshProUGUI>();
					descriptionTextRef.gameObject.layer = LayerMask.NameToLayer("UI");

					descriptionTextRef.transform.SetParent(ScreenTransform);
					descriptionTextRef.transform.localPosition = Vector2.down * 20f;
					descriptionTextRef.transform.localScale = Vector3.one * 1f;
					descriptionTextRef.transform.SetSiblingIndex(ScreenTransform.Find("TooltipBase").GetSiblingIndex());

					var rect = descriptionTextRef.transform as RectTransform;
					rect.sizeDelta = new(480f, 96f);
					rect.pivot = new(0.5f, 1f);

					descriptionTextRef.color = Color.black;
					descriptionTextRef.alignment = TextAlignmentOptions.Top;


					var loc = descriptionTextRef.gameObject.AddComponent<TextLocalizer>();
					loc.key = "Men_PickMode";
				}
				return descriptionTextRef;
			}
		}

		internal TextMeshProUGUI descriptionTextRef;
		internal bool allowedToChangeDescriptionText = true;

		/// <summary>
		/// Property that returns the sprite and sets the sprite used by the background.
		/// <para>If the screen isn't allowed to have the sprite changed, you'll get an exception.</para>
		/// </summary>
		/// <exception cref="System.InvalidOperationException"></exception>
		public Sprite Background
		{
			get => bg.sprite;
			set
			{
				if (!allowBackgroundAddition)
					throw new System.InvalidOperationException($"Cannot change the background of this screen ({ScreenTransform}).");
				bg.sprite = value;
			}
		}

		internal Image bg;
		internal bool allowBackgroundAddition = true;

		/// <summary>
		/// The <see cref="TooltipController"/> that this class holds.
		/// </summary>
		public TooltipController ToolTipControl { get; private set; }

		/// <summary>
		/// The screen this instance is overriding.
		/// </summary>
		public RectTransform ScreenTransform { get; }

		// ************************ Internal/Private Methods *************************

		internal void CreateLink(ModeObject screenToReturn)
		{
			IsLinked = true;
			var backBut = StandardButtonBuilder.CreateTransitionButton(screenToReturn, true)
				.AddHighlightAnimation(ResourceStorage.backArrowSheet[1], ResourceStorage.backArrowSheet[0])
				.AddVisual(ResourceStorage.backArrowSheet[1]);

			backBut.name = "BackButton";
			var rectTrans = backBut.transform as RectTransform;
			rectTrans.sizeDelta = Vector2.one * 32f;
			rectTrans.localPosition = new(-240f, 180f);
			rectTrans.pivot = new(0f, 1f);
		}

		void CreateToolTipBase()
		{
			ToolTipControl = ScreenTransform.gameObject.AddComponent<TooltipController>();
			ToolTipControl.enabled = false;

			var toolTipBase = Object.Instantiate(ResourceStorage.tooltipBase);
			toolTipBase.name = "TooltipBase";
			toolTipBase.SetParent(ScreenTransform);
			toolTipBase.SetSiblingIndex(ScreenTransform.Find("Bottom").GetSiblingIndex());
			toolTipBase.localPosition = new(-240f, 180f);
			toolTipBase.localScale = Vector3.one;

			var toolTip = toolTipBase.Find("Tooltip");

			ToolTipControl.tooltipBgRect = (RectTransform)toolTip.Find("BG");
			ToolTipControl.tooltipBgRect.GetComponent<Image>().pixelsPerUnitMultiplier = 100f;

			ToolTipControl.tooltipRect = (RectTransform)toolTip;
			ToolTipControl.tooltipTmp = toolTip.GetComponentInChildren<TextMeshProUGUI>();

			ToolTipControl.xBuffer = 6;
			ToolTipControl.yBuffer = 8;
			ToolTipControl.xMin = 10f;
			ToolTipControl.xMax = 470f;
		}

		void CreateTogglers()
		{
			manager.pageTogglers = new StandardMenuButton[2];

			var leftToggler = StandardButtonBuilder.CreateBlankButton("PageLeftToggler", false) // Left toggler
				.AddHighlightAnimation(ResourceStorage.togglersSheet[0], ResourceStorage.togglersSheet[2])
				.AddVisual(ResourceStorage.togglersSheet[2]);

			leftToggler.OnPress.AddListener(() => manager.SwitchPage(false));
			((RectTransform)leftToggler.transform).sizeDelta = Vector2.one * 32f;

			leftToggler.transform.localPosition = Vector2.left * togglerOffset;

			manager.pageTogglers[0] = leftToggler;

			var rightToggler = StandardButtonBuilder.CreateBlankButton("PageRightToggler", false) // Right toggler
				.AddHighlightAnimation(ResourceStorage.togglersSheet[1], ResourceStorage.togglersSheet[3])
				.AddVisual(ResourceStorage.togglersSheet[1]);

			rightToggler.OnPress.AddListener(() => manager.SwitchPage(true));
			((RectTransform)rightToggler.transform).sizeDelta = Vector2.one * 32f;

			rightToggler.transform.localPosition = Vector2.right * togglerOffset;

			manager.pageTogglers[1] = rightToggler;
		}

		// ************************ Internal Getters *************************

		internal bool IsLinked { get; set; } = false;
		internal SeedInput SeedInput { get; set; }
		internal bool allowSeedInputCreation = true;

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
		/// The challenges mode selection screen
		/// </summary>
		ChallengesScreen
	}
}
