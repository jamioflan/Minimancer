using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class LevelController : MonoBehaviour 
{
	public static readonly float fMIN_X_COORD = -3.0f;
	public static readonly float fMAX_X_COORD = 20.0f;
	public static readonly float fMIN_Z_COORD = -3.0f;
	public static readonly float fMAX_Z_COORD = -fMIN_Z_COORD;

	public enum LevelState
	{
		PRE_INIT,
		PLAYING,
		COMPLETE_SCREEN,
	}

	public LevelState state = LevelState.PRE_INIT;
	public bool bWon = false;

	public Location location;
	public PlayerRig instance;
	private TeamRoster roster;

	public int iCurrentWave = 0;
	public int iNumSpawnedInWave = 0;
	public float fCurrentWaveDuration = 0.0f;
	public float fGracePeriodDuration = 0.0f;

	private int iHighestWave = -1;

	public bool bFastForward = false;

	public float fMeleeMinionsHealRate = 6.0f, fRangedMinionsHealRate = 6.0f;

	// Both these lie in the range 0~7.
	public float fRangedZoneMin = 0.0f; 
	public float fRangedZoneMax = 7.0f;
	// Can be up to 3.0f
	public float fMeleeZoneSize = 1.0f;

	public float fLaneWidth = 6.0f;

	public bool bWalls = false;

	public Actor_Player[] playerActors = new Actor_Player[(int)MinionSlot.NUM_MINION_SLOTS];
	public List<Actor_Enemy> enemyActors = new List<Actor_Enemy> ();

	public List<MinionTemplate> pendingSpawns = new List<MinionTemplate>();
	public int[] aiNumKilledPerWave = new int[3];
	public int[] aiNumSpawnedPerWave = new int[3];

	public bool[] abNewlyUnlocked = new bool[3]{ false, false, false };

	public List<Resurrection> singleUseResurrections = new List<Resurrection>();

	public bool bHasSomeDamageBeenTaken = false;

    public Light sun;
    public float sunTime = 0.0f;
    public Quaternion originalSunPos;

	public TeamRoster GetRoster() { return roster; }


	public void StartLevel(TeamRoster selectedRoster)
	{
		// This oddly named object points to all sorts of useful things that exist in all levels
		instance = FindObjectOfType<PlayerRig>();

		if (instance == null)
		{
			instance = Instantiate<PlayerRig>(Core.theCore.playerRigPrefab);
			instance.transform.position = Vector3.zero;
		}

        GameObject sunObject = GameObject.Find("Directional Light");
        if (sunObject != null)
        {
            sun = sunObject.GetComponent<Light>();
            originalSunPos = sun.transform.rotation;
        }

		instance.gameObject.SetActive(true);

		Range[] ranges = new Range[2] { Range.WIDE, Range.WIDE };

		bool bAgainstElement = false;

		fMeleeZoneSize = 1.0f;

		Core.theCore.fEnemyTimescale = 1.0f;
		Core.theCore.fPlayerTimescale = 1.0f;

		// Spawn the player's minions immediately. Then give a countdown to start the game

		roster = selectedRoster;
		for (int i = 0; i < (int)MinionSlot.NUM_MINION_SLOTS; i++)
		{
			Transform spawnPoint = instance.playerSpawnPoints [i];
			Minion minion = roster.minions [i];
			if (spawnPoint != null && minion != null)
			{
				// Create new game object
				GameObject go = new GameObject ("PlayerMinion_" + i + "_" + minion.template.name);

				// Fill it with actor components
				Actor_Player actor = go.AddComponent<Actor_Player>();
				actor.InitFromMinion(minion);
				actor.transform.position = spawnPoint.position;

				// Add renderer to actor
				RenderActor render = Instantiate<RenderActor>(minion.template.render);
				render.transform.SetParent(actor.transform);
				render.transform.localPosition = Vector3.zero;
				render.transform.localScale = new Vector3 (-1.0f, 1.0f, 1.0f);
				render.Init(actor);
				actor.render = render;

				// Add audio sources
				actor.soundEffect = go.AddComponent<AudioSource>();
				actor.soundEffect.clip = minion.template.soundEffect;
				actor.soundEffect.playOnAwake = false;
				actor.soundEffect.outputAudioMixerGroup = Core.GetAudioManager().soundEffectGroup;

				// And combo numbers
				if ((minion.template.canCombo || minion.template.bDeathtoll || minion.template.bRelentless) && minion.template.comboNumbers != null)
				{
					ComboNumbers combo = Instantiate<ComboNumbers>(minion.template.comboNumbers);
					combo.transform.SetParent(actor.transform);
					combo.transform.localPosition = new Vector3 (-0.17f, 0.215f, -0.13f);
					actor.comboNumbers = combo;
				}

				// Save a reference for later
				playerActors [i] = actor;

				// 
				if (((MinionSlot)i).GetSlotType() == MinionSlotType.RANGED && (minion.template is Minion_Ranged))
				{
					ranges [i == (int)MinionSlot.RANGED_1 ? 0 : 1] = ((Minion_Ranged)minion.template).range;
				}

				if (minion.template is Minion_Support && ((Minion_Support)(minion.template)).bWalls)
				{
					bWalls = true;
				}

				foreach (Resurrection res in minion.template.resurrectionTriggers)
				{
					singleUseResurrections.Add(new Resurrection(res));
				}

				if (minion.template.element.GetDamageMultiplier(location.element) < 1.0f && ((MinionSlot)i).GetSlotType() != MinionSlotType.SUPPORT)
					bAgainstElement = true;

				Core.theCore.fEnemyTimescale += minion.GetBuff(Stat.ENEMY_TIMESCALE);
				Core.theCore.fPlayerTimescale += minion.GetBuff(Stat.PLAYER_TIMESCALE);

				fMeleeZoneSize += minion.GetBuff(Stat.MELEEZONE_SIZE);
			}
		}

		roster.bHasThreeResurrectsAvailable = singleUseResurrections.Count == 6;
		roster.bHasActiveCollector = false;

		// After ALL player minions are created, then calculate their passive buffs and store them off.
		for (int i = 0; i < (int)MinionSlot.NUM_MINION_SLOTS; i++)
		{
			if (playerActors [i] != null)
			{
				playerActors [i].CalculateMyAggregateBuffs();
				playerActors [i].SetMaxHealthFromBuffs();
				playerActors [i].minion.ResetTemporaryData();
			}
		}

		roster.RecalculateHealths();

		fRangedZoneMin = Ranges.GetMinRangeForPair(ranges [0], ranges [1]);
		fRangedZoneMax = Ranges.GetMaxRangeForPair(ranges [0], ranges [1]);
		fLaneWidth = bWalls ? 4.0f : 6.0f;
		InitZones(bWalls);

		aiNumKilledPerWave = new int[location.numWaves];
		aiNumSpawnedPerWave = new int[location.numWaves];
        abNewlyUnlocked = new bool[location.numWaves];


        iCurrentWave = -1;
		AdvanceWave();
		fGracePeriodDuration = location.gracePeriodDuration;

		instance.elementalHint.enabled = bAgainstElement;
		if (bAgainstElement)
		{
			instance.elementalHint.text = LocalizationManager.GetLoc(location.element.GetHintText());
		}

		Core.GetAudioManager().SetLevelMusic(location.music);

		RequestState(LevelState.PLAYING);
	}

	private void InitZones(bool bWalls)
	{
		instance.SetMeleeZone(fMIN_X_COORD, fMIN_X_COORD + fMeleeZoneSize, fLaneWidth);
		instance.SetRangedZone(GetRangedZoneMin(), GetRangedZoneMax(), fLaneWidth);

		if (bWalls)
		{
			Transform walls = Instantiate<Transform>(instance.wallsPrefab);
			walls.SetParent(instance.transform);
			walls.localPosition = Vector3.zero;
			walls.localScale = new Vector3 (1.0f, 1.0f, 1.0f);
		}
	}


	private void RequestState(LevelState newState)
	{
		switch (newState)
		{
			case LevelState.COMPLETE_SCREEN:
			{
				LevelCompleteScreen completeScreen = instance.levelCompleteScreen;
				Debug.Assert(completeScreen != null, "No complete screen?");
				if (bWon)
				{
					completeScreen.OnMissionSuccess();
				}
				else
					completeScreen.OnMissionFail();
				break;
			}
		}
		state = newState;
	}

	public int GetCurrentWave()
	{
		if (fGracePeriodDuration < location.gracePeriodDuration)
		{
			return iCurrentWave - 1;
		}
		return iCurrentWave;
	}

	void Start () 
	{
		
	}

	public void Shutdown()
	{
		Core.GetWorldMap().SetLocationCompletion(location, GetHighestCompletedWave());

		Core.GetAudioManager().ReturnToWorldMapMusic();

		Core.GetTutorialManager().AdvanceTutorialStateIfInState(TutorialManager.TutorialState.MISSION_FAIL);
		Core.GetTutorialManager().AdvanceTutorialStateIfInState(TutorialManager.TutorialState.TUTORIAL_COMPLETE);

		Core.Save();
		Core.ReturnToWorldMap();
		foreach (Actor_Enemy enemy in enemyActors)
		{
			if(enemy != null)
				Destroy(enemy.gameObject);
		}
		foreach (Actor_Player player in playerActors)
		{
			if(player != null)
				Destroy(player.gameObject);
		}
		Destroy(gameObject);
	}

	public float GetParametricProgress() 
	{ 
		return (iCurrentWave + fCurrentWaveDuration / location.waveDuration) / location.numWaves; 
	}

	public float GetWaveCompletion(int iWave)
	{
		return (float)aiNumKilledPerWave [iWave] / (float)aiNumSpawnedPerWave [iWave];
	}

	public int GetHighestCompletedWave()
	{
		int iCompletedWave = -1;
		for (int i = 0; i < location.numWaves; i++)
		{
			if (GetWaveCompletion(i) >= 1.0f)
				iCompletedWave = i;
		}

		return iCompletedWave;
	}

	public void Update () 
	{
        if (sun != null)
        {
            sunTime += Core.GetDeltaTime();
            Quaternion newPos = Quaternion.SlerpUnclamped(originalSunPos, Quaternion.identity, sunTime * 0.001f);
            sun.transform.rotation = newPos;
        }

        switch (state)
		{
			case LevelState.PLAYING:
			{
				if (fGracePeriodDuration < location.gracePeriodDuration)
				{
					fGracePeriodDuration += Core.GetDeltaTime();
				}
				else
				{
					// Check for wave advance
					fCurrentWaveDuration += Core.GetDeltaTime();
					if (fCurrentWaveDuration >= location.waveDuration)
					{
						AdvanceWave();
					}

					// Simulate current wave if valid
					if (iCurrentWave < location.numWaves)
					{
						float fParametricProgress = (fCurrentWaveDuration / location.waveDuration);
						// Cumulative sum of a normal distribution of spawns. Totals to numSpawnsPerWave, and has 0 spawn rate at the start and end
						float fExpectedNumSpawns = location.GetNumSpawnsPerWave(iCurrentWave) * fParametricProgress * fParametricProgress * (3 - 2 * fParametricProgress);

						while (iNumSpawnedInWave < fExpectedNumSpawns)
						{
							iNumSpawnedInWave++;
							SpawnRandomGroup();
						}
					}
				}

				while (GetHighestCompletedWave() > iHighestWave)
				{
					iHighestWave++;

					foreach (Actor_Player player in playerActors)
					{
						if (player == null)
							continue;
						player.minion.template.TriggerBuff(BuffTrigger.WAVE_COMPLETED, null);
					}

					if (iHighestWave < location.unlocks.Length && location.unlocks [iHighestWave] != null)
					{
						if (!Core.GetPlayerProfile().pool.unlocks.Contains(location.unlocks [iHighestWave]))
						{
							abNewlyUnlocked [iHighestWave] = true;
						}
						Core.GetPlayerProfile().pool.AddMinion(location.unlocks [iHighestWave]);
					}
				}

				if (GetNumEnemiesInMeleeZone() == 0)
				{
					roster.HealGroup(MinionSlotType.MELEE, fMeleeMinionsHealRate * Core.GetPlayerDeltaTime());
				}

				if (GetNumEnemiesInRangedZone() == 0)
				{
					roster.HealGroup(MinionSlotType.RANGED, fRangedMinionsHealRate * Core.GetPlayerDeltaTime());
				}

				float fHeals = 0.0f;
				foreach (Minion minion in roster.minions)
				{
					if (minion.template.GetSlotType() == MinionSlotType.SUPPORT)
					{
						fHeals += ((Minion_Support)minion.template).fHealPerSecond;
					}
				}
				roster.HealGroup(MinionSlotType.MELEE, fHeals * Core.GetPlayerDeltaTime());
				roster.HealGroup(MinionSlotType.RANGED, fHeals * Core.GetPlayerDeltaTime());

				if (GetParametricProgress() >= 1.0f)
				{
					bool bAllWavesComplete = true;
					for (int i = 0; i < location.numWaves; i++)
					{
						if (GetWaveCompletion(i) < 1.0f)
							bAllWavesComplete = false;
					}

					if (bAllWavesComplete)
					{
						PlayerWon();
					}
				}

				break;
			}

		}
	}

	private void SpawnRandomGroup()
	{
		if (pendingSpawns.Count == 0)
		{
			Debug.Assert(false, "Ran out of spawns");
			return;
		}
		int iPick = Random.Range(0, pendingSpawns.Count);

		MinionTemplate template = pendingSpawns [iPick];
		pendingSpawns.RemoveAt(iPick);

		Vector3 spawnPos = GetBestSpawnPoint();

		{
			// Create new game object
			GameObject go = new GameObject("EnemyMinion_" + template.name);

			// Create a minion and attach it to the actor's game object. Enemy minions are less permanent data structures than player ones, so they can just be chucked into the battlefield
			MinionTemplateManager mtm = Core.GetMinionTemplateManager();
			Minion minion = mtm.CreateMinion(template);
			minion.transform.SetParent(go.transform);
			minion.transform.localPosition = Vector3.zero;

			// Fill it with actor components
			Actor_Enemy actor = go.AddComponent<Actor_Enemy>();
			actor.InitFromMinion(minion);
			actor.iWave = iCurrentWave;
			aiNumSpawnedPerWave [iCurrentWave]++;
			Vector2 randomOffset = Random.insideUnitCircle * Random.Range(0.0f, 0.5f);
			actor.transform.position = spawnPos + new Vector3(randomOffset.x, 0.0f, randomOffset.y);
			// Push the next member of the group back a bit
			spawnPos = new Vector3 (spawnPos.x + 1.0f, spawnPos.y, spawnPos.z);

			// Add renderer to actor
			RenderActor render = Instantiate<RenderActor>(template.render);
			render.transform.SetParent(actor.transform);
			render.transform.localPosition = Vector3.zero;
			render.Init(actor);
			actor.render = render;

			// Add audio sources
			actor.soundEffect = go.AddComponent<AudioSource>();
			actor.soundEffect.clip = minion.template.soundEffect;
			actor.soundEffect.playOnAwake = false;
			actor.soundEffect.outputAudioMixerGroup = Core.GetAudioManager().soundEffectGroup;

			// Add healthbar
			Healthbar healthbar = Instantiate<Healthbar>(mtm.healthbarPrefab);
			healthbar.transform.SetParent(actor.transform);
			healthbar.transform.localPosition = new Vector3 (0.0f, template.fHeight - 1.0f, 0.0f);
			actor.healthbar = healthbar;

			actor.CalculateMyAggregateBuffs();

			// Store a reference for later
			enemyActors.Add(actor);
		}
	}

	public void KillAllEnemies()
	{
		List<Actor_Enemy> killList = new List<Actor_Enemy> ();
		killList.AddRange(enemyActors);
		foreach (Actor_Enemy enemy in killList)
		{
			enemy.Damage(instance.fatalDamage, 1.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f);
		}
	}

	private void AdvanceWave()
	{
		if (iCurrentWave == location.numWaves - 1)
			return;

		iCurrentWave++;
		iNumSpawnedInWave = 0;
		fCurrentWaveDuration = 0.0f;
		fGracePeriodDuration = 0.0f;

		Debug.Assert(pendingSpawns.Count == 0, "Not finished spawning the previous wave");
		pendingSpawns.Clear();

		pendingSpawns.AddRange(location.GetSpawns(iCurrentWave));


	}

	private Vector3 GetBestSpawnPoint()
	{
		// TODO: Implement some sort of spreading out code / picking empty spawns code
		int iPick = Random.Range(0, instance.enemySpawnPoints.Length);

		if (bWalls)
		{
			iPick = Random.Range(1, instance.enemySpawnPoints.Length - 1);
		}

		return instance.enemySpawnPoints [iPick].position;
	}

	public void StartNextWave()
	{
		if (fGracePeriodDuration < location.gracePeriodDuration)
		{
			fGracePeriodDuration = location.gracePeriodDuration;
		}
		else
		{
			//Debug.Assert(false, "Trying to skip a wave mid-wave!");
		}
	}

	public void FastForward(bool bSet)
	{
		bFastForward = bSet;
	}

	public void Abort()
	{
		PlayerLost();
	}

	public void KillPlayerActor(MinionSlot slot)
	{
		playerActors [(int)slot].Kill();
	}

	// Called when all player's minions are dead
	public void PlayerLost()
	{
		bWon = false;
		if (GetHighestCompletedWave() >= 0)
		{
			Core.IncrementStat("NUM_WAVES_COMPLETED", GetHighestCompletedWave() + 1);
		}
		Core.GetTutorialManager().AdvanceTutorialStateIfInState(TutorialManager.TutorialState.WATCH_FIGHT);
		RequestState(LevelState.COMPLETE_SCREEN);
		Core.PauseGame();
	}

	public void PlayerWon()
	{
		bWon = true;
		Core.IncrementStat("NUM_WAVES_COMPLETED", location.numWaves);
		Core.TriggerAchievement("CLEANING_UP");

		if (roster.bHasActiveCollector)
		{
			Core.TriggerAchievement("COLLECTOR");
		}
		bool bSingleElementTeam = true;
		for (int i = 1; i < (int)MinionSlot.NUM_MINION_SLOTS; i++)
		{
			if (roster.minions [i].template.element != roster.minions [0].template.element)
			{
				bSingleElementTeam = false;
				break;
			}
		}
		if (bSingleElementTeam)
		{
			if (roster.minions [0].template.element.GetDamageMultiplier(location.element) < 1.0f)
				Core.TriggerAchievement("OVERCOMING_ADVERSITY");
		}

		if (!bHasSomeDamageBeenTaken)
		{
			Core.TriggerAchievement("UNTOUCHABLE");
		}

		Core.GetTutorialManager().AdvanceTutorialStateIfInState(TutorialManager.TutorialState.WATCH_FIGHT);
		RequestState(LevelState.COMPLETE_SCREEN);
		Core.PauseGame();
	}

	public Actor_Enemy GetClosestEnemyToMeleeZone()
	{
		float fBestDistance = 100.0f;
		Actor_Enemy bestActor = null;
		foreach (Actor_Enemy enemy in enemyActors)
		{
			if (enemy.transform.position.x < fBestDistance)
			{
				fBestDistance = enemy.transform.position.x;
				bestActor = enemy;
			}
		}

		return bestActor;
	}

	public Actor_Enemy GetClosestEnemyToRangedZone()
	{
		float fRangedZoneCentre = (GetRangedZoneMin() + GetRangedZoneMax()) / 2.0f;

		float fBestDistance = 100.0f;
		Actor_Enemy bestActor = null;
		foreach (Actor_Enemy enemy in enemyActors)
		{
			float fX = enemy.transform.position.x;

			float fDist = Mathf.Abs(fX - fRangedZoneCentre);
			if (fDist < fBestDistance)
			{
				fBestDistance = fDist;
				bestActor = enemy;
			}
		}

		return bestActor;
	}

	public float GetRangedZoneMin()
	{
		return fMIN_X_COORD + 3.0f + fRangedZoneMin;
	}

	public float GetRangedZoneMax()
	{
		return fMIN_X_COORD + 3.0f + fRangedZoneMax;
	}

	public float GetMeleeZoneLimit()
	{
		return fMIN_X_COORD + fMeleeZoneSize;
	}

	public static float GetWidth()
	{
		return Core.GetLevel().bWalls ? 2.0f : 3.0f;
	}


	public int GetNumEnemiesInMeleeZone()
	{
		int iNumEnemies = 0;
		foreach (Actor_Enemy enemy in enemyActors)
		{
			if (enemy.IsInMeleeZone())
				iNumEnemies++;
		}
		return iNumEnemies;
	}

	public int GetNumEnemiesInRangedZone()
	{
		int iNumEnemies = 0;
		foreach (Actor_Enemy enemy in enemyActors)
		{
			if (enemy.IsInRangedZone())
				iNumEnemies++;
		}
		return iNumEnemies;
	}

	private static readonly MinionSlotType[] rangedPriorities = new MinionSlotType[]{ MinionSlotType.RANGED, MinionSlotType.SUPPORT, MinionSlotType.MELEE };

	public Actor_Player GetRangedTarget()
	{
		foreach (MinionSlotType slotType in rangedPriorities)
		{
			if (GetRoster().IsAnyoneAlive(slotType))
			{
				int iPick = Random.Range(0, slotType.GetNumSlots());
				return playerActors [(int)slotType.GetFirst() + iPick];
			}
		}
		return null;
	}

	public void DamageRadius(Damage damage, Vector3 pos, float fRadius, float fMultiplier, float fStunTime, float fAdditionalCritChance, float fAdditionalCritDamage, float fVampirism, float fAdditionalDamage)
	{
		List<Actor_Enemy> tempEnemyList = new List<Actor_Enemy>();
		tempEnemyList.AddRange(enemyActors);
		foreach(Actor_Enemy enemy in tempEnemyList)
		{
			Vector3 dPos = enemy.transform.position - pos;
			dPos.y = 0.0f;
			if (dPos.sqrMagnitude <= fRadius * fRadius)
			{
				float fElementalMultiplier = Elements.GetDamageMultiplier(damage.GetElement(), enemy.minion.template.element);
				float fMod = enemy.GetInherentElementalResistanceModifier();
				if (fElementalMultiplier < 1.0f)
				{
					fElementalMultiplier = 1.0f - ((1.0f - fElementalMultiplier) * fMod);
				}

				if (!enemy.IsDead())
				{
					enemy.Damage(damage, fMultiplier * fElementalMultiplier, fStunTime, fAdditionalCritChance, fAdditionalCritDamage, fVampirism, fAdditionalDamage);
				}
			}
		}
	}

	public void EnemyDied(Actor_Enemy enemy)
	{
		foreach (Actor_Player player in playerActors)
		{
			if (player == null)
				continue;
			player.minion.template.TriggerBuff(BuffTrigger.ENEMY_DIED, null);
			if (player.minion.template.bDeathtoll)
			{
				player.minion.template.IncrementCombo(player);
			}
			if (enemy.IsInMeleeZone()) // Approximation
			{
				player.minion.template.TriggerBuff(BuffTrigger.ENEMY_DIED_TO_MELEE, null);
			}
		}
		enemyActors.Remove(enemy);
		aiNumKilledPerWave [enemy.iWave]++;
	}

	public float GetTimeScaleMultiplier()
	{
		if (bFastForward)
			return 5.0f;
		return 1.0f;
	}
}

