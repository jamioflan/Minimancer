using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System;
using System.Runtime.Serialization;
using System.IO;

public class ConfigFile : MonoBehaviour 
{
	private ConfigFileSaveLoad saveLoad;
	private BinaryFormatter formatter;

	public float fMusicVolume = 1.0f;
	public float fInGameVolume = 1.0f;

	public void Init()
	{
		formatter = new BinaryFormatter ();
		formatter.Binder = new PlayerProfile.VersionDeserializationBinder ();
	}

	public void ReadFrom(SerializationInfo data)
	{
		int version = data.GetInt32("Version");

		if (version >= 1)
		{
			fMusicVolume = data.GetSingle("Music");
			fInGameVolume = data.GetSingle("InGame");
		}
	}

	public void WriteTo(SerializationInfo data)
	{
		data.AddValue("Version", 1);

		// Version 1
		data.AddValue("Music", fMusicVolume);
		data.AddValue("InGame", fInGameVolume);

	}

	public void Load()
	{
		Stream stream = null;
		string filename = "config/default.dat";
		try
		{
			if (File.Exists(filename))
			{
				stream = File.Open(filename, FileMode.Open);
				object data = formatter.Deserialize(stream);
				if (data is ConfigFileSaveLoad)
				{
					saveLoad = (ConfigFileSaveLoad)data;
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

	public void Save()
	{
		string filename = "config/default.dat";
		if (saveLoad == null)
		{
			saveLoad = new ConfigFileSaveLoad ();
		}
		Stream stream = File.Open(filename, FileMode.Create);
		formatter.Serialize(stream, saveLoad);
		stream.Close();
	}

	[Serializable]
	public class ConfigFileSaveLoad : ISerializable
	{
		public ConfigFileSaveLoad() {}

		public ConfigFileSaveLoad(SerializationInfo data, StreamingContext context)
		{
			Core.theCore.config.ReadFrom(data);
		}

		public void GetObjectData(SerializationInfo data, StreamingContext context)
		{
			Core.theCore.config.WriteTo(data);
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
}
