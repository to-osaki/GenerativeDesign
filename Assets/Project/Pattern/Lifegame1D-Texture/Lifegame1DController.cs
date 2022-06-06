using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lifegame1DController : MonoBehaviour
{
	[SerializeField]
	Lifegame1DDrawer DrawerTemplate;
	[SerializeField]
	Lifegame1DDrawer.RowType FirstRow;

	private void Start()
	{
		for(int rule = 0; rule < 256; ++rule)
		{
			var instance = GameObject.Instantiate<Lifegame1DDrawer>(DrawerTemplate, DrawerTemplate.transform.parent ?? this.transform);
			instance.Rule = (byte)rule;
			instance.FirstRow = this.FirstRow;
			instance.transform.localPosition = new Vector2(rule % 16 * 300, -rule / 16 * 300);
		}
		DrawerTemplate.gameObject.SetActive(false);
	}
}