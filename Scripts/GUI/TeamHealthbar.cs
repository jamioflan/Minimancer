using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TeamHealthbar : MonoBehaviour 
{
	public Image meleeBar, rangedBar;
	public Sprite meleeBarZombie, rangedBarZombie;

	public Image[] playerIcons = new Image[(int)MinionSlot.NUM_MINION_SLOTS];

	void Start () 
	{
		
	}

	void Update () 
	{
		TeamRoster roster = Core.GetCurrentRoster();

		if (roster == null)
			return;

		for (int i = 0; i < (int)MinionSlot.NUM_MINION_SLOTS; i++)
		{
			if (roster.minions [i] != null)
			{
				playerIcons [i].sprite = roster.minions [i].template.icon;

				if (roster.minions [i].isZombified)
				{
					switch (((MinionSlot)i).GetSlotType())
					{
						case MinionSlotType.MELEE:
						{
							meleeBar.sprite = meleeBarZombie;
							break;
						}
						case MinionSlotType.RANGED:
						{
							rangedBar.sprite = rangedBarZombie;
							break;
						}
					}
				}
			}
		}

		meleeBar.fillAmount = GetParametric(roster.afGroupHealths [(int)MinionSlotType.MELEE], roster.afGroupMaxHealths [(int)MinionSlotType.MELEE]);
		rangedBar.fillAmount = GetParametric(roster.afGroupHealths [(int)MinionSlotType.RANGED], roster.afGroupMaxHealths [(int)MinionSlotType.RANGED]);
	}

	private float GetParametric(float fCur, float fMax)
	{
		float fParam = fMax > 0.0f ? (fCur / fMax) : 0.0f;
		return 0.08f + 0.72f * fParam;
	}
}
