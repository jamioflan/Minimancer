using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderActor_Sprites : RenderActor
{
	private static readonly float fONE_OVER_SHEET_SIZE_X = 1.0f / 4.0f;
	private static readonly float fONE_OVER_SHEET_SIZE_Y = 1.0f / 4.0f;

	// Texture sheet processing
	private static readonly int[,] aiXCoords = 
		new int[4, 2] { { 0, 1 }, 
			{ 2, 0 },
			{ 1, 2 },
			{ 0, 1 }
		};
	public static int GetConvertedTextureX(int x, int y)
	{
		return aiXCoords[y,x];
	}
	private static readonly int[,] aiYCoords = 
		new int[4, 2] { { 0, 0 }, 
		{ 0, 1 },
		{ 1, 1 },
		{ 2, 2 }
	};
	public static int GetConvertedTextureY(int x, int y)
	{
		return aiYCoords[y,x];
	}

	private MeshFilter mf;


	private float[] afTimePerFrame = new float[(int)AnimState.NUM_ANIM_STATES] 
	{ 1.0f, 0.25f, 1.0f, 0.4f };
	public int iNumFrames = 2;
	private int iCurrentFrame = 0;
	public bool bSpread = true;

	protected override void Start () 
	{
		base.Start();
		mf = GetComponent<MeshFilter>();
	}

	public void SetStunPFX()
	{
		afTimePerFrame [0] = 0.25f;
	}

	public override void Init(Actor actor)
	{
		for(int i = 0; i < (int)AnimState.NUM_ANIM_STATES; i++)
		{
			afTimePerFrame [i] = actor.minion.template.afTimePerFrame [i];
		}

		fTimeInState = Random.Range(0.0f, afTimePerFrame[0]);
		iNumFrames = actor.minion.template.iNumFrames;
	}

	public override void UpdateAnimation(float fDeltaTime)
	{
		base.UpdateAnimation(fDeltaTime);

		fTimeInState += fDeltaTime;

		if (fTimeInState > afTimePerFrame[(int)animState] * iNumFrames)
		{
			fTimeInState -= afTimePerFrame[(int)animState] * iNumFrames;
			if (bDead)
			{
				Destroy(gameObject);
			}
			if (nextState != AnimState.NUM_ANIM_STATES)
			{
				bReverse = false;
				animState = nextState;
				nextState = AnimState.NUM_ANIM_STATES;
				UpdateUVs();
			}
		}

		int iNewFrame = Mathf.FloorToInt(fTimeInState / afTimePerFrame[(int)animState]);
		if (iNewFrame != iCurrentFrame)
		{
			iCurrentFrame = iNewFrame;
			UpdateUVs();
		}
	}

	private static readonly float fPADDING = 1.0f / 16.0f;

	protected override void UpdateUVs()
	{
		if(mf == null)
			mf = GetComponent<MeshFilter>();

		List<Vector2> uvs = new List<Vector2> ();

		int iRow = (int)(AnimState.NUM_ANIM_STATES - 1) - (int)animState;
		int iCol = bReverse ? 
			1 - Mathf.FloorToInt(fTimeInState / afTimePerFrame[(int)animState]) :
			Mathf.FloorToInt(fTimeInState / afTimePerFrame[(int)animState]);

		iRow = Mathf.Clamp(iRow, 0, 3);
		iCol = Mathf.Clamp(iCol, 0, iNumFrames - 1);

		float fY1 = iRow * fONE_OVER_SHEET_SIZE_Y;
		float fY2 = (iRow + 1) * fONE_OVER_SHEET_SIZE_Y;
		float fX1 = iCol * fONE_OVER_SHEET_SIZE_X;
		float fX2 = (iCol + 1) * fONE_OVER_SHEET_SIZE_X;

		if (bSpread)
		{
			int iConvertedCol = GetConvertedTextureX(iCol, 3 - iRow);
			int iConvertedRow = 2 - GetConvertedTextureY(iCol, 3 - iRow);

			fY1 = iConvertedRow * fONE_OVER_SHEET_SIZE_Y 		+ (iConvertedRow + 1) * fPADDING;
			fY2 = (iConvertedRow + 1) * fONE_OVER_SHEET_SIZE_Y 	+ (iConvertedRow + 1) * fPADDING;
			fX1 = iConvertedCol * fONE_OVER_SHEET_SIZE_X 		+ (iConvertedCol + 1) * fPADDING;
			fX2 = (iConvertedCol + 1) * fONE_OVER_SHEET_SIZE_X 	+ (iConvertedCol + 1) * fPADDING;
		}

		uvs.Add(new Vector2 (fX2, fY1));
		uvs.Add(new Vector2 (fX1, fY1));
		uvs.Add(new Vector2 (fX1, fY2));
		uvs.Add(new Vector2 (fX2, fY2));

		uvs.Add(new Vector2 (fX1, fY1));
		uvs.Add(new Vector2 (fX2, fY1));
		uvs.Add(new Vector2 (fX2, fY2));
		uvs.Add(new Vector2 (fX1, fY2));


		mf.mesh.SetUVs(0, uvs);
	}

	protected override void Update () 
	{
		base.Update();

		if (bDead)
		{
			UpdateAnimation(Time.deltaTime);
		}
	}

	// At this point, we should now expect to be an orphaned renderer, just rendering our last moments
	public override void PlayDeathAnimation()
	{
		base.PlayDeathAnimation();
		SetAnimState(AnimState.DEATH);
	}

}
