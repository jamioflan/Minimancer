using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffEffect_Heal : BuffEffect
{
	public float fAmount,
	fAmountParametric,
	fAmountPerCombo;

	public override float GetModifier(Stat eStat) 
	{
		return 0.0f;
	}

	public override void OnTriggered(Buff buff, Actor actor)
	{
		float fHealAmount = fAmount;
		fHealAmount += (actor == null ? 0.0f : actor.minion.iCombo * fAmountPerCombo);

		if (buff == null || buff.targetRanged)
		{
			Core.GetCurrentRoster().HealGroup(MinionSlotType.RANGED, 
				fHealAmount + fAmountParametric * Core.GetCurrentRoster().afGroupMaxHealths[(int)MinionSlotType.RANGED]);
		}
		if (buff == null || buff.targetMelee)
		{
			Core.GetCurrentRoster().HealGroup(MinionSlotType.MELEE, 
				fHealAmount + fAmountParametric * Core.GetCurrentRoster().afGroupMaxHealths[(int)MinionSlotType.MELEE]);
		}
	}
		
}
