using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Buff : MonoBehaviour 
{
	[System.Serializable]
	public class Pair_BuffTrigger_BuffEffect
	{
		public BuffTrigger eTrigger;
		public BuffEffect effect;
	}
	
	// -- Buff target specification --
	public bool targetPhysical = true, 
		targetHoly = true,
		targetUnholy = true,
		targetFire = true,
		targetEarth = true,
		targetWater = true,
		targetAir = true,
		targetNoElement = true,
		targetMelee = true,
		targetSupport = false,
		targetRanged = true,
		targetOnlyZombies = false,
		targetEnemies = false,
		targetSelfOnly = false;

	// -- Buff effects --
	public List<BuffEffect> effects = new List<BuffEffect>();

	private Element element;

	public List<Pair_BuffTrigger_BuffEffect> triggeredEffects;

	public void Start ()
	{
	}

	public void ApplyActiveBuffs()
	{
		
	}

	public void SetElement(Element e)
	{
		element = e;
	}

	public Element GetElement() 
	{
		return element;
	}

	public void OnTrigger(BuffTrigger eTrigger, Actor triggerer)
	{
		foreach (Pair_BuffTrigger_BuffEffect pair in triggeredEffects)
		{
			if (pair.eTrigger == eTrigger)
			{
				pair.effect.OnTriggered(this, triggerer);
			}
		}
	}

	public bool ShouldApply(Element element, MinionSlotType type, bool bZombie, bool bEnemy)
	{
		if (targetSelfOnly)
			return false;

		if (targetEnemies != bEnemy)
			return false;

		if (targetOnlyZombies && !bZombie)
			return false;

		switch (type)
		{
			case MinionSlotType.MELEE:
				if (!targetMelee)
					return false;
				break;
			case MinionSlotType.SUPPORT:
				if (!targetSupport)
					return false;
				break;
			case MinionSlotType.RANGED:
				if (!targetRanged)
					return false;
				break;
		}

		switch (element)
		{
			case Element.PHYSICAL:
				return targetPhysical;
			case Element.HOLY:
				return targetHoly;
			case Element.UNHOLY:
				return targetUnholy;
			case Element.EARTH:
				return targetEarth;
			case Element.AIR:
				return targetAir;
			case Element.FIRE:
				return targetFire;
			case Element.WATER:
				return targetWater;
			case Element.NO_ELEMENT:
				return targetNoElement;
		}

		return false;
	}

	public float GetModifier(Stat eStat)
	{
		float fModifier = 0.0f;
		foreach (BuffEffect effect in effects)
		{
			fModifier += effect.GetModifier(eStat);
		}
		return fModifier;
	}
}
