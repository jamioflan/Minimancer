using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minion_Ranged_Hook : Minion_Ranged
{
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
			actor.currentTarget = GetBestTarget(actor, true);
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
				PlaySoundEffect(actor);

				// Spawn a projectile
				Projectile projectile = Instantiate<Projectile>(projectilePrefab);
				projectile.firer = actor;
				projectile.firerTemplate = this;
				projectile.launchPos = actor.transform.position + new Vector3(0.0f, fProjectileLaunchHeight, 0.0f);
				projectile.target = actor.currentTarget;
				projectile.transform.position = projectile.launchPos;

				actor.fTimeSinceLastAttack = 0.0f;
				actor.currentTarget = null;
				actor.render.SetAnimStateAndNext(AnimState.ATTACK, AnimState.IDLE);
			}
		}
	}

	public override void OnProjectileHit(Actor firer, Actor target, Vector3 position)
	{	
		if(target is Actor_Enemy)
			((Actor_Enemy)target).fPullToX = -2.5f;

		base.OnProjectileHit(firer, target, position);
	}
}
