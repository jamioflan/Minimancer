using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MinionSlotType
{
	MELEE,
	SUPPORT,
	RANGED,

	NUM_MINION_SLOT_TYPES
}

public enum MinionSlot
{
	MELEE_1,
	MELEE_2,

	SUPPORT_1,
	SUPPORT_2,
	SUPPORT_3,

	RANGED_1,
	RANGED_2,

	NUM_MINION_SLOTS
}

public static class MinionSlots
{
	public static MinionSlot GetFirst(this MinionSlotType slotType)
	{
		switch (slotType)
		{
			case MinionSlotType.MELEE:
				return MinionSlot.MELEE_1;
			case MinionSlotType.SUPPORT:
				return MinionSlot.SUPPORT_1;
			case MinionSlotType.RANGED:
				return MinionSlot.RANGED_1;
		}
		return MinionSlot.NUM_MINION_SLOTS;
	}

	public static int GetNumSlots(this MinionSlotType slotType)
	{
		switch (slotType)
		{
			case MinionSlotType.MELEE:
				return 2;
			case MinionSlotType.SUPPORT:
				return 3;
			case MinionSlotType.RANGED:
				return 2;
		}
		return 0;
	}

	public static MinionSlotType GetSlotType(this MinionSlot slot)
	{
		switch (slot)
		{
			case MinionSlot.MELEE_1:
			case MinionSlot.MELEE_2:
				return MinionSlotType.MELEE;
			case MinionSlot.SUPPORT_1:
			case MinionSlot.SUPPORT_2:
			case MinionSlot.SUPPORT_3:
				return MinionSlotType.SUPPORT;
			case MinionSlot.RANGED_1:
			case MinionSlot.RANGED_2:
				return MinionSlotType.RANGED;
		}
		return MinionSlotType.NUM_MINION_SLOT_TYPES;
	}
}
