using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MinionStatBlock : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
	public Sprite emptyBackground;
	public Sprite[] elementalBackgrounds = new Sprite[(int)Element.NUM_ELEMENTS];
	public Image background;
	public Image highlight;
	public Text title, desc, health;
	public TeamBuilder parent;
	public MinionSlot slot = MinionSlot.NUM_MINION_SLOTS;

	public void SetMinion(MinionTemplate template, MinionSlot setSlot = MinionSlot.NUM_MINION_SLOTS)
	{
		slot = setSlot;
		if (template == null)
		{
			background.sprite = emptyBackground;
			title.text = "";
			desc.text = "";
			health.text = "";
		}
		else
		{
			background.sprite = elementalBackgrounds [(int)template.element];
			title.text = LocalizationManager.GetLoc(template.unlocName);
			desc.text = LocalizationManager.GetLoc(template.unlocDesc);
			if (template.GetSlotType() == MinionSlotType.SUPPORT)
			{
				health.text = "-";
			}
			else
			{
				string hpString = LocalizationManager.GetLoc("HP");
				health.text = string.Format(hpString, Mathf.FloorToInt(template.fMaxHealth));
			}
		}
	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void SetHighlighted(bool bSet)
	{
		highlight.enabled = bSet;
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		SetHighlighted(true);
		if (parent != null)
		{
			parent.HoverMinion((int)slot);
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		SetHighlighted(false);
		if (parent != null)
		{
			parent.UnhoverMinion();
		}
	}
}
