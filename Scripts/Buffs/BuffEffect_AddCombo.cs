using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffEffect_AddCombo : BuffEffect
{
	public int iAmount;

	public override float GetModifier(Stat eStat) 
	{
		return 0.0f;
	}

	public override void OnTriggered(Buff buff, Actor actor)
	{
		if (buff == null || buff.targetRanged)
		{
			Core.GetCurrentRoster().AddComboToGroup(MinionSlotType.RANGED, iAmount);
		}
		if (buff == null || buff.targetMelee)
		{
			Core.GetCurrentRoster().HealGroup(MinionSlotType.MELEE, iAmount);
		}
	}
		
}
