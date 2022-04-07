using Unity.Mathematics;
using UnityEngine;
using Unity.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine.Rendering;
using static Unity.Mathematics.math;
using Unity.Collections.LowLevel.Unsafe;


namespace Geometry {

	public interface IMeshStreams { 
        void Setup(Mesh.MeshData data, Bounds bounds, int vertexCount, int indexCount);
        void SetVertex(int index, Vertex data);
        void SetTriangle(int index, int3 triangle);
		Mesh.MeshData MeshData {get; set;}
    }
}

namespace Geometry.Streams {

    public struct SingleStream : IMeshStreams {
		public Mesh.MeshData MeshData {get;set;}

        [StructLayout(LayoutKind.Sequential)]
		struct Stream0 {
			public float3 position, normal;
			public float4 tangent;
			public float3 color;
			public float2 texCoord0;
		}
		
		[NativeDisableContainerSafetyRestriction]
		NativeArray<Stream0> stream0;
		[NativeDisableContainerSafetyRestriction]
        NativeArray<TriangleUInt16> triangles;
        public void Setup (Mesh.MeshData meshData, Bounds bounds, int vertexCount, int indexCount) 
		{
			MeshData = meshData;
			NativeArray<VertexAttributeDescriptor> descriptor = new NativeArray<VertexAttributeDescriptor>(5, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
			descriptor[0] = new VertexAttributeDescriptor(dimension: 3);
			descriptor[1] = new VertexAttributeDescriptor(VertexAttribute.Normal, dimension: 3);
			descriptor[2] = new VertexAttributeDescriptor(VertexAttribute.Tangent, dimension: 4);
			descriptor[3] = new VertexAttributeDescriptor(VertexAttribute.Color, dimension: 3);
			descriptor[4] = new VertexAttributeDescriptor(VertexAttribute.TexCoord0, dimension: 2);

			meshData.SetVertexBufferParams(vertexCount, descriptor);
			descriptor.Dispose();

			meshData.SetIndexBufferParams(indexCount, IndexFormat.UInt16);
			
			meshData.subMeshCount = 1;
			meshData.SetSubMesh(0, new SubMeshDescriptor(0, indexCount) {bounds = bounds, vertexCount = vertexCount}, MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontValidateIndices);

            stream0 = meshData.GetVertexData<Stream0>();
            triangles = meshData.GetIndexData<ushort>().Reinterpret<TriangleUInt16>(2);
		}

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetVertex (int index, Vertex vertex) => stream0[index] = new Stream0 
		{
			position = vertex.position,
			normal = vertex.normal,
			tangent = vertex.tangent,
			color = vertex.color,
			texCoord0 = vertex.texCoord0
		};
        public void SetTriangle (int index, int3 triangle) => triangles[index] = triangle;
    }
}

namespace Geometry.Streams {

	[StructLayout(LayoutKind.Sequential)]
	public struct TriangleUInt16 
    {
		public ushort a, b, c;
		public static implicit operator TriangleUInt16 (int3 t) => new TriangleUInt16 
        {
			a = (ushort)t.x,
			b = (ushort)t.y,
			c = (ushort)t.z
		};
	}
}