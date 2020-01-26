using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization;

public class TeamPool : MonoBehaviour 
{
	public List<MinionTemplate> unlocks = new List<MinionTemplate>();
	private List<MinionTemplate> newList = new List<MinionTemplate>();

	public bool IsNew(MinionTemplate template)
	{
		return newList.Contains(template);
	}

	public void SetNotNew(MinionTemplate template)
	{
		newList.Remove(template);
	}

	void Start () 
	{
		
	}

	void Update () 
	{
		
	}

	public void AddMinion(MinionTemplate template)
	{
		if (unlocks.Contains(template))
		{
			return;
		}


		unlocks.Add(template);
		newList.Add(template);

		UpdateMinionAchievement();
	}

	public void UpdateMinionAchievement()
	{
		if (unlocks.Count > Core.GetStat("MINIONS_UNLOCKED"))
		{
			Core.IncrementStat("MINIONS_UNLOCKED", unlocks.Count - Core.GetStat("MINIONS_UNLOCKED"));
		}
	}

	public void ReadFrom(SerializationInfo data, string prefix)
	{
		unlocks.Clear();

		MinionTemplateManager mtm = Core.GetMinionTemplateManager();

		int count = data.GetInt32(prefix + "NumMinions");
		for (int i = 0; i < count; i++)
		{
			int minionHash = data.GetInt32(prefix + "Minion" + i + ".Hash");
			MinionTemplate template = mtm.GetTemplate(minionHash);
			if (template != null)
			{
				if(!unlocks.Contains(template))
					unlocks.Add(template);
			}
		}

		count = data.GetInt32(prefix + "NumNew");
		for (int i = 0; i < count; i++)
		{
			int minionHash = data.GetInt32(prefix + "New" + i + ".Hash");
			MinionTemplate template = mtm.GetTemplate(minionHash);
			if (template != null)
			{
				if(!newList.Contains(template))
					newList.Add(template);
			}
		}
	}

	public void WriteTo(SerializationInfo data, string prefix)
	{
		data.AddValue(prefix + "NumMinions", unlocks.Count);
		for (int i = 0; i < unlocks.Count; i++)
		{
			data.AddValue(prefix + "Minion" + i + ".Hash", unlocks [i].GetHashCode());
		}

		data.AddValue(prefix + "NumNew", newList.Count);
		for (int i = 0; i < newList.Count; i++)
		{
			data.AddValue(prefix + "New" + i + ".Hash", newList [i].GetHashCode());
		}
	}
}
