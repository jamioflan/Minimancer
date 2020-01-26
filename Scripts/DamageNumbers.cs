using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageNumbers : MonoBehaviour 
{
	public Sprite[] sprites = new Sprite[10];
	public float fLifetime = 0.7f;
	public float fDigitSeparation = 0.2f;
	public DamageNumbers nextNumbers = null;

	public void Init(int iDamage)
	{
		List<SpriteRenderer> digits = new List<SpriteRenderer>();

		if (iDamage == 0)
		{
			GameObject go = new GameObject ("Digit");
			go.transform.SetParent(transform);
			SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
			sr.sprite = sprites [0];
			digits.Add(sr);
		}
		else while (iDamage > 0)
		{
			int iDigit = iDamage % 10;

			GameObject go = new GameObject ("Digit");
			go.transform.SetParent(transform);
			SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
			sr.sprite = sprites [iDigit];
			digits.Add(sr);

			iDamage /= 10;
		}

		for (int i = 0; i < digits.Count; i++)
		{
			float fPosX = -((float)(i - (float)(digits.Count - 1) / 2.0f));
			float fPosY = -Mathf.Abs(fPosX) * 0.025f;
			digits [i].transform.localPosition = new Vector3 (fDigitSeparation * fPosX, fPosY, 0.0f);
		}
	}

	public void Bump()
	{
		if (fLifetime > 0.5f)
		{
			transform.position += new Vector3 (Random.Range(-0.1f, 0.1f), 0.25f, 0.1f);
			if (nextNumbers != null)
			{
				nextNumbers.Bump();
			}
		}
	}

	void Start () 
	{
		
	}

	void Update () 
	{
		transform.position += new Vector3 (0.0f, 1.0f * Time.deltaTime * fLifetime, 0.0f);
		fLifetime -= Time.deltaTime;
		if (fLifetime < 0.0f)
		{
			Destroy(gameObject);
		}
	}
}
