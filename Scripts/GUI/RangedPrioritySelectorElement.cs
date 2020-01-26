using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RangedPrioritySelectorElement : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
	public bool isSelected;
	public Text highlight;

	public void Start()
	{
		Deselect();
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		highlight.enabled = true;
		if (!isSelected)
			highlight.color = Color.grey;
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (!isSelected)
			highlight.enabled = false;
		highlight.color = Color.white;
	}

	public void Deselect()
	{
		isSelected = false;
		highlight.enabled = false;
	}

	public void Select()
	{
		isSelected = true;
		highlight.enabled = true;
		highlight.color = Color.white;
	}
}
