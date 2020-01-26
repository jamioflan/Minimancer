using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffEffect_Damage : BuffEffect 
{
	public Damage damage;

	public override float GetModifier(Stat eStat) 
	{
		return 0.0f;
	}

	public override void OnTriggered(Buff buff, Actor actor)
	{
		actor.Damage(damage, 1.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f);
	}
}
