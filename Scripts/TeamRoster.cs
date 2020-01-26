using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization;

public class TeamRoster : MonoBehaviour {

	public Minion[] minions = new Minion[(int)MinionSlot.NUM_MINION_SLOTS];

	public float[] afGroupMaxHealths = new float[(int)MinionSlotType.NUM_MINION_SLOT_TYPES];
	public float[] afGroupHealths = new float[(int)MinionSlotType.NUM_MINION_SLOT_TYPES];

	public Minion GetMinion(MinionSlot slot) { return minions [(int)slot]; }
	public Minion GetSupport(int i) { return minions [(int)MinionSlot.SUPPORT_1 + i]; }
	public Minion GetMelee(int i) { return minions[(int)MinionSlot.MELEE_1 + i]; }
	public Minion GetRanged(int i) { return minions[(int)MinionSlot.RANGED_1 + i]; }

	// Make player minions inherently 2x stronger. Allows for bigger waves of enemies, rather than facing off 1v1.
	public static readonly float fRangedIncomingDamageModifier = 0.25f, fMeleeIncomingDamageModifier = 0.5f;

	public bool bDirty = false;
	public bool bHasTriggeredVolatileYet = false;
	public bool bHasThreeResurrectsAvailable = false;
	public bool bHasActiveCollector = false;

	public string teamName = "";

	public Actor_Enemy mostRecentRangedAttacker = null;

	void Start () 
	{
		
	}

	void Update () 
	{
		
	}

	public void ReadFrom(SerializationInfo data, string prefix)
	{
		int version = data.GetInt32(prefix + "Version");
		if (version >= 1)
		{
			teamName = data.GetString(prefix + "TeamName");

			for (int i = 0; i < (int)MinionSlot.NUM_MINION_SLOTS; i++)
			{
				int hashCode = data.GetInt32(prefix + "Minion" + i + ".Hash");
				minions [i] = Core.GetMinionTemplateManager().CreateMinion(((MinionSlot)i).GetSlotType(), hashCode);

				minions [i].transform.SetParent(transform);
				minions [i].transform.localPosition = Vector3.zero;
				minions [i].slot = (MinionSlot)i;
				minions [i].ReadFrom(data, prefix + "Minion" + i + ".");
			}
		}
	}

	public void WriteTo(SerializationInfo data, string prefix)
	{
		data.AddValue(prefix + "Version", 1);

		// Version 1
		data.AddValue(prefix + "TeamName", teamName);

		for (int i = 0; i < (int)MinionSlot.NUM_MINION_SLOTS; i++)
		{
			data.AddValue(prefix + "Minion" + i + ".Hash", minions[i].template.GetHashCode());
			minions [i].WriteTo(data, prefix + "Minion" + i + ".");
		}
	}

	public Minion SetMinion(MinionSlot slot, MinionTemplate template)
	{
		Minion minion = Core.GetMinionTemplateManager().CreateMinion(template);
		minion.transform.SetParent(transform);
		minion.transform.localPosition = Vector3.zero;
		minion.slot = slot;

		minions [(int)slot] = minion;
		RecalculateHealths();
		bDirty = true;

		return minion;
	}

	public void RecalculateHealths()
	{
		for (int i = 0; i < (int)MinionSlotType.NUM_MINION_SLOT_TYPES; i++)
		{
			MinionSlotType slotType = (MinionSlotType)i;
			RecalculateHealths(slotType);
		}

		bHasTriggeredVolatileYet = false;
	}

	public void RecalculateHealths(MinionSlotType slotType)
	{
		afGroupMaxHealths [(int)slotType] = 0.0f;

		for (int j = 0; j < slotType.GetNumSlots(); j++)
		{
			MinionSlot slot = (MinionSlot)((int)slotType.GetFirst() + j);
			if (minions [(int)slot] != null)
			{
				afGroupMaxHealths [(int)slotType] += minions [(int)slot].fMaxHealthPostBuffs;
			}
		}

		afGroupHealths [(int)slotType] = afGroupMaxHealths [(int)slotType];
	}

	public float MeleeAttack(Actor_Enemy attacker, Damage damage, MinionSlot slot)
	{
		return Attack(attacker, damage, fMeleeIncomingDamageModifier, MinionSlotType.MELEE, slot);
	}

	public float RangedAttack(Actor_Enemy attacker, Damage damage, MinionSlot slot)
	{
		mostRecentRangedAttacker = attacker;
		return Attack(attacker, damage, fRangedIncomingDamageModifier, MinionSlotType.RANGED, slot);
	}

	private float Attack(Actor_Enemy attacker, Damage damage, float fMod, MinionSlotType slotType, MinionSlot slot)
	{
		float fTotalDealt = 0.0f;
		
		float fDamage = damage.fAmount * fMod;
		if (fDamage > 0.0f)
		{
			float fTotalMultiplierFromMinions = 0.0f;
			for (int i = 0; i < slotType.GetNumSlots(); i++)
			{
				Minion minion = minions [(int)slotType.GetFirst() + i];
				if (minion != null)
				{
					float fBuffModifier = minion.GetBuff(Stats.GetStatForDamageMultiplier(damage.GetElement()));
					fBuffModifier += minion.GetBuff(Stat.DAMAGE_MULTIPLIER);
					fBuffModifier += minion.GetBuff(slotType == MinionSlotType.MELEE ? Stat.DAMAGE_MULTIPLIER_MELEE : Stat.DAMAGE_MULTIPLIER_RANGED);
					fBuffModifier += minion.GetBuff(Stat.DAMAGE_MULTIPLIER_PER_COMBO) * minion.iCombo;

					fTotalMultiplierFromMinions += (1.0f + fBuffModifier) * Elements.GetDamageMultiplier(damage.GetElement(), minion.template.element);
				}
				else
				{
					fTotalMultiplierFromMinions += 1.0f;
				}
			}
			float fAverageMinionMultiplier = fTotalMultiplierFromMinions / slotType.GetNumSlots();
			fDamage *= fAverageMinionMultiplier;

			fDamage *= (1.0f + Mathf.Clamp(damage.fRadius, 0.0f, 5.0f));
			if (attacker != null && attacker.minion.template.canCombo)
			{
				fDamage *= 3.0f;
			}

			float fDamageToDeal = Mathf.Min(afGroupHealths [(int)slotType], fDamage);
			if (fDamageToDeal > 0.0f)
			{
				float fParametricHealthPreDamage = afGroupHealths[(int)slotType] / afGroupMaxHealths[(int)slotType];
				afGroupHealths [(int)slotType] -= fDamageToDeal;

				float fParametricHealthPostDamage = afGroupHealths[(int)slotType] / afGroupMaxHealths[(int)slotType];
				if (!bHasTriggeredVolatileYet && fParametricHealthPreDamage > 0.25f && fParametricHealthPostDamage <= 0.25f)
				{
					bHasTriggeredVolatileYet = true;
					for (int i = 0; i < slotType.GetNumSlots(); i++)
					{
						if (minions [(int)slotType.GetFirst() + i].template.bVolatile)
						{
							// TODO: Play PFX
							// Play sound
							Core.GetLevel().KillAllEnemies();
						}
					}
				}

				// Do damage numbers
				Actor_Player actorHit = null;
				if (slot == MinionSlot.NUM_MINION_SLOTS)
				{
					actorHit = Core.GetLevel().playerActors [(int)slotType.GetFirst() + Random.Range(0, slotType.GetNumSlots())];
				}
				else
				{
					actorHit = Core.GetLevel().playerActors [(int)slot];
				}
				if (actorHit != null)
				{
					int iDamage = Mathf.FloorToInt(fDamage);
					actorHit.MakeDamageNumbers(iDamage, Core.GetMinionTemplateManager().playerDamage);
				}

				if (afGroupHealths [(int)slotType] <= 0.0f)
				{
					KillGroup(slotType);
				}
				fDamage -= fDamageToDeal;
				fTotalDealt += fDamageToDeal;

				Core.GetLevel().bHasSomeDamageBeenTaken = true;
			}
		}

		return fTotalDealt;
	}

	private void KillGroup(MinionSlotType slotType)
	{
		foreach (Actor_Player actor in Core.GetLevel().playerActors)
		{
			actor.minion.template.TriggerBuff(slotType == MinionSlotType.MELEE ? BuffTrigger.PLAYER_MELEE_DIED : BuffTrigger.PLAYER_RANGED_DIED, null);
		}

		List<Resurrection> resList = Core.GetLevel().singleUseResurrections;
		for(int i = 0; i < resList.Count; i++)
		{
			Resurrection res = resList [i];
			if (res.slotToRes == slotType)
			{
				resList.RemoveAt(i);

				Core.GetAudioManager().PlayResSFX(res.resSound);

				// Do achievements
				Core.TriggerAchievement("BACK_FROM_THE_DEAD");
				Core.IncrementStat("NUM_RESURRECTIONS", 2);
				if (bHasThreeResurrectsAvailable)
				{
					bool bLastResUsed = true;
					foreach(Resurrection check in resList)
					{
						if (check.slotToRes == slotType)
						{
							bLastResUsed = false;
						}
					}
					if (bLastResUsed)
					{
						Core.TriggerAchievement("AND_HERE_TO_STAY");
					}
				}

				for (int j = 0; j < slotType.GetNumSlots(); j++)
				{
					MinionSlot slot = (MinionSlot)((int)slotType.GetFirst() + j);
					minions [(int)slot].fMaxHealthPreBuffs *= res.fMaxHealthModifier;
					minions [(int)slot].fMaxHealthPostBuffs *= res.fMaxHealthModifier;
					minions [(int)slot].isZombified = true;

					// Play PFX
					PFX_DebuffIcon pfx = Instantiate<PFX_DebuffIcon>(res.resPFX);
					pfx.transform.SetParent(Core.GetLevel().playerActors [(int)slot].transform);
					pfx.transform.localPosition = new Vector3 (0.0f, 0.0f, -0.25f);
					pfx.Init(true);
				}

				RecalculateHealths(slotType);

				return;
			}
		}

		for (int i = 0; i < slotType.GetNumSlots(); i++)
		{
			MinionSlot slot = (MinionSlot)((int)slotType.GetFirst() + i);
			KillMinion(slot);
		}

		Core.GetLevel().PlayerLost();
	}

	private void KillMinion(MinionSlot slot)
	{
		//Minion minionToKill = minions [(int)slot];

		// Kill player actor. They can take their time and do animations or whatever
		Core.GetLevel().KillPlayerActor(slot);
		// Remove the minion from the minion pool
		//Core.GetPlayerProfile().pool.Remove(minionToKill);
		// Remove the minion from the current roster
		//minions[(int)slot] = null;
		// Now destroy the minion
		//Destroy(minionToKill.gameObject);
	}
		
	public bool IsAnyoneAlive(MinionSlotType slotType)
	{
		if (Core.GetLevel() == null)
			return false;

		bool bAnyoneAlive = false;

		for (int i = 0; i < slotType.GetNumSlots(); i++)
		{
			Actor_Player player = Core.GetLevel().playerActors [(int)slotType.GetFirst() + i];
			//Debug.Assert(player == null || player.minion != null);
			if(player != null && player.minion != null && player.minion.fCurrentHealth > 0.0f)
			{
				bAnyoneAlive = true;
			}	
		}

		return bAnyoneAlive;
	}

	public void HealGroup(MinionSlotType slotType, float fAmount)
	{
		float fTotalMultiplier = 0.0f;
		for (int i = 0; i < slotType.GetNumSlots(); i++)
		{
			Minion minion = minions [(int)slotType.GetFirst() + i];
			fTotalMultiplier += (1.0f + minion.GetBuff(Stat.HEAL_MULTIPLIER));
		}
		fTotalMultiplier /= slotType.GetNumSlots();

		float fAmountToHeal = Mathf.Clamp(fAmount * fTotalMultiplier, 0.0f, afGroupMaxHealths [(int)slotType] - afGroupHealths [(int)slotType]);

		afGroupHealths [(int)slotType] += fAmountToHeal;

		Core.IncrementStat("HEALTH_HEALED", Mathf.FloorToInt(fAmountToHeal));
	}

	public void AddComboToGroup(MinionSlotType slotType, int iAmount)
	{
		for (int i = 0; i < slotType.GetNumSlots(); i++)
		{
			Minion minion = minions [(int)slotType.GetFirst() + i];
			if (minion != null && (minion.template.canCombo || minion.template.bDeathtoll))
			{
				for (int j = 0; j < iAmount; j++)
				{
					minion.template.IncrementCombo(Core.GetLevel().playerActors [(int)slotType.GetFirst() + i]);
				}
			}
		}
	}

	public bool Contains(Minion minion)
	{
		foreach (Minion min in minions)
		{
			if (min == minion)
				return true;
		}

		return false;
	}

	public int GetNumZombies()
	{
		int numZoms = 0;
		foreach (Minion minion in minions)
		{
			if (minion.template.isZombie || minion.isZombified)
				numZoms++;
		}

		return numZoms;
	}
}
