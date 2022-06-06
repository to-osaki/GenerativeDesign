using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

[BurstCompile]
public struct BoidsUpdateJob : IJobParallelFor
{
	public int Num;
	public BoidsController.Parameters Params;
	[ReadOnly]
	public NativeArray<Vector3> Pos;
	[ReadOnly]
	public NativeArray<Vector3> Vec;
	[WriteOnly]
	public NativeArray<Vector3> DeltaV;

	private float LengthL2_2(Vector3 a) => Vector3.Dot(a, a);

	public void Execute(int index)
	{
		float COHESION_DISTANCE = Params.COHESION_DISTANCE * Params.COHESION_DISTANCE;
		float SEPARATION_DISTANCE = Params.SEPARATION_DISTANCE * Params.SEPARATION_DISTANCE;
		float ALIGNMENT_DISTANCE = Params.ALIGNMENT_DISTANCE * Params.ALIGNMENT_DISTANCE;

		var self_pos = Pos[index];
		var self_vec = Vec[index];

		Vector3 cohCenter = Vector3.zero;
		int cohs = 0;
		Vector3 sepTotal = Vector3.zero;
		int seps = 0;
		Vector3 aliVec = Vector3.zero;
		int alis = 0;
		for (int k = 0; k < Num; ++k)
		{
			var that_pos = Pos[k];
			var that_vec = Vec[k];
			var self_to_that = that_pos - self_pos;
			float cos_ = Vector3.Dot(self_vec, self_to_that);
			if (cos_ < 0f) { continue; }

			float d2 = LengthL2_2(self_to_that);
			if (d2 < COHESION_DISTANCE)
			{
				cohCenter += that_pos;
				++cohs;
			}
			if (d2 < SEPARATION_DISTANCE)
			{
				sepTotal += self_to_that;
				++seps;
			}
			if (d2 < ALIGNMENT_DISTANCE)
			{
				aliVec += that_vec;
				++alis;
			}
		}

		float dist = (self_pos - Vector3.zero).magnitude;
		Vector3 dv = -Params.BOUNDARY_FORCE * self_pos * (dist - Params.SIZE) / dist;
		{
			cohCenter = cohCenter / cohs;
			dv += (cohCenter - self_pos) * Params.COHESION_FORCE;
		}
		{
			dv += sepTotal * Params.SEPARATION_FORCE;
		}
		{
			aliVec = aliVec / alis;
			dv += aliVec * Params.SEPARATION_FORCE;
		}
		DeltaV[index] = dv;
	}
}