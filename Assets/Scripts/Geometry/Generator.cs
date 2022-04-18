using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;


using static Unity.Mathematics.math;

namespace Geometry {
    public interface IMeshGenerator {

		Bounds Bounds { get; }
        int VertexCount { get;}
		int FaceAmount { get; set;}
		int IndexCount { get; }
        int JobLength { get; }
		NativeArray<Face> Faces {get;set;}
		void Execute<S>(int i, S streams) where S : struct, IMeshStreams;
	}
}

namespace Geometry.Generators {

	public struct GenerateFaces : IMeshGenerator {
		public int FaceAmount{get; set;}
        public int VertexCount => 4 * FaceAmount;
		public int IndexCount => 6 * FaceAmount;
		public int JobLength => 1;
		[ReadOnly]
        NativeArray<Face> faces;
		public Bounds Bounds => new Bounds(new Vector3(0.5f, 0.5f), new Vector3(1f, 1f));
        public NativeArray<Face> Faces { get => faces; set => faces = value; }
        public void Execute<S>(int z, S streams) where S : struct, IMeshStreams 
		{
			Vertex vertex = new Vertex();
			int v0, v1, v2, v3 = 0;

			// // TEST
			// int[] neighbours = new int[20];
			// neighbours[0] =1;

			for (int i = 0; i < FaceAmount; i++)
			{
				vertex.color = Faces[i].color;
				vertex.tangent = Faces[i].direction.ToTangent();
				vertex.normal = Faces[i].direction.FloatToNormal();

				vertex.position = Faces[i].x;
				v0 = i * 4;
				v1 = i * 4 + 1;
				v2 = i * 4 + 2;
				v3 = i * 4 + 3;

				vertex.texCoord0 = float2(0,0);
				streams.SetVertex(v0, vertex);

				vertex.texCoord0 = float2(0,1);
				vertex.position = Faces[i].y;
				streams.SetVertex(v1, vertex);

				vertex.texCoord0 = float2(1,1);
				vertex.position = Faces[i].z;
				streams.SetVertex(v2, vertex);

				vertex.texCoord0 = float2(1,0);
				vertex.position = Faces[i].w;
				streams.SetVertex(v3, vertex);

				streams.SetTriangle(i * 2, int3(v2, v0, v1));
				streams.SetTriangle(i * 2 + 1, int3(v0, v2, v3));
			}
			
			// bool optimze = true;
			// if (optimze)
			// {
			// 	// for (int i = 0; i < streams.MeshData.GetIndices; i++)
			// 	// {
			// 	// 	Debug.Log("Optimze");
			// 	// }
			// }


		}

    } 


}