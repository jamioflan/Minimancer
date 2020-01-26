using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MinionSelector : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
	public Image icon, highlight;
	public MinionTemplate template;
	public MinionStatBlock statBlock;

	public void SetMinion(MinionTemplate temp)
	{
		template = temp;

		icon.sprite = template.icon;
		highlight.enabled = false;

	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void Select()
	{
		Core.GetPlayerProfile().pool.AddMinion(template);
		Core.GetLevel().Shutdown();
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		highlight.enabled = true;
		statBlock.SetMinion(template);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		highlight.enabled = false;
		statBlock.SetMinion(null);
	}
}
