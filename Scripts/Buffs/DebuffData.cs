using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DebuffData 
{
	public enum Debuff
	{
		NO_DEBUFF,
		FREEZE,
		DOUBLE_MELEE_DAMAGE,
		SLOW,
		DOUBLE_NEXT_HIT,
		SPOTTED, // All hits are crits
		STUNNED,
	}

	public Debuff debuff = Debuff.NO_DEBUFF;
	public float fTimeRemaining = 1.0f;
	public PFX_DebuffIcon debuffIcon;

	public DebuffData(DebuffData old)
	{
		debuff = old.debuff;
		fTimeRemaining = old.fTimeRemaining;
		debuffIcon = old.debuffIcon;
	}

	public DebuffData() {}
}
