using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Range
{
	SHORT,
	LONG,
	WIDE,

	NUM_RANGES
}

public static class Ranges
{
	public static float[,] aafMinRanges = new float[(int)Range.NUM_RANGES, (int)Range.NUM_RANGES]
	{
		{-2.0f, 0.0f, -2.0f},
		{0.0f, 2.0f, 0.0f},
		{-2.0f, 0.0f, -2.0f},
	};

	public static float[,] aafMaxRanges = new float[(int)Range.NUM_RANGES, (int)Range.NUM_RANGES]
	{
		{3.0f, 5.0f, 5.0f},
		{5.0f, 7.0f, 7.0f},
		{5.0f, 7.0f, 7.0f},
	};

	//public static float GetMinRangeForPair(Range range1, Range range2) { return aafMinRanges[(int)range1, (int)range2]; }
	//public static float GetMaxRangeForPair(Range range1, Range range2) { return aafMaxRanges[(int)range1, (int)range2]; }

	public static float GetMinRangeForPair(Range range1, Range range2) { return -3.0f; }
	public static float GetMaxRangeForPair(Range range1, Range range2) { return 7.0f; }
}
