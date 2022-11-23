using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(MeshRenderer))]
public class CustomRenderTextureVisualizer : MonoBehaviour
{
	[SerializeField]
	public CustomRenderTexture texture;
	[SerializeField]
	public int defaultPassIndex = 0;
	[SerializeField]
	public int touchPassIndex = 1;
	[SerializeField]
	public Vector2 zoneSize = Vector2.one * 1 / 32;
	[SerializeField]
	public float zoneRotation = 0f;

	public CustomRenderTexture GetTexture() => texture;
	public void SetTouchIndex(int n) => touchPassIndex = n;

	private CustomRenderTextureZoneUpdater m_updater;

	[System.Serializable]
	public class Parameter
	{
		public string name;
		public float value;
	}

	public void Initialize()
	{
		m_updater.Initialize();
	}

	void Start()
	{
		m_updater = new CustomRenderTextureZoneUpdater(texture);
		m_updater.Initialize();

		var renderer = GetComponent<MeshRenderer>();
		if (renderer.sharedMaterial == null)
		{
			var mat = new Material(Shader.Find("Unlit/Texture"));
			mat.mainTexture = texture;
			renderer.sharedMaterial = mat;
		}
	}

	void Update()
	{
		UpdateZones();

		m_updater.DefaultUpdatePassIndex = defaultPassIndex;
		m_updater.Update();

		ZoomCamera();
	}

	void ZoomCamera()
	{
		var p = Camera.main.transform.position;
		if (Input.GetKey(KeyCode.W))
		{
			p.y = Mathf.Max(0.1f, p.y - 0.1f);
			Camera.main.transform.position = p;

		}
		else if (Input.GetKey(KeyCode.S))
		{
			p.y = Mathf.Min(10f, p.y + 0.1f);
			Camera.main.transform.position = p;
		}
	}

	void UpdateZones()
	{
		bool leftClick = Input.GetMouseButton(0);
		if (!leftClick) return;

		var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		if (Physics.Raycast(ray, out RaycastHit hit))
		{
			m_updater.RequestUpdateZone(touchPassIndex, new Vector2(hit.textureCoord.x, 1f - hit.textureCoord.y), zoneSize, zoneRotation);
		}
	}
}