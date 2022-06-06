using System.Linq;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;

// https://www.oreilly.co.jp/books/9784873118475/
public class BoidsController : MonoBehaviour
{
	[SerializeField, Range(512, 2048)]
	public int N = 512;
	[SerializeField]
	public Parameters Params;

	[ContextMenu(nameof(ResetParameters))]
	void ResetParameters()
	{
		// 力の強さ
		Params.COHESION_FORCE = 0.008f;
		Params.SEPARATION_FORCE = 0.4f;
		Params.ALIGNMENT_FORCE = 0.06f;
		// 力の働く距離
		Params.COHESION_DISTANCE = 0.5f;
		Params.SEPARATION_DISTANCE = 0.05f;
		Params.ALIGNMENT_DISTANCE = 0.1f;
		// 速度の上限/下限
		Params.MIN_VEL = 0.005f;
		Params.MAX_VEL = 0.03f;
		// 境界で働く力（0にすると自由境界）
		Params.BOUNDARY_FORCE = 0.001f;
		// フィールドサイズ
		Params.SIZE = 1f;
	}

	[System.Serializable]
	public struct Parameters
	{
		// 力の強さ
		public float COHESION_FORCE;
		public float SEPARATION_FORCE;
		public float ALIGNMENT_FORCE;
		// 力の働く距離
		public float COHESION_DISTANCE;
		public float SEPARATION_DISTANCE;
		public float ALIGNMENT_DISTANCE;
		// 速度の上限/下限
		public float MIN_VEL;
		public float MAX_VEL;
		// 境界で働く力（0にすると自由境界）
		public float BOUNDARY_FORCE;
		// フィールドサイズ
		public float SIZE;
	}

	public class Instance
	{
		public Vector3 pos;
		public Vector3 vec;
		public Vector3 dp;

		public Instance(Vector3 pos)
		{
			this.pos = pos;
		}

		public void ApplyDelta()
		{
			pos += dp;
			dp = Vector3.zero;
		}
	}

	private NativeArray<Vector3> Pos;
	private NativeArray<Vector3> Vec;
	private NativeArray<Vector3> DeltaV;
	private int[] Indices;

	private Mesh m_mesh;

	// Start is called before the first frame update
	void Start()
	{
		Indices = Enumerable.Range(0, N).ToArray();
		Pos = new NativeArray<Vector3>(
			Indices.Select(_ => new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f))).ToArray(),
			Allocator.Persistent);
		Vec = new NativeArray<Vector3>(
			Indices.Select(_ => new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)) * Params.MIN_VEL).ToArray(),
			Allocator.Persistent);
		DeltaV = new NativeArray<Vector3>(
			Indices.Select(_ => Vector3.zero).ToArray(),
			Allocator.Persistent);

		m_mesh = new Mesh();
		m_mesh.name = "Boids";
		this.gameObject.GetComponent<MeshFilter>().mesh = m_mesh;
	}

	private void OnDestroy()
	{
		Pos.Dispose();
		Vec.Dispose();
		DeltaV.Dispose();
	}

	// Update is called once per frame
	void Update()
	{
		UpdateBoids();

		ApplyBoids();

		RecalculateMesh();
	}

	void UpdateBoids()
	{
		var update = new BoidsUpdateJob()
		{
			Pos = Pos,
			Vec = Vec,
			DeltaV = DeltaV,
			Num = N,
			Params = Params,
		};
		var handle = update.Schedule(N, N / 8);
		handle.Complete();
	}

	void ApplyBoids()
	{
		for (int i = 0; i < N; ++i)
		{
			Vector3 v = Vec[i] + DeltaV[i];
			float vl = v.magnitude;
			Vector3 vn = v / vl;
			vl = Mathf.Clamp(vl, Params.MIN_VEL, Params.MAX_VEL);
			v = vn * vl;

			Vec[i] = v;
			Pos[i] += v;
		}
	}

	void RecalculateMesh()
	{
		m_mesh.SetVertices(Pos);
		m_mesh.SetIndices(Indices, MeshTopology.Points, 0);
		m_mesh.RecalculateBounds();
	}
}
