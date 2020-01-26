using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MinionIcon : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
	public Image icon, highlight;
	public TeamBuilder parent;
	public MinionSlot index;

	void Start () 
	{
		
	}

	void Update () 
	{
		
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if(parent.HoverMinion((int)index))
			highlight.enabled = true;
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		highlight.enabled = false;
		parent.UnhoverMinion();
	}

	public void SetMinion(Minion minion)
	{
		icon.sprite = minion.template.icon;
	}

	public void SetSlotDroppable(bool bSet)
	{
		icon.color = bSet ? Color.white : Color.grey;
	}
}
