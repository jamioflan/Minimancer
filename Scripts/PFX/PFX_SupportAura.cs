using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PFX_SupportAura : MonoBehaviour 
{
	public MeshRenderer[] rings = new MeshRenderer[3];
	public float fAnimSpeed = 1.0f;
	private float fAnimTimer = 0.0f;

	void Start () 
	{
		
	}

	void Update () 
	{
		fAnimTimer += Core.GetPlayerDeltaTime() * fAnimSpeed;

		for (int i = 0; i < rings.Length; i++)
		{
			float fRingSize = fAnimTimer + ((float)i / (float)rings.Length) - 1.0f;
			float fAlpha = 1.0f - fRingSize;
			if (fRingSize <= 0.0f || fRingSize >= 1.0f)
			{
				rings [i].transform.localScale = Vector3.zero;
			}
			else
			{
				rings [i].transform.localScale = new Vector3 (1.0f, 1.0f, 1.0f) * fRingSize;
				rings [i].material.color = new Color (1.0f, 1.0f, 1.0f, fAlpha);
			}
			
		}
	}

	public void Trigger()
	{
		fAnimTimer = 0.0f;
	}
}
