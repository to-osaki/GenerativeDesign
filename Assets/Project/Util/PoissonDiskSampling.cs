using System.Collections.Generic;
using UnityEngine;

static public class PoissonDiskSampling
{
	static public List<Vector3> GetSamplesXY(Vector2 size, float distance, int iterationLimit = 10)
	{
		List<Vector3> points;
		if (size.x > 0 && size.y > 0 && distance > 0 && iterationLimit > 0)
		{
			points = GetSamplesXY(size, distance, null, iterationLimit);
		}
		else
		{
			points = new List<Vector3> { Vector3.zero };
		}
		return points;
	}

	static public List<Vector3> GetSamplesXY(Vector2 size, float minDistance, System.Func<int, Vector2, float> funcDistance, int iterationLimit = 10)
	{
		float DefaultFuncDistance(int n, Vector2 p) => minDistance;
		funcDistance ??= DefaultFuncDistance;

		// Fast Poisson Disk Sampling
		// https://www.cs.ubc.ca/~rbridson/docs/bridson-siggraph07-poissondisk.pdf
		float cellSize = minDistance / Mathf.Sqrt(2);
		int w = Mathf.CeilToInt(size.x / cellSize);
		int h = Mathf.CeilToInt(size.y / cellSize);
		Rect area = new Rect(0, 0, size.x, size.y);
		List<Vector3> points = new List<Vector3>(w * h);
		List<Vector2> actives = new List<Vector2>(w * h);
		// make grid
		var grid = new Vector2?[w + 1, h + 1];

		// put first point randomly
		{
			var first = (size / 2) + GetPointInCircle(0f, Mathf.Min(size.x, size.y) / 2);
			points.Add(first);
			actives.Add(first);
			var gi = GetGridIndex(first, cellSize);
			grid[gi.x, gi.y] = first;
		}

		while (actives.Count > 0 && actives.Count < w * h)
		{
			// choose target point randomly
			int targetIdx = Random.Range(0, actives.Count);
			var target = actives[targetIdx];
			// sample around target point
			bool sampled = false;
			for (int i = 0; i < iterationLimit; ++i)
			{
				// sample point randomly
				float distance = funcDistance(points.Count, target);
				Debug.Assert(distance >= minDistance);
				var sample = target + GetPointInCircle(distance, 2 * distance);
				if (!area.Contains(sample)) { continue; }

				// search around sample point
				var sampleIdx = GetGridIndex(sample, cellSize);
				if (grid[sampleIdx.x, sampleIdx.y].HasValue) { continue; }

				int searchSize = Mathf.CeilToInt(distance / minDistance);
				var min = sampleIdx - (Vector2Int.one * searchSize);
				var max = sampleIdx + (Vector2Int.one * searchSize);
				float sqrDist = distance * distance;

				bool discard = false;
				for (int x = Mathf.Max(min.x, 0); x < Mathf.Min(max.x, w) && !discard; ++x)
				{
					for (int y = Mathf.Max(min.y, 0); y < Mathf.Min(max.y, h) && !discard; ++y)
					{
						var tmp = grid[x, y];
						if (tmp.HasValue && (tmp.Value - sample).sqrMagnitude < sqrDist)
						{
							discard = true;
						}
					}
				}

				if (discard)
				{
					continue;
				}
				else
				{
					Debug.Assert(!grid[sampleIdx.x, sampleIdx.y].HasValue);
					points.Add(sample);
					actives.Add(sample);
					grid[sampleIdx.x, sampleIdx.y] = sample;
					sampled = true;
				}
			}
			// not sampled, disable target point
			if (!sampled)
			{
				actives.RemoveAt(targetIdx);
			}
		}
		Debug.Assert(actives.Count == 0);
		return points;
	}

	static Vector2Int GetGridIndex(Vector2 p, float cellSize)
	{
		return new Vector2Int((int)(p.x / cellSize), (int)(p.y / cellSize));
	}

	static Vector2 GetPointInCircle(float minR, float maxR)
	{
		float theta = Random.Range(0f, Mathf.PI * 2);
		float r = Random.Range(minR, maxR);
		return new Vector2(r * Mathf.Cos(theta), r * Mathf.Sin(theta));
	}
}