using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyProgressbar : MonoBehaviour 
{
	public Image[] progressBar = new Image[3];
	public Image[] waveActiveImage = new Image[3];
	public Text[] timeToNextWave = new Text[2];
	public Button startNextWaveButton;
	public Button fastForwardButton;
	public Image[] unlockIcons = new Image[3];
	public Image[] unlockHighlights = new Image[3];
	public Sprite unlockedSprite;
	public CanvasGroup unlockBox;
	public Image unlockBoxIcon;
	public Text[] unlockBoxName = new Text[2];
	public bool[] wavesCompleted = new bool[3];
	public float fUnlockBoxAppearTime = 10.0f;

	void Start () 
	{
		LevelController level = Core.GetLevel();

		if (level == null)
		{
			Debug.Assert(false, "Level is null");
			return;
		}
		
		for (int i = 0; i < 3; i++)
		{
			MinionTemplate unlock = level.location.unlocks [i];
			if (unlock == null)
			{
				unlockIcons [i].enabled = false;
				unlockHighlights [i].enabled = false;
			}
			else
			{
				unlockIcons [i].sprite = unlock.icon;
				// Check for already unlocked
				if (Core.GetPlayerProfile().pool.unlocks.Contains(unlock))
				{
					unlockHighlights [i].sprite = unlockedSprite;
				}
			}
		}
	}

	void Update () 
	{
		LevelController level = Core.GetLevel();

		if (level == null)
			return;

		for (int i = 0; i < 3; i++)
		{
			if (!wavesCompleted [i] && level.GetWaveCompletion(i) >= 1.0f)
			{
				wavesCompleted [i] = true;

				MinionTemplate unlock = level.location.unlocks [i];
				if (unlock != null && unlockHighlights[i].sprite != unlockedSprite)
				{
					unlockHighlights [i].sprite = unlockedSprite;
					fUnlockBoxAppearTime = 0.0f;
					unlockBoxIcon.sprite = unlock.icon;
					for(int j = 0; j < 2; j++)
						unlockBoxName[j].text = LocalizationManager.GetLoc(unlock.unlocName);
				}
			}
		}

		unlockBox.alpha = Mathf.Clamp(2 - fUnlockBoxAppearTime * 0.25f, 0.0f, 1.0f);

		fUnlockBoxAppearTime += Core.GetDeltaTime();

		/*
		float fProgress = level.GetParametricProgress();
		for (int i = 0; i < 3; i++)
		{
			if (fProgress < i / 3.0f)
			{
				progressBar [i].fillAmount = 0.0f;
			}
			else if (fProgress > (i + 1) / 3.0f)
			{
				progressBar [i].fillAmount = 1.0f;
			}
			else
			{
				progressBar [i].fillAmount = fProgress * 3.0f - i;
			}
		}
		*/

		int iCurrentWave = level.iCurrentWave;
		if (level.fGracePeriodDuration < level.location.gracePeriodDuration && iCurrentWave < level.location.numWaves)
		{
			for (int i = 0; i < 2; i++)
			{
				timeToNextWave [i].text = string.Format(LocalizationManager.GetLoc("TIME_TO_NEXT_WAVE"), Mathf.CeilToInt(level.location.gracePeriodDuration - level.fGracePeriodDuration));
			}
			startNextWaveButton.interactable = true;
			iCurrentWave--;
		}
		else
		{
			for (int i = 0; i < 2; i++)
			{
				timeToNextWave [i].text = "";
			}
			startNextWaveButton.interactable = false;
		}

		for (int i = 0; i < 3; i++)
		{
			if(iCurrentWave >= i)
			{
				waveActiveImage [i].enabled = true;
			}

			progressBar [i].fillAmount = level.GetWaveCompletion(i);
		}

		if (Input.GetKeyDown(KeyCode.Return))
		{
			StartNextWave();
		}

		if (Input.GetKeyDown(KeyCode.Space))
		{
			FastForward(true);
		}
		else if (Input.GetKeyUp(KeyCode.Space))
		{
			FastForward(false);
		}

		if (Input.GetKeyDown(KeyCode.Escape))
		{
			AbortLevel();
		}
	}

	public void StartNextWave()
	{
		Core.GetLevel().StartNextWave();
	}

	public void FastForward(bool bSet)
	{
		Core.GetLevel().FastForward(bSet);
	}

	public void AbortLevel()
	{
		Core.GetLevel().Abort();
	}
}
