using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minion_Ranged : MinionTemplate 
{
	public Range range;
	public Projectile projectilePrefab;
	public float fProjectileLaunchHeight = 0.5f;

	public override MinionSlotType GetSlotType()
	{
		return MinionSlotType.RANGED;
	}

	protected override void Start () 
	{
		base.Start();	
		// Hack to turn this off
		range = Range.WIDE;
	}

	protected override void Update () 
	{
		base.Update();	
	}

	private float GetScoreForCandidate(Actor_Player firer, Actor_Enemy candidate)
	{
		// Higher score is more likely to be picked
		switch (firer.minion.priority)
		{
			case TargetPriority.CLOSEST_ENEMY:
			{
				return Core.GetLevel().GetRangedZoneMax() - candidate.transform.position.x;
			}
			case TargetPriority.FARTHEST_ENEMY:
			{
				return candidate.transform.position.x - Core.GetLevel().GetRangedZoneMin();
			}
			case TargetPriority.HIGHEST_HP_ENEMY:
			{
				return candidate.minion.fCurrentHealth;
			}
			case TargetPriority.LOWEST_HP_ENEMY:
			{
				return 1.0f / candidate.minion.fCurrentHealth;
			}
		}

		return -1.0f;
	}

	protected Actor_Enemy GetBestTarget(Actor_Player firer, bool bNotInMeleeZone = false)
	{
		Actor_Player otherRangedMinion = Core.GetLevel().playerActors [firer.minion.slot == MinionSlot.RANGED_1 ? (int)MinionSlot.RANGED_2 : (int)MinionSlot.RANGED_1];


		Actor_Enemy bestCandidate = null;
		float fBestScore = 0.0f;
		foreach (Actor_Enemy enemy in Core.GetLevel().enemyActors)
		{
			if (enemy == null || enemy.IsDead() || enemy.bAboutToDie)
				continue;
			if (!enemy.IsInRangedZone())
				continue;
			if (enemy.IsInMeleeZone() && bNotInMeleeZone)
				continue;

			float fScore = GetScoreForCandidate(firer, enemy);
			if (enemy == otherRangedMinion.currentTarget)
			{
				fScore *= 0.5f;
			}

			if (fScore > fBestScore)
			{
				bestCandidate = enemy;
				fBestScore = fScore;
			}
		}
			
		return bestCandidate;
	}

	// Think of this as the actor being a vehicle and the minion doing the driving. Different minions will drive their actors differently
	public override void SimulatePlayer(Actor_Player actor)
	{
	}
	public override void SimulatePlayerFixedUpdate(Actor_Player actor)
	{
		actor.fTimeSinceLastAttack += Core.GetPlayerDeltaTime();

		//if (actor.currentTarget == null || actor.currentTarget.minion.fCurrentHealth <= 0.0f)
		{
			// Find new target
			actor.currentTarget = GetBestTarget(actor);
		}

		if (actor.minion.slot == MinionSlot.RANGED_1)
		{
			Actor_Player otherRangedMinion = Core.GetLevel().playerActors [(int)MinionSlot.RANGED_2];
			if (otherRangedMinion.fTimeSinceLastAttack >= fAttackInterval)
			{
				actor.fTimeSinceLastAttack = fAttackInterval * 0.5f;
			}
		}

		int iSlot = actor.minion.slot == MinionSlot.RANGED_1 ? 0 : 1;
		Transform reticule = Core.GetLevel().instance.targets [iSlot]; 
		Transform radius = Core.GetLevel().instance.radii [iSlot];

		if (actor.currentTarget != null)
		{
			reticule.gameObject.SetActive(true);
			reticule.position = Vector3.Lerp(reticule.position, actor.currentTarget.transform.position + new Vector3 (0.0f, 0.04f, 0.0f), Core.GetPlayerDeltaTime() * actor.minion.template.reticuleMoveSpeed * actor.GetAttackSpeedMultiplier());
			float fRadius = damage.fRadius * actor.GetAttackRadiusMultiplier() * 2.0f;
			radius.localScale = new Vector3 (fRadius, 1.0f, fRadius);

			if (!actor.currentTarget.IsInRangedZone())
			{
				actor.currentTarget = null;
			}
			else
			if (actor.fTimeSinceLastAttack >= fAttackInterval * actor.GetAttackSpeedMultiplier())
			{
				actor.fTimeSinceLastAttack = 0.0f;
				// Spawn a projectile
				for (int i = 0; i < numAttacks; i++)
				{
					Projectile projectile = Instantiate<Projectile>(projectilePrefab);
					projectile.firer = actor;
					projectile.firerTemplate = this;
					projectile.launchPos = actor.transform.position + new Vector3 (0.5f * i, fProjectileLaunchHeight, 0.0f);
					projectile.fProgress = i * 0.1f;
					projectile.target = actor.currentTarget;
					projectile.transform.position = projectile.launchPos;
				}

				PlaySoundEffect(actor);

				actor.render.SetAnimStateAndNext(AnimState.ATTACK, AnimState.IDLE);
			}
		}
		else
		{
			//reticule.gameObject.SetActive(false);
		}
	}

	private float GetTargetRangeMin()
	{
		switch (range)
		{
			case Range.SHORT:
				return 0.0f;
			case Range.LONG:
				return 2.5f;
			case Range.WIDE:
				return 0.0f;
		}
		return 0.0f;
	}

	private float GetTargetRangeMax()
	{
		switch (range)
		{
			case Range.SHORT:
				return 4.5f;
			case Range.LONG:
				return 7.0f;
			case Range.WIDE:
				return 7.0f;
		}
		return 0.0f;
	}

	public override void SimulateEnemy(Actor_Enemy actor)
	{

	}

	public override void SimulateEnemyFixedUpdate(Actor_Enemy actor)
	{
		if(actor.fTargetX == 0.0f)
			actor.fTargetX = Random.Range(Core.GetLevel().GetRangedZoneMin(), Core.GetLevel().GetRangedZoneMax());

		if (actor.IsStunned() || actor.IsFrozen())
			return;
		
		actor.fTimeSinceLastAttack += Core.GetPlayerDeltaTime();

		float fMin = actor.fTargetX;
		float fMax = Core.GetLevel().GetRangedZoneMax();

		fMax -= (fMax - fMin) * 0.25f;

		bool bInRange = actor.transform.position.x <= fMax;

		if (bInRange)
		{
			actor.render.transform.localScale = new Vector3 (1.0f, 1.0f, 1.0f);
			if (actor.fTimeSinceLastAttack >= fAttackInterval * actor.GetAttackSpeedMultiplier())
			{
				actor.fTimeSinceLastAttack = 0.0f;
				// Spawn a projectile
				for (int i = 0; i < numAttacks; i++)
				{
					Projectile projectile = Instantiate<Projectile>(projectilePrefab);
					projectile.firer = actor;
					projectile.firerTemplate = this;
					projectile.launchPos = actor.transform.position + new Vector3 (0.5f * i, fProjectileLaunchHeight, 0.0f);
					projectile.fProgress = i * 0.1f;
					projectile.target = Core.GetLevel().GetRangedTarget();
					projectile.transform.position = projectile.launchPos;
				}

				PlaySoundEffect(actor);

				actor.render.SetAnimStateAndNext(AnimState.ATTACK, AnimState.IDLE);
			}
			else if (actor.fTimeSinceLastAttack >= fAttackInterval * actor.GetAttackSpeedMultiplier() * 0.5f)
			{
				float fSpeed = fMoveSpeed;
				if (actor.transform.position.x - fMin < 1.0f)
					fSpeed *= (actor.transform.position.x - fMin);

				// Cap backwards speed to prevent magnetic bugs being useless
				if (fSpeed < 0.0f)
					fSpeed = -1.0f;

				actor.Move(new Vector3 (-fSpeed, 0.0f, 0.0f));
				actor.render.SetAnimState(AnimState.WALKING, false);
				actor.render.transform.localScale = new Vector3 (fSpeed > 0.0f ? 1.0f : -1.0f, 1.0f, 1.0f);
			}
		}
		else
		{
			actor.Move(new Vector3 (-fMoveSpeed, 0.0f, 0.0f));
			actor.render.SetAnimState(AnimState.WALKING, false);
		}
	}

	public override void OnProjectileHit(Actor firer, Actor target, Vector3 position)
	{			
		if (firer == null)
		{
			RangedAttackPlayer(null, damage);
		}
		else if (firer is Actor_Enemy)
		{
			RangedAttackPlayer((Actor_Enemy)firer, damage);
		}
		else if (firer is Actor_Player)
		{
			if (DealDamageToEnemy((Actor_Player)firer, (Actor_Enemy)target, position, 0.0f))
			{
				((Actor_Player)firer).currentTarget = null;
			}
		}
		else
		{
			Debug.Assert(false, "Firer is invalid");
		}
	}
}
