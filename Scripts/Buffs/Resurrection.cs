using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Resurrection 
{
	public float fMaxHealthModifier = 1.0f;
	public bool bZombify = true;
	public MinionSlotType slotToRes = MinionSlotType.MELEE;
	public PFX_DebuffIcon resPFX;
	public AudioClip resSound;

	public Resurrection() {}
	public Resurrection(Resurrection other)
	{
		fMaxHealthModifier = other.fMaxHealthModifier;
		bZombify = other.bZombify;
		slotToRes = other.slotToRes;
		resPFX = other.resPFX;
		resSound = other.resSound;
	}
}
