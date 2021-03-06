using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minion_Melee_Wurm : Minion_Melee 
{
	public override void SimulateEnemy(Actor_Enemy actor)
	{
	}

	public override void SimulateEnemyFixedUpdate(Actor_Enemy actor)
	{
		// Wurm acts like a support, but teleports between spots. Go stand in a player zone, leave it, stand in it, repeat
		if (actor.IsFrozen() || actor.IsStunned())
			return;
		
		actor.fTimeSinceLastAttack += Core.GetEnemyDeltaTime();
		if (actor.GetDistanceFromPlayerArea() <= fAttackRange && actor.fTimeSinceLastAttack >= fAttackInterval)
		{
			// ---- ATTACK CODE ----
			if (actor.fTimeSinceLastAttack >= fAttackInterval)
			{
				PlaySoundEffect(actor);

				actor.fTimeSinceLastAttack = 0.0f;
				MeleeAttackPlayer(actor, damage, MinionSlot.MELEE_1);
				MeleeAttackPlayer(actor, damage, MinionSlot.MELEE_2);
				RangedAttackPlayer(actor, damage, MinionSlot.RANGED_1);
				RangedAttackPlayer(actor, damage, MinionSlot.RANGED_2);
				actor.render.SetAnimStateAndNext(AnimState.ATTACK, AnimState.IDLE);
				if (actor.render.attackParticles != null)
				{
					actor.render.attackParticles.Play();
				}
			}
		}
		// If we are waiting, do that.
		else if (actor.fTimeToNextMove > 0.0f)
		{
			actor.fTimeToNextMove -= Core.GetEnemyDeltaTime();
			if (actor.fTimeToNextMove <= 0.0f && actor.render.GetAnimState() != AnimState.ATTACK)
			{
				if (actor.bPickedPositionInPlayerZone)
				{
					// Last pick was in a zone, so wherever is fine this time.
					actor.target = new Vector3 (Random.Range(LevelController.fMIN_X_COORD, Core.GetLevel().GetRangedZoneMax() + 3.0f), 0.0f, Random.Range(LevelController.fMIN_Z_COORD, LevelController.fMAX_Z_COORD));
				}
				else
				{
					// We just wandered off wherever, so make sure we walk back through a zone this time.
					if (Random.Range(0, 2) == 0)
					{
						actor.target = new Vector3 (Random.Range(LevelController.fMIN_X_COORD, Core.GetLevel().GetMeleeZoneLimit()), 0.0f, Random.Range(LevelController.fMIN_Z_COORD, LevelController.fMAX_Z_COORD));
					}
					else
					{
						actor.target = new Vector3 (Random.Range(Core.GetLevel().GetRangedZoneMin(), Core.GetLevel().GetRangedZoneMax()), 0.0f, Random.Range(LevelController.fMIN_Z_COORD, LevelController.fMAX_Z_COORD));
					}
				}

				actor.bPickedPositionInPlayerZone = !actor.bPickedPositionInPlayerZone;

				actor.render.SetAnimStateAndNext(AnimState.WALKING, AnimState.IDLE);
				if (actor.render.moveParticles != null)
				{
					actor.render.moveParticles.Play();
				}
			}
		}
		// Else, we made it to idle, so teleport and re-emerge by playing a reverse walk
		else if(actor.render.GetAnimState() == AnimState.IDLE)
		{
			actor.Move(actor.target - actor.transform.position, true);
			actor.render.SetAnimStateAndNext(AnimState.WALKING, AnimState.IDLE);
			actor.render.SetReverse(true);
			actor.fTimeToNextMove = 2.0f;
		}
	}
}
