using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinionTemplateManager : MonoBehaviour 
{
	public List<MinionTemplate> meleeList = new List<MinionTemplate>();
	public List<MinionTemplate> supportList = new List<MinionTemplate>();
	public List<MinionTemplate> rangedList = new List<MinionTemplate>();
	private List<MinionTemplate> fullList = new List<MinionTemplate> ();
	public Minion minionPrefab;
	public Healthbar healthbarPrefab;
	public PFX_DebuffIcon stunPFXPrefab;

	public DamageNumbers playerDamage, enemyDamage, criticalDamage;

	public List<MinionTemplate> GetMinionList(MinionSlotType eSlotType)
	{
		switch (eSlotType)
		{
			case MinionSlotType.MELEE:
				return meleeList;
			case MinionSlotType.SUPPORT:
				return supportList;
			case MinionSlotType.RANGED:
				return rangedList;
		}
		return null;
	}

	public List<MinionTemplate> GetFullList() 
	{
		return fullList;
	}

	private class MinionTemplateSorter : IComparer<MinionTemplate>
	{
		public int Compare(MinionTemplate x, MinionTemplate y)
		{
			// First compare element
			if ((int)x.element < (int)y.element)
				return -1;
			if ((int)x.element > (int)y.element)
				return 1;

			// Second compare type
			if ((int)x.GetSlotType() < (int)y.GetSlotType())
				return -1;
			if ((int)x.GetSlotType() > (int)y.GetSlotType())
				return 1;


			return 0;
		}
	}

	void Start () 
	{
		fullList.AddRange(meleeList);
		fullList.AddRange(supportList);
		fullList.AddRange(rangedList);

		fullList.Sort(new MinionTemplateSorter());
	}

	void Update () 
	{
		
	}

	public MinionTemplate GetTemplate(int hashCode)
	{
		foreach (MinionTemplate template in fullList)
		{
			if (template.GetHashCode() == hashCode)
			{
				return template;
			}
		}
		return null;
	}

	public Minion CreateMinion(MinionTemplate template)
	{
		Minion newMinion = Instantiate<Minion>(minionPrefab);
		newMinion.InitFromTemplate(template);
		return newMinion;
	}

	public Minion CreateMinion(MinionSlotType slotType, int hashCode)
	{
		foreach (MinionTemplate template in GetMinionList(slotType))
		{
			if (template.GetHashCode() == hashCode)
			{
				return CreateMinion(template);
			}
		}
		return null;
	}
}
