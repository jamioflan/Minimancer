using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadBobbing : MonoBehaviour 
{
	// Heads bob around on "neck" GameObjects
	public float fBobAmount = 1.0f;
	public float fBobSpeed = 1.0f;

	private float fBobTimer = 0.0f;
	void Start () 
	{
		//fBobTimer = Random.Range (0.0f, fBobSpeed * Mathf.PI * 2.0f);
	}

	void Update () 
	{
		fBobTimer += Time.deltaTime * fBobSpeed;
		transform.localPosition = new Vector3 (0.0f, Mathf.Sin (fBobTimer) * fBobAmount, 0.0f);
	}
}
