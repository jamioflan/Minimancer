using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minion_Ranged_Phoenix : Minion_Ranged
{
	public float fHealthTrigger = 0.25f;
	public Damage burnDamage;
	public float fTimeBetweenBurns = 1.0f;

	protected override void Start () 
	{
		base.Start();
		burnDamage.SetElement(element);
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
		base.SimulatePlayer(actor);

		if (Core.GetCurrentRoster().afGroupHealths [(int)MinionSlotType.RANGED] / Core.GetCurrentRoster().afGroupMaxHealths [(int)MinionSlotType.RANGED] <= fHealthTrigger)
		{
			actor.fTimeToNextBurn -= Core.GetPlayerDeltaTime();
			if (actor.fTimeToNextBurn <= 0.0f)
			{
				actor.fTimeToNextBurn = fTimeBetweenBurns;
				List<Actor_Enemy> enemies = new List<Actor_Enemy> ();
				enemies.AddRange(Core.GetLevel().enemyActors);
				foreach (Actor_Enemy enemy in enemies)
				{
					enemy.Damage(burnDamage, actor.GetAttackDamageMultiplier(), 0.0f, 0.0f, 0.0f, 0.0f, 0.0f);
				}
			}
		}

	}
}
