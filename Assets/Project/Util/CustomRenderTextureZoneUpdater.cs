using System.Collections.Generic;
using UnityEngine;

public class CustomRenderTextureZoneUpdater
{
	public int DefaultUpdatePassIndex
	{
		get => m_tex.shaderPass;
		set => m_tex.shaderPass = value;
	}

	public bool DisableDefaultUpdate { get; set; }

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

	public void Update(int count = 1)
	{
		m_tex.ClearUpdateZones();

		if (m_zones.Count == 0)
		{
			if (!DisableDefaultUpdate)
			{
				m_tex.Update(count);
			}
		}
		else
		{
			ApplyRequiredZones();
			m_tex.Update(count);
		}
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
		var zones = new CustomRenderTextureUpdateZone[1 + m_zones.Count];
		// https://tips.hecomi.com/entry/2017/05/17/020037
		var defaultZone = new CustomRenderTextureUpdateZone()
		{
			needSwap = true,
			passIndex = DefaultUpdatePassIndex,
			rotation = 0f,
			// if DisableDefaultUpdate, set invalid zone
			updateZoneCenter = DisableDefaultUpdate ? Vector2.zero : Vector2.one / 2f,
			updateZoneSize = DisableDefaultUpdate ? Vector2.zero : Vector2.one,
		};

		zones[0] = defaultZone;
		m_zones.CopyTo(0, zones, 1, m_zones.Count);
		m_tex.SetUpdateZones(zones);

		m_zones.Clear();
	}
}
