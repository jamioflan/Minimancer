using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Damage : MonoBehaviour 
{

	private Element element = Element.PHYSICAL;
	public float fAmount = 0.0f;
	public float fRadius = 0.0f;
	public float fCritChance = 0.0f;
	public float fCritMultiplier = 2.0f;
	public float fPushAmount = 0.0f;
	public float fInstadeathThreshold = -1.0f;
	public BuffEffect onHitEffect;
	private MinionSlotType attackerType = MinionSlotType.MELEE;

	public DebuffData.Debuff debuff = DebuffData.Debuff.NO_DEBUFF;
	public float fDebuffDuration = 1.0f;
	public PFX_DebuffIcon debuffIcon;

	public Element GetElement() { return element; }
	public void SetElement(Element ele) { element = ele; }
	public MinionSlotType GetSlotType() { return attackerType; }
	public void SetSlotType(MinionSlotType type) { attackerType = type; }
	public bool IsRadial() { return fRadius > 0.0f; }

	void Start () 
	{
		
	}

	void Update () 
	{
		
	}
}
