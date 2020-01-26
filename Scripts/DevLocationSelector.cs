using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DevLocationSelector : MonoBehaviour 
{
	void Start () 
	{
		List<string> options = new List<string>();

		foreach(Location loc in Core.GetWorldMap().locations)
		{
			options.Add(loc.name);
		}

		Dropdown dd = GetComponentInChildren<Dropdown>();
		if (dd != null)
		{
			dd.AddOptions(options);
		}

		dd.value = 1;
		dd.value = 0;
	}

	public void SelectLocation(int iSelection)
	{
		WorldMap map = Core.GetWorldMap();
		map.Select(iSelection);
	}
}
