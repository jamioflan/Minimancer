using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BuffEffect : MonoBehaviour 
{
	public abstract float GetModifier(Stat eStat);

	public abstract void OnTriggered(Buff parent, Actor actor);
}
