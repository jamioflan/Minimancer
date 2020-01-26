using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization;

public class WorldMap : MonoBehaviour 
{
	[System.Serializable]
	public class LocationCompletion
	{
		public Location location;
		public int iNumWaves;
	}

	public List<Location> locations = new List<Location>();
	public List<LocationCompletion> locationsCompleted = new List<LocationCompletion>();
	public Location selectedLocation;
	public bool[] abUnfinished = new bool[(int)Element.NO_ELEMENT] { false, false, false, false, false, false, false };

	void Start () 
	{
		MapNode.RefreshAllNodes();
	}

	void Update () 
	{
		
	}

	public MapNode.NodeState GetState(Location location)
	{
		if (IsCompleted(location))
			return MapNode.NodeState.COMPLETED;
		
		if (IsInProgress(location))
			return MapNode.NodeState.PARTIALLY_COMPLETED;

		if (location.bIsGauntlet)
		{
			for (int i = 0; i < (int)Element.NO_ELEMENT; i++)
			{
				if(abUnfinished[i])
					return MapNode.NodeState.INACCESSIBLE;
			}

			return MapNode.NodeState.ACCESSIBLE;
		}

		if (location.prerequisite == null || IsInProgress(location.prerequisite))
			return MapNode.NodeState.ACCESSIBLE;

		return MapNode.NodeState.INACCESSIBLE;
	}

	public int GetHighestCompletedWave(Location location)
	{
		foreach (LocationCompletion data in locationsCompleted)
		{
			if (data.location == location)
				return data.iNumWaves;
		}
		return -1;
	}

	public void SetLocationCompletion(Location location, int iWave)
	{
		bool bFound = false;
		foreach (LocationCompletion data in locationsCompleted)
		{
			if(data.location == location)
			{
				bFound = true;
				if (iWave > data.iNumWaves)
					data.iNumWaves = iWave;
			}
		}

		if (!bFound)
		{
			LocationCompletion data = new LocationCompletion();
			data.location = location;
			data.iNumWaves = iWave;
			locationsCompleted.Add(data);
		}
			
		MapNode.RefreshAllNodes();
		CheckForAchievements();
	}

	private void CheckForAchievements()
	{
		bool bHasBeenBeyondNeutralArea = false;
		foreach (LocationCompletion data in locationsCompleted)
		{
			if (data.location.element != Element.PHYSICAL)
				bHasBeenBeyondNeutralArea = true;
			if (data.iNumWaves < 2 && data.location.element != Element.NO_ELEMENT)
				abUnfinished [(int)data.location.element] = true;

            if (data.location.element == Element.NO_ELEMENT)
            {
                if (data.iNumWaves > -1)
                    Core.TriggerAchievement("GAUNTLET_CHALLENGER");
                if (data.iNumWaves >= 7)
                    Core.TriggerAchievement("GAUNTLET_MASTER");
            }
        }

		foreach (Location location in locations)
		{
			if (!IsInProgress(location) && location.element != Element.NO_ELEMENT)
			{
				abUnfinished [(int)location.element] = true;
			}
		}

		if (!abUnfinished [(int)Element.PHYSICAL])
			Core.TriggerAchievement("TALK_OF_THE_TOWN");
		if (!abUnfinished [(int)Element.HOLY])
			Core.TriggerAchievement("SLAYER_OF_ANGELS");
		if (!abUnfinished [(int)Element.UNHOLY])
			Core.TriggerAchievement("DESTROYER_OF_ABOMINATIONS");
		if (!abUnfinished [(int)Element.AIR])
			Core.TriggerAchievement("CHAMPION_OF_THE_SKY");
		if (!abUnfinished [(int)Element.FIRE])
			Core.TriggerAchievement("FIRE_RESISTANT");
		if (!abUnfinished [(int)Element.EARTH])
			Core.TriggerAchievement("DOWN_TO_EARTH");
		if (!abUnfinished [(int)Element.WATER])
			Core.TriggerAchievement("WORLD_RECORD_BREATH_HOLDER");


		if (locationsCompleted.Count == locations.Count)
			Core.TriggerAchievement("WANDERER");

		if (bHasBeenBeyondNeutralArea)
			Core.TriggerAchievement("ADVENTURER");

	}

	public bool IsInProgress(Location location)
	{
		foreach (LocationCompletion data in locationsCompleted)
		{
			if (data.location == location)
				return data.iNumWaves >= 0;
		}
		return false;
	}

	public bool IsCompleted(Location location)
	{
		foreach (LocationCompletion data in locationsCompleted)
		{
			if (data.location == location)
				return data.iNumWaves == location.numWaves - 1;
		}
		return false;
	}

	public void Select(int iSelection)
	{
		Select(locations[iSelection]);
	}

	public bool Select(Location location)
	{
		if(!locations.Contains(location))
		{
			Debug.Assert(false, "Picked a bad location");
			return false;
		}

		switch (GetState(location))
		{
			case MapNode.NodeState.INACCESSIBLE:
			{
				// TODO: Play a sound effect or something
				return false;
			}
			case MapNode.NodeState.ACCESSIBLE:
			case MapNode.NodeState.PARTIALLY_COMPLETED:
			case MapNode.NodeState.COMPLETED:
			{
				selectedLocation = location;
				return true;
			}
		}
			
		Debug.Assert(false, "Invalid case");
		return false;
	}

	public void ReadFrom(SerializationInfo data, string prefix)
	{
		locationsCompleted.Clear();

		int count = data.GetInt32(prefix + "NumComplete");
		for (int i = 0; i < count; i++)
		{
			LocationCompletion completionData = new LocationCompletion();
			int hashCode = data.GetInt32(prefix + "Complete" + i + ".Hash");
			foreach (Location location in locations)
			{
				if (location.GetHashCode() == hashCode)
				{
					completionData.location = location;
					break;
				}
			}
			completionData.iNumWaves = data.GetByte(prefix + "Complete" + i + ".NumWaves");
			if (completionData.iNumWaves > completionData.location.numWaves)
				completionData.iNumWaves = -1;

			locationsCompleted.Add(completionData);
		}

		MapNode.RefreshAllNodes();
		CheckForAchievements();
	}

	public void WriteTo(SerializationInfo data, string prefix)
	{
		data.AddValue(prefix + "NumComplete", locationsCompleted.Count);
		for (int i = 0; i < locationsCompleted.Count; i++)
		{
			data.AddValue(prefix + "Complete" + i + ".Hash", locationsCompleted[i].location.GetHashCode());
			data.AddValue(prefix + "Complete" + i + ".NumWaves", (byte)locationsCompleted[i].iNumWaves);
		}
	}
}
