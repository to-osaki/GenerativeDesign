using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomRenderTextureZoneUpdater
{
	public int DefaultUpdatePassIndex { get; set; } = 0;

	private readonly CustomRenderTexture m_tex;
	private readonly List<CustomRenderTextureUpdateZone> m_zones = new(0);

	public CustomRenderTextureZoneUpdater(CustomRenderTexture texture)
	{
		this.m_tex = texture ?? throw new System.ArgumentNullException(nameof(texture));
	}

	public void Initialize()
	{
		m_tex.Initialize();
	}

	public void Update()
	{
		m_tex.ClearUpdateZones();
		ApplyRequiredZones();
		m_tex.Update(1);
	}

	public void RequestUpdateZone(int passIndex, Vector2 center, Vector2 size, float rotation)
	{
		var zone = new CustomRenderTextureUpdateZone();
		zone.needSwap = true;
		zone.passIndex = passIndex;
		zone.rotation = rotation;
		zone.updateZoneCenter = center;
		zone.updateZoneSize = size;
		m_zones.Add(zone);
	}

	private void ApplyRequiredZones()
	{
		if (m_zones.Count == 0) { return; }

		var zones = new CustomRenderTextureUpdateZone[1 + m_zones.Count];

		var defaultZone = new CustomRenderTextureUpdateZone();
		defaultZone.needSwap = true;
		defaultZone.passIndex = DefaultUpdatePassIndex;
		defaultZone.rotation = 0f;
		defaultZone.updateZoneCenter = new Vector2(0.5f, 0.5f);
		defaultZone.updateZoneSize = new Vector2(1f, 1f);

		zones[0] = defaultZone;
		m_zones.CopyTo(0, zones, 1, m_zones.Count);
		m_tex.SetUpdateZones(zones);

		m_zones.Clear();
	}
}
