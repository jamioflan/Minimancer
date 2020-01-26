using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class LocalizationManager : MonoBehaviour 
{
	[System.Serializable]
	public class LocalizedDictionary
	{
		public string localizedName = "UNSET";
		public Dictionary<string, string> localizedText = new Dictionary<string, string>();
	}
		
	public static LocalizationManager instance;
	public int iSelection = -1;
	public List<LocalizedDictionary> dictionaries;
	private string missingTextString = "MISSINGTEXT";

	// Use this for initialization
	void Awake () 
	{
		if (instance == null) 
		{
			instance = this;
			LoadLocalizedText("lang_en.json");

			SelectLanguage(0);
		} 
		else if (instance != this)
		{
			Destroy (gameObject);
		}
	}

	public void SelectLanguage(int i)
	{
		iSelection = i;
	}

	public void LoadLocalizedText(string fileName)
	{
		LocalizedDictionary dictionary = new LocalizedDictionary ();
		string filePath = Path.Combine (Application.streamingAssetsPath, fileName);

		if (File.Exists (filePath)) 
		{
			string dataAsJson = File.ReadAllText (filePath);
			LocalizationData loadedData = JsonUtility.FromJson<LocalizationData> (dataAsJson);

			for (int i = 0; i < loadedData.items.Length; i++) 
			{
				dictionary.localizedText.Add (loadedData.items [i].unloc, loadedData.items [i].loc);   
			}

			if (dictionary.localizedText.ContainsKey("LOC_NAME"))
			{
				dictionary.localizedName = dictionary.localizedText ["LOC_NAME"];
			}

			dictionaries.Add(dictionary);

			Debug.Log ("Loaded localization data for " + dictionary.localizedName + ", dictionary contains: " + dictionary.localizedText.Count + " entries");
		} 
		else 
		{
			Debug.Assert (false, "Cannot find file!");
		}
	}

	public static string GetLoc(string key)
	{
		return instance.GetLocalizedValue(key);
	}

	public string GetLocalizedValue(string key)
	{
		string result = missingTextString;
		if (iSelection != -1 && dictionaries[iSelection].localizedText.ContainsKey (key)) 
		{
			result = dictionaries[iSelection].localizedText [key];
		}

		return result;
	}
}
