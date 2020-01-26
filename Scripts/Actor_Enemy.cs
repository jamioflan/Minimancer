using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Actor_Enemy : Actor 
{
	private CharacterController cc;
	public bool bPickedPositionInPlayerZone = false;
	public Vector3 target = Vector3.zero;
	public MinionSlot targetSlot = MinionSlot.NUM_MINION_SLOTS;
	public float fTimeToNextMove = 1.0f;
	public Healthbar healthbar;
	public int iWave;
	public float fTargetX = 0.0f;
	public bool bAboutToDie = false;
	public float fPushAmount = 0.0f;
	public float fPullToX = -1000.0f;
	public float fTimeInMeleeZone = 0.0f;

	protected override void Start () 
	{
		cc = gameObject.AddComponent<CharacterController>();
		cc.radius = 0.25f; // Read from minion template?
		cc.center = new Vector3(0.0f, 0.5f, 0.0f);
		cc.height = 1.0f;
		cc.minMoveDistance = 0.0001f;
	}

	protected override void Update () 
	{
		base.Update();

		render.UpdateAnimation(Core.GetEnemyDeltaTime());
		if (summon != null)
			summon.UpdateAnimation(Core.GetEnemyDeltaTime());
		
		minion.template.SimulateEnemy(this);

		healthbar.gameObject.SetActive(minion.fCurrentHealth < minion.fMaxHealthPostBuffs);
		healthbar.fill.fillAmount = minion.fCurrentHealth / minion.fMaxHealthPostBuffs;

		if (IsInMeleeZone())
		{
			fTimeInMeleeZone += Core.GetEnemyDeltaTime();
		}
		else
		{
			fTimeInMeleeZone = 0.0f;
		}
	}

	public void FixedUpdate()
	{
		if (fPushAmount > 0.0f)
		{
			Vector3 push = new Vector3 (1.0f, 0.0f, 0.0f);
			float fMag = (Mathf.Clamp((fPushAmount + 1.0f) / 3.0f, 0.0f, 1.0f));
			fPushAmount -= fMag * Core.GetEnemyDeltaTime();
			MoveCC(push * fMag * 5.0f);
		}

		if (fPullToX > -3.0f)
		{
			Vector3 push = new Vector3 (fPullToX - transform.position.x - 1.0f, 0.0f, 0.0f);
			MoveCC(push);
			if(Mathf.Abs(transform.position.x - fPullToX) < 0.1f)
			{
				fPullToX = -1000.0f;
			}
		}

		minion.template.SimulateEnemyFixedUpdate(this);
	}

	public void Move(Vector3 velocity, bool bInstant = false, bool bExternalForce = false)
	{
		if ((IsFrozen() || IsStunned()) && !bExternalForce)
			return;
		if(fPullToX > -3.0f)
			return;
		if (IsSlowed() && !bExternalForce)
			velocity *= 0.5f;
		if (fPushAmount > 0.0f)
			return;
		
		velocity *= GetMoveSpeedMultiplier();

		MoveCC(velocity, bInstant);
	}

	public void MoveCC(Vector3 velocity, bool bInstant = false)
	{
		if (cc == null)
			return;
		
		bool bInRangedZoneBefore = IsInRangedZone();
		bool bInMeleeZoneBefore = IsInMeleeZone();

		if (bInstant)
		{
			cc.enabled = false;
			transform.position += velocity;
			cc.enabled = true;
		}
		else
		{
			cc.Move(velocity * Core.GetEnemyFixedDeltaTime());
		}

		if (!bInRangedZoneBefore && IsInRangedZone())
		{
			TriggerPlayerDebuffsOnMe(BuffTrigger.ENTER_RANGED_ZONE);
		}

		if (!bInMeleeZoneBefore && IsInMeleeZone())
		{
			TriggerPlayerDebuffsOnMe(BuffTrigger.ENTER_MELEE_ZONE);
		}
	}


	public void TriggerPlayerDebuffsOnMe(BuffTrigger trigger)
	{
		foreach (Minion otherMinion in Core.GetCurrentRoster().minions)
		{
			foreach (Buff buff in otherMinion.template.triggeredBuffs)
			{
				if (buff.ShouldApply(minion.template.element, minion.template.GetSlotType(), minion.template.isZombie, true))
				{
					buff.OnTrigger(trigger, this);
				}
			}
		}
	}

	public float GetDistanceFromPlayerArea()
	{
		return transform.position.x - LevelController.fMIN_X_COORD;
	}

	public bool IsInMeleeZone()
	{
		return transform.position.x < Core.GetLevel().GetMeleeZoneLimit();
	}

	public bool IsInRangedZone()
	{
		return Core.GetLevel().GetRangedZoneMin() <= transform.position.x && transform.position.x <= Core.GetLevel().GetRangedZoneMax();
	}
		
	public override void InitFromMinion(Minion newMinion)
	{
		base.InitFromMinion(newMinion);

		minion.template.InitEnemy(this);
	}

	public override bool Damage(Damage damage, float fMultiplier, float fStunTime, float fAdditionalCritChance, float fAdditionalCritMultiplier, float fVampirism, float fAdditionalDamage)
	{
		if (IsDead())
		{
			return true;
		}

		if (fStunTime > 0.0f)
		{
			DebuffData data = new DebuffData ();
			data.debuff = DebuffData.Debuff.STUNNED;
			data.fTimeRemaining = fStunTime;
			data.debuffIcon = Core.GetMinionTemplateManager().stunPFXPrefab;
			PFX_DebuffIcon debuffIcon = minion.ApplyDebuff(data, fStunTime);
			if (debuffIcon != null)
			{
				debuffIcon.transform.SetParent(transform);
				debuffIcon.transform.localPosition = Vector3.zero;
			}
		}

		if (damage.fPushAmount > 0.0f)
		{
			fPushAmount = damage.fPushAmount;
		}

		float fCritChance = damage.fCritChance + fAdditionalCritChance;
		bool bCrit = Random.Range(0.0f, 1.0f) < fCritChance;

		if (IsSpotted())
			bCrit = true;

		float fCritMultiplier = bCrit ? damage.fCritMultiplier + fAdditionalCritMultiplier : 1.0f;

		if (damage.GetSlotType() == MinionSlotType.MELEE)
		{
			fMultiplier *= GetMeleeDamageModifierFromDebuffs();
		}
		if (damage.GetSlotType() == MinionSlotType.RANGED)
		{
			OnRangedHit();
		}

		fMultiplier *= GetDamageModifierFromDebuffs();

		// Give vampiric health to players
		Core.GetCurrentRoster().HealGroup(damage.GetSlotType(), (damage.fAmount + fAdditionalDamage) * fMultiplier * fCritMultiplier * fVampirism);
	
		if (damage.onHitEffect != null)
		{
			damage.onHitEffect.OnTriggered(null, null);
		}

		int iDamage = Mathf.FloorToInt((damage.fAmount + fAdditionalDamage) * fMultiplier * fCritMultiplier);

		if (bCrit)
		{
			Core.IncrementStat("CRITICAL_HITS", 1);
		}
		Core.IncrementStat("DAMAGE_DEALT", iDamage);
		if (iDamage >= 500)
		{
			Core.TriggerAchievement(damage.GetSlotType() == MinionSlotType.MELEE ? "BRUTAL" : "SNIPER");
		}

		MakeDamageNumbers(iDamage, bCrit ? Core.GetMinionTemplateManager().criticalDamage : Core.GetMinionTemplateManager().enemyDamage);

		bool bDead = minion.Damage(damage, fMultiplier * fCritMultiplier, fAdditionalDamage);
		if (bDead)
		{
			minion.template.OnDeath(this);
			Core.GetLevel().EnemyDied(this);
			render.PlayDeathAnimation();
			render.transform.SetParent(null);
			Destroy(gameObject);
		}

		return bDead;
	}

	private void OnRangedHit()
	{
		TriggerPlayerDebuffsOnMe(BuffTrigger.ENEMY_HIT_WITH_RANGED);
	}

	public override void CalculateMyAggregateBuffs()
	{
		foreach (Minion otherMinion in Core.GetCurrentRoster().minions)
		{
			foreach (Buff buff in otherMinion.template.passiveBuffs)
			{
				if (buff.ShouldApply(minion.template.element, minion.template.GetSlotType(), minion.template.isZombie, true))
				{
					minion.currentBuffs.Add(buff);
				}
			}
		}
	}
}
