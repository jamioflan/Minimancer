using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LocalizedText : MonoBehaviour 
{
	public string unlocalized;
	private Text text;

	void Start () 
	{
		text = GetComponent<Text>();
	}

	void Update () 
	{
		text.text = LocalizationManager.GetLoc(unlocalized);
	}
}
