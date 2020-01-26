using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minion_Ranged_Summoner : Minion_Ranged 
{
	public int iNumHits = 3;
	public RenderActor summonPrefab;
	public AudioSource summonEffect;
	public bool bLoopingSoundEffect;
	public float fSummonHeight = 2.0f;

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

		if (bLoopingSoundEffect && !actor.soundEffect.isPlaying)
		{
			actor.soundEffect.loop = true;
			actor.soundEffect.clip = soundEffect;
			actor.soundEffect.Play();
		}

		if (actor.currentTarget != null)
		{
			int iSlot = actor.minion.slot == MinionSlot.RANGED_1 ? 0 : 1;
			Transform reticule = Core.GetLevel().instance.targets [iSlot]; 
			Transform radius = Core.GetLevel().instance.radii [iSlot];
			reticule.gameObject.SetActive(true);
			reticule.position = Vector3.Lerp(reticule.position, actor.currentTarget.transform.position + new Vector3(0.0f, 0.04f, 0.0f), Core.GetPlayerDeltaTime() * actor.minion.template.reticuleMoveSpeed * actor.GetAttackSpeedMultiplier());
			float fRadius = damage.fRadius * actor.GetAttackRadiusMultiplier() * 2.0f;
			radius.localScale = new Vector3 (fRadius, 1.0f, fRadius);

			if (actor.iNumHitsWithSummon >= iNumHits)
			{
				actor.summon.PlayDeathAnimation();
				actor.summon = null;
				actor.iNumHitsWithSummon = 0;
				reticule.position = actor.currentTarget.transform.position + new Vector3 (0.0f, 0.04f, 0.0f);
			}

			if (actor.summon == null)
			{
				// TODO : Do spawn PFX
				actor.summon = Instantiate<RenderActor>(summonPrefab);
				actor.summon.transform.SetParent(reticule);
				actor.summon.transform.localPosition = new Vector3 (0.0f, fSummonHeight, 0.0f);

				if (summonEffect != null)
				{
					summonEffect.Play();
				}
			}

			if (!actor.currentTarget.IsInRangedZone())
			{
				actor.currentTarget = null;
			}
			else if (actor.fTimeSinceLastAttack >= fAttackInterval * actor.GetAttackSpeedMultiplier())
			{
				actor.fTimeSinceLastAttack = 0.0f;

				if(!bLoopingSoundEffect)
					PlaySoundEffect(actor);

				if (DealDamageToEnemy(actor, actor.currentTarget, actor.currentTarget.transform.position, 0.0f))
				{
					// We killed them
					actor.currentTarget = null;
				}

				actor.iNumHitsWithSummon++;

				actor.summon.SetAnimStateAndNext(AnimState.ATTACK, AnimState.IDLE);


			}
		}
	}

	public override void SimulateEnemy(Actor_Enemy actor)
	{
	}

	public override void SimulateEnemyFixedUpdate(Actor_Enemy actor)
	{
		actor.fTimeSinceLastAttack += Core.GetPlayerDeltaTime();

		float fMin = Core.GetLevel().GetRangedZoneMin(); //GetTargetRangeMin();
		float fMax = Core.GetLevel().GetRangedZoneMax(); //GetTargetRangeMax();

		fMax -= (fMax - fMin) * 0.25f;

		if (actor.targetSlot == MinionSlot.NUM_MINION_SLOTS)
		{
			actor.targetSlot = (MinionSlot)((int)MinionSlotType.RANGED.GetFirst() + Random.Range(0, MinionSlotType.RANGED.GetNumSlots()));
		}

		if (bLoopingSoundEffect && !actor.soundEffect.isPlaying)
		{
			actor.soundEffect.loop = true;
			actor.soundEffect.clip = soundEffect;
			actor.soundEffect.Play();
		}

		if (actor.summon == null)
		{
			// TODO : Do spawn PFX
			actor.summon = Instantiate<RenderActor>(summonPrefab);
			actor.summon.transform.position = actor.transform.position;

			if (summonEffect != null)
			{
				summonEffect.Play();
			}
		}

		if (actor.summon != null && Core.GetLevel().playerActors [(int)actor.targetSlot] != null)
		{
			Vector3 targetPos = Core.GetLevel().playerActors [(int)actor.targetSlot].transform.position;
			targetPos.y += fSummonHeight;
			actor.summon.transform.position = Vector3.Lerp(actor.summon.transform.position, targetPos, Core.GetEnemyDeltaTime() * reticuleMoveSpeed);
		}

		bool bInRange = fMin <= actor.transform.position.x && actor.transform.position.x <= fMax;

		if (bInRange)
		{
			if (actor.fTimeSinceLastAttack >= fAttackInterval * actor.GetAttackSpeedMultiplier())
			{
				actor.fTimeSinceLastAttack = 0.0f;

				RangedAttackPlayer(actor, damage, actor.targetSlot);

				if (!bLoopingSoundEffect)
				{
					PlaySoundEffect(actor);
				}

				actor.render.SetAnimStateAndNext(AnimState.ATTACK, AnimState.IDLE);

				// Pick a new target slot for the next attack
				actor.targetSlot = (MinionSlot)((int)MinionSlotType.RANGED.GetFirst() + Random.Range(0, MinionSlotType.RANGED.GetNumSlots()));
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

	public override void OnDeath(Actor_Enemy actor) 
	{
		actor.summon.PlayDeathAnimation();
		actor.summon = null;
	}
}
