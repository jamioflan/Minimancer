using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TeamBuilder : MonoBehaviour 
{
	public static TeamBuilder instance;

	public MinionSlotType currentlyComparing = MinionSlotType.SUPPORT;
	public MinionStatBlock[] comparisonBlocks = new MinionStatBlock[3];
	public MinionStatBlock hoverBlock;
	public MinionIcon[] minionIcons;

	public Filters filters;
	public InputField teamName;

	public TeamRoster currentRoster;

	public MinionPoolGUI minionPool;
	public RangedPrioritySelector[] prioritySelectors = new RangedPrioritySelector[2];
	public Image dragDrop;
	public MinionTemplate dragDropSelection = null;
	public MinionSlot hoveringOverMinion = MinionSlot.NUM_MINION_SLOTS;
	public MinionPoolGUIEntry hoveringOverPoolEntry = null;
	public GameObject glossary;

	public Sprite[] teamBuilderBackgrounds = new Sprite[5];

	// Use this for initialization
	void Start () 
	{
		Debug.Assert(instance == null, "Two instances???");
		instance = this;

		foreach (MinionStatBlock statBlock in comparisonBlocks)
		{
			statBlock.parent = this;
		}
	}
	
	// Update is called once per frame
	void Update () 
	{
	}

	public void EditRoster(int index)
	{
		GetComponent<Image>().sprite = teamBuilderBackgrounds [index];
		currentRoster = Core.GetPlayerProfile().GetRoster(index);
		UpdateMinionIcons();
		minionPool.InitList(currentRoster);
		InitRangedPrioritySelectors();
		teamName.text = currentRoster.teamName;
		Core.GetPlayerProfile().iSelectedIndex = index;
		if (WorldMapSlider.instance.IsOnTeamBuilder())
		{
			WorldMapSlider.instance.OnChangeSelectionInTeamBuilder();
		}
		WorldMapSlider.instance.SlideToTeamBuilder();
	}

	public void SelectRoster(int index)
	{
		Core.GetPlayerProfile().iSelectedIndex = index;
		WorldMapSlider.instance.SlideToWorldMap();
	}


	public void SetRosterName(string s)
	{
		currentRoster.teamName = s;
	}

	void OnGUI()
	{
		// Set up drag-drop cursor
		Vector3 v = GUIUtility.ScreenToGUIPoint(Event.current.mousePosition);
		dragDrop.rectTransform.position = new Vector3 (v.x, Screen.height - v.y, v.z);
		dragDrop.enabled = (dragDropSelection != null);
		if (dragDropSelection != null)
		{
			dragDrop.sprite = dragDropSelection.icon;
		}

		// Listen for events
		if (Event.current.isMouse)
		{
			switch (Event.current.type)
			{
				case EventType.MouseDown:
				{
					Debug.Assert(dragDropSelection == null, "How can we be clicking when we already have a selection?");
					if (hoveringOverPoolEntry != null && hoveringOverPoolEntry.bAvailable)
					{
						dragDropSelection = hoveringOverPoolEntry.template;
						foreach (MinionIcon icon in minionIcons)
						{
							icon.SetSlotDroppable(dragDropSelection.GetSlotType() == icon.index.GetSlotType());
						}
						Core.GetAudioManager().PlayPickupMinion();
					}
					break;
				}
				case EventType.MouseUp:
				{
					if (dragDropSelection != null)
					{
						if (hoveringOverMinion == MinionSlot.NUM_MINION_SLOTS)
						{
							// Meh, do nothing. It returns to the pool automatically
						}
						else
						{
							if (hoveringOverMinion.GetSlotType() == dragDropSelection.GetSlotType())
							{
								currentRoster.SetMinion(hoveringOverMinion, dragDropSelection);
								Core.GetAudioManager().PlayDropMinion();
								UpdateMinionIcons();
								SetComparisonType(currentlyComparing);
							}
							else
							{
								Core.GetAudioManager().PlayGUIReject();
							}
						}

						//hoverBlock.SetMinion(null);
						dragDropSelection = null;
						foreach (MinionIcon icon in minionIcons)
						{
							icon.SetSlotDroppable(true);
						}
					}
					break;
				}
			}
		}
	}

	public bool HoverMinion(int index)
	{
		MinionSlotType slotType = ((MinionSlot)index).GetSlotType();

		if (dragDropSelection != null && dragDropSelection.GetSlotType() != slotType)
			return false;

		if (currentlyComparing != slotType)
		{
			SetComparisonType(slotType);
		}

		int subIndex = index - (int)slotType.GetFirst();
		for (int i = 0; i < 3; i++)
		{
			comparisonBlocks [i].SetHighlighted(i == subIndex);
		}

		hoveringOverMinion = (MinionSlot)index;

		return true;
	}

	public void UnhoverMinion()
	{
		for (int i = 0; i < 3; i++)
		{
			comparisonBlocks [i].SetHighlighted(false);
		}

		hoveringOverMinion = MinionSlot.NUM_MINION_SLOTS;
	}

	public void HoverPoolEntry(MinionPoolGUIEntry entry)
	{
		if (dragDropSelection == null)
		{
			hoveringOverPoolEntry = entry;
			hoverBlock.SetMinion(entry.template);
			SetComparisonType(entry.template.GetSlotType());
		}
	}

	public void UnhoverPoolEntry()
	{
		hoveringOverPoolEntry = null;
		if (dragDropSelection == null)
		{
			hoverBlock.SetMinion(null);
		}
	}

	public void SetComparisonType(MinionSlotType slotType)
	{
		if (currentRoster == null)
		{
			Debug.Assert(false, "Trying to edit null roster!");
			return;
		}

		for (int i = 0; i < slotType.GetNumSlots(); i++)
		{
			comparisonBlocks [i].gameObject.SetActive(true);
			comparisonBlocks [i].SetMinion(currentRoster.minions [(int)(slotType.GetFirst()) + i].template, (MinionSlot)((int)(slotType.GetFirst()) + i));
		}
		for (int i = slotType.GetNumSlots(); i < 3; i++)
		{
			comparisonBlocks [i].gameObject.SetActive(false);
		}

		currentlyComparing = slotType;
	}

	public void UpdateMinionIcons()
	{
		for (int i = 0; i < (int)MinionSlot.NUM_MINION_SLOTS; i++)
		{
			minionIcons [i].SetMinion(currentRoster.minions [i]);
		}
	}

	public void InitRangedPrioritySelectors()
	{
		prioritySelectors [0].SelectPriority((int)currentRoster.GetMinion(MinionSlot.RANGED_1).priority);
		prioritySelectors [1].SelectPriority((int)currentRoster.GetMinion(MinionSlot.RANGED_2).priority);
	}

	public void OpenGlossary()
	{
		Core.GetAudioManager().PlayGUIClick();
		glossary.SetActive(true);
	}

	public void CloseGlossary()
	{
		Core.GetAudioManager().PlayGUIClick();
		glossary.SetActive(false);
	}
}
