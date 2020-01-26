using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Location : MonoBehaviour 
{
	public Location prerequisite;
	public bool bIsGauntlet = false;

	public string sceneName;
	public MinionTemplate[] spawns1, spawns2, spawns3, 
		spawns4, spawns5, spawns6, spawns7, spawns8;
	public int numWaves = 3;
	public float waveDuration = 30.0f;
	public float gracePeriodDuration = 5.0f;
	public MinionTemplate[] unlocks = new MinionTemplate[3];
	public string unlocName;
	public AudioClip music;
	public Element element;

	public int GetNumSpawnsPerWave(int wave)
	{
		switch (wave)
		{
			case 0:
			return spawns1.Length;
			case 1:
			return spawns2.Length;
			case 2: 
			return spawns3.Length;
			case 3:
			return spawns4.Length;
			case 4:
			return spawns5.Length;
			case 5: 
			return spawns6.Length;
			case 6:
			return spawns7.Length;
			case 7:
			return spawns8.Length;
		}
		Debug.Assert(false, "Invalid");
		return spawns1.Length;
	}

	public MinionTemplate[] GetSpawns(int wave)
	{
		switch (wave)
		{
			case 0:
				return spawns1;
			case 1:
				return spawns2;
			case 2: 
				return spawns3;
			case 3:
				return spawns4;
			case 4:
				return spawns5;
			case 5: 
				return spawns6;
			case 6: 
				return spawns7;
			case 7: 
				return spawns8;
			default:
				Debug.Assert(false, "Invalid");
				return new MinionTemplate[0];
			case -1:
			case 8:
				return new MinionTemplate[0];
		}
			
	}

	void Start () 
	{
	}

	void Update () 
	{
		
	}

	public override int GetHashCode()
	{
		return name.GetHashCode();
	}
}
