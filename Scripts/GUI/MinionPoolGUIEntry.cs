using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MinionPoolGUIEntry : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{	
	public MinionTemplate template;
	public Image icon, typeIcon, newIcon, typeBackground, lancerHighlight;
	public int targetIndex = -1;
	public bool bAvailable = false;
	public bool bNew = false;
	public float fNewBlink = 0.0f;

	public void OnGUI()
	{
		icon.enabled = targetIndex != -1;
		typeIcon.enabled = targetIndex != -1;
		typeBackground.enabled = targetIndex != -1;
		newIcon.enabled = bNew && targetIndex != -1;
		lancerHighlight.enabled = template.bIsLancer && Core.GetPlayerProfile().tutorialState == TutorialManager.TutorialState.RETURN_TO_WORLD;

		fNewBlink += Time.deltaTime * 3.0f;
		newIcon.color = new Color (1.0f, 1.0f, 1.0f, Mathf.Sin(fNewBlink) * 0.5f + 0.5f);

		UpdatePosition(5.0f * Time.deltaTime);
	}

	private void UpdatePosition(float fLerp)
	{
		int iRow = targetIndex / 6;
		int iCol = targetIndex % 6;
		// Square grid
		Vector3 targetPos = new Vector3 (70.0f + 128.0f * iCol, -70.0f - 128.0f * iRow, 0.0f);
		// Diamond grid
		//Vector3 targetPos = new Vector3 (70.0f + 124.0f * iCol, -70.0f - 62.0f * iRow, 0.0f);
		//if (iRow % 2 == 1)
		//	targetPos.x += 62.0f;
		RectTransform rt = GetComponent<RectTransform>();

		rt.localPosition = Vector3.Lerp(rt.localPosition, targetPos, fLerp);
	}

	public void SetIndex(int index)
	{
		if (targetIndex == -1)
		{
			targetIndex = index;
			UpdatePosition(1.0f);
		}
		else
		{
			targetIndex = index;
		}
	}

	public void SetAvailable(bool bSet)
	{
		bAvailable = bSet;
		icon.color = bSet ? Color.white : Color.gray;
		typeBackground.color = bSet ? Color.white : Color.gray;
		typeIcon.color = bSet ? Color.white : Color.gray;
	}

	public void SetNew(bool bSet)
	{
		bNew = bSet;
		newIcon.enabled = bSet;
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		TeamBuilder.instance.HoverPoolEntry(this);
		if (bNew)
		{
			bNew = false;
			newIcon.enabled = false;
			Core.GetPlayerProfile().pool.SetNotNew(template);
		}

		if (!bAvailable)
		{
			foreach (Location location in Core.GetWorldMap().locations)
			{
				for (int i = 0 ; i < location.numWaves; i++)
				{
					if (location.unlocks[i] == template)
					{
						string hoverText = string.Format(LocalizationManager.GetLoc("UNLOCK_CONDITION"), (i + 1), LocalizationManager.GetLoc(location.unlocName));
						WorldMapSlider.instance.SetLocalizedHoverText(hoverText);
					}
				}
			}
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		TeamBuilder.instance.UnhoverPoolEntry();
		WorldMapSlider.instance.SetHoverText("");
	}
}
