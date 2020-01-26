using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minion_Ranged_Chain : Minion_Ranged
{
	public int iChain = 0;
	public float fMaxChainGap = 1.0f;
	public float fChainPFXDuration = 0.1f;

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
		actor.fTimeSinceLastAttack += Core.GetPlayerDeltaTime();

		//if (actor.currentTarget == null || actor.currentTarget.minion.fCurrentHealth <= 0.0f)
		{
			// Find new target
			actor.currentTarget = GetBestTarget(actor);
		}

		if (actor.currentTarget != null)
		{
			int iSlot = actor.minion.slot == MinionSlot.RANGED_1 ? 0 : 1;
			Transform reticule = Core.GetLevel().instance.targets [iSlot]; 
			Transform radius = Core.GetLevel().instance.radii [iSlot];
			reticule.gameObject.SetActive(true);
			reticule.position = Vector3.Lerp(reticule.position, actor.currentTarget.transform.position + new Vector3(0.0f, 0.04f, 0.0f), Core.GetPlayerDeltaTime() * actor.minion.template.reticuleMoveSpeed * actor.GetAttackSpeedMultiplier());
			radius.localScale = new Vector3 (0.0f, 1.0f, 0.0f);

			if (!actor.currentTarget.IsInRangedZone())
			{
				actor.currentTarget = null;
			}
			else if (actor.fTimeSinceLastAttack >= fAttackInterval * actor.GetAttackSpeedMultiplier())
			{
				actor.fTimeSinceLastAttack = 0.0f;

				List<Vector3> pfxPositions = new List<Vector3> ();
				pfxPositions.Add(actor.transform.position + new Vector3(0.0f, fProjectileLaunchHeight, 0.0f));

				List<Actor_Enemy> alreadyHit = new List<Actor_Enemy> ();
				Actor_Enemy currentTarget = actor.currentTarget;
				float fChainRemaining = iChain * actor.GetChainCountMultiplier();

				PlaySoundEffect(actor);

				while (fChainRemaining > 0.0f)
				{
					DealDamageToEnemy(actor, currentTarget, currentTarget.transform.position, 0.0f);
					if (currentTarget.HasWaterDebuff())
						fChainRemaining -= 0.5f;
					else
						fChainRemaining -= 1.0f;

					alreadyHit.Add(currentTarget);

					// Add a line
					Vector3 prevPos = pfxPositions [pfxPositions.Count - 1];
					Vector3 newPos = currentTarget.transform.position + new Vector3(0.0f, fProjectileLaunchHeight, 0.0f);
					Vector3 dPos = newPos - prevPos;
					float fLength = dPos.magnitude;
					// If it is longer than 2 units, split it into segments
					if (fLength >= 2.0f)
					{
						dPos.Normalize();
						int numSegments = Mathf.FloorToInt(fLength / 2.0f);
						for (int i = 0; i < numSegments; i++)
						{
							Vector3 pos = prevPos + dPos * 2.0f * (i + 1);
							Vector3 offset = Random.onUnitSphere;
							offset.y = 0.0f;
							pos += offset;
							pfxPositions.Add(pos);
						}
					}

					pfxPositions.Add(newPos);

					Actor_Enemy bestCandidate = null;
					float fBestDistance = fMaxChainGap * actor.GetChainGapMultiplier();
					foreach (Actor_Enemy enemy in Core.GetLevel().enemyActors)
					{
						if (enemy != null && !alreadyHit.Contains(enemy))
						{
							float fDist = (currentTarget.transform.position - enemy.transform.position).magnitude;

							if (fDist < fBestDistance)
							{
								fBestDistance = fDist;
								bestCandidate = enemy;
							}
						}
					}
					if (bestCandidate == null)
						break;

					currentTarget = bestCandidate;
				}

				actor.render.SetChainPFXActive(fChainPFXDuration, pfxPositions.ToArray());
				actor.render.SetAnimStateAndNext(AnimState.ATTACK, AnimState.IDLE);
			}
		}
	}

	public override void SimulateEnemy(Actor_Enemy actor)
	{
	}

	public override void SimulateEnemyFixedUpdate(Actor_Enemy actor)
	{
		if (actor.IsFrozen() || actor.IsStunned())
			return;
		
		actor.fTimeSinceLastAttack += Core.GetPlayerDeltaTime();

		float fMin = Core.GetLevel().GetRangedZoneMin(); //GetTargetRangeMin();
		float fMax = Core.GetLevel().GetRangedZoneMax(); //GetTargetRangeMax();

		fMax -= (fMax - fMin) * 0.25f;

		bool bInRange = fMin <= actor.transform.position.x && actor.transform.position.x <= fMax;

		if (bInRange)
		{
			if (actor.fTimeSinceLastAttack >= fAttackInterval * actor.GetAttackSpeedMultiplier())
			{
				actor.fTimeSinceLastAttack = 0.0f;

				PlaySoundEffect(actor);

				// Zap both player ranged characters
				RangedAttackPlayer(actor, damage, MinionSlot.RANGED_1);
				RangedAttackPlayer(actor, damage, MinionSlot.RANGED_2);

				List<Vector3> pfxPositions = new List<Vector3> ();
				pfxPositions.Add(actor.transform.position + new Vector3(0.0f, fProjectileLaunchHeight, 0.0f));

				int firstHit = Random.Range(0, 2);

				// Add a line
				Vector3 prevPos = pfxPositions [pfxPositions.Count - 1];
				Actor_Player firstPlayerHit = Core.GetLevel().playerActors[(int)MinionSlot.RANGED_1 + firstHit];
				Actor_Player secondPlayerHit = Core.GetLevel().playerActors[(int)MinionSlot.RANGED_1 + (1 - firstHit)];
				Vector3 newPos = firstPlayerHit.transform.position + new Vector3(0.0f, fProjectileLaunchHeight, 0.0f);
				Vector3 dPos = newPos - prevPos;
				float fLength = dPos.magnitude;
				// If it is longer than 2 units, split it into segments
				if (fLength >= 2.0f)
				{
					dPos.Normalize();
					int numSegments = Mathf.FloorToInt(fLength / 2.0f);
					for (int i = 0; i < numSegments; i++)
					{
						Vector3 pos = prevPos + dPos * 2.0f * (i + 1);
						Vector3 offset = Random.onUnitSphere;
						offset.y = 0.0f;
						pos += offset;
						pfxPositions.Add(pos);
					}
				}
				pfxPositions.Add(firstPlayerHit.transform.position);
				pfxPositions.Add(secondPlayerHit.transform.position);

				actor.render.SetChainPFXActive(fChainPFXDuration, pfxPositions.ToArray());
				actor.render.SetAnimStateAndNext(AnimState.ATTACK, AnimState.IDLE);
			}
			else if (actor.fTimeSinceLastAttack >= fAttackInterval * actor.GetAttackSpeedMultiplier() * 0.5f)
			{
				float fSpeed = fMoveSpeed;
				if (actor.transform.position.x - fMin < 1.0f)
					fSpeed *= (actor.transform.position.x - fMin);

				actor.Move(new Vector3 (-fSpeed, 0.0f, 0.0f));
				actor.render.SetAnimState(AnimState.WALKING, false);
			}
		}
		else
		{
			actor.Move(new Vector3 (-fMoveSpeed, 0.0f, 0.0f));
			actor.render.SetAnimState(AnimState.WALKING, false);
		}
	}
}
