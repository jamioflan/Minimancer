using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WorldMapSlider : MonoBehaviour 
{
	public static WorldMapSlider instance = null;
	public float fSlide = -1.0f;
	private float fTimeSinceClick = 0.0f;
	private bool bTransitioning = false;
	private float fTransitionDirection = 1.0f;
	public TeamRosterPreview[] previews = new TeamRosterPreview[5];
	public RosterPanel[] rosterPanels = new RosterPanel[5];
	public RectTransform hoverBox;
	public Text hoverTextWhite, hoverTextShadow;
	public MapNodePopup popup;
	public GauntletStep[] gauntletSteps = new GauntletStep[7];

	void Start () 
	{
		Debug.Assert(instance == null, "Two instances?");
		instance = this;
		for(int i = 0; i < 5; i++)
		{
			rosterPanels [i].OnOpenWorldMap(Core.GetPlayerProfile().iSelectedIndex == i);
		}
		RefreshGauntlet();
	}

	public void Toggle()
	{
		CloseAllMapNodes();
		RefreshGauntlet();
		bTransitioning = true;
		fTimeSinceClick = 0.0f;
		fTransitionDirection = -Mathf.Sign(fSlide);
		Core.Save();
	}

	public void SlideToTeamBuilder()
	{
		if (fSlide < 0.0f)
		{
			Core.GetTutorialManager().AdvanceTutorialStateIfInState(TutorialManager.TutorialState.EDIT_ROSTER);

			CloseAllMapNodes();
			bTransitioning = true;
			fTimeSinceClick = 0.0f;
			fTransitionDirection = -Mathf.Sign(fSlide);
			for(int i = 0; i < 5; i++)
			{
				rosterPanels [i].OnOpenTeamBuilder(Core.GetPlayerProfile().iSelectedIndex == i);
			}
		}
	}

	public bool IsOnWorldMap()
	{
		return fSlide < 0.0f;
	}

	public bool IsOnTeamBuilder()
	{
		return fSlide > 0.0f;
	}

	public void SlideToWorldMap()
	{
		if (fSlide > 0.0f)
		{
			Core.GetTutorialManager().AdvanceTutorialStateIfInState(TutorialManager.TutorialState.RETURN_TO_WORLD);

			CloseAllMapNodes();
			RefreshGauntlet();
			bTransitioning = true;
			fTimeSinceClick = 0.0f;
			fTransitionDirection = -Mathf.Sign(fSlide);
			for(int i = 0; i < 5; i++)
			{
				rosterPanels [i].OnOpenWorldMap(Core.GetPlayerProfile().iSelectedIndex == i);
			}
		}
		Core.Save();
	}

	public void OnChangeSelectionInTeamBuilder()
	{
		for(int i = 0; i < 5; i++)
		{
			rosterPanels [i].OnChangeSelectionInTeamBuilder(Core.GetPlayerProfile().iSelectedIndex == i);
		}
	}

	void OnGUI () 
	{
		if (bTransitioning)
		{
			fTimeSinceClick += Time.deltaTime;
			if (fTimeSinceClick >= 0.5f)
			{
				bTransitioning = false;
				fTimeSinceClick = 0.5f;
			}


			fSlide = fTransitionDirection * (-32.0f * fTimeSinceClick * fTimeSinceClick * fTimeSinceClick + 24.0f * fTimeSinceClick * fTimeSinceClick - 1);
			fSlide = Mathf.Clamp(fSlide, -1.0f, 1.0f);
		}

		RectTransform tf = GetComponent<RectTransform>();
		tf.localPosition = new Vector3 (848.0f * fSlide, 0.0f, 0.0f);

		Vector3[] fourCorners = new Vector3[4];
		tf.GetWorldCorners(fourCorners);

		float fMinX = fourCorners [0].x / Screen.width;
		float fMaxX = fourCorners [2].x / Screen.width;
		float fMinY = fourCorners [0].y / Screen.height;
		float fMaxY = fourCorners [2].y / Screen.height;

		float fCentreX = (fMinX + fMaxX) * 0.5f;
		float fWidthX = fMaxX - fMinX;

		float fCentreY = (fMinY + fMaxY) * 0.5f;
		float fHeightY = fMaxY - fMinY;

		// Should be -0.883333333 to 1
		float fScaleX = fWidthX / 1.8833333333333f;
		float fScaleY = fHeightY / 1.0f;

		for (int i = 0; i < 5; i++)
		{
			if (previews [i] != null)
			{
				//Rect old = previews [i].cam.rect;
				//float fParametric = 0.5f * (fSlide + 1.0f);
				//previews [i].cam.rect = new Rect (fParametric * 0.8875f, old.y, 0.1125f, old.height);

				previews [i].cam.rect = new Rect (
					fCentreX - (0.1125f * 0.5f * fScaleX), 
					fCentreY + (-0.5f + (4 - i) * 0.2f) * fScaleY, 
					0.1125f * fScaleX, 
					0.2f * fScaleY);
			}
		}

		if (Event.current != null)
		{
			Vector3 v = GUIUtility.ScreenToGUIPoint(Event.current.mousePosition);
			hoverBox.position = new Vector3 (v.x, Screen.height - v.y, v.z);
		}
	}

	public void RefreshGauntlet()
	{
		for (int i = 0; i < (int)Element.NO_ELEMENT; i++)
		{
			gauntletSteps [i].SetGauntletStepActive(!Core.GetWorldMap().abUnfinished [i]);
		}
	}

	public void CloseAllMapNodes()
	{
		MapNode.CloseAllMapNodes();
	}

	public void SetLocalizedHoverText(string localized)
	{
		hoverTextWhite.text = localized;
		hoverTextShadow.text = localized;
	}

	public void SetHoverText(string data)
	{
		string localized = LocalizationManager.GetLoc(data);
		hoverTextWhite.text = localized;
		hoverTextShadow.text = localized;
	}

	public void QuitToMainMenu()
	{
		Core.ReturnToMainMenu();
		Core.GetAudioManager().PlayGUIClick();
	}
}
