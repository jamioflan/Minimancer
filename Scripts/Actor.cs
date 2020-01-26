using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Actor : MonoBehaviour 
{
	public Minion minion;
	public float fTimeSinceLastAttack = 100.0f;
	public RenderActor render;
	public float fPlayerTimeSpentAttacking = 0.0f;
	public RenderActor summon;
	public int iNumHitsWithSummon = 0;
	public PFX_SupportAura auraPFX;
	public float fTimeToNextAura = 0.0f;

	public float fTimeInPlay = 0.0f;
	public float fDamageDealt = 0.0f;

	public AudioSource soundEffect;

	public ComboNumbers comboNumbers;

	public DamageNumbers baseNumbers = null;

	private bool bEnemy;

	public bool IsDead()
	{
		return minion.fCurrentHealth <= 0.0f;
	}

	protected virtual void Start () 
	{
		bEnemy = this is Actor_Enemy;
	}

	protected virtual void Update () 
	{		
		if (Core.GetLevel().GetNumEnemiesInMeleeZone() > 0)
		{
			fPlayerTimeSpentAttacking += Core.GetPlayerDeltaTime();
			minion.template.SetRelentlessCombo(Mathf.FloorToInt(fPlayerTimeSpentAttacking), this);
		}
		else
		{
			minion.template.ResetCombo(this);
			fPlayerTimeSpentAttacking = 0.0f;
		}

		float fDeltaTime = bEnemy ? Core.GetEnemyDeltaTime() : Core.GetPlayerDeltaTime();

		fTimeInPlay += fDeltaTime;

		List<DebuffData> toRemove = new List<DebuffData> ();
		foreach (DebuffData data in minion.timedDebuffs)
		{
			data.fTimeRemaining -= fDeltaTime;
			if (data.fTimeRemaining <= 0.0f)
			{
				toRemove.Add(data);
				Destroy(data.debuffIcon.gameObject);
			}
		}
		foreach (DebuffData data in toRemove)
		{
			minion.timedDebuffs.Remove(data);
		}
	}

	public virtual void InitFromMinion(Minion newMinion)
	{
		minion = newMinion;
		minion.currentBuffs.Clear();

		// Add self buffs
		foreach (Buff buff in minion.template.passiveBuffs)
		{
			if (buff.targetSelfOnly)
			{
				minion.currentBuffs.Add(buff);
			}
		}
	}

	public virtual void CalculateMyAggregateBuffs()
	{
		
	}

	public void MakeDamageNumbers(int iDamage, DamageNumbers prefab)
	{
		DamageNumbers dn = Instantiate<DamageNumbers>(prefab);
		dn.transform.position = transform.position + new Vector3 (0.0f, minion.template.fHeight, 0.0f);
		dn.Init(iDamage);		
		if (baseNumbers != null)
		{
			baseNumbers.Bump();
			dn.nextNumbers = baseNumbers;
		}
		baseNumbers = dn;
	}

	public void OnDealtDamage(float fDamage)
	{
		fDamageDealt += fDamage;
	}

	public abstract bool Damage(Damage damage, float fMultiplier, float fStunTime, float fAdditionalCritChance, float fAdditionalCritMultiplier, float fVampirism, float fAdditionalDamage);

	public float GetAttackDamageMultiplier()
	{
		float fModifier = 1.0f;
		fModifier += minion.GetBuff(Stat.ATTACK_DAMAGE);
		fModifier += minion.GetBuff(Stat.ATTACK_DAMAGE_PER_COMBO) * minion.iCombo;
		fModifier += minion.GetBuff(Stat.ATTACK_DAMAGE_PER_ENEMY_IN_MELEE_ZONE) * Core.GetLevel().GetNumEnemiesInMeleeZone();
		fModifier += minion.GetBuff(Stat.ATTACK_DAMAGE_PER_SECOND_ATTACKING) * (fPlayerTimeSpentAttacking) * (1.0f + minion.GetBuff(Stat.RELENTLESS_MULTIPLIER));
		fModifier += minion.GetBuff(Stat.ATTACK_DAMAGE_PER_ZOMBIE_ALLY) * Core.GetCurrentRoster().GetNumZombies();
		return fModifier;
	}

	public float GetAttackRadiusMultiplier()
	{
		float fModifier = 1.0f;
		fModifier += minion.GetBuff(Stat.ATTACK_AOE_SIZE);
		return fModifier;
	}

	public float GetAttackSpeedMultiplier()
	{
		float fModifier = 1.0f;
		fModifier += minion.GetBuff(Stat.ATTACK_SPEED);
		fModifier += minion.GetBuff(Stat.ATTACK_SPEED_PER_COMBO) * minion.iCombo;
		return 1.0f / fModifier;
	}

	public float GetMoveSpeedMultiplier()
	{
		float fModifier = 1.0f;
		fModifier += minion.GetBuff(Stat.MOVE_SPEED);
		return fModifier;
	}

	public float GetMaxHealthModifier()
	{
		float fModifier = 1.0f;
		fModifier += minion.GetBuff(Stat.MAX_HEALTH);
		return fModifier;
	}

	public float GetInherentElementalResistanceModifier()
	{
		float fModifier = 1.0f;
		fModifier += minion.GetBuff(Stat.INHERENT_RESISTANCE_MULTIPLIER);
		Debug.Assert(fModifier <= 1.0f, "Don't support this " + fModifier);

		if (fModifier < 0.0f)
			fModifier = 0.0f;

		return fModifier;
	}

	public float GetChainCountMultiplier()
	{
		float fModifier = 1.0f;
		fModifier += minion.GetBuff(Stat.CHAIN_COUNT_MULTIPLIER);
		return fModifier;
	}

	public float GetChainGapMultiplier()
	{
		float fModifier = 1.0f;
		fModifier += minion.GetBuff(Stat.CHAIN_GAP_MULTIPLIER);
		return fModifier;
	}

	public float GetStunTime()
	{
		float fTime = 0.0f;
		fTime += minion.GetBuff(Stat.STUN_TIME);
		return fTime;
	}

	public bool IsFrozen()
	{
		foreach (DebuffData debuff in minion.timedDebuffs)
		{
			if (debuff.debuff == DebuffData.Debuff.FREEZE)
				return true;
		}
		return false;
	}

	public bool IsSlowed()
	{
		foreach (DebuffData debuff in minion.timedDebuffs)
		{
			if (debuff.debuff == DebuffData.Debuff.SLOW)
				return true;
		}
		return false;
	}

	public bool IsSpotted()
	{
		foreach (DebuffData debuff in minion.timedDebuffs)
		{
			if (debuff.debuff == DebuffData.Debuff.SPOTTED)
				return true;
		}
		return false;
	}

	public bool IsStunned()
	{
		foreach (DebuffData debuff in minion.timedDebuffs)
		{
			if (debuff.debuff == DebuffData.Debuff.STUNNED)
				return true;
		}
		return false;
	}

	public float GetMeleeDamageModifierFromDebuffs()
	{
		float fDamageModifier = 1.0f;
		foreach (DebuffData debuff in minion.timedDebuffs)
		{
			if (debuff.debuff == DebuffData.Debuff.DOUBLE_MELEE_DAMAGE)
				fDamageModifier += 1.0f;
		}
		return fDamageModifier;
	}

	public float GetDamageModifierFromDebuffs()
	{
		float fDamageModifier = 1.0f;
		foreach(DebuffData data in minion.timedDebuffs)
		{
			if (data.debuff == DebuffData.Debuff.DOUBLE_NEXT_HIT)
			{
				fDamageModifier += 1.0f;
				data.fTimeRemaining = 0.0f;
			}
		}
		return fDamageModifier;
	}

	public bool HasWaterDebuff()
	{
		return IsFrozen() || IsSlowed();
	}
}
