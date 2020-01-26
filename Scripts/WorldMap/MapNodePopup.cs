using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapNodePopup : MonoBehaviour 
{
	public Text locationName;
	public Image[] minionIcons = new Image[3];
	public Image[] minionHighlights = new Image[3];
	public Sprite locked, unlocked;
	public Button button;
	public Location location;
	public MapNode mapNode;

	public void SetActive(MapNode node)
	{
		mapNode = node;
		location = node.location;
		gameObject.SetActive (true);
		locationName.text = LocalizationManager.GetLoc(location.unlocName);
		button.interactable = Core.GetWorldMap().GetState(location) != MapNode.NodeState.INACCESSIBLE;

		GetComponent<RectTransform>().position = node.popup.position;
		for (int i = 0; i < 3; i++) 
		{
			int iUnlock = i;

			if (location.numWaves == 8 && i == 2)
				iUnlock = 7;

			if (location.unlocks [iUnlock] == null) 
			{
				minionIcons [i].enabled = false;
				minionHighlights [i].enabled = false;
			} 
			else 
			{
				minionIcons [i].enabled = true;
				minionIcons [i].sprite = location.unlocks [iUnlock].icon;	
				minionHighlights [i].enabled = true;
				minionHighlights [i].sprite = Core.GetPlayerProfile ().pool.unlocks.Contains (location.unlocks [iUnlock]) ? unlocked : locked;
			}

		}
	}

	public void SetInactive()
	{
		gameObject.SetActive (false);
		location = null;
	}

	public void Launch()
	{
		mapNode.Launch ();
	}
}
