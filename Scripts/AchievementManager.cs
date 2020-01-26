using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;

public class AchievementManager : MonoBehaviour 
{
	private class AchievementProgressData
	{
		public AchievementProgressData(string achName, int i, int j)
		{
			achievementName = achName;
			iInterval = i;
			iMax = j;
		}
		public string achievementName;
		public int iInterval;
		public int iMax;
	}

	private CGameID gameID;

	private bool bRequestedStats;

	protected Callback<UserStatsReceived_t> statsRecievedCallback;
	protected Callback<UserStatsStored_t> statsStoredCallback;
	protected Callback<UserAchievementStored_t> achievementStoredCallback;

	private Dictionary<string, int> stats = new Dictionary<string, int>();
	private Dictionary<string, AchievementProgressData[]> achievementProgress = new Dictionary<string, AchievementProgressData[]>();

	public void Init()
	{
		if (!SteamManager.Initialized)
		{
			Debug.Assert(false, "SteamManager not initialized");
		}

		stats.Clear();
		stats.Add("NUM_WAVES_COMPLETED", 0);
		stats.Add("DAMAGE_DEALT", 0);
		stats.Add("CRITICAL_HITS", 0);
		stats.Add("COMBOS_GAINED", 0);
		stats.Add("HEALTH_HEALED", 0);
		stats.Add("NUM_RESURRECTIONS", 0);
		stats.Add("MINIONS_UNLOCKED", 0);
		achievementProgress.Clear();
		achievementProgress.Add("NUM_WAVES_COMPLETED", new AchievementProgressData[2] { 
			new AchievementProgressData("GETTING_STARTED", 25, 100), 
			new AchievementProgressData("VETERAN_MINIMANCER", 50, 250) });		
		achievementProgress.Add("DAMAGE_DEALT", new AchievementProgressData[2] { 
				new AchievementProgressData("DAMAGE_DEALER", 25000, 100000), 
				new AchievementProgressData("DAMAGE_MASTER", 100000, 1000000) });
		achievementProgress.Add("CRITICAL_HITS", new AchievementProgressData[1] { 
			new AchievementProgressData("CRITICALLY_ACCLAIMED", 100, 500)});
		achievementProgress.Add("COMBOS_GAINED", new AchievementProgressData[1] { 
			new AchievementProgressData("COMBO_COLLECTOR", 250, 1000)});
		achievementProgress.Add("HEALTH_HEALED", new AchievementProgressData[1] { 
			new AchievementProgressData("MASTER_HEALER", 2500, 10000)});
		achievementProgress.Add("NUM_RESURRECTIONS", new AchievementProgressData[1] { 
			new AchievementProgressData("UNKILLABLE", 10, 30)});
		achievementProgress.Add("MINIONS_UNLOCKED", new AchievementProgressData[1] { 
			new AchievementProgressData("COMPLETIONIST", 10, 113)});


		gameID = new CGameID (SteamUtils.GetAppID());

		statsRecievedCallback = Callback<UserStatsReceived_t>.Create(OnStatsReceived);
		statsStoredCallback = Callback<UserStatsStored_t>.Create(OnStatsStored);
		achievementStoredCallback = Callback<UserAchievementStored_t>.Create(OnAchievementStored);

		bRequestedStats = SteamUserStats.RequestCurrentStats();
	}

	private void LoadStatsFromSteam()
	{
		if (!SteamManager.Initialized)
			return;
		
		List<string> keys = new List<string> ();
		foreach(string key in stats.Keys)
		{
			keys.Add(key);
		}

		foreach (string key in keys)
		{
			int iValue = 0;
			SteamUserStats.GetStat(key, out iValue);
			stats [key] = iValue;
		}
	}

	private void OnStatsReceived(UserStatsReceived_t data)
	{
		if (data.m_nGameID == (ulong)gameID)
		{
			if (data.m_eResult == EResult.k_EResultOK)
			{
				Debug.Log("Received stats and achievements from Steam");
				LoadStatsFromSteam();
			}
			else
			{
				Debug.Log("RequestStats - failed, " + data.m_eResult);
			}
		}

	}

	private void OnStatsStored(UserStatsStored_t data)
	{
		if (data.m_nGameID == (ulong)gameID)
		{
			switch (data.m_eResult)
			{
				case EResult.k_EResultOK:
				{
					Debug.Log("StoreStats - success");
					break;
				}
				case EResult.k_EResultInvalidParam:
				{
					Debug.Assert(false, "StoreStats - some failed to validate");
					// We done messed up. Reset stats to some earlier point
					LoadStatsFromSteam();
					break;
				}
				default:
				{
					Debug.Log("StoreStats - failed, " + data.m_eResult);
					break;
				}
			}
		}
	}

	private void OnAchievementStored(UserAchievementStored_t data)
	{
		if (data.m_nGameID == (ulong)gameID)
		{
			if (data.m_nMaxProgress == 0)
				Debug.Log("Achivement '" + data.m_rgchAchievementName + "' unlocked");
			else
				Debug.Log("Achievement '" + data.m_rgchAchievementName + "' progress callback, (" + data.m_nCurProgress + "," + data.m_nMaxProgress + ")");
		}
	}

	public int GetStat(string apiName)
	{
		return stats [apiName];
	}

	public void IncrementStat(string apiName, int iValue)
	{
		if (!SteamManager.Initialized)
			return;
		
		foreach (AchievementProgressData data in achievementProgress[apiName])
		{
			if (stats [apiName] < data.iMax)
			{
				if ((stats [apiName] / data.iInterval) !=
				   ((stats [apiName] + iValue) / data.iInterval))
				{
					SteamUserStats.IndicateAchievementProgress(data.achievementName, (uint)(stats [apiName] + iValue), (uint)data.iMax);
				}

				if (stats [apiName] < data.iMax &&
				   (stats [apiName] + iValue) >= data.iMax)
				{
					TriggerAchievement(data.achievementName);
				}
			}
		}

		stats [apiName] += iValue;
		SteamUserStats.SetStat(apiName, stats[apiName]);
	}

	public void TriggerAchievement(string apiName)
	{
		if (!SteamManager.Initialized)
			return;
		
		bool bAchieved = false;
		SteamUserStats.GetAchievement(apiName, out bAchieved);
		if (!bAchieved)
		{
			SteamUserStats.SetAchievement(apiName);
			SteamUserStats.StoreStats();
		}
	}
}
