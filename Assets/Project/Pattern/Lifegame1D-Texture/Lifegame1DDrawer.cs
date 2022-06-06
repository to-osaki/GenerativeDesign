using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lifegame1DDrawer : MonoBehaviour
{
	public enum RowType
	{
		Center = 0,
		Random = 1,
		Filled = 2,
	}

	[SerializeField]
	public UnityEngine.UI.RawImage Image;
	[SerializeField]
	public RowType FirstRow = RowType.Center;
	[SerializeField]
	public Vector2Int Size = new Vector2Int(256, 256);
	[SerializeField]
	public byte Rule = 30;

	readonly Color32 Black32 = new Color32(0, 0, 0, 255);
	readonly Color32 White32 = new Color32(255, 255, 255, 255);

	readonly Vector3Int[] Patterns = new Vector3Int[8]
	{
		new Vector3Int(0, 0, 0),
		new Vector3Int(0, 0, 1),
		new Vector3Int(0, 1, 0),
		new Vector3Int(0, 1, 1),
		new Vector3Int(1, 0, 0),
		new Vector3Int(1, 0, 1),
		new Vector3Int(1, 1, 0),
		new Vector3Int(1, 1, 1),
	};

	private Texture2D m_tex;

	private int[] m_row;
	private Color32[] m_colors;

	[ContextMenu("Draw Rule")]
	void DrawRule()
	{
		m_tex = new Texture2D(Size.x, Size.y, TextureFormat.ARGB32, mipChain: false);

		InitFirstRow();
		UpdateRow();

		m_tex.Apply();
		Image.texture = m_tex;
	}

	void InitFirstRow()
	{
		m_row = new int[Size.x];
		if (FirstRow == RowType.Center)
		{
			m_row[Size.x / 2] = 1;
		}
		else if (FirstRow == RowType.Random)
		{
			for (int i = 0; i < Size.x; ++i)
			{
				m_row[i] = Random.Range(0, 2);
			}
		}
		else if (FirstRow == RowType.Filled)
		{
			for (int i = 0; i < Size.x; ++i)
			{
				m_row[i] = 1;
			}
		}

		m_colors = new Color32[Size.x];
		for (int i = 0; i < Size.x; ++i)
		{
			m_colors[i] = m_row[i] > 0 ? White32 : Black32;
		}

		m_tex.SetPixels32(0, 0, Size.x, 1, m_colors);
	}

	void UpdateRow()
	{
		for (int v = 0; v < Size.y; ++v)
		{
			int prev = 0;
			int self = 0;
			Vector3Int pattern;
			for (int i = 1; i < Size.x - 1; ++i)
			{
				pattern = new Vector3Int(m_row[i - 1], m_row[i], m_row[i + 1]);

				int patternNo = System.Array.IndexOf(Patterns, pattern);
				self = (Rule >> patternNo) & 1; // dead or alive

				m_row[i - 1] = prev;
				m_colors[i - 1] = prev > 0 ? White32 : Black32;

				prev = self;
			}
			// fill last cell
			m_row[Size.x - 1] = self;
			m_colors[Size.x - 1] = self > 0 ? White32 : Black32;

			m_tex.SetPixels32(0, v, Size.x, 1, m_colors);
		}
	}

	private void Start()
	{
		Image ??= this.GetComponent<UnityEngine.UI.RawImage>();
		DrawRule();
	}

	void OnDestroy()
	{
		Image.texture = null;
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
