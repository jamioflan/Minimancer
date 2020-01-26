using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization;
using System;
using System.Reflection;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class PlayerProfile : MonoBehaviour, IComparable<PlayerProfile>
{
	private PlayerProfileSaveLoad saveLoad;
	private BinaryFormatter formatter;
	public TeamPool pool;
	public WorldMap world;
	public TeamRoster[] rosters;
	public int iSelectedIndex = 0;
	public DateTime lastUsed = DateTime.MinValue;
	public TutorialManager.TutorialState tutorialState = TutorialManager.TutorialState.WELCOME;

	void Start () 
	{

	}

	void Update () 
	{
		
	}

	public void Init()
	{
		formatter = new BinaryFormatter ();
		formatter.Binder = new VersionDeserializationBinder ();
	}

	public void ReadFrom(SerializationInfo data)
	{
		int version = data.GetInt32("Version");

		pool.ReadFrom(data, "Pool.");

		int numRosters = data.GetInt32("#Rosters");
		foreach (TeamRoster roster in rosters)
		{
			Destroy(roster.gameObject);
		}

		rosters = new TeamRoster[numRosters];
		for (int i = 0; i < numRosters; i++)
		{
			GameObject go = new GameObject ("Roster_" + i);
			go.transform.SetParent(transform);
			rosters [i] = go.AddComponent<TeamRoster>();
			rosters [i].ReadFrom(data, "Roster" + i + ".");
		}

		if (version >= 1)
			world.ReadFrom(data, "World.");
		if (version >= 2)
			lastUsed = data.GetDateTime("LastUsed");
		if (version >= 4)
			tutorialState = data.GetBoolean("TutorialComplete") ? TutorialManager.TutorialState.DONE : TutorialManager.TutorialState.WELCOME;
	}

	public void WriteTo(SerializationInfo data)
	{
		data.AddValue("Version", 4);

		pool.WriteTo(data, "Pool.");
	
		data.AddValue("#Rosters", rosters.Length);
		for (int i = 0; i < rosters.Length; i++)
		{
			rosters [i].WriteTo(data, "Roster" + i + ".");
		}

		// Version 1
		world.WriteTo(data, "World.");

		// Version 2
		data.AddValue("LastUsed", lastUsed);

		// Version 4
		data.AddValue("TutorialComplete", tutorialState == TutorialManager.TutorialState.DONE);
	}

	public static PlayerProfile currentlyLoading = null;

	public void Load(string filename)
	{
		Stream stream = null;
		currentlyLoading = this;

		try
		{
			if (File.Exists(filename))
			{
				stream = File.Open(filename, FileMode.Open);
				object data = formatter.Deserialize(stream);
				if (data is PlayerProfileSaveLoad)
				{
					saveLoad = (PlayerProfileSaveLoad)data;
				}
				else
				{
					Debug.Assert(false, "Invalid save data");
				}
				stream.Close();
			}
			else
			{
				Debug.Log("Could not find save file");
			}
		}
		catch(Exception e)
		{
			Debug.LogError(e.ToString());
			stream.Close();
		}
	}

	public void Save(string filename)
	{
		currentlyLoading = this;
		if (saveLoad == null)
		{
			saveLoad = new PlayerProfileSaveLoad ();
		}
		Stream stream = File.Open(filename, FileMode.Create);
		formatter.Serialize(stream, saveLoad);
		stream.Close();
	}

	public TeamRoster GetRoster(int index) { return rosters[index]; } 
	public TeamRoster GetSelectedRoster() { return rosters [iSelectedIndex]; }

	[Serializable]
	public class PlayerProfileSaveLoad : ISerializable
	{
		public PlayerProfileSaveLoad() {}

		public PlayerProfileSaveLoad(SerializationInfo data, StreamingContext context)
		{
			currentlyLoading.ReadFrom(data);
		}

		public void GetObjectData(SerializationInfo data, StreamingContext context)
		{
			currentlyLoading.WriteTo(data);
		}
	}

	public class VersionDeserializationBinder : SerializationBinder
	{
		public override Type BindToType(string assemblyName, string typeName)
		{
			if (string.IsNullOrEmpty(assemblyName) || string.IsNullOrEmpty(typeName))
			{
				Debug.Assert(false, "Empty assembly or type name!");
				return null;
			}
				
			return Type.GetType( string.Format( "{0}, {1}", typeName, assemblyName));
		}
	}

	public int CompareTo(PlayerProfile profile)
	{
		if(profile.lastUsed < lastUsed) 
			return -1;

		return profile.lastUsed > lastUsed ? 1 : 0;
	}
}
