using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class LevelCompleteMinionIcon : MonoBehaviour , IPointerEnterHandler, IPointerExitHandler
{
	public int index = 0;
	public LevelCompleteScreen parent;

	public void OnPointerEnter(PointerEventData eventData)
	{
		parent.Hover(index);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		parent.Unhover();
	}
}
