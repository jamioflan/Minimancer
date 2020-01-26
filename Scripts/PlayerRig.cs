using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerRig : MonoBehaviour 
{
	public Transform[] playerSpawnPoints = new Transform[(int)MinionSlot.NUM_MINION_SLOTS];
	public Transform[] enemySpawnPoints = new Transform[6];
	public Transform meleeZoneTop, meleeZoneBottom, meleeZoneLeft, meleeZoneRight,
	rangedZoneTop, rangedZoneBottom, rangedZoneLeft, rangedZoneRight;
	public Transform[] targets = new Transform[2], meleeTargets = new Transform[2];
	public Transform[] radii = new Transform[2], meleeRadii = new Transform[2];
	public LevelCompleteScreen levelCompleteScreen;
	public Damage fatalDamage;
	public RectTransform guiPanel;
	public Camera mainCamera;
	public Text elementalHint;
	public Transform wallsPrefab;

	public void Update()
	{
		Vector3[] fourCorners = new Vector3[4];
		guiPanel.GetWorldCorners(fourCorners);

		float fMinX = fourCorners [0].x / Screen.width;
		float fMaxX = fourCorners [2].x / Screen.width;
		float fMinY = fourCorners [0].y / Screen.height;
		float fMaxY = fourCorners [2].y / Screen.height;

		float fCentreX = (fMinX + fMaxX) * 0.5f;
		float fWidthX = fMaxX - fMinX;

		float fCentreY = (fMinY + fMaxY) * 0.5f;
		float fHeightY = fMaxY - fMinY;

		mainCamera.rect = new Rect (
			fCentreX - 0.5f * fWidthX, 
			fCentreY - 0.5f * fHeightY, 
			fWidthX, 
			fHeightY);
	}

	public void SetMeleeZone(float fMin, float fMax, float fHeight)
	{
		fMin += 0.1f;
		fMax -= 0.1f;
		fHeight -= 0.2f;

		float fCentre = (fMin + fMax) * 0.5f;
		float fWidth = fMax - fMin;

		meleeZoneTop.localPosition = new Vector3 (fCentre, 0.0f, fHeight * 0.5f);
		meleeZoneTop.localScale = new Vector3 (0.1f * fWidth, 0.1f, 0.005f);
		meleeZoneBottom.localPosition = new Vector3 (fCentre, 0.0f, -fHeight * 0.5f);
		meleeZoneBottom.localScale = new Vector3 (0.1f * fWidth, 0.1f, 0.005f);

		meleeZoneLeft.localPosition = new Vector3 (fMin + 0.025f, 0.0f, 0.0f); 
		meleeZoneLeft.localScale = new Vector3 (0.005f, 0.1f, 0.1f * fHeight);
		meleeZoneRight.localPosition = new Vector3 (fMax - 0.025f, 0.0f, 0.0f); 
		meleeZoneRight.localScale = new Vector3 (0.005f, 0.1f, 0.1f * fHeight);
	}

	public void SetRangedZone(float fMin, float fMax, float fHeight)
	{
		float fCentre = (fMin + fMax) * 0.5f;
		float fWidth = fMax - fMin;

		rangedZoneTop.localPosition = new Vector3 (fCentre, 0.0f, fHeight * 0.5f);
		rangedZoneTop.localScale = new Vector3 (0.1f * fWidth, 0.1f, 0.005f);
		rangedZoneBottom.localPosition = new Vector3 (fCentre, 0.0f, -fHeight * 0.5f);
		rangedZoneBottom.localScale = new Vector3 (0.1f * fWidth, 0.1f, 0.005f);

		rangedZoneLeft.localPosition = new Vector3 (fMin + 0.025f, 0.0f, 0.0f); 
		rangedZoneLeft.localScale = new Vector3 (0.005f, 0.1f, 0.1f * fHeight);
		rangedZoneRight.localPosition = new Vector3 (fMax - 0.025f, 0.0f, 0.0f); 
		rangedZoneRight.localScale = new Vector3 (0.005f, 0.1f, 0.1f * fHeight);
	}
}
