using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BossProgressBar : MonoBehaviour 
{
	public Image[] progressBar = new Image[7];
	public Image[] waveActiveImage = new Image[8];

	void Start()
	{
		for (int i = 0; i < 8; i++)
		{
			waveActiveImage [i].enabled = false;
		}
	}

	void Update () 
	{
		LevelController level = Core.GetLevel();

		if (level == null)
			return;

		int iCurrentWave = level.iCurrentWave;
		if (level.fGracePeriodDuration < level.location.gracePeriodDuration && iCurrentWave <= 7)
		{
			iCurrentWave--;
		}

		for (int i = 0; i < 8; i++)
		{
			if(iCurrentWave >= i)
			{
				waveActiveImage [i].enabled = true;
			}

			if(i < 7)
				progressBar [i].fillAmount = level.GetWaveCompletion(i);
		}
	}
}
