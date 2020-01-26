using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LocalizationData 
{
	public LocalizationItem[] items;
}

[System.Serializable]
public class LocalizationItem
{
	public string unloc;
	public string loc;
}
