using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffEffect_ConditionalStatBoost : BuffEffect_StatBoost
{
	public enum BuffEffect_Condition
	{
		MINIONS_ALL_DIFFERENT_COLOUR,
	}

	public BuffEffect_Condition condition;

	public override float GetModifier(Stat eStat) 
	{
		if (IsConditionSatisfied())
		{
			return base.GetModifier(eStat);
		}
		return 0.0f;
	}

	private bool IsConditionSatisfied()
	{
		switch (condition)
		{
			case BuffEffect_Condition.MINIONS_ALL_DIFFERENT_COLOUR:
			{
				bool[] abElementPresent = new bool[(int)Element.NO_ELEMENT] {false, false, false, false, false, false, false};
				foreach (Minion minion in Core.GetCurrentRoster().minions)
				{
					abElementPresent [(int)minion.template.element] = true;
				}

				for (int i = 0; i < (int)Element.NO_ELEMENT; i++)
				{
					if (!abElementPresent [i])
						return false;
				}

				Core.GetCurrentRoster().bHasActiveCollector = true;

				return true;
			}
		}

		Debug.Assert(false, "Invalid condition!");
		return false;
	}
}
