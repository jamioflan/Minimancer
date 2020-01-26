using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System;
using Steamworks;

public class Core : MonoBehaviour 
{
	public enum State
	{
		DEBUG_MENU, // Direct-to-level selection

		LOAD_GAME,
		EXIT_GAME,
		MAIN_MENU,

		LOAD_WORLD,
		EXIT_WORLD,
		MAP_SCREEN,

		LOAD_LEVEL,
		EXIT_LEVEL,
		PLAYING,
	}

	public static readonly bool IS_DEMO = false;
	public static bool IsDemo() { return IS_DEMO; }

	public static Core theCore;
	public static MinionTemplateManager GetMinionTemplateManager() { return theCore.minionTemplateManager; }
	public static WorldMap GetWorldMap() { return theCore.currentProfile.world; }
	public static PlayerProfile GetPlayerProfile() { return theCore.currentProfile; }
	public static AudioManager GetAudioManager() { return theCore.audioManager; }
	public static LevelController GetLevel() { return theCore.currentLevel; }
	public static TeamRoster GetCurrentRoster() { return theCore.currentLevel == null ? theCore.currentProfile.GetSelectedRoster() : theCore.currentLevel.GetRoster(); }
	public static TutorialManager GetTutorialManager() { return theCore.tutorialManager; }

	public PlayerRig playerRigPrefab;
	public PlayerProfile playerProfilePrefab;

	public AsyncOperation sceneLoadOp = null;
	public State currentState = State.LOAD_GAME;
	public PlayerProfile currentProfile;
	public LevelController currentLevel;
	public MinionTemplateManager minionTemplateManager;
	public AudioManager audioManager;
	public AchievementManager achievementManager;
	public TutorialManager tutorialManager;
	public string worldMapScene, mainMenuScene;

	public float fEnemyTimescale = 1.0f;
	public float fPlayerTimescale = 1.0f;

	public ConfigFile config;

	public List<PlayerProfile> savedGames = new List<PlayerProfile>();
	public int lastSavedGameIndex = 0;

	public static float GetEnemyFixedDeltaTime() 
	{ 
		float fTime = Time.fixedDeltaTime * theCore.fEnemyTimescale;
		if (GetLevel() != null)
		{
			fTime *= GetLevel().GetTimeScaleMultiplier(); 
		}
		return fTime;
	}

	public static float GetEnemyDeltaTime() 
	{ 
		float fTime = Time.deltaTime * theCore.fEnemyTimescale;
		if (GetLevel() != null)
		{
			fTime *= GetLevel().GetTimeScaleMultiplier(); 
		}
		return fTime;
	}

	public static float GetPlayerDeltaTime() 
	{ 
		float fTime = Time.deltaTime * theCore.fPlayerTimescale;
		if (GetLevel() != null)
		{
			fTime *= GetLevel().GetTimeScaleMultiplier(); 
		}
		return fTime;
	}

	public static float GetDeltaTime()
	{
		float fTime = Time.deltaTime;
		if (GetLevel() != null)
		{
			fTime *= GetLevel().GetTimeScaleMultiplier(); 
		}
		return fTime;
	}

	void Awake () 
	{
		if (theCore != null)
		{
			DestroyImmediate(gameObject);
			return;
		}
		theCore = this;
		DontDestroyOnLoad(this);

		if (SteamManager.Initialized)
		{
			Debug.Log("Steamworks initialised for user " + SteamFriends.GetPersonaName());
		}
	}

	public static void PauseGame()
	{
		theCore.fEnemyTimescale = 0.0f;
		theCore.fPlayerTimescale = 0.0f;
	}

	void Update () 
	{
		UpdateStates ();
	}

	void UpdateStates()
	{
		switch (currentState) 
		{
			case State.DEBUG_MENU:
			{	
				
				break;
			}
			case State.LOAD_GAME:
			{	
				// Request state once done. Currently nothing to do.
				achievementManager.Init();
				LoadAllProfiles();
				RequestState (State.MAIN_MENU);
				break;
			}
			case State.EXIT_GAME:
			{	
				// Request state once done. Currently nothing to do.
				RequestQuit();
				break;
			}
			case State.MAIN_MENU:
			{	

				break;
			}
			case State.LOAD_WORLD:
			{	
				// Request state once done. Wait for load operation
				if (sceneLoadOp.isDone)
				{
					audioManager.SetWorldMapMusic();
					MapNode.RefreshAllNodes();
					RequestState(State.MAP_SCREEN);
				}
				break;
			}
			case State.EXIT_WORLD:
			{	
				// Request state once done. Currently nothing to do.
				// Save world here
				if (sceneLoadOp.isDone)
				{
					audioManager.SetMainMenuMusic();
					RequestState(State.MAIN_MENU);
				}
				break;
			}
			case State.MAP_SCREEN:
			{	
				if (currentProfile.world == null)
				{
					Debug.Assert (false, "In map screen state without a world! Aborting");
					RequestState (State.EXIT_WORLD);
				}
				break;
			}
			case State.LOAD_LEVEL:
			{	
				// Request state once done. Wait for load operation
				if (sceneLoadOp.isDone)
				{
					TeamRoster roster = currentProfile.GetSelectedRoster();
					RequestState(State.PLAYING);
					currentLevel.StartLevel(roster);
				}
				break;
			}
			case State.EXIT_LEVEL:
			{	
				// Request state once done. Wait for load operation
				if (sceneLoadOp.isDone)
				{
					foreach (Minion minion in GetCurrentRoster().minions)
					{
						minion.InitFromTemplate(minion.template);
					}
					MapNode.RefreshAllNodes();
					RequestState(State.MAP_SCREEN);
				}
				break;
			}
			case State.PLAYING:
			{	
				if (currentLevel == null)
				{
					Debug.Assert (false, "In playing state without a level! Aborting");
					RequestState (State.EXIT_LEVEL);
				}
				break;
			}
		}
	}


	public void RequestState(State newState)
	{
		// Perform an immediate swap for now.
		Debug.Log("Switched from state " + currentState + " to " + newState);
		currentState = newState;
	}

	private void RequestQuit()
	{
		// Immediate quit!
		Application.Quit ();
	}

	public static void LaunchSelectedLevel()
	{
		Location loc = theCore.currentProfile.world.selectedLocation;

		if (loc != null)
		{
			// Create a new game object that holds the level controller
			GameObject go = new GameObject ("Level_" + loc.name);
			go.transform.SetParent(theCore.transform);
			theCore.currentLevel = go.AddComponent<LevelController>();
			theCore.currentLevel.location = loc;

			theCore.tutorialManager.AdvanceTutorialStateIfInState(TutorialManager.TutorialState.SELECT_TOWN_NODE);
			theCore.tutorialManager.AdvanceTutorialStateIfInState(TutorialManager.TutorialState.RESELECT_TOWN_NODE);

			// Now load the required scene and start playing
			theCore.sceneLoadOp = SceneManager.LoadSceneAsync(loc.sceneName);
			if (theCore.sceneLoadOp == null)
			{
				Debug.Assert(false, "Failed to start scene load for " + loc.sceneName);
				return;
			}
			theCore.RequestState(State.LOAD_LEVEL);
		}
		else
		{
			Debug.Assert(false, "Can't launch selected level, something is wrong");
		}
	}

	public static void ReturnToWorldMap()
	{
		LoadWorldMap();

		theCore.RequestState(Core.State.EXIT_LEVEL);
		theCore.currentLevel = null;
	}

	public static void ReturnToMainMenu()
	{
		Save();
		theCore.currentProfile = null;

		LoadMainMenu();
		theCore.RequestState(Core.State.EXIT_WORLD);
	}


	public static void LoadWorldMap()
	{
		// Now load the required scene and start playing
		theCore.sceneLoadOp = SceneManager.LoadSceneAsync(theCore.worldMapScene);
		if (theCore.sceneLoadOp == null)
		{
			Debug.Assert(false, "Failed to start scene load for WorldMap");
			return;
		}
	}

	public static void LoadMainMenu()
	{
		// Now load the required scene and start playing
		theCore.sceneLoadOp = SceneManager.LoadSceneAsync(theCore.mainMenuScene);
		if (theCore.sceneLoadOp == null)
		{
			Debug.Assert(false, "Failed to start scene load for MainMenu");
			return;
		}
	}

	public static void CreateSaveGame(string saveName)
	{
		// Create and load a new slot
		PlayerProfile profile = Instantiate<PlayerProfile>(theCore.playerProfilePrefab);
		profile.Init();
		profile.name = saveName;
		profile.transform.SetParent(theCore.transform);
		theCore.savedGames.Add(profile);
		LoadSavedGame(theCore.savedGames.Count - 1);
		Save();
	}

	public static void LoadSavedGame(int slot)
	{
		// Load an existing slot
		theCore.currentProfile = theCore.savedGames[slot];
		theCore.currentProfile.lastUsed = DateTime.Now;
		theCore.currentProfile.pool.UpdateMinionAchievement();
		LoadWorldMap();
		theCore.RequestState(State.LOAD_WORLD);
	}

	public static void ResumeLastSavedGame()
	{
		LoadSavedGame(theCore.lastSavedGameIndex);
	}

	public static int GetNumSaveGames()
	{
		return theCore.savedGames.Count;
	}

	public void ApplyConfig()
	{
		Core.GetAudioManager().SetMusicVolume(100.0f * Mathf.Sqrt(config.fMusicVolume) - 80.0f);
		Core.GetAudioManager().SetGameVolume(100.0f * Mathf.Sqrt(config.fInGameVolume) - 80.0f);
	}

	public void LoadAllProfiles()
	{
		if (!Directory.Exists("config/"))
			Directory.CreateDirectory("config/");
		
		config.Init();
		// Load config
		config.Load();
		ApplyConfig();

		foreach (PlayerProfile profile in savedGames)
		{
			Destroy(profile.gameObject);
		}
		savedGames.Clear();

		if (Directory.Exists("saves/"))
		{
			foreach (string filename in Directory.GetFiles("saves/"))
			{
				PlayerProfile profile = Instantiate<PlayerProfile>(playerProfilePrefab);
				profile.Init();
				profile.name = filename.Remove(0, "saves/".Length).Split('.') [0];
				profile.Load(filename);
				profile.transform.SetParent(transform);
				savedGames.Add(profile);
			}
		}
		else
		{
			Directory.CreateDirectory("saves/");
			Debug.Log("Did not find save directory.");
		}

		savedGames.Sort();
	}

	public static void Save()
	{
		theCore.config.Save();

		foreach (PlayerProfile profile in theCore.savedGames)
		{
			profile.Save("saves/" + profile.name + ".save");
		}
	}

	public static void BuffPlayerTimescale(float fMod)
	{
		theCore.fPlayerTimescale += fMod;
	}

	public static int GetStat(string apiName)
	{
		return theCore.achievementManager.GetStat(apiName);
	}

	public static void IncrementStat(string apiName, int iValue)
	{
		theCore.achievementManager.IncrementStat(apiName, iValue);
	}

	public static void TriggerAchievement(string apiName)
	{
		theCore.achievementManager.TriggerAchievement(apiName);
	}
}
