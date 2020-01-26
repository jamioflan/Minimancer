using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffEffect_BuffTimescale : BuffEffect
{
	public float fModifier;

	public override float GetModifier(Stat eStat) 
	{
		return 0.0f;
	}

	public override void OnTriggered(Buff buff, Actor actor)
	{
		Core.BuffPlayerTimescale(fModifier);
	}

}
