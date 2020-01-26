using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class DevMinionSelector : MonoBehaviour 
{
	public MinionSlot slot = MinionSlot.MELEE_1;
	public Image icon;
	public Text description, statBlock;
	public Dropdown minionSelector;

	void Start () 
	{
		if (minionSelector != null)
		{
			List<string> options = new List<string>();

			foreach(MinionTemplate template in Core.GetMinionTemplateManager().GetMinionList(slot.GetSlotType()))
			{
				options.Add(template.name);
			}

			minionSelector.AddOptions(options);

			minionSelector.value = 1;
			minionSelector.value = 0;
		}
	}

	void Update()
	{
		MinionTemplateManager mtm = Core.GetMinionTemplateManager();
		List<MinionTemplate> list = mtm.GetMinionList(slot.GetSlotType());

		MinionTemplate dropdownSelection = mtm.GetMinionList(slot.GetSlotType()) [minionSelector.value];
		Minion currentSelection = Core.GetPlayerProfile().rosters[0].minions [(int)slot];
		if (currentSelection.template != dropdownSelection)
		{
			for (int i = 0; i < list.Count; i++)
			{
				if (list [i] == currentSelection.template)
				{
					minionSelector.value = i;
					return;
				}
			}
		}
	}

	public void SelectMinion(int iSelection)
	{
		Minion oldMinion = Core.GetPlayerProfile().rosters [0].GetMinion(slot);
		if (oldMinion != null)
		{
			Destroy(oldMinion.gameObject);
		}
			
		MinionTemplateManager mtm = Core.GetMinionTemplateManager();
		Minion minion = Core.GetPlayerProfile().rosters [0].SetMinion(slot, mtm.GetMinionList(slot.GetSlotType()) [iSelection]);

		icon.sprite = minion.template.icon;
		description.text = minion.template.debugDescriptionText;

		if (slot.GetSlotType() != MinionSlotType.SUPPORT)
		{
			statBlock.text = "HP: " + minion.template.fMaxHealth + ", Damage: " + minion.template.damage.fAmount + ", Attack Speed: " + (1.0f / minion.template.fAttackInterval);
		}
	}

	public void SelectRangedPriority(int iSelection)
	{
		Minion minion = Core.GetPlayerProfile().rosters [0].GetMinion(slot);
		minion.priority = (TargetPriority)iSelection;
	}
}
