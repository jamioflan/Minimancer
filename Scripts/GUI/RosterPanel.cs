using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RosterPanel : MonoBehaviour 
{
	public Image highlight;
	public Text selectButtonText;
	public int index = 0;
	public RectTransform dualMode, editMode, playMode;

	public void Highlight()
	{
		foreach (RosterPanel panel in FindObjectsOfType<RosterPanel>())
		{
			panel.Unhighlight();
		}

		highlight.enabled = true;
	}

	public void Unhighlight()
	{
		highlight.enabled = false;
	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void SetMode(int mode)
	{
		dualMode.gameObject.SetActive(mode == 0);
		editMode.gameObject.SetActive(mode == 1);
		playMode.gameObject.SetActive(mode == 2);
	}

	public void OnOpenTeamBuilder(bool bSelected)
	{
		OnChangeSelectionInTeamBuilder(bSelected);
		if (bSelected)
			Highlight();
	}

	public void OnOpenWorldMap(bool bSelected)
	{
		SetMode(0);
		if (bSelected)
			Highlight();
	}

	public void OnChangeSelectionInTeamBuilder(bool bSelected)
	{
		SetMode(bSelected ? 2 : 1);
	}

	public void Edit()
	{
		Core.GetAudioManager().PlayGUIClick();
		TeamBuilder.instance.EditRoster(index);
	}

	public void Select()
	{
		Core.GetAudioManager().PlayGUIClick();
		TeamBuilder.instance.SelectRoster(index);
	}
}
