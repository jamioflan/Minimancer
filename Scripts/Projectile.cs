using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour 
{
	public Actor firer;
	public MinionTemplate firerTemplate;

	public float fTravelDuration = 1.0f;
	public float fProgress;

	public Actor target;
	private Vector3 lastTargetPos;

	public Vector3 launchPos;

	private static readonly Vector3 centreOfMassPoint = new Vector3(0.0f, 0.3f, 0.0f);

	void Start () 
	{
		
	}

	void Update () 
	{
		fProgress += Core.GetDeltaTime();

		if (target != null)
		{
			lastTargetPos = target.transform.position + centreOfMassPoint;
		}

		Vector3 dPos = lastTargetPos - launchPos;

		float fX = dPos.x * fProgress / fTravelDuration;
		float fY = -0.5f * Physics.gravity.y * (fTravelDuration - fProgress) * fProgress; 
		float fZ = dPos.z * fProgress / fTravelDuration;

		transform.position = launchPos + new Vector3 (fX, fY, fZ);
		transform.localEulerAngles = new Vector3(0.0f, 0.0f, Mathf.Rad2Deg * Mathf.Atan2(0.5f * Physics.gravity.y * (2 * fProgress - fTravelDuration), dPos.x / fTravelDuration));

		if (fProgress >= fTravelDuration)
		{
			OnHit();
		}
	}

	private void OnHit()
	{
		AudioSource hitSound = GetComponent<AudioSource>();
		if (hitSound != null)
		{
			hitSound.Play();
		}

		if (firer != null && firer.minion != null)
		{
			firer.minion.template.OnProjectileHit(firer, target, lastTargetPos);
		}
		else
		if (firerTemplate != null)
		{
			firerTemplate.OnProjectileHit(null, target, lastTargetPos);
		}

		Destroy(gameObject);
	}
}
