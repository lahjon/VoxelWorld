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

	void Awake () 
	{
		mesh = new Mesh { name = "Procedural Mesh"};
		GetComponent<MeshFilter>().mesh = mesh;
		meshCollider = GetComponent<MeshCollider>();
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

	// public void GenerateMesh(float3 norm, float4 tangent, float4 color, int face) 
	// {
	// 	Mesh.MeshDataArray meshDataArray = Mesh.AllocateWritableMeshData(1);
	// 	Mesh.MeshData meshData = meshDataArray[0];


	// 	NativeArray<Face> faces = new NativeArray<Face>(6, Allocator.TempJob);
	// 	faces[0] = Direction.XPos.CreateFace();
	// 	faces[1] = Direction.XNeg.CreateFace();
	// 	faces[2] = Direction.YPos.CreateFace();
	// 	faces[3] = Direction.YNeg.CreateFace();
	// 	faces[4] = Direction.ZPos.CreateFace();
	// 	faces[5] = Direction.ZNeg.CreateFace();
		
	// 	// Face newFace = new Face(); 
	// 	// newFace.tangent = tangent;
	// 	// newFace.color = color;
	// 	// newFace.normal = norm;
	// 	// newFace.x = faces[face].x;
	// 	// newFace.y = faces[face].y;
	// 	// newFace.z = faces[face].z;
	// 	// newFace.w = faces[face].w;
	// 	// faces[face] = newFace;

	// 	MeshJob<GenerateFaces, SingleStream>.ScheduleParallel(mesh, meshData, faces, default).Complete();

	// 	faces.Dispose();

	// 	Mesh.ApplyAndDisposeWritableMeshData(meshDataArray, mesh);
	// }

	void AddFace()
	{
		
	}
}

    // Mesh mesh;
    // [StructLayout(LayoutKind.Sequential)]
    // struct Vertex {
	// 	public float3 position, normal;
	// 	public half4 tangent;
    //     public half4 color;
	// 	public half2 texCoord0;
	// }
    // void OnEnable () 
    // {
    //     int vertexAttributeCount = 5;
    //     int vertexCount = 4;
    //     int triangleIndexCount = 6;
        

	// 	Mesh.MeshDataArray meshDataArray = Mesh.AllocateWritableMeshData(1);
    //     Mesh.MeshData meshData = meshDataArray[0];

    //     var vertexAttributes = new NativeArray<VertexAttributeDescriptor>(
	// 		vertexAttributeCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory
	// 	);

    //     vertexAttributes[0] = new VertexAttributeDescriptor(dimension: 3);
    //     vertexAttributes[1] = new VertexAttributeDescriptor(
	// 		VertexAttribute.Normal, dimension: 3
	// 	);

	// 	vertexAttributes[2] = new VertexAttributeDescriptor(
	// 		VertexAttribute.Tangent, VertexAttributeFormat.Float16, 4
	// 	);

	// 	vertexAttributes[3] = new VertexAttributeDescriptor(
	// 		VertexAttribute.Color, VertexAttributeFormat.Float16, 4
	// 	);

    //     vertexAttributes[4] = new VertexAttributeDescriptor(
	// 		VertexAttribute.TexCoord0, VertexAttributeFormat.Float16, 2
	// 	);

    //     meshData.SetVertexBufferParams(vertexCount, vertexAttributes);
	// 	vertexAttributes.Dispose();

    //     NativeArray<Vertex> vertices = meshData.GetVertexData<Vertex>();

    //     half h0 = half(0f), h1 = half(1f);
    //     half c1 = half(1f), c2 = half(.2f), c3 = half(.2f), c4 = half(.2f);

	// 	var vertex = new Vertex {
	// 		normal = back(),
	// 		tangent = half4(h1, h0, h0, half(-1f)),
    //         color = half4(c1, c2, c3, c4)
	// 	};

	// 	vertex.position = 0f;
	// 	vertex.texCoord0 = h0;
	// 	vertices[0] = vertex;

	// 	vertex.position = right();
	// 	vertex.texCoord0 = half2(h1, h0);
	// 	vertices[1] = vertex;

	// 	vertex.position = up();
	// 	vertex.texCoord0 = half2(h0, h1);
	// 	vertices[2] = vertex;

	// 	vertex.position = float3(1f, 1f, 0f);
	// 	vertex.texCoord0 = h1;
	// 	vertices[3] = vertex;

    //     meshData.SetIndexBufferParams(triangleIndexCount, IndexFormat.UInt16);
    //     NativeArray<ushort> triangleIndices = meshData.GetIndexData<ushort>();
	// 	triangleIndices[0] = 0;
	// 	triangleIndices[1] = 2;
	// 	triangleIndices[2] = 1;
	// 	triangleIndices[3] = 1;
	// 	triangleIndices[4] = 2;
	// 	triangleIndices[5] = 3;

    //     var bounds = new Bounds(new Vector3(0.5f, 0.5f), new Vector3(1f, 1f));

    //     meshData.subMeshCount = 1;
	// 	meshData.SetSubMesh(0, new SubMeshDescriptor(0, triangleIndexCount) {
	// 		bounds = bounds,
	// 		vertexCount = vertexCount
	// 	});

    //     mesh = new Mesh {
	// 		name = "Procedural Mesh"
	// 	};

	// 	Mesh.ApplyAndDisposeWritableMeshData(meshDataArray, mesh);
	// 	GetComponent<MeshFilter>().mesh = mesh;
	// }