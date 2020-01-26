using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PFX_DebuffIcon : MonoBehaviour 
{	
	public float fHoverCircle = 0.0f;
	public float fSpeed = 1.0f;
	public float fMag = 1.0f;

	public RenderActor_Sprites renderActor;

	public void Init(bool bRes = false)
	{
		if (renderActor != null)
		{
			renderActor.SetAnimState(AnimState.IDLE);
			renderActor.SetStunPFX();

			if (bRes)
			{
				renderActor.iNumFrames = 4;
				renderActor.PlayDeathAnimation();
			}
		}
	}

	void Update () 
	{
		if(renderActor != null)
			renderActor.UpdateAnimation(Time.deltaTime);
		fHoverCircle += Time.deltaTime * fSpeed;
		//transform.localPosition = new Vector3 (Mathf.Cos(fHoverCircle), Mathf.Sin(fHoverCircle), 0.0f) * fMag;
	}
}
