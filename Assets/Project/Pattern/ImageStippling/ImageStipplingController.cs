using UnityEngine;

// https://daiki-yamanaka.hatenablog.com/entry/2013/07/13/163657
[RequireComponent(typeof(MeshFilter))]
public class ImageStipplingController : MonoBehaviour
{
	public enum ColorType
	{
		Gray = 0, 
		R, G, B, 
	}

	[SerializeField]
	public ColorType DistanceBy;
	[SerializeField, Range(1.0f, 100.0f)]
	public float MinDistance = 5f;
	[SerializeField, Range(1.0f, 100.0f)]
	public float MaxDistance = 25f;
	[SerializeField]
	public Texture2D TargetImage;

	private Mesh m_mesh;

	[ContextMenu(nameof(Generate))]
	void Generate()
	{
		if(!Application.isPlaying)
		{
			DestroyImmediate(m_mesh);
		}
		else
		{
			Destroy(m_mesh);
		}
		GenerateMesh();
	}

	// Start is called before the first frame update
	void Start()
	{
		GenerateMesh();
	}

	private void GenerateMesh()
	{
		int w = TargetImage.width;
		int h = TargetImage.height;
		Color[] colors = TargetImage.GetPixels();
		var table = new float[w, h];
		for (int x = 0; x < w; ++x)
		{
			for (int y = 0; y < h; ++y)
			{
				float v = DistanceBy switch
				{
					ColorType.Gray => colors[y * w + x].grayscale,
					ColorType.R => colors[y * w + x].r,
					ColorType.G => colors[y * w + x].g,
					_ => colors[y * w + x].b,
				};
				table[x, y] = Mathf.Lerp(MinDistance, MaxDistance, v);
			}
		}

		var points = PoissonDiskSampling.GetSamplesXY(new Vector2(w, h), MinDistance, pos => table[(int)pos.x, (int)pos.y]);
		int[] indices = new int[points.Count];
		for (int i = 0; i < points.Count; ++i)
		{
			indices[i] = i;
		}

		m_mesh = new Mesh();
		m_mesh.name = "ImageStippling";
		m_mesh.SetVertices(points);
		m_mesh.SetIndices(indices, MeshTopology.Points, 0);
		m_mesh.RecalculateBounds();
		this.GetComponent<MeshFilter>().mesh = m_mesh;
	}

	private void OnDestroy()
	{
		Destroy(m_mesh);
	}
}
