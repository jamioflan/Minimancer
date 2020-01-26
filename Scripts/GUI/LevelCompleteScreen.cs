using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelCompleteScreen : MonoBehaviour 
{
	public Sprite unlocked, locked, newlyUnlocked;
	public GameObject successPanel;
	public Text defeatText, reasonText;
	public Text buttonText;
	public Text[] successWaveText = new Text[3];
	public Image[] unlockIcons = new Image[3];
	public Image[] unlockHighlights = new Image[3];
	//public Text[] unlockNames = new Text[3];
	public MinionStatBlock statBlock;
	public Image[] fillBars = new Image[3];
	public Image[] padlocks = new Image[3];

	public void OnMissionFail()
	{
		OnFinished();

		string unlocDefeat = "";
		switch (Core.GetLevel().GetCurrentWave())
		{
			case 0:
				unlocDefeat = "DEFEAT_WAVE_1";
				break;
			case 1:
				unlocDefeat = "DEFEAT_WAVE_2";
				break;
			case 2:
				unlocDefeat = "DEFEAT_WAVE_3";
				break;
            case 3:
                unlocDefeat = "DEFEAT_WAVE_4";
                break;
            case 4:
                unlocDefeat = "DEFEAT_WAVE_5";
                break;
            case 5:
                unlocDefeat = "DEFEAT_WAVE_6";
                break;
            case 6:
                unlocDefeat = "DEFEAT_WAVE_7";
                break;
            case 7:
                unlocDefeat = "DEFEAT_WAVE_8";
                break;
        }
		defeatText.text = LocalizationManager.GetLoc(unlocDefeat);
		if (Core.GetCurrentRoster().IsAnyoneAlive(MinionSlotType.MELEE))
		{
			reasonText.text = LocalizationManager.GetLoc("RANGED_OVERWHELMED");
		}
		else
		{
			reasonText.text = LocalizationManager.GetLoc("MELEE_OVERWHELMED");
		}

		buttonText.text = LocalizationManager.GetLoc("RETREAT_TO_MAP");

		//defeatText.text = "Party Defeated During Wave " + (Core.GetLevel().GetCurrentWave() + 1);
		//reasonText.text = "Your " + (Core.GetCurrentRoster().IsAnyoneAlive(MinionSlotType.MELEE) ? "ranged" : "melee") + " minions were overwhelmed";
	}

	public void OnMissionSuccess()
	{
		OnFinished();

		defeatText.text = LocalizationManager.GetLoc("ALL_WAVES_COMPLETE");
		buttonText.text = LocalizationManager.GetLoc("CONTINUE_TO_MAP");
		reasonText.text = "";
	}

	public void OnFinished()
	{
		gameObject.SetActive(true);
		LevelController level = Core.GetLevel();
		Location location = level.location;
		successPanel.SetActive(true);
        if (successWaveText.Length > 0)
        {
            successWaveText[0].text = LocalizationManager.GetLoc(Core.GetLevel().GetWaveCompletion(0) >= 1.0f ? "WAVE_1_COMPLETE" : "WAVE_1_FAILED");
            successWaveText[1].text = LocalizationManager.GetLoc(Core.GetLevel().GetWaveCompletion(1) >= 1.0f ? "WAVE_2_COMPLETE" : "WAVE_2_FAILED");
            successWaveText[2].text = LocalizationManager.GetLoc(Core.GetLevel().GetWaveCompletion(2) >= 1.0f ? "WAVE_3_COMPLETE" : "WAVE_3_FAILED");
        }

		for (int i = 0; i < location.numWaves; i++)
		{
            if (fillBars[i] != null)
            {
                fillBars[i].fillAmount = level.GetWaveCompletion(i);
            }

            if (unlockIcons[i] != null)
            {
                if (location.unlocks[i] == null)
                {
                    unlockIcons[i].enabled = false;
                    unlockHighlights[i].enabled = false;
                    unlockHighlights[i].color = Color.grey;
                    unlockHighlights[i].sprite = locked;
                    padlocks[i].enabled = false;
                    //unlockNames [i].enabled = false;
                }
                else
                {
                    unlockIcons[i].enabled = true;
                    unlockHighlights[i].enabled = true;

                    //unlockNames [i].enabled = true;

                    unlockIcons[i].sprite = location.unlocks[i].icon;
                    //unlockNames [i].text = LocalizationManager.GetLoc(location.unlocks [i].unlocName);
                    if (Core.GetPlayerProfile().pool.unlocks.Contains(level.location.unlocks[i]))
                    {
                        unlockHighlights[i].sprite = level.abNewlyUnlocked[i] ? newlyUnlocked : unlocked;
                        unlockHighlights[i].color = Color.white;
                        unlockIcons[i].color = Color.white;
                        padlocks[i].enabled = false;
                        //unlockNames [i].color = Color.white;
                    }
                    else
                    {
                        unlockHighlights[i].sprite = locked;
                        unlockHighlights[i].color = Color.grey;
                        unlockIcons[i].color = Color.grey;
                        padlocks[i].enabled = true;
                        //unlockNames [i].color = Color.black;
                    }
                }
            }
		}
	}
		
		

	// Use this for initialization
	void Start () 
	{
		//successPanel.SetActive(false);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void ReturnToMap()
	{
		Core.GetLevel().Shutdown();
	}

	public void Hover(int index)
	{
		LevelController level = Core.GetLevel();
		Location location = level.location;
		statBlock.SetMinion(location.unlocks [index]);
	}

	public void Unhover()
	{
		statBlock.SetMinion(null);
	}
}
