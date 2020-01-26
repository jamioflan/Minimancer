using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;


public class SpriteSheetSpreader : EditorWindow 
{
	[System.Serializable]
	public class SpriteSpreaderData
	{
		public List<Texture2D> spritesToProcess = new List<Texture2D> ();
	}

	[MenuItem ("Window/Sprite Sheet Spreader")]
	static void Init()
	{
		EditorWindow.GetWindow (typeof(SpriteSheetSpreader)).Show ();
	}

	public SpriteSpreaderData data = new SpriteSpreaderData();
	public string path = "Assets/Textures/Minions/Neutral/";

	private void OnGUI()
	{
		if (data == null)
			data = new SpriteSpreaderData ();
		SerializedObject serializedObject = new SerializedObject (this);

		EditorGUILayout.PropertyField (serializedObject.FindProperty ("data"), true);
		EditorGUILayout.PropertyField (serializedObject.FindProperty ("path"), true);

		serializedObject.ApplyModifiedProperties ();

		if (GUILayout.Button("Process"))
		{
			foreach (Texture2D tex in data.spritesToProcess)
			{
				ProcessTexture(tex);
			}
		}

        if (GUILayout.Button("Create Normal Maps"))
        {
            foreach (Texture2D tex in data.spritesToProcess)
            {
                CreateNormalMap(tex);
            }
        }
    }

	public void ProcessTexture(Texture2D texture)
	{
		Texture2D newTex = new Texture2D (texture.width, texture.height);
		for (int i = 0; i < texture.width; i++)
		{
			for (int j = 0; j < texture.height; j++)
			{
				newTex.SetPixel(i, j, new Color(0.0f, 0.0f, 0.0f, 0.0f));
			}
		}

		for (int iRow = 0; iRow < 4; iRow++)
		{
			for (int iCol = 0; iCol < 2; iCol++)
			{
				int x = RenderActor_Sprites.GetConvertedTextureX(iCol, iRow);
				int y = RenderActor_Sprites.GetConvertedTextureY(iCol, iRow);

				CopyPixels(texture, newTex, 
					Mathf.FloorToInt(iCol * texture.width * 0.25f), 
					Mathf.FloorToInt((3 - iRow) * texture.height * 0.25f), 
					Mathf.FloorToInt(x * texture.width * 0.3125f + texture.width * 0.0625f), // 5 / 16 one sprite + one bit of padding
					Mathf.FloorToInt((2 - y) * texture.height * 0.3125f + texture.height * 0.0625f),
					Mathf.FloorToInt(texture.width * 0.25f),
					Mathf.FloorToInt(texture.height * 0.25f));
			}
		}

		// Special case for icon
		CopyPixels(texture, newTex,
			Mathf.FloorToInt(3 * texture.width * 0.25f), 
			0, 
			Mathf.FloorToInt(2 * texture.width * 0.3125f + texture.width * 0.0625f),
			Mathf.FloorToInt(texture.height * 0.0625f),
			Mathf.FloorToInt(texture.width * 0.25f),
			Mathf.FloorToInt(texture.height * 0.25f));

		File.WriteAllBytes(path + texture.name + ".png" , newTex.EncodeToPNG());
	}

    public void CreateNormalMap(Texture2D texture)
    {
        Texture2D newTex = new Texture2D(texture.width, texture.height);
        for (int i = 0; i < texture.width; i++)
        {
            for (int j = 0; j < texture.height; j++)
            {
                // Solid pixel check
                if (texture.GetPixel(i, j).a > 0.0f)
                {
                    newTex.SetPixel(i, j, new Color(0.5f, 0.5f, 1.0f, 1.0f));
                    int xSum = 0;
                    int ySum = 0;
                    for (int x = i - 2; x <= i + 2; x++)
                    {
                        for (int y = j - 2; y <= j + 2; y++)
                        {
                            if (x < 0 || x >= texture.width || y < 0 || y >= texture.height)
                            {
                                continue;
                            }
                            if (texture.GetPixel(x, y).a > 0.0f)
                            {
                                // Solid neighbour. Note it down
                                xSum -= (x - i);
                                ySum -= (y - j);
                            }
                        }
                    }

                    newTex.SetPixel(i, j, new Color((float)xSum / 15.0f + 0.5f, (float)ySum / 15.0f + 0.5f, 1.0f, 1.0f));

                }
                else
                {
                    newTex.SetPixel(i, j, new Color(0.5f, 0.5f, 1.0f, 1.0f));
                }
            }
        }

        string normalPath = path + texture.name + "_normals" + ".png";
        File.WriteAllBytes(normalPath, newTex.EncodeToPNG());

        string materialPath = path + "Materials/" + texture.name + ".mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
        Debug.Assert(material != null, "Could not find material at " + materialPath);
        material.SetTexture("_BumpMap", AssetDatabase.LoadAssetAtPath<Texture2D>(normalPath));
    }

	public void CopyPixels(Texture2D src, Texture2D dst, int sX, int sY, int dX, int dY, int w, int h)
	{
		for (int i = 0; i < w; i++)
		{
			for (int j = 0; j < h; j++)
			{
				dst.SetPixel(dX + i, dY + j, src.GetPixel(sX + i, sY + j));
			}
		}
	}
}
