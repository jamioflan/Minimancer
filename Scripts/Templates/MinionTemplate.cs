using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MinionTemplate : MonoBehaviour 
{
	// -- General stats --
	public float fMaxHealth = 100.0f;
	public RenderActor render;
	public ComboNumbers comboNumbers;
	public Element element;
	public bool isZombie;
	public Sprite icon;
	public float fHeight = 1.0f;
	public float reticuleMoveSpeed = 10.0f;
	public int numAttacks = 1;

	public float fAttackInterval = 1.0f;
	public Damage damage;
	public bool canCombo = false, bDeathtoll = false, bRelentless = false;

	public bool bVolatile = false;

	public AudioClip soundEffect;

	public bool bIsLancer = false;
	public bool bImmuneToDebuffs = false;


	public List<Resurrection> resurrectionTriggers = new List<Resurrection>();

	public float[] afTimePerFrame = new float[(int)AnimState.NUM_ANIM_STATES] 
	{ 1.0f, 0.25f, 0.25f, 0.4f };
	public int iNumFrames = 2;

	public List<Buff> passiveBuffs, triggeredBuffs;

	// -- Player stats --
	public string unlocName = "";
	public string unlocDesc = "";
	public string debugDescriptionText = "A minion";

	// -- Enemy stats --
	public float fMoveSpeed = 2.0f;

	public abstract MinionSlotType GetSlotType();

	public override int GetHashCode()
	{
		return name.GetHashCode();
	}

	protected virtual void Start () 
	{
		if (damage != null)
		{
			damage.SetElement(element);
			damage.SetSlotType(GetSlotType());
		}
		foreach (Buff buff in passiveBuffs)
			buff.SetElement(element);
		foreach (Buff buff in triggeredBuffs)
			buff.SetElement(element);
	}

	protected virtual void Update () 
	{
		
	}

	private void Init()
	{
		if (damage != null)
		{
			damage.SetElement(element);
			damage.SetSlotType(GetSlotType());
		}
		foreach (Buff buff in passiveBuffs)
			buff.SetElement(element);
		foreach (Buff buff in triggeredBuffs)
			buff.SetElement(element);
	}

	public virtual void InitPlayer(Actor_Player actor) 
	{
		Init();
	}

	public virtual void InitEnemy(Actor_Enemy actor) 
	{
		Init();
	}

	// Think of this as the actor being a vehicle and the minion doing the driving. Different minions will drive their actors differently
	public abstract void SimulatePlayer(Actor_Player actor);
	public abstract void SimulateEnemy(Actor_Enemy actor);
	public abstract void SimulatePlayerFixedUpdate(Actor_Player actor);
	public abstract void SimulateEnemyFixedUpdate(Actor_Enemy actor);
	public abstract void OnProjectileHit(Actor firer, Actor target, Vector3 position);
	public virtual void OnDeath(Actor_Enemy actor) 
	{
	}

	protected void MeleeAttackPlayer(Actor_Enemy attacker, Damage damage, MinionSlot slot = MinionSlot.NUM_MINION_SLOTS)
	{
		float fDamageDealt = Core.GetCurrentRoster().MeleeAttack(attacker, damage, slot);
		attacker.OnDealtDamage(fDamageDealt);
	}

	protected void RangedAttackPlayer(Actor_Enemy attacker, Damage damage, MinionSlot slot = MinionSlot.NUM_MINION_SLOTS)
	{
		float fDamageDealt = Core.GetCurrentRoster().RangedAttack(attacker, damage, slot);
		if(attacker != null)
			attacker.OnDealtDamage(fDamageDealt);
	}

	protected bool TryToAttackEnemy(Actor_Player player, Actor_Enemy enemy)
	{
		if (player.fTimeSinceLastAttack >= fAttackInterval * player.GetAttackSpeedMultiplier())
		{
			PlaySoundEffect(player);

			player.render.SetAnimStateAndNext(AnimState.ATTACK, AnimState.IDLE);
			player.fTimeSinceLastAttack = 0.0f;

			if (player.render.attackParticles != null)
			{
				//player.render.attackParticles.Stop();
				player.render.attackParticles.Play();
			}
				
			for (int i = 0; i < numAttacks; i++)
			{
				IncrementCombo(player);
				if (DealDamageToEnemy(player, enemy, enemy.transform.position, 0.0f))
					return true;
			}
		}
		return false;
	}

	protected bool DealDamageToEnemy(Actor_Player player, Actor_Enemy enemy, Vector3 position, float fRadius)
	{
		float fMultiplier = player.GetAttackDamageMultiplier();
		if (position.x <= Core.GetLevel().GetMeleeZoneLimit())
			fMultiplier += player.minion.GetBuff(Stat.DAMAGE_MULTIPLIER_IN_MELEE_ZONE);
		
		if (enemy.IsStunned())
			fMultiplier += player.minion.GetBuff(Stat.ATTACK_DAMAGE_ON_STUNNED_ENEMIES);

		if (damage.IsRadial())
		{

			Core.GetLevel().DamageRadius(damage, 
				position, 
				damage.fRadius * player.GetAttackRadiusMultiplier(), 
				fMultiplier, 
				player.GetStunTime(), 
				player.minion.GetBuff(Stat.CRITICAL_CHANCE), 
				player.minion.GetBuff(Stat.CRITICAL_DAMAGE), 
				player.minion.GetBuff(Stat.VAMPIRISM), 
				player.minion.GetBuff(Stat.ATTACK_DAMAGE_ABSOLUTE));

			return enemy == null || enemy.IsDead();
		}
		else if(enemy != null)
		{
			if (enemy.IsDead())
				return true;

			float fElementalMultiplier = Elements.GetDamageMultiplier(damage.GetElement(), enemy.minion.template.element);
			float fMod = enemy.GetInherentElementalResistanceModifier();
			if (fElementalMultiplier < 1.0f)
			{
				fElementalMultiplier = 1.0f - ((1.0f - fElementalMultiplier) * fMod);
			}

			fMultiplier *= fElementalMultiplier;

			player.OnDealtDamage(damage.fAmount * fMultiplier);

			return enemy.Damage(damage, 
				fMultiplier, 
				player.GetStunTime(), 
				player.minion.GetBuff(Stat.CRITICAL_CHANCE), 
				player.minion.GetBuff(Stat.CRITICAL_DAMAGE), 
				player.minion.GetBuff(Stat.VAMPIRISM),
				player.minion.GetBuff(Stat.ATTACK_DAMAGE_ABSOLUTE));
		}

		return true;
	}

	protected void PlaySoundEffect(Actor actor)
	{
		if (actor.soundEffect != null)
		{
			actor.soundEffect.pitch = Random.Range(0.8f, 1.2f);
			actor.soundEffect.PlayOneShot(soundEffect);
		}
	}

	public void IncrementCombo(Actor actor)
	{
		if (canCombo || bDeathtoll)
		{
			actor.minion.iCombo++;

			if (canCombo)
			{
				Core.IncrementStat("COMBOS_GAINED", 1);
				if (actor.minion.iCombo == 50)
				{
					Core.TriggerAchievement("COMBO_APPRENTICE");
				}
				if (actor.minion.iCombo == 100)
				{
					Core.TriggerAchievement("COMBO_MASTER");
				}

				TriggerBuff(BuffTrigger.COMBO_INCREMENTED, actor);
			}

			if (actor.comboNumbers != null)
			{
				actor.comboNumbers.SetValue(actor.minion.iCombo);
			}
		}
	}

	public void SetRelentlessCombo(int i, Actor actor)
	{
		if (bRelentless)
		{
			actor.minion.iCombo = i;
			if (actor.comboNumbers != null)
			{
				actor.comboNumbers.SetValue(actor.minion.iCombo);
			}
		}
	}

	public void ResetCombo(Actor actor)
	{
		if (canCombo || bDeathtoll || bRelentless)
		{
			if (actor.comboNumbers != null)
			{
				actor.comboNumbers.SetValue(0);
			}
			actor.minion.iCombo = 0;
		}
	}

	public void TriggerBuff(BuffTrigger eTrigger, Actor actor)
	{
		foreach (Buff buff in triggeredBuffs)
		{
			buff.OnTrigger(eTrigger, actor);
		}
	}
}
