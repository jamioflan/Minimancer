using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class FilterIcon : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
	public WorldMapSlider parent;
	public string showHoverText, hideHoverText;
	public bool bActive = true;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void SetActive(bool bSet)
	{
		bActive = bSet;
		parent.SetHoverText(bActive ? hideHoverText : showHoverText);
	}

	public void OnPointerEnter(PointerEventData data)
	{
		parent.SetHoverText(bActive ? hideHoverText : showHoverText);
	}

	public void OnPointerExit(PointerEventData data)
	{
		parent.SetHoverText("");
	}
}
