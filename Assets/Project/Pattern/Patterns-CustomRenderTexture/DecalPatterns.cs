using UnityEngine;

public class DecalPatterns : MonoBehaviour
{
	[SerializeField]
	CustomRenderTexture rt_;

	CustomRenderTextureZoneUpdater zoneUpdater_;

	private void Awake()
	{
		var mat = new Material(Shader.Find("Unlit/Texture"));
		mat.mainTexture = rt_;
		GetComponent<Renderer>().material = mat;
	}

	// Start is called before the first frame update
	void Start()
	{
		zoneUpdater_ = new CustomRenderTextureZoneUpdater(rt_);
		zoneUpdater_.DisableDefaultUpdate = true; // disable default update 
		zoneUpdater_.Initialize();
	}

	// Update is called once per frame
	void Update()
	{
		DaishouArare();
		this.enabled = false;
	}

	void DaishouArare()
	{
		var samples = PoissonDiskSampling.GetSamplesXY(Vector2.one, 0.01f, (idx, pos) => 1 / 16f);
		for (int i = 0; i < samples.Count; ++i)
		{
			float scale = i < samples.Count / 10 || i % 10 == 0 ? 1 / 16f : 1 / 32f;
			zoneUpdater_.RequestUpdateZone(1, (Vector2)samples[i], Vector2.one * scale, 0f);
		}
		zoneUpdater_.Update();
	}
}
