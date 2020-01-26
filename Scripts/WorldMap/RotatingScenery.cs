using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotatingScenery : MonoBehaviour 
{
	public float fSpeed = 1.0f;

	void Update () 
	{
		transform.RotateAround(transform.position, Vector3.up, fSpeed * Time.deltaTime);
	}
}
