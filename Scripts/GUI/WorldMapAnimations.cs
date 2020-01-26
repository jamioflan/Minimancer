using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WorldMapAnimations : MonoBehaviour 
{
	public float fTimer = 0.0f;
	public Image lavaLayer;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () 
	{
		fTimer += Time.deltaTime;

		float fBrightness = 0.125f * Mathf.Sin(fTimer) * Mathf.Sin(fTimer) + 0.875f;
		lavaLayer.color = new Color (fBrightness, fBrightness, fBrightness);
	}
}
