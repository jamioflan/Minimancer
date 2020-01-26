using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Actor_Player : Actor 
{
	public Actor_Enemy currentTarget;

	public float fTimeToNextBurn;

	protected override void Start () 
	{
		fTimeToNextAura = Random.Range(2.0f, 3.0f);
	}

	protected override void Update () 
	{
		base.Update();

		render.UpdateAnimation(Core.GetPlayerDeltaTime());
		if (summon != null)
			summon.UpdateAnimation(Core.GetPlayerDeltaTime());

		switch (minion.template.GetSlotType())
		{
			case MinionSlotType.MELEE:
			{
				break;
			}
			case MinionSlotType.SUPPORT:
			{
				break;
			}
			case MinionSlotType.RANGED:
			{
				break;
			}
		}

		minion.template.SimulatePlayer(this);
	}

	public void FixedUpdate()
	{
		minion.template.SimulatePlayerFixedUpdate(this);
	}

	public override void InitFromMinion(Minion newMinion)
	{
		base.InitFromMinion(newMinion);

		minion.template.InitPlayer(this);
	}

	public void Kill()
	{
		minion = null;
		// Set the renderer to auto, unlink it and destroy ourselves
		render.PlayDeathAnimation();
		render.transform.SetParent(null);
		Destroy(gameObject);
	}

	public override bool Damage(Damage damage, float fMultiplier, float fStunTime, float fAdditionalCritChance, float fAdditionalCritMultiplier, float fVampirism, float fAdditionalDamage)
	{
		Debug.Assert(false, "Player actors should not be directly attacked");

		return false;
	}

	public override void CalculateMyAggregateBuffs()
	{
		foreach (Minion otherMinion in Core.GetCurrentRoster().minions)
		{
			foreach (Buff buff in otherMinion.template.passiveBuffs)
			{
				if (buff.ShouldApply(minion.template.element, minion.template.GetSlotType(), minion.template.isZombie, false))
				{
					minion.currentBuffs.Add(buff);
				}
			}
		}
	}

	public void SetMaxHealthFromBuffs()
	{
		minion.fMaxHealthPostBuffs = minion.fMaxHealthPreBuffs * GetMaxHealthModifier();
	}
}
