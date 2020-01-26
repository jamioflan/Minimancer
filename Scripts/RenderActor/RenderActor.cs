using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class RenderActor : MonoBehaviour 
{
	protected bool bDead = false;

	protected AnimState animState = AnimState.IDLE;
	protected AnimState nextState = AnimState.NUM_ANIM_STATES;
	protected float fTimeInState = 0.0f;
	protected bool bReverse = false;
	public ParticleSystem attackParticles, moveParticles;
	private LineRenderer lr;
	private float fChainTime = 0.0f;

	protected virtual void Start () 
	{
		lr = GetComponent<LineRenderer>();
	}

	protected virtual void Update () 
	{
		
	}

	public AnimState GetAnimState()
	{
		return animState;
	}

	public void SetAnimState(AnimState state, bool bRetrigger = true)
	{
		if(bRetrigger || animState != state)
		{
			animState = state;
			nextState = AnimState.NUM_ANIM_STATES;
			fTimeInState = 0.0f;
			UpdateUVs();
		}
	}

	public void SetAnimStateAndNext(AnimState state, AnimState next)
	{
		animState = state;
		nextState = next;
		fTimeInState = 0.0f;
		UpdateUVs();
	}

	public void SetChainPFXActive(float fTime, Vector3[] positions)
	{
		lr.enabled = true;
		lr.positionCount = positions.Length;
		lr.SetPositions(positions);
		fChainTime = fTime;
	}

	public void SetReverse(bool bSet)
	{
		bReverse = bSet;
	}

	protected abstract void UpdateUVs();

	// At this point, we should now expect to be an orphaned renderer, just rendering our last moments
	public virtual void PlayDeathAnimation()
	{
		bDead = true;
	}

	public abstract void Init(Actor actor);

	public virtual void UpdateAnimation(float fDeltaTime)
	{
		fChainTime -= fDeltaTime;
		if (fChainTime < 0.0f)
		{
			if (lr != null)
			{
				lr.enabled = false;
			}
		}
	}
}
