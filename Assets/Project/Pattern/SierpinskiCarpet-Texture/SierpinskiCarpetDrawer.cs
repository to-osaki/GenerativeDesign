using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SierpinskiCarpetDrawer : MonoBehaviour
{
	[SerializeField]
	private UnityEngine.UI.RawImage Image;
	[SerializeField]
	private Vector2Int Size;
	[SerializeField]
	private Vector3Int Pattern;
	[SerializeField]
	private int P;
	[SerializeField]
	private Gradient ColorGradient;

	private Texture2D m_tex;

	private int[] m_row;
	private Color32[] m_colors;

	[ContextMenu("DrawCarpet")]
	void DrawCarpet()
	{
		Pattern = new Vector3Int(1, 1, 1);
		Draw();
	}

	[ContextMenu("DrawGasket")]
	void DrawGasket()
	{
		Pattern = new Vector3Int(1, 0, 1);
		Draw();
	}

	void Draw()
	{
		m_tex = new Texture2D(Size.x, Size.y, TextureFormat.ARGB32, mipChain: false);

		InitFirstRow();
		UpdateRow(Pattern);

		m_tex.Apply();
		Image.texture = m_tex;
	}

	void InitFirstRow()
	{
		m_row = new int[Size.x];
		m_colors = new Color32[Size.x];
		for (int i = 0; i < Size.x; ++i)
		{
			m_row[i] = 1;
			// byte c = (byte)(255 * 1 / (P - 1));
			m_colors[i] = ColorGradient.Evaluate((float)1 / (P - 1));
		}
		m_tex.SetPixels32(0, 0, Size.x, 1, m_colors);
	}

	void UpdateRow(Vector3Int weight)
	{
		for (int v = 0; v < Size.y; ++v)
		{
			int left = 1;
			int top = 0;
			int right = 0;
			for (int i = 1; i < Size.x; ++i)
			{
				top = m_row[i - 1];
				right = m_row[i];
				int n = (int)Vector3.Dot(new Vector3(left, top, right), weight) % P;
				m_row[i - 1] = left;
				left = n;

				byte c = (byte)(255 * n / (P - 1));
				m_colors[i] = ColorGradient.Evaluate((float)n / (P - 1));
			}
			m_row[Size.x - 1] = left + top + right; // fill last cell
			m_tex.SetPixels32(0, v, Size.x, 1, m_colors);
		}
	}

	private void Start()
	{
		Draw();
	}

	void OnDestroy()
	{
		Image.texture = null;

		if (m_tex != null)
		{
			if (Application.isPlaying)
			{
				Destroy(m_tex);
			}
			else
			{
				DestroyImmediate(m_tex);
			}
		}
	}
}
