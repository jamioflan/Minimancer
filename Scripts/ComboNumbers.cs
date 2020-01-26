using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComboNumbers : MonoBehaviour 
{
	public Sprite[] sprites = new Sprite[10];
	public float fDigitSeparation = 0.2f;
	public float fBlink = 0.0f;

	public void SetValue(int iCombo)
	{
		List<SpriteRenderer> digits = new List<SpriteRenderer>();

		foreach (SpriteRenderer sr in transform.GetComponentsInChildren<SpriteRenderer>())
		{
			Destroy(sr.gameObject);
		}

		fBlink = 1.0f;

		if (iCombo == 0)
		{
			// Nothing
		}
		else while (iCombo > 0)
		{
			int iDigit = iCombo % 10;

			GameObject go = new GameObject ("Digit");
			go.transform.SetParent(transform);
			SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
			sr.sprite = sprites [iDigit];
			digits.Add(sr);

			iCombo /= 10;
		}

		for (int i = 0; i < digits.Count; i++)
		{
			float fPosX = -((float)(i - (float)(digits.Count - 1) / 2.0f));
			float fPosY = -Mathf.Abs(fPosX) * 0.025f;
			digits [i].transform.localPosition = new Vector3 (fDigitSeparation * fPosX, fPosY, 0.0f);
		}
	}

	void Start () 
	{

	}

	void Update () 
	{
		if(fBlink > 0.0f) 
			fBlink -= Core.GetPlayerDeltaTime();
		
		transform.localScale = new Vector3 (1.0f, 1.0f, 1.0f) * (1.0f + fBlink * 0.5f);
	}
}
