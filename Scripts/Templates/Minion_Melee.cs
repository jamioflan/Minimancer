using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minion_Melee : MinionTemplate 
{
	// -- Shared stats --


	// -- Player stats --

	// -- Enemy stats --
	public override MinionSlotType GetSlotType()
	{
		return MinionSlotType.MELEE;
	}

	public float fAttackRange = 1.0f;

	protected override void Start () 
	{
		base.Start();	
	}

	protected override void Update () 
	{
		base.Update();	
	}

	// Think of this as the actor being a vehicle and the minion doing the driving. Different minions will drive their actors differently
	public override void SimulatePlayer(Actor_Player actor)
	{
	}

	public override void SimulatePlayerFixedUpdate(Actor_Player actor)
	{

		if (damage.fAmount > 0.0f || damage.fPushAmount > 0.0f) // 0 means no attack
		{
			actor.fTimeSinceLastAttack += Core.GetPlayerDeltaTime();

			if (actor.currentTarget == null || actor.currentTarget.minion.fCurrentHealth <= 0.0f || !actor.currentTarget.IsInMeleeZone())
			{
				actor.currentTarget = null;

				// Find new target
				Actor_Enemy closestEnemy = Core.GetLevel().GetClosestEnemyToMeleeZone();
				if (closestEnemy != null && closestEnemy.IsInMeleeZone())
				{
					actor.currentTarget = closestEnemy;
				}
				else
				{
					// No valid target.

				}
			}

			int iSlot = actor.minion.slot == MinionSlot.MELEE_1 ? 0 : 1;
			Transform reticule = Core.GetLevel().instance.meleeTargets [iSlot]; 
			Transform radius = Core.GetLevel().instance.meleeRadii [iSlot];

			if (actor.currentTarget != null)
			{
				reticule.gameObject.SetActive(true);
				reticule.position = Vector3.Lerp(reticule.position, actor.currentTarget.transform.position + new Vector3 (0.0f, 0.04f, 0.0f), Core.GetPlayerDeltaTime() * actor.minion.template.reticuleMoveSpeed * actor.GetAttackSpeedMultiplier());
				float fRadius = damage.fRadius * actor.GetAttackRadiusMultiplier() * 2.0f;
				radius.localScale = new Vector3 (fRadius, 1.0f, fRadius);

				if (TryToAttackEnemy(actor, actor.currentTarget))
				{
					// We killed them
					actor.currentTarget = null;
				}
			}
			else
			{
				//reticule.gameObject.SetActive(false);
			}
		}
	}

	public override void SimulateEnemyFixedUpdate(Actor_Enemy actor)
	{
		if (actor.IsFrozen() || actor.IsStunned())
			return;

		float fDistance = actor.transform.position.x - LevelController.fMIN_X_COORD;

		Vector3 diagonal = new Vector3 (-1.0f, 0.0f, -0.25f * Mathf.Sign(actor.transform.position.z) / (fDistance + 1.0f));
		diagonal.Normalize();
		actor.Move(diagonal * fMoveSpeed);

		// Small delay to give player actors first strike and therefore allow zephyrs to push before damage
		if (actor.GetDistanceFromPlayerArea() <= fAttackRange && !actor.IsStunned() && actor.fTimeInMeleeZone > 0.1f)
		{
			// ---- ATTACK CODE ----
			actor.fTimeSinceLastAttack += Core.GetEnemyFixedDeltaTime();
			if (actor.fTimeSinceLastAttack >= fAttackInterval)
			{
				actor.fTimeSinceLastAttack = 0.0f;
				PlaySoundEffect(actor);

				for(int i = 0; i < numAttacks; i++)
					MeleeAttackPlayer(actor, damage);
				actor.render.SetAnimStateAndNext(AnimState.ATTACK, AnimState.IDLE);
			}
		}
		else
		{
			actor.render.SetAnimState(AnimState.WALKING, false);
		}
	}

	public override void SimulateEnemy(Actor_Enemy actor)
	{

	}

	public override void OnProjectileHit(Actor firer, Actor target, Vector3 position)
	{
		Debug.Assert(false, "Melee projectiles? Really?");
	}
}
