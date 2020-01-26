using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GauntletStep : MonoBehaviour 
{
	private bool bActive = false;
	private Image image;
	public Sprite[] icons = new Sprite[4];
	private float fAnimTime = 0.0f;
	public float fAnimSpeed = 1.0f;

	void Awake () 
	{
		image = GetComponent<Image>();
	}

	public void SetGauntletStepActive(bool bSet)
	{
		bActive = bSet;
		image.enabled = bSet;
	}

	void Update () 
	{
		fAnimTime += Time.deltaTime * fAnimSpeed;

		int iSprite = Mathf.FloorToInt(fAnimTime) % 4;

		image.sprite = icons [iSprite];
	}
}
