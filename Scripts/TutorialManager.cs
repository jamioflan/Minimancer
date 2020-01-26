using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviour {

	public enum TutorialState
	{
		WELCOME,
		SELECT_TOWN_NODE,
		WATCH_FIGHT,
		MISSION_FAIL,
		EDIT_ROSTER,
		WELCOME_TO_TEAM_BUILDER,
		ADD_A_MEDIC,
		RETURN_TO_WORLD,
		RESELECT_TOWN_NODE,
		TUTORIAL_COMPLETE,
		DONE,
	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public TutorialOverlays currentOverlays;

	public bool IsInState(TutorialState state)
	{
		return Core.GetPlayerProfile().tutorialState == state;
	}

	public void AdvanceTutorialStateIfInState(TutorialState state)
	{
		if (Core.GetPlayerProfile().tutorialState == state)
		{
			Debug.Assert(currentOverlays != null);
			if (currentOverlays != null)
			{
				currentOverlays.AdvanceTutorialState();
			}
		}
	}

	public TutorialState AdvanceTutorialState()
	{
		TutorialState newState = (TutorialState)((int)Core.GetPlayerProfile().tutorialState + 1);
		Core.GetPlayerProfile().tutorialState = newState;
		return newState;
	}

	public void RequestState(TutorialState ts)
	{
		Core.GetPlayerProfile().tutorialState = ts;
	}

	public void SkipTutorial()
	{
		RequestState(TutorialState.DONE);
	}
}
