using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MinionPoolGUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
	public MinionPoolGUIEntry prefab;
	public Scrollbar scrollbar;
	public RectTransform contentBox;
	public bool bHover = false;
	public float fScrollWheelSensitivity = 1.0f;
	private List<MinionPoolGUIEntry> allEntries = new List<MinionPoolGUIEntry>();
	public Sprite[] typeIcons = new Sprite[3];
	public Sprite[] typeBackgrounds = new Sprite[3];

	public void InitList(TeamRoster currentRoster)
	{
		foreach (MinionPoolGUIEntry entry in allEntries)
		{
			Destroy(entry.gameObject);
		}
		allEntries.Clear();

		TeamPool pool = Core.GetPlayerProfile().pool;

		foreach (MinionTemplate template in Core.GetMinionTemplateManager().GetFullList())
		{
			MinionPoolGUIEntry entry = Instantiate<MinionPoolGUIEntry>(prefab);
			entry.template = template;
			entry.icon.sprite = template.icon;
			entry.typeIcon.sprite = typeIcons [(int)template.GetSlotType()];
			entry.typeBackground.sprite = typeBackgrounds [(int)template.GetSlotType()];
			entry.transform.SetParent(contentBox);
			entry.transform.localScale = new Vector3 (1.0f, 1.0f, 1.0f);
			allEntries.Add(entry);
		}

		UpdateList(pool);
		ApplyFilters(Filters.instance);
	}

	public void UpdateList(TeamPool pool)
	{
		foreach (MinionPoolGUIEntry entry in allEntries)
		{
			entry.SetAvailable(pool.unlocks.Contains(entry.template));
			entry.SetNew(pool.IsNew(entry.template));
		}
	}

	public void ScrollbarValueChanged()
	{
		float fMaxY = contentBox.sizeDelta.y;
		float fViewportY = 870.0f;

		float fParametric = scrollbar.value;

		if (fMaxY > fViewportY)
		{
			float fDelta = fMaxY - fViewportY;
			float fTarget = fDelta * fParametric;

			contentBox.localPosition = new Vector2 (0.0f, fTarget);
		}
		else
		{
			contentBox.localPosition = new Vector2 (0.0f, 0.0f);
		}
	}

	public void ApplyFilters(Filters filters)
	{
		bool bElementalFilterActive = filters.IsAnyElementalFilterActive();
		bool bSlotTypeFilterActive = filters.IsAnySlotTypeFilterActive();

		int index = 0;
		foreach (MinionPoolGUIEntry entry in allEntries)
		{
			MinionTemplate template = entry.template;
			bool bAllowed = (!bElementalFilterActive || filters.abElementalFilters [(int)template.element])
							&& (!bSlotTypeFilterActive || filters.abSlotTypeFilters [(int)template.GetSlotType()])
							&& (entry.bAvailable || filters.bShowLocked)
							&& (!filters.bShowNewOnly || entry.bNew);
			if (bAllowed)
			{
				entry.SetIndex(index);
				index++;
			}
			else
			{
				entry.SetIndex(-1);
			}
		}

		int numRows = (index + 5) / 6;

		contentBox.sizeDelta = new Vector2 (contentBox.sizeDelta.x, 128.0f * numRows + 16.0f);
		ScrollbarValueChanged();
	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void OnGUI()
	{
		if (bHover &&
			Event.current.isScrollWheel)
		{
			scrollbar.value += Event.current.delta.y * fScrollWheelSensitivity;
			scrollbar.value = Mathf.Clamp(scrollbar.value, 0.0f, 1.0f);
			ScrollbarValueChanged();
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		bHover = true;
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		bHover = false;
	}
}
