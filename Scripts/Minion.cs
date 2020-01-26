using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization;

public class Minion : MonoBehaviour 
{
	public MinionTemplate template;
	public TargetPriority priority = TargetPriority.CLOSEST_ENEMY;
	public float fMaxHealthPreBuffs;
	public float fMaxHealthPostBuffs;
	public float fCurrentHealth;
	public int iCombo = 0;
	public bool isZombified = false;
	public List<Buff> currentBuffs = new List<Buff>();
	public List<DebuffData> timedDebuffs = new List<DebuffData> ();
	public MinionSlot slot = MinionSlot.NUM_MINION_SLOTS;

	void Start () 
	{
	}

	void Update () 
	{
	}

	public void InitFromTemplate(MinionTemplate newTemplate)
	{
		template = newTemplate;
		fCurrentHealth = fMaxHealthPreBuffs = fMaxHealthPostBuffs = template.fMaxHealth;
		isZombified = false;
	}

	public void ResetTemporaryData()
	{
		isZombified = false;
		iCombo = 0;
	}

	public bool Damage(Damage damage, float fMultiplier, float fAdditionalDamage)
	{
		fCurrentHealth -= (damage.fAmount + fAdditionalDamage) * fMultiplier;

		if (damage.debuff != DebuffData.Debuff.NO_DEBUFF)
		{
			DebuffData data = new DebuffData ();
			data.debuff = damage.debuff;
			data.debuffIcon = damage.debuffIcon;
			PFX_DebuffIcon debuffIcon = ApplyDebuff(data, damage.fDebuffDuration);
			if (debuffIcon != null)
			{
				debuffIcon.transform.SetParent(transform);
				debuffIcon.transform.localPosition = Vector3.zero;
			}
		}

		if ((fCurrentHealth <= 0.0f) || (damage.fInstadeathThreshold > 0.0f && fCurrentHealth / fMaxHealthPostBuffs < damage.fInstadeathThreshold))
		{
			Destroy(gameObject);
			return true;
		}

		return false;
	}

	public PFX_DebuffIcon ApplyDebuff(DebuffData dataToApply, float fDuration)
	{
		if (template.bImmuneToDebuffs)
			return null;

		foreach (DebuffData old in timedDebuffs)
		{
			if (old.debuff == dataToApply.debuff)
			{
				old.fTimeRemaining = fDuration;
				return null;
			}
		}

		DebuffData data = new DebuffData ();
		data.debuff = dataToApply.debuff;
		data.fTimeRemaining = fDuration;
		if (dataToApply.debuffIcon != null)
		{
			data.debuffIcon = Instantiate<PFX_DebuffIcon>(dataToApply.debuffIcon);
		}
		timedDebuffs.Add(data);
		return data.debuffIcon;
	}

	public float GetBuff(Stat eStat)
	{
		float fModifier = 0.0f;
		foreach (Buff buff in currentBuffs)
		{
			fModifier += buff.GetModifier(eStat);
		}

		return fModifier;
	}

	public void ReadFrom(SerializationInfo data, string prefix)
	{
		priority = (TargetPriority)data.GetInt16(prefix + "Priority");
	}

	public void WriteTo(SerializationInfo data, string prefix)
	{
		data.AddValue(prefix + "Priority", (short)priority);
	}
}
