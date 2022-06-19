using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Collections;
using UnityEngine;

public struct AttractorParticle
{
	public Vector3 emit;
	public Vector3 pos;
	public Vector3 vel;
	public float life;
	public Vector2 size;
	public Vector4 color;

	public AttractorParticle(Vector3 emit, Vector2 size, Vector4 color)
	{
		this.emit = emit;
		this.pos = Vector3.zero;
		this.vel = Vector3.zero;
		this.life = 0f;
		this.size = size;
		this.color = color;
	}
}

// https://github.com/IndieVisualLab/UnityGraphicsProgrammingBook3
public class AttractorController : MonoBehaviour
{
	#region Attractor
	[SerializeField]
	ComputeShader Compute;
	[SerializeField]
	string AttractorKernel = "ThomasAttractorUpdate";
	[SerializeField]
	int ParticleNum;

	[SerializeField]
	Mesh particleMesh;
	[SerializeField]
	Material particleMat;

	GraphicsBuffer buffer = null;
	readonly int BufferID = Shader.PropertyToID("buffer");

	Vector3Int GpuThreads;
	int EmitKernelID, UpdateKernelID;

	GraphicsBuffer args = null;

	void InitCompute()
	{
		EmitKernelID = Compute.FindKernel("Emit");
		UpdateKernelID = Compute.FindKernel(AttractorKernel);
		uint x, y, z;
		Compute.GetKernelThreadGroupSizes(UpdateKernelID, out x, out y, out z);
		GpuThreads = new Vector3Int((int)x, (int)y, (int)z);
	}
	#endregion

	#region ThomasCyclicallySymmetricAttractor
	[SerializeField]
	float EmitterField;
	[SerializeField]
	Gradient ParticleColor;
	[SerializeField]
	Vector2 particleSize;

	[SerializeField]
	Vector4 constants = new Vector4(0f, 0.32899f, 0f, 0f);

	readonly int ConstantsID = Shader.PropertyToID("constants");

	void InitBuffer()
	{
		if (args == null)
		{
			args = new GraphicsBuffer(GraphicsBuffer.Target.IndirectArguments, 5, sizeof(int));
			args.SetData(new uint[] { particleMesh.GetIndexCount(0), (uint)ParticleNum });
		}

		if (buffer != null) { buffer.Release(); }

		buffer = new GraphicsBuffer(
			GraphicsBuffer.Target.Structured, GraphicsBuffer.UsageFlags.None, ParticleNum, Marshal.SizeOf<AttractorParticle>());
		NativeArray<AttractorParticle> array = new NativeArray<AttractorParticle>(ParticleNum, Allocator.Temp);
		for (int i = 0; i < ParticleNum; ++i)
		{
			float r = (float)i / ParticleNum;
			Color color = ParticleColor.Evaluate(r);
			array[i] = new AttractorParticle(Random.insideUnitCircle * EmitterField * r, particleSize, color);
		}

		buffer.SetData(array);

		Compute.SetBuffer(EmitKernelID, BufferID, buffer);
		int x = Mathf.CeilToInt((float)ParticleNum / GpuThreads.x);
		Compute.Dispatch(EmitKernelID, x, GpuThreads.y, GpuThreads.z);
	}

	void UpdateBuffer()
	{
		Compute.SetVector(ConstantsID, constants);

		Compute.SetBuffer(UpdateKernelID, BufferID, buffer);
		int x = Mathf.CeilToInt((float)ParticleNum / GpuThreads.x);
		Compute.Dispatch(UpdateKernelID, x, GpuThreads.y, GpuThreads.z);
	}
	#endregion

	// Start is called before the first frame update
	void Start()
	{
		InitCompute();
		InitBuffer();
	}

	// Update is called once per frame
	void Update()
	{
		UpdateBuffer();

		particleMat.SetPass(0);
		particleMat.SetBuffer(Shader.PropertyToID("buf"), buffer);
		particleMat.SetMatrix(Shader.PropertyToID("modelMatrix"), transform.localToWorldMatrix);
		Graphics.DrawMeshInstancedIndirect(particleMesh, 0, particleMat, new Bounds(Vector3.zero, Vector3.one * 100), args);
	}

	private void OnDestroy()
	{
		if (args != null)
		{
			args.Release();
			args = null;
		}
		if (buffer != null)
		{
			buffer.Release();
			buffer = null;
		}
	}
}
