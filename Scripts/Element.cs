using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Element 
{
	PHYSICAL,
	HOLY,
	UNHOLY,

	FIRE,
	EARTH,
	AIR,
	WATER,

	NO_ELEMENT,

	NUM_ELEMENTS
}

public static class Elements
{
	public static float[,] aafDamageMultiplers = new float[(int)Element.NUM_ELEMENTS, (int)Element.NUM_ELEMENTS]
	{
		{1.0f, 1.5f, 0.75f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f},
		{0.75f, 1.0f, 1.5f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f},
		{1.5f, 0.75f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f},
		{1.0f, 1.0f, 1.0f, 1.0f, 1.5f, 1.0f, 0.75f, 1.0f},
		{1.0f, 1.0f, 1.0f, 0.75f, 1.0f, 1.5f, 1.0f, 1.0f},
		{1.0f, 1.0f, 1.0f, 1.0f, 0.75f, 1.0f, 1.5f, 1.0f},
		{1.0f, 1.0f, 1.0f, 1.5f, 1.0f, 0.75f, 1.0f, 1.0f},
		{1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f}
	};
	
	public static float GetDamageMultiplier(this Element attacker, Element defender)
	{
		return aafDamageMultiplers [(int)attacker, (int)defender];
	}

	public static string GetHintText(this Element ele)
	{
		switch (ele)
		{
			case Element.PHYSICAL:
				return "MINION_WEAKNESS_HOLY";
			case Element.HOLY:
				return "MINION_WEAKNESS_UNHOLY";
			case Element.UNHOLY:
				return "MINION_WEAKNESS_PHYSICAL";
			case Element.FIRE:
				return "MINION_WEAKNESS_EARTH";
			case Element.EARTH:
				return "MINION_WEAKNESS_AIR";
			case Element.AIR:
				return "MINION_WEAKNESS_WATER";
			case Element.WATER:
				return "MINION_WEAKNESS_FIRE";
			case Element.NO_ELEMENT:
				return "MINION_WEAKNESS_NO_ELEMENT";
		}

		return "MAIN_MENU";
	}
}	

