using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadGameEntry : MonoBehaviour 
{
	public Text saveName;
	public int index;
	public int scroll;
	public float fLerpSpeed = 1.0f;

	public void SelectSlot()
	{
		Core.GetAudioManager().PlayGUIClick();
		MainMenu.instance.ConfirmLoadSlot(index);
	}
		
	void Start () 
	{
		
	}

	void Update () 
	{
		int iRow = index % 5;
		int iCol = index / 5;
		iCol -= scroll;

		Vector3 target = new Vector3 (660.0f + iCol * 1920.0f, 410.0f - iRow * 120.0f, 0.0f);
		transform.localPosition = Vector3.Lerp(transform.localPosition, target, fLerpSpeed * Time.deltaTime);
	}

	public void SetScroll(int set)
	{
		scroll = set;
	}
}
