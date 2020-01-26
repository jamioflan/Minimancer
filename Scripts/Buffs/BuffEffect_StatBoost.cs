using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffEffect_StatBoost : BuffEffect 
{
	public Stat stat;
	public float fModifier = 0.0f;

	public override float GetModifier(Stat eStat) 
	{
		return stat == eStat ? fModifier : 0.0f;
	}

	public override void OnTriggered(Buff parent, Actor actor) {} // Nothing, this is a passive buff
}
