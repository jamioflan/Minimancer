using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffEffect_Debuff : BuffEffect
{
	public DebuffData data;
	public float fDuration = 1000.0f;

	public override float GetModifier(Stat eStat) 
	{
		return 0.0f;
	}

	public override void OnTriggered(Buff buff, Actor actor)
	{
		PFX_DebuffIcon debuffIcon = actor.minion.ApplyDebuff(data, fDuration);
		if (debuffIcon != null)
		{
			debuffIcon.transform.SetParent(actor.transform);
			debuffIcon.transform.localPosition = Vector3.zero;
		}
	}
}
