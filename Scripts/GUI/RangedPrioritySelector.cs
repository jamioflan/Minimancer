using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RangedPrioritySelector : MonoBehaviour 
{
	public TeamBuilder builder;
	public RangedPrioritySelectorElement[] highlights = new RangedPrioritySelectorElement[(int)TargetPriority.NUM_TARGET_PRIORITIES];
	public int iSelection = 0;
	public MinionSlot slot;

	void Start () 
	{

	}

	void Update () 
	{
		
	}

	public void SelectPriority(int index)
	{
		highlights [iSelection].Deselect();
		highlights [index].Select();
		iSelection = index;

		Core.GetAudioManager().PlayGUIClick();

		Minion minion = builder.currentRoster.GetMinion(slot);
		minion.priority = (TargetPriority)iSelection;
	}
}
