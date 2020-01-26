using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Filters : MonoBehaviour 
{
	public static Filters instance;

	public MinionPoolGUI poolGUI;

	public bool[] abElementalFilters = new bool[(int)Element.NUM_ELEMENTS]{false, false, false, false, false, false, false, false};
	public bool[] abSlotTypeFilters = new bool[(int)MinionSlotType.NUM_MINION_SLOT_TYPES]{false, false, false};
	public bool bShowLocked = true;
	public bool bShowNewOnly = false;

	void Start () 
	{
		instance = this;
	}

	void Update () 
	{
		
	}

	public bool IsAnyElementalFilterActive()
	{
		foreach (bool bFilter in abElementalFilters)
		{
			if (bFilter)
				return true;
		}
		return false;
	}

	public bool IsAnySlotTypeFilterActive()
	{
		foreach (bool bFilter in abSlotTypeFilters)
		{
			if (bFilter)
				return true;
		}
		return false;
	}

	public void SetPhysical(bool bSet) { abElementalFilters [(int)Element.PHYSICAL] = bSet; poolGUI.ApplyFilters(this); }
	public void SetHoly(bool bSet) { abElementalFilters [(int)Element.HOLY] = bSet; poolGUI.ApplyFilters(this); }
	public void SetUnholy(bool bSet) { abElementalFilters [(int)Element.UNHOLY] = bSet; poolGUI.ApplyFilters(this); }
	public void SetFire(bool bSet) { abElementalFilters [(int)Element.FIRE] = bSet; poolGUI.ApplyFilters(this); }
	public void SetEarth(bool bSet) { abElementalFilters [(int)Element.EARTH] = bSet; poolGUI.ApplyFilters(this); }
	public void SetAir(bool bSet) { abElementalFilters [(int)Element.AIR] = bSet; poolGUI.ApplyFilters(this); }
	public void SetWater(bool bSet) { abElementalFilters [(int)Element.WATER] = bSet; poolGUI.ApplyFilters(this); }
	public void SetUnelemented(bool bSet) {abElementalFilters [(int)Element.NO_ELEMENT] = bSet; poolGUI.ApplyFilters(this); }

	public void SetMelee(bool bSet) { abSlotTypeFilters [(int)MinionSlotType.MELEE] = bSet; poolGUI.ApplyFilters(this); }
	public void SetSupport(bool bSet) { abSlotTypeFilters [(int)MinionSlotType.SUPPORT] = bSet; poolGUI.ApplyFilters(this); }
	public void SetRanged(bool bSet) { abSlotTypeFilters [(int)MinionSlotType.RANGED] = bSet; poolGUI.ApplyFilters(this); }

	public void SetLocked(bool bSet) { bShowLocked = bSet; poolGUI.ApplyFilters(this); }
	public void SetNew(bool bSet) { bShowNewOnly = bSet; poolGUI.ApplyFilters(this); }
}
