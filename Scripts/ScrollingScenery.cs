using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollingScenery : MonoBehaviour 
{
	public float fScrollSpeed = 1.0f;

	void Start () 
	{
		
	}

	void Update () 
	{
		transform.localPosition += new Vector3 (fScrollSpeed * Time.deltaTime, 0.0f, 0.0f);
	}
}
