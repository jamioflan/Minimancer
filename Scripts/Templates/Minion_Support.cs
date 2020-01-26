using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minion_Support : MinionTemplate 
{
	public float fPullSpeed = 0.0f;
	public float fWaterDebuffAttractSpeed = 0.0f;
	public bool bWalls = false;
	public float fHealPerSecond = 0.0f;
	public float fTimeBetweenSFXMin = 1.0f, fTimeBetweenSFXMax = 2.0f;

	public PFX_SupportAura auraPFX;

	public override MinionSlotType GetSlotType()
	{
		return MinionSlotType.SUPPORT;
	}

	protected override void Start () 
	{
		base.Start();	
	}
	
	protected override void Update () 
	{
		base.Update();	
	}

	public override void InitEnemy(Actor_Enemy actor)
	{
		base.InitEnemy(actor);

		if (auraPFX != null)
		{
			actor.auraPFX = Instantiate<PFX_SupportAura>(auraPFX);
			actor.auraPFX.transform.SetParent(actor.transform);
			actor.auraPFX.transform.localPosition = Vector3.zero;
		}

		actor.fTimeToNextAura = Random.Range(fTimeBetweenSFXMin, fTimeBetweenSFXMax);
	}

	public override void InitPlayer(Actor_Player actor) 
	{
		base.InitPlayer(actor);
		if (auraPFX != null)
		{
			actor.auraPFX = Instantiate<PFX_SupportAura>(auraPFX);
			actor.auraPFX.transform.SetParent(actor.transform);
			actor.auraPFX.transform.localPosition = Vector3.zero;
		}

		actor.fTimeToNextAura = Random.Range(fTimeBetweenSFXMin, fTimeBetweenSFXMax);
	}

	// Think of this as the actor being a vehicle and the minion doing the driving. Different minions will drive their actors differently
	public override void SimulatePlayer(Actor_Player actor)
	{
	}
	public override void SimulatePlayerFixedUpdate(Actor_Player actor)
	{
		// Default support. Do nothing. It's all in the buffs
		actor.fTimeToNextAura -= Core.GetPlayerDeltaTime();

		if (actor.auraPFX != null && actor.fTimeToNextAura <= 0.0f)
		{
			actor.fTimeToNextAura = Random.Range(fTimeBetweenSFXMin, fTimeBetweenSFXMax);
			actor.auraPFX.Trigger();
			if(soundEffect != null)
			{
				actor.soundEffect.PlayOneShot(soundEffect);
			}
		}

		List<Actor_Enemy> enemyList = new List<Actor_Enemy> ();
		enemyList.AddRange(Core.GetLevel().enemyActors);

		if (fPullSpeed > 0.0f)
		{
			foreach (Actor_Enemy enemy in enemyList)
			{
				if (enemy != null)
				{
					enemy.Move(new Vector3 (-fPullSpeed, 0.0f, 0.0f), false, true);
				}
			}
		}

		if (fWaterDebuffAttractSpeed > 0.0f)
		{
			for (int i = 0; i < enemyList.Count; i++)
			{
				if (enemyList [i].HasWaterDebuff())
				{
					for (int j = i + 1; j < enemyList.Count; j++)
					{
						if (enemyList [j].HasWaterDebuff())
						{
							Vector3 dPos = enemyList [j].transform.position - enemyList [i].transform.position;
							dPos.Normalize();
							dPos *= fWaterDebuffAttractSpeed;
							enemyList [i].Move(dPos, false, true);
							enemyList [j].Move(-dPos, false, true);
						}
					}
				}
			}
		}
	}

	public override void SimulateEnemy(Actor_Enemy actor)
	{
	}

	public override void SimulateEnemyFixedUpdate(Actor_Enemy actor)
	{
		// Default support. Go stand in a player zone, leave it, stand in it, repeat

		// If we are waiting, do that.
		if (actor.fTimeToNextMove > 0.0f)
		{
			actor.fTimeToNextMove -= Core.GetEnemyDeltaTime();
			if (actor.fTimeToNextMove <= 0.0f)
			{
				if (actor.bPickedPositionInPlayerZone)
				{
					// Last pick was in a zone, so wherever is fine this time.
					actor.target = new Vector3(Random.Range(LevelController.fMIN_X_COORD, Core.GetLevel().GetRangedZoneMax() + 3.0f), 0.0f, Random.Range(-LevelController.GetWidth() + 0.25f, LevelController.GetWidth() - 0.25f));
				}
				else
				{
					// We just wandered off wherever, so make sure we walk back through a zone this time.
					if (Random.Range(0, 2) == 0)
					{
						actor.target = new Vector3 (Random.Range(LevelController.fMIN_X_COORD, Core.GetLevel().GetMeleeZoneLimit()), 0.0f, Random.Range(-LevelController.GetWidth() + 0.25f, LevelController.GetWidth() - 0.25f));
					}
					else
					{
						actor.target = new Vector3(Random.Range(Core.GetLevel().GetRangedZoneMin(), Core.GetLevel().GetRangedZoneMax()), 0.0f, Random.Range(-LevelController.GetWidth() + 0.25f, LevelController.GetWidth() - 0.25f));
					}
				}

				actor.bPickedPositionInPlayerZone = !actor.bPickedPositionInPlayerZone;
			}
		}
		// Else, if we just reached a target, start waiting
		else if ((actor.transform.position - actor.target).sqrMagnitude < 0.1f * 0.1f)
		{
			actor.fTimeToNextMove = 2.0f;
			actor.render.SetAnimState(AnimState.IDLE, false);
		}
		// Otherwise, we must be moving to a target
		else
		{
			Vector3 move = actor.target - actor.transform.position;
			move.Normalize();
			move *= fMoveSpeed;
			actor.Move(move);
			actor.render.SetAnimState(AnimState.WALKING, false);
		}

		actor.fTimeToNextAura -= Core.GetEnemyDeltaTime();

		if (actor.auraPFX != null && actor.fTimeToNextAura <= 0.0f)
		{
			actor.fTimeToNextAura = Random.Range(fTimeBetweenSFXMin, fTimeBetweenSFXMax);
			actor.auraPFX.Trigger();
			if(soundEffect != null)
			{
				actor.soundEffect.PlayOneShot(soundEffect);
			}
		}
	}

	public override void OnProjectileHit(Actor firer, Actor target, Vector3 position)
	{
		Debug.Assert(false, "Support projectiles? REALLY???");
	}
}
