using System.Collections.Generic;
using Geometry;
using Geometry.Generators;
using Geometry.Streams;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;


[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ProceduralMesh : MonoBehaviour
{
	Mesh mesh;
	MeshCollider meshCollider;
	MeshRenderer meshRenderer;

	void Awake () 
	{
		mesh = new Mesh { name = "Procedural Mesh"};
		GetComponent<MeshFilter>().mesh = mesh;
		meshRenderer = GetComponent<MeshRenderer>();
		meshCollider = GetComponent<MeshCollider>();
	}
	public void SetMaterial(Material material)
	{
		Material newMat = Instantiate(material);
		meshRenderer.material = newMat;
	}
	public void GenerateMesh(Face[] facesArr) 
	{
		Mesh.MeshDataArray meshDataArray = Mesh.AllocateWritableMeshData(1);
		Mesh.MeshData meshData = meshDataArray[0];

		NativeArray<Face> faces = new NativeArray<Face>(facesArr.Length, Allocator.TempJob);
		faces.CopyFrom(facesArr);

		MeshJob<GenerateFaces, SingleStream>.ScheduleParallel(mesh, meshData, faces, default).Complete();

		faces.Dispose();

		Mesh.ApplyAndDisposeWritableMeshData(meshDataArray, mesh);
		mesh.RecalculateBounds();
		meshCollider.sharedMesh = Instantiate<Mesh>(mesh);
	}
}