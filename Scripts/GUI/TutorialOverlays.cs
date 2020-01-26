using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialOverlays : MonoBehaviour 
{
	public GameObject[] overlays = new GameObject[0];

	void Start () 
	{
		Core.GetTutorialManager().currentOverlays = this;
		SwitchToOverlay(Core.GetPlayerProfile().tutorialState);
	}

	void Update () 
	{
	}
		
	private void SwitchToOverlay(TutorialManager.TutorialState state)
	{
		foreach (GameObject go in overlays)
		{
			if (go != null)
				go.SetActive(false);
		}

		if (overlays [(int)state] != null)
		{
			overlays [(int)state].SetActive(true);
		}
	}

	public void AdvanceTutorialState()
	{
		TutorialManager.TutorialState state = Core.GetTutorialManager().AdvanceTutorialState();

		SwitchToOverlay(state);
	}

	public void SkipTutorial()
	{
		Core.GetTutorialManager().SkipTutorial();
		SwitchToOverlay(TutorialManager.TutorialState.DONE);
	}
}
