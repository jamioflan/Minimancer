using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GenericHoverText : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
	public WorldMapSlider slider;
	public string unlocText;

	public void OnPointerEnter(PointerEventData data)
	{
		slider.SetHoverText(unlocText);
	}

	public void OnPointerExit(PointerEventData data)
	{
		slider.SetHoverText("");
	}
}
